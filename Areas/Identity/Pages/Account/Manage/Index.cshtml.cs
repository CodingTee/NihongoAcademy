using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.RegularExpressions;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace NihongoAcademy.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _env = env;
        }

        // ===== Avatar JSON allowlist =====
        // The avatar config is written straight into an SVG on community pages and in
        // the notification bell, so it must never carry learner-controlled free text.
        // Every value is remapped onto the known option set below before it is stored.
        private static readonly string[] AllowedSkinTones = { "#F5C4B3", "#E5C2A8", "#C9966B", "#8D5E3C", "#5E3A22" };
        private static readonly string[] AllowedHairColors = { "#4A3426", "#222222", "#F0C040", "#A0522D", "#C84B31", "#FFFFFF" };
        private static readonly string[] AllowedHairStyles = { "short-bob", "long-wavy", "spiky", "ponytail", "buzz", "mens-spiky" };
        private static readonly string[] AllowedExpressions = { "smile", "happy", "surprised", "calm", "wink" };
        private static readonly string[] AllowedAccessories = { "none", "glasses", "headphones", "sakura-pin", "bow-tie" };
        private static readonly string[] AllowedBackgrounds = { "none", "#FFF0F3", "#F0F3F6", "#F5EDE0", "#E3F2FD", "#E8F5E9", "#EDE7F6" };

        /// <summary>
        /// Rebuilds the avatar config from known-safe values only. Returns null when the
        /// payload is not a JSON object, so an unusable avatar falls back to the defaults.
        /// </summary>
        private static string? SanitizeAvatarJson(string? rawAvatarJson)
        {
            if (string.IsNullOrWhiteSpace(rawAvatarJson)) return null;

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(rawAvatarJson);
            }
            catch (JsonException)
            {
                return null;
            }

            using (document)
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object) return null;

                string? Pick(string name, string[] allowed)
                {
                    if (!document.RootElement.TryGetProperty(name, out var value)) return null;
                    if (value.ValueKind != JsonValueKind.String) return null;
                    var text = value.GetString();
                    return allowed.FirstOrDefault(a => string.Equals(a, text, StringComparison.Ordinal));
                }

                var fields = new[]
                {
                    ("skinTone", AllowedSkinTones),
                    ("hairColor", AllowedHairColors),
                    ("hairStyle", AllowedHairStyles),
                    ("expression", AllowedExpressions),
                    ("accessory", AllowedAccessories),
                    ("background", AllowedBackgrounds)
                };

                using var buffer = new MemoryStream();
                using (var writer = new Utf8JsonWriter(buffer))
                {
                    writer.WriteStartObject();
                    foreach (var (name, allowed) in fields)
                    {
                        var safe = Pick(name, allowed);
                        if (safe != null) writer.WriteString(name, safe);
                    }
                    writer.WriteEndObject();
                }

                return System.Text.Encoding.UTF8.GetString(buffer.ToArray());
            }
        }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsEmailConfirmed { get; set; }

        public string? CurrentAvatarImagePath { get; set; }
        public string CurrentAvatarMode { get; set; } = "generated";

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Username is required.")]
            [StringLength(32, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 32 characters.")]
            [RegularExpression(@"^[a-zA-Z0-9_.@+-]+$", ErrorMessage = "Use letters, numbers, dot, hyphen or underscore only.")]
            public string Username { get; set; } = string.Empty;

            [Display(Name = "Upload avatar")]
            public IFormFile? AvatarFile { get; set; }

            [StringLength(50, MinimumLength = 2, ErrorMessage = "Display name must be between 2 and 50 characters.")]
            [Display(Name = "Display name")]
            public string? DisplayName { get; set; }

            [StringLength(240, ErrorMessage = "Bio cannot exceed 240 characters.")]
            [Display(Name = "Bio")]
            public string? Bio { get; set; }

            [StringLength(30)]
            [Display(Name = "Native language")]
            public string? NativeLanguage { get; set; }

            [StringLength(50)]
            [Display(Name = "Country")]
            public string? Country { get; set; }

            [StringLength(10)]
            [Display(Name = "Theme")]
            public string? ThemePreference { get; set; }

            [Display(Name = "Send me a daily learning reminder")]
            public bool DailyReminder { get; set; }

            public string? AvatarJson { get; set; }

            public string AvatarMode { get; set; } = "generated";

            [Phone(ErrorMessage = "Please enter a valid phone number.")]
            [Display(Name = "Phone number")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Learning goals")]
            public List<string> LearningGoals { get; set; } = new();

            [StringLength(200)]
            public string? LearningGoal { get; set; }

            [StringLength(50), RegularExpression("^(Absolute beginner|Knows hiragana|Conversational basics|Intermediate or above)$")]
            public string? CurrentLevel { get; set; }

            [StringLength(30), RegularExpression("^(Under 18|18 – 24|25 – 34|35 or older)$")]
            public string? AgeGroup { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            Username = userName ?? string.Empty;
            Email = email ?? string.Empty;
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            CurrentAvatarImagePath = profile?.AvatarImagePath;
            CurrentAvatarMode = profile?.AvatarMode ?? "generated";

            var goalList = !string.IsNullOrWhiteSpace(profile?.LearningGoal)
                ? profile.LearningGoal
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList()
                : new List<string>();

            Input = new InputModel
            {
                Username = userName ?? string.Empty,
                PhoneNumber = phoneNumber,
                AvatarJson = profile?.AvatarJson,
                AvatarMode = profile?.AvatarMode ?? "generated",
                AvatarFile = null,
                DisplayName = profile?.DisplayName,
                Bio = profile?.Bio,
                NativeLanguage = profile?.NativeLanguage,
                Country = profile?.Country,
                ThemePreference = profile?.ThemePreference ?? "light",
                DailyReminder = profile?.DailyReminder ?? false,
                LearningGoal = profile?.LearningGoal,
                LearningGoals = goalList,
                CurrentLevel = profile?.CurrentLevel,
                AgeGroup = profile?.AgeGroup
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnGetCheckUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new JsonResult(new { valid = false, available = false, message = "Username cannot be empty." });
            }
            username = username.Trim();
            if (username.Length < 3 || username.Length > 32)
            {
                return new JsonResult(new { valid = false, available = false, message = "Username must be between 3 and 32 characters." });
            }
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_.@+-]+$"))
            {
                return new JsonResult(new { valid = false, available = false, message = "Only letters, numbers, dot, hyphen or underscore allowed." });
            }

            var currentUserId = _userManager.GetUserId(User);
            var existing = await _userManager.FindByNameAsync(username);
            if (existing != null && existing.Id != currentUserId)
            {
                return new JsonResult(new { valid = true, available = false, message = "This username is already taken. Please choose another." });
            }

            return new JsonResult(new { valid = true, available = true, message = "Username is available!" });
        }

        public async Task<IActionResult> OnPostUploadAvatarAsync(IFormFile avatarFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (avatarFile == null || avatarFile.Length == 0)
            {
                return new JsonResult(new { success = false, message = "No file selected." });
            }

            var ext = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!allowed.Contains(ext))
            {
                return new JsonResult(new { success = false, message = "Only JPG, PNG, GIF or WebP images are allowed." });
            }
            if (avatarFile.Length > 3 * 1024 * 1024)
            {
                return new JsonResult(new { success = false, message = "Image size must be smaller than 3 MB." });
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{user.Id}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/avatars/{fileName}?t={DateTime.UtcNow.Ticks}";
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new UserProfile { UserId = user.Id };
                _context.UserProfiles.Add(profile);
            }
            profile.AvatarImagePath = $"/uploads/avatars/{fileName}";
            profile.AvatarMode = "uploaded";
            profile.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, avatarUrl = relativePath, message = "Photo uploaded and saved successfully!" });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // ----- Step 1: Process uploaded avatar FIRST so it is safely saved even if validation has notices -----
            string? newlyUploadedAvatarPath = null;
            if (Input.AvatarFile != null && Input.AvatarFile.Length > 0)
            {
                var ext = Path.GetExtension(Input.AvatarFile.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("Input.AvatarFile", "Only JPG, PNG, GIF or WebP images are allowed.");
                }
                else if (Input.AvatarFile.Length > 3 * 1024 * 1024)
                {
                    ModelState.AddModelError("Input.AvatarFile", "Image must be smaller than 3 MB.");
                }
                else
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                    Directory.CreateDirectory(uploadsDir);
                    var fileName = $"{user.Id}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Input.AvatarFile.CopyToAsync(stream);
                    }
                    newlyUploadedAvatarPath = $"/uploads/avatars/{fileName}";

                    var currentProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                    if (currentProfile == null)
                    {
                        currentProfile = new UserProfile { UserId = user.Id };
                        _context.UserProfiles.Add(currentProfile);
                    }
                    currentProfile.AvatarImagePath = newlyUploadedAvatarPath;
                    currentProfile.AvatarMode = "uploaded";
                    currentProfile.ModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                if (!string.IsNullOrEmpty(newlyUploadedAvatarPath))
                {
                    CurrentAvatarImagePath = newlyUploadedAvatarPath;
                    CurrentAvatarMode = "uploaded";
                    Input.AvatarMode = "uploaded";
                }
                return Page();
            }

            // ----- Step 2: Username check & update -----
            var currentUsername = await _userManager.GetUserNameAsync(user);
            var cleanUsername = Input.Username.Trim();
            if (cleanUsername != currentUsername)
            {
                var owner = await _userManager.FindByNameAsync(cleanUsername);
                if (owner != null && owner.Id != user.Id)
                {
                    ModelState.AddModelError("Input.Username", "That username is already taken. Please choose another.");
                    await LoadAsync(user);
                    if (!string.IsNullOrEmpty(newlyUploadedAvatarPath))
                    {
                        CurrentAvatarImagePath = newlyUploadedAvatarPath;
                        CurrentAvatarMode = "uploaded";
                    }
                    return Page();
                }

                var setUserResult = await _userManager.SetUserNameAsync(user, cleanUsername);
                if (!setUserResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set username.";
                    await LoadAsync(user);
                    return Page();
                }
            }

            // ----- Step 3: Phone number -----
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    await LoadAsync(user);
                    return Page();
                }
            }

            // ----- Step 4: Profile table -----
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new UserProfile { UserId = user.Id };
                _context.UserProfiles.Add(profile);
            }

            profile.AvatarJson = SanitizeAvatarJson(Input.AvatarJson);
            profile.AvatarMode = Input.AvatarMode == "uploaded" ? "uploaded" : "generated";
            if (!string.IsNullOrEmpty(newlyUploadedAvatarPath))
            {
                profile.AvatarImagePath = newlyUploadedAvatarPath;
            }
            profile.DisplayName = Input.DisplayName?.Trim();
            profile.Bio = Input.Bio?.Trim();
            profile.NativeLanguage = Input.NativeLanguage;
            profile.Country = Input.Country;
            profile.ThemePreference = Input.ThemePreference;
            profile.DailyReminder = Input.DailyReminder;
            var validGoals = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Travel & culture", "Work & career", "Study & exams", "Anime & entertainment"
            };
            var chosenGoals = Input.LearningGoals?.Where(g => validGoals.Contains(g)).Distinct().ToList() ?? new List<string>();
            if (chosenGoals.Count == 0 && !string.IsNullOrWhiteSpace(Input.LearningGoal))
            {
                var fallbackGoals = Input.LearningGoal
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(g => validGoals.Contains(g));
                chosenGoals.AddRange(fallbackGoals);
            }
            profile.LearningGoal = chosenGoals.Count > 0 ? string.Join(", ", chosenGoals) : null;
            profile.CurrentLevel = Input.CurrentLevel;
            profile.AgeGroup = Input.AgeGroup;
            profile.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = profile.AvatarMode == "uploaded"
                ? "Your profile and community photo have been saved successfully."
                : "Your profile and community avatar have been saved successfully.";
            return RedirectToPage();
        }
    }
}
