using System.IO.Compression;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Left off while there is no email sender registered: flipping this on without one
    // would make every registration unconfirmable. Enable per environment once SMTP and
    // an IEmailSender are wired up.
    options.SignIn.RequireConfirmedAccount =
        builder.Configuration.GetValue("Identity:RequireConfirmedAccount", false);
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Re-validate the security stamp on EVERY request. Combined with
// UserManager.UpdateSecurityStampAsync after a role change / disable / admin password
// reset, this forces the affected account's existing sessions to expire immediately
// (the next authenticated request fails validation and signs them out) instead of
// lingering for the default 30-minute window.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    options.ValidationInterval = TimeSpan.Zero);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Admin/AccessDenied";
});

// The tool factory reads the signed-in learner from HttpContext, so the accessor must be
// registered before the controllers that depend on it.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Static assets only: HTML stays uncompressed so authenticated pages avoid BREACH-style risk.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[]
    {
        "text/css", "application/javascript", "text/javascript",
        "image/svg+xml", "application/xml", "text/plain"
    };
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

// Shared cache for read-only catalogue data and navbar summaries.
builder.Services.AddMemoryCache();

builder.Services.AddScoped<LessonMasteryService>();
builder.Services.AddSingleton<IAppClock, AppClock>();
builder.Services.AddScoped<ProgressSummaryService>();
builder.Services.AddScoped<AchievementService>();
builder.Services.AddScoped<XpService>();
builder.Services.AddSingleton<AudioAssetService>();
builder.Services.AddScoped<PublicCatalogCache>();

// This app no longer sends email (the SMTP / verification-code system was removed).
// The scaffolded Identity UI pages that still inject IEmailSender (ResendEmailConfirmation,
// Manage/Email) need a registered implementation, so a no-op stub is provided instead of a
// real mail server.
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, NullEmailSender>();

// Audit: login + operation logs.
builder.Services.AddScoped<AuditLogService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Render HTTP errors inside the academy experience while preserving the status code.
app.UseStatusCodePagesWithReExecute("/Home/StatusPage", "?code={0}");

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Versioned CSS/JS (asp-append-version) is safe to cache long; uploaded files are not.
        var path = ctx.Context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=604800";
        }
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();
app.MapRazorPages();

// Auto-apply pending migrations on startup only when there is work to do.
using (var migrateScope = app.Services.CreateScope())
{
    var db = migrateScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var applyMigrationsOnStartup = app.Configuration.GetValue("DatabaseStartup:ApplyMigrations", true);
    if (applyMigrationsOnStartup && (await db.Database.GetPendingMigrationsAsync()).Any())
    {
        await db.Database.MigrateAsync();
    }
}

// Seed the database only for first-run setup, unless explicitly requested.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Roles (and the dev admin account) must always exist, independent of
        // whether the curriculum is (re)seeded — otherwise a database that already
        // has content would never get the SuperAdmin role created, leaving the
        // role-accounts admin surface unreachable.
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DataSeeder.SeedRolesAsync(roleManager);
        if (app.Environment.IsDevelopment())
        {
            var userManagerForSeed = services.GetRequiredService<UserManager<IdentityUser>>();
            await DataSeeder.SeedAdminUserAsync(userManagerForSeed, app.Configuration);
        }

        var context = services.GetRequiredService<ApplicationDbContext>();
        var seedOnStartup = app.Configuration.GetValue("DatabaseStartup:SeedOnStartup", false);
        var validateCurriculumOnStartup = app.Configuration.GetValue("DatabaseStartup:ValidateCurriculumOnStartup", false);
        var hasCoreContent = await context.Categories.AnyAsync() && await context.Lessons.AnyAsync();

        if (seedOnStartup || !hasCoreContent)
        {
            await DataSeeder.SeedAsync(services, validateCurriculumOnStartup);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex, "Database seeding or curriculum validation failed. Startup has been stopped to avoid serving incomplete learning content.");
        throw;
    }
}

// Give the weekly leaderboard some company on a fresh install. This used to run
// inside GetWeeklyLeaderboardAsync, meaning every page view wrote to the database.
using (var communityScope = app.Services.CreateScope())
{
    try
    {
        var xpService = communityScope.ServiceProvider.GetRequiredService<XpService>();
        await xpService.EnsureSeedCommunityActivityAsync();
    }
    catch (Exception ex)
    {
        var logger = communityScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Community leaderboard seeding skipped.");
    }
}

app.Run();
