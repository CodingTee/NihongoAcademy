using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using NihongoAcademy.Services;
using Xunit;

namespace NihongoAcademy.Tests;

public class XpServiceTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public XpServiceTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static IConfiguration BuildConfig(bool seed = false)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseStartup:SeedCommunityActivity"] = seed ? "true" : "false"
            })
            .Build();
    }

    private (ApplicationDbContext, XpService) NewScope()
    {
        var context = _fixture.CreateContext();
        return (context, new XpService(context, BuildConfig()));
    }

    [Fact]
    public async Task AwardXpAsync_Increments_TotalXp_And_Returns_Weekly()
    {
        var userId = "award-" + Guid.NewGuid();
        var (_, service) = NewScope();

        var (total, weekly) = await service.AwardXpAsync(userId, 10, "lesson");

        Assert.Equal(10, total);
        Assert.Equal(10, weekly);
    }

    [Fact]
    public async Task AwardCappedXpAsync_Stops_At_Daily_Cap()
    {
        var userId = "cap-" + Guid.NewGuid();
        var (_, service) = NewScope();

        var r1 = await service.AwardCappedXpAsync(userId, 10, "community_post", 25);
        var r2 = await service.AwardCappedXpAsync(userId, 10, "community_post", 25);
        var r3 = await service.AwardCappedXpAsync(userId, 10, "community_post", 25);

        Assert.Equal(10, r1.rewarded);
        Assert.Equal(10, r2.rewarded);
        Assert.Equal(5, r3.rewarded);   // only 5 XP left under the 25/day cap
        Assert.Equal(25, r3.newTotalXp);
    }

    [Fact]
    public async Task AwardCappedXpAsync_Ignores_Client_Amount_Per_Source()
    {
        var userId = "cap2-" + Guid.NewGuid();
        var (_, service) = NewScope();

        // Two different sources are capped independently; a single source cannot exceed its cap.
        var post = await service.AwardCappedXpAsync(userId, 999, "community_post", 30);
        var comment = await service.AwardCappedXpAsync(userId, 999, "community_comment", 25);

        Assert.Equal(30, post.rewarded);   // capped to the configured 30, not 999
        Assert.Equal(25, comment.rewarded); // capped to 25, independent of the post cap
    }

    [Fact]
    public async Task AwardXpAsync_Concurrent_Awards_Do_Not_Lose_Updates()
    {
        var userId = "concurrent-" + Guid.NewGuid();

        // Seed a profile at 0 so every award is an increment, not an insert.
        using (var ctx = _fixture.CreateContext())
        {
            ctx.UserProfiles.Add(new UserProfile { UserId = userId, TotalXp = 0, CreatedDate = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        const int awards = 20;
        const int amount = 5;
        var tasks = Enumerable.Range(0, awards)
            .Select(_ => Task.Run(async () =>
            {
                using var ctx = _fixture.CreateContext();
                var svc = new XpService(ctx, BuildConfig());
                await svc.AwardXpAsync(userId, amount, "concurrent");
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        using var verify = _fixture.CreateContext();
        var total = await verify.UserProfiles
            .Where(p => p.UserId == userId)
            .Select(p => p.TotalXp)
            .FirstAsync();

        // Without the atomic database-level increment this would be < 100 because
        // concurrent read-modify-write cycles overwrite each other.
        Assert.Equal(awards * amount, total);
    }

    [Fact]
    public async Task GetWeeklyLeaderboard_Ranks_By_WeeklyXp_Desc()
    {
        _fixture.Reset();
        var u1 = "lb-" + Guid.NewGuid();
        var u2 = "lb-" + Guid.NewGuid();

        using (var ctx = _fixture.CreateContext())
        {
            ctx.Users.Add(new IdentityUser { Id = u1, UserName = "Alice" });
            ctx.Users.Add(new IdentityUser { Id = u2, UserName = "Bob" });
            ctx.UserProfiles.Add(new UserProfile { UserId = u1, TotalXp = 500, CreatedDate = DateTime.UtcNow });
            ctx.UserProfiles.Add(new UserProfile { UserId = u2, TotalXp = 900, CreatedDate = DateTime.UtcNow });
            ctx.UserXpLogs.Add(new UserXpLog { UserId = u1, Amount = 50, Source = "t", EarnedAt = DateTime.UtcNow });
            ctx.UserXpLogs.Add(new UserXpLog { UserId = u2, Amount = 80, Source = "t", EarnedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        using (var ctx = _fixture.CreateContext())
        {
            var svc = new XpService(ctx, BuildConfig());
            var lb = await svc.GetWeeklyLeaderboardAsync(null, top: 50);

            var bob = lb.TopEntries.FirstOrDefault(e => e.UserId == u2);
            var alice = lb.TopEntries.FirstOrDefault(e => e.UserId == u1);

            Assert.NotNull(bob);
            Assert.NotNull(alice);
            Assert.Equal(1, bob!.Rank);   // 80 weekly > 50 weekly
            Assert.Equal(2, alice!.Rank);
            Assert.Equal(80, bob.WeeklyXp);
        }
    }
}
