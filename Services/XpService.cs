using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Services
{
    public class LeaderboardEntryViewModel
    {
        public int Rank { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? AvatarJson { get; set; }
        public string? AvatarImagePath { get; set; }
        public string AvatarMode { get; set; } = "generated";
        public int WeeklyXp { get; set; }
        public int TotalXp { get; set; }
        public string? LearningGoal { get; set; }
        public string? CurrentLevel { get; set; }
        public bool IsCurrentUser { get; set; }
        public string TierBadge { get; set; } = string.Empty;
        public string TierClass { get; set; } = string.Empty;
    }

    public class UserXpSummaryViewModel
    {
        public int WeeklyXp { get; set; }
        public int TotalXp { get; set; }
        public int CurrentRank { get; set; }
        public int TotalParticipants { get; set; }
        public int XpToNextRank { get; set; }
        public DateTime WeekEndsAt { get; set; }
    }

    public class LeaderboardPageViewModel
    {
        public List<LeaderboardEntryViewModel> TopEntries { get; set; } = new();
        public LeaderboardEntryViewModel? CurrentUserEntry { get; set; }
        public UserXpSummaryViewModel UserSummary { get; set; } = new();
        public DateTime WeekStartsAt { get; set; }
        public DateTime WeekEndsAt { get; set; }
    }

    public class XpService
    {
        private readonly ApplicationDbContext _context;
        private readonly bool _seedCommunityActivity;

        public XpService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _seedCommunityActivity = configuration.GetValue("DatabaseStartup:SeedCommunityActivity", true);
        }

        public static DateTime GetCurrentWeekStartUtc()
        {
            var now = DateTime.UtcNow;
            int diff = (7 + (int)now.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return now.Date.AddDays(-1 * diff);
        }

        public static DateTime GetCurrentWeekEndUtc()
        {
            return GetCurrentWeekStartUtc().AddDays(7);
        }

        public async Task<(int newTotalXp, int weeklyXp)> AwardXpAsync(string userId, int amount, string source, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(userId) || amount <= 0)
                return (0, 0);

            var now = DateTime.UtcNow;

            // 1. The log is the source of truth for both totals and daily caps.
            _context.UserXpLogs.Add(new UserXpLog
            {
                UserId = userId,
                Amount = amount,
                Source = source,
                Description = description,
                EarnedAt = now
            });

            // 2. Make sure a profile row exists so the atomic increment below has a target.
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                _context.UserProfiles.Add(new UserProfile
                {
                    UserId = userId,
                    TotalXp = 0,
                    CreatedDate = now
                });
            }

            await _context.SaveChangesAsync();

            // 3. Increment at the database level. A naive in-memory `profile.TotalXp += amount`
            //    is a read-modify-write that loses updates when two awards for the same user
            //    run on different threads (e.g. a cheer and a practice finishing together).
            await _context.UserProfiles
                .Where(p => p.UserId == userId)
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.TotalXp, x => x.TotalXp + amount));

            // 4. Return the freshly persisted values the callers expect.
            var newTotalXp = await _context.UserProfiles
                .Where(p => p.UserId == userId)
                .Select(p => p.TotalXp)
                .FirstAsync();

            var weekStart = GetCurrentWeekStartUtc();
            var weeklyXp = await _context.UserXpLogs
                .Where(x => x.UserId == userId && x.EarnedAt >= weekStart)
                .SumAsync(x => x.Amount);

            return (newTotalXp, weeklyXp);
        }

        /// <summary>
        /// Awards XP but never past a ceiling for that source on that day. Actions the
        /// server cannot verify (posting, commenting, self-reported sentence drills) are
        /// otherwise repeatable without limit and would swamp the weekly leaderboard.
        /// </summary>
        public async Task<(int newTotalXp, int weeklyXp, int rewarded)> AwardCappedXpAsync(
            string userId, int amount, string source, int dailyCap, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(userId) || amount <= 0 || dailyCap <= 0)
                return (await GetTotalXpAsync(userId), await GetWeeklyXpAsync(userId), 0);

            var dayStart = DateTime.UtcNow.Date;
            var awardedToday = await _context.UserXpLogs
                .Where(x => x.UserId == userId && x.Source == source && x.EarnedAt >= dayStart)
                .SumAsync(x => (int?)x.Amount) ?? 0;

            var remaining = dailyCap - awardedToday;
            if (remaining <= 0)
                return (await GetTotalXpAsync(userId), await GetWeeklyXpAsync(userId), 0);

            var rewarded = Math.Min(amount, remaining);
            var (newTotalXp, weeklyXp) = await AwardXpAsync(userId, rewarded, source, description);
            return (newTotalXp, weeklyXp, rewarded);
        }

        private async Task<int> GetTotalXpAsync(string userId) =>
            await _context.UserProfiles
                .Where(p => p.UserId == userId)
                .Select(p => (int?)p.TotalXp)
                .FirstOrDefaultAsync() ?? 0;

        private async Task<int> GetWeeklyXpAsync(string userId)
        {
            var weekStart = GetCurrentWeekStartUtc();
            return await _context.UserXpLogs
                .Where(x => x.UserId == userId && x.EarnedAt >= weekStart)
                .SumAsync(x => (int?)x.Amount) ?? 0;
        }

        private static readonly (string Email, string DisplayName, string Level, string Goal, string Country, string Bio, string AvatarJson, int BaseTotal, int WeeklyTarget)[] SeedLearners = new[]
        {
            ("miku@nihongoseed.community",   "Miku Takahashi",  "JLPT N4",   "Anime & Manga",      "Japan",         "Studying Japanese every evening through anime subtitles and JLPT mock drills!", "{\"skinTone\":\"#F5C4B3\",\"hairColor\":\"#4A3426\",\"hairStyle\":\"long-wavy\",\"expression\":\"happy\",\"accessory\":\"headphones\",\"background\":\"#FFF0F3\"}", 3420, 520),
            ("neko@nihongoseed.community",   "Neko Sensei",     "JLPT N3",   "Daily Conversation", "Canada",        "Long-time learner helping community peers with sentence corrections and natural nuances.", "{\"skinTone\":\"#C9966B\",\"hairColor\":\"#222222\",\"hairStyle\":\"spiky\",\"expression\":\"calm\",\"accessory\":\"glasses\",\"background\":\"#F5EDE0\"}", 4150, 465),
            ("sakura@nihongoseed.community", "Sakura Sato",     "JLPT N4",   "Travel to Kyoto",    "Singapore",     "Planning a 2-week cultural trip to Kyoto and Kansai. Practicing daily kanji!", "{\"skinTone\":\"#E5C2A8\",\"hairColor\":\"#222222\",\"hairStyle\":\"ponytail\",\"expression\":\"wink\",\"accessory\":\"bow-tie\",\"background\":\"#EDE7F6\"}", 2980, 410),
            ("haru@nihongoseed.community",   "Haru Tanaka",     "JLPT N5",   "Pass JLPT N5",       "United States", "Building sentence foundations and mastering basic particles with NihongoAcademy.", "{\"skinTone\":\"#C9966B\",\"hairColor\":\"#A0522D\",\"hairStyle\":\"spiky\",\"expression\":\"calm\",\"accessory\":\"glasses\",\"background\":\"#E3F2FD\"}", 2120, 365),
            ("kenji@nihongoseed.community",  "Kenji Ito",       "JLPT N5",   "Grammar Mastery",    "Australia",     "Focused on verb conjugations and te-form sentence structures.", "{\"skinTone\":\"#F5C4B3\",\"hairColor\":\"#4A3426\",\"hairStyle\":\"mens-spiky\",\"expression\":\"smile\",\"accessory\":\"none\",\"background\":\"#E3F2FD\"}", 1890, 320),
            ("sora@nihongoseed.community",   "Sora Yamamoto",   "JLPT N4",   "Study in Tokyo",     "United Kingdom","Working towards a university exchange program in Tokyo next spring.", "{\"skinTone\":\"#F5C4B3\",\"hairColor\":\"#4A3426\",\"hairStyle\":\"long-wavy\",\"expression\":\"smile\",\"accessory\":\"bow-tie\",\"background\":\"#E8F5E9\"}", 2350, 280),
            ("yume@nihongoseed.community",   "Yume Watanabe",   "Beginner",  "Read Light Novels",  "Malaysia",      "Dreaming of reading Japanese light novels without looking at a dictionary.", "{\"skinTone\":\"#E5C2A8\",\"hairColor\":\"#F0C040\",\"hairStyle\":\"short-bob\",\"expression\":\"happy\",\"accessory\":\"sakura-pin\",\"background\":\"#FFF0F3\"}", 1640, 245),
            ("kenta@nihongoseed.community",  "Kenta Suzuki",    "JLPT N5",   "Travel Japan",       "Germany",       "Practicing ordering at izakayas and asking for subway directions.", "{\"skinTone\":\"#C9966B\",\"hairColor\":\"#222222\",\"hairStyle\":\"buzz\",\"expression\":\"wink\",\"accessory\":\"headphones\",\"background\":\"#FFF8E1\"}", 1480, 210),
            ("aoi@nihongoseed.community",    "Aoi Nakamura",    "JLPT N5",   "J-Pop & Lyrics",     "Taiwan",        "Learning kanji stroke orders through favorite J-Pop and Vocaloid songs.", "{\"skinTone\":\"#E5C2A8\",\"hairColor\":\"#C84B31\",\"hairStyle\":\"ponytail\",\"expression\":\"surprised\",\"accessory\":\"none\",\"background\":\"#FFF8E1\"}", 1220, 180),
            ("hana@nihongoseed.community",   "Hana Yamada",     "JLPT N4",   "Work in Japan",      "South Korea",   "Polishing polite keigo expressions and business greetings step by step.", "{\"skinTone\":\"#E5C2A8\",\"hairColor\":\"#222222\",\"hairStyle\":\"ponytail\",\"expression\":\"happy\",\"accessory\":\"sakura-pin\",\"background\":\"#E8F5E9\"}", 1760, 150),
            ("mina@nihongoseed.community",   "Mina Kobayashi",  "JLPT N5",   "Daily Practice",     "France",        "Committed to 15 minutes of Japanese practice every morning with my tea.", "{\"skinTone\":\"#F5C4B3\",\"hairColor\":\"#A0522D\",\"hairStyle\":\"short-bob\",\"expression\":\"surprised\",\"accessory\":\"glasses\",\"background\":\"#FFF8E1\"}", 1150, 125),
            ("yuki@nihongoseed.community",   "Yuki Kato",       "Beginner",  "Kanji Mastery",      "New Zealand",   "Mastered hiragana & katakana! Now conquering the first 100 kanji characters.", "{\"skinTone\":\"#F5C4B3\",\"hairColor\":\"#FFFFFF\",\"hairStyle\":\"long-wavy\",\"expression\":\"calm\",\"accessory\":\"none\",\"background\":\"#EDE7F6\"}", 980, 100),
            ("ren@nihongoseed.community",    "Ren Yoshida",     "Beginner",  "Conversation",       "Philippines",   "Focusing on natural pronunciation drills and listening comprehension.", "{\"skinTone\":\"#C9966B\",\"hairColor\":\"#F0C040\",\"hairStyle\":\"mens-spiky\",\"expression\":\"smile\",\"accessory\":\"glasses\",\"background\":\"#F5EDE0\"}", 850, 80),
            ("minoru@nihongoseed.community", "Minoru Saito",    "JLPT N5",   "Reading News",       "Netherlands",   "Reading NHK Easy Japanese articles every morning to build reading speed.", "{\"skinTone\":\"#8D5E3C\",\"hairColor\":\"#222222\",\"hairStyle\":\"buzz\",\"expression\":\"smile\",\"accessory\":\"none\",\"background\":\"#E8F5E9\"}", 720, 60),
            ("tomo@nihongoseed.community",   "Tomo Shimizu",    "Beginner",  "Travel Prep",        "Brazil",        "First solo trip to Tokyo planned for cherry blossom season next spring!", "{\"skinTone\":\"#F5C4B3\",\"hairColor\":\"#4A3426\",\"hairStyle\":\"spiky\",\"expression\":\"happy\",\"accessory\":\"headphones\",\"background\":\"#F5EDE0\"}", 630, 45),
            ("rika@nihongoseed.community",   "Rika Inoue",      "JLPT N4",   "Pass JLPT N4",       "Sweden",        "Reviewing intermediate sentence patterns and taking daily practice quizzes.", "{\"skinTone\":\"#E5C2A8\",\"hairColor\":\"#FFFFFF\",\"hairStyle\":\"short-bob\",\"expression\":\"wink\",\"accessory\":\"sakura-pin\",\"background\":\"#FFF0F3\"}", 1420, 30),
        };

        /// <summary>
        /// Tops up the demo learners so the weekly leaderboard is not empty on a fresh
        /// install. Called once from Program.cs at startup: it must never run inside a
        /// page render, because that turns every leaderboard view into a write.
        /// </summary>
        public async Task EnsureSeedCommunityActivityAsync()
        {
            if (!_seedCommunityActivity) return;

            await EnsureWeeklySeedActivityAsync(GetCurrentWeekStartUtc());
        }

        private async Task EnsureWeeklySeedActivityAsync(DateTime weekStart)
        {
            var activeWeeklyCount = await _context.UserXpLogs
                .Where(x => x.EarnedAt >= weekStart)
                .Select(x => x.UserId)
                .Distinct()
                .CountAsync();

            if (activeWeeklyCount >= 10)
            {
                return;
            }

            var seedEmails = SeedLearners.Select(s => s.Email).ToHashSet();
            var seedUsers = await _context.Users
                .Where(u => u.Email != null && seedEmails.Contains(u.Email))
                .ToListAsync();

            if (!seedUsers.Any())
            {
                return;
            }

            var userMap = seedUsers.ToDictionary(u => u.Email!, u => u);
            var seedUserIds = seedUsers.Select(u => u.Id).ToHashSet();

            var profiles = await _context.UserProfiles
                .Where(p => seedUserIds.Contains(p.UserId))
                .ToListAsync();
            var profileMap = profiles.ToDictionary(p => p.UserId);

            var existingLogsThisWeek = await _context.UserXpLogs
                .Where(x => seedUserIds.Contains(x.UserId) && x.EarnedAt >= weekStart)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();
            var hasLogsThisWeek = existingLogsThisWeek.ToHashSet();

            var now = DateTime.UtcNow;

            foreach (var learner in SeedLearners)
            {
                if (!userMap.TryGetValue(learner.Email, out var user))
                    continue;

                // 1. Update or create profile with rich details
                if (!profileMap.TryGetValue(user.Id, out var profile))
                {
                    profile = new UserProfile
                    {
                        UserId = user.Id,
                        CreatedDate = now.AddMonths(-3)
                    };
                    _context.UserProfiles.Add(profile);
                    profileMap[user.Id] = profile;
                }

                profile.DisplayName = learner.DisplayName;
                profile.CurrentLevel = learner.Level;
                profile.LearningGoal = learner.Goal;
                profile.Country = learner.Country;
                profile.Bio = learner.Bio;
                if (string.IsNullOrWhiteSpace(profile.AvatarJson))
                {
                    profile.AvatarJson = learner.AvatarJson;
                }
                profile.TotalXp = Math.Max(profile.TotalXp, learner.BaseTotal + learner.WeeklyTarget);

                // 2. Add realistic weekly XP logs if user doesn't have logs this week
                if (!hasLogsThisWeek.Contains(user.Id))
                {
                    int target = learner.WeeklyTarget;
                    int log1 = (int)(target * 0.45);
                    int log2 = (int)(target * 0.35);
                    int log3 = target - log1 - log2;

                    var time1 = weekStart.AddDays(1).AddHours(10);
                    if (time1 > now) time1 = now.AddMinutes(-120);

                    var time2 = weekStart.AddDays(2).AddHours(14);
                    if (time2 > now) time2 = now.AddMinutes(-60);

                    var time3 = weekStart.AddDays(3).AddHours(18);
                    if (time3 > now) time3 = now.AddMinutes(-15);

                    _context.UserXpLogs.Add(new UserXpLog
                    {
                        UserId = user.Id,
                        Amount = log1,
                        Source = "Practice Drill",
                        Description = "Completed lesson practice drill",
                        EarnedAt = time1
                    });

                    _context.UserXpLogs.Add(new UserXpLog
                    {
                        UserId = user.Id,
                        Amount = log2,
                        Source = "Sentence Builder",
                        Description = "Formed Japanese sentence challenge",
                        EarnedAt = time2
                    });

                    if (log3 > 0)
                    {
                        _context.UserXpLogs.Add(new UserXpLog
                        {
                            UserId = user.Id,
                            Amount = log3,
                            Source = "Vocabulary Quiz",
                            Description = "Aced vocabulary review quiz",
                            EarnedAt = time3
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<LeaderboardPageViewModel> GetWeeklyLeaderboardAsync(string? currentUserId, int top = 50)
        {
            var weekStart = GetCurrentWeekStartUtc();
            var weekEnd = GetCurrentWeekEndUtc();

            // Aggregate weekly XP from logs. No writing here: this runs on every page
            // view through the navbar, so a stray SaveChanges would hit the database
            // on each request.
            var weeklyAggregates = await _context.UserXpLogs
                .Where(x => x.EarnedAt >= weekStart)
                .GroupBy(x => x.UserId)
                .Select(g => new { UserId = g.Key, WeeklyXp = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.WeeklyXp)
                .ToListAsync();

            // Get all user profiles with positive total XP or weekly XP
            var candidateUserIds = weeklyAggregates.Select(x => x.UserId).ToHashSet();
            if (!string.IsNullOrEmpty(currentUserId))
            {
                candidateUserIds.Add(currentUserId);
            }

            // Also include top registered users if weekly log count is small (to give lively community feeling)
            var existingProfiles = await _context.UserProfiles
                .Where(p => candidateUserIds.Contains(p.UserId) || p.TotalXp > 0)
                .ToListAsync();

            foreach (var p in existingProfiles)
            {
                candidateUserIds.Add(p.UserId);
            }

            var users = await _context.Users
                .Where(u => candidateUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "Learner");

            var profileMap = existingProfiles.ToDictionary(p => p.UserId);

            // Build full list of ranked participants
            var allRanked = new List<LeaderboardEntryViewModel>();
            var weeklyMap = weeklyAggregates.ToDictionary(x => x.UserId, x => x.WeeklyXp);

            foreach (var uid in candidateUserIds)
            {
                profileMap.TryGetValue(uid, out var prof);
                users.TryGetValue(uid, out var uName);

                int wXp = weeklyMap.GetValueOrDefault(uid, 0);
                int tXp = prof?.TotalXp ?? wXp;

                // If user has 0 weekly XP and 0 total XP and is not current user, skip to keep leaderboard clean
                if (wXp == 0 && tXp == 0 && uid != currentUserId)
                    continue;

                allRanked.Add(new LeaderboardEntryViewModel
                {
                    UserId = uid,
                    UserName = uName ?? "Learner",
                    DisplayName = !string.IsNullOrWhiteSpace(prof?.DisplayName) ? prof.DisplayName : (uName ?? "Sakura Learner"),
                    AvatarJson = prof?.AvatarJson,
                    AvatarImagePath = prof?.AvatarImagePath,
                    AvatarMode = prof?.AvatarMode ?? "generated",
                    WeeklyXp = wXp,
                    TotalXp = tXp,
                    LearningGoal = prof?.LearningGoal,
                    CurrentLevel = prof?.CurrentLevel,
                    IsCurrentUser = uid == currentUserId
                });
            }

            // Order by Weekly XP desc, then Total XP desc
            allRanked = allRanked
                .OrderByDescending(x => x.WeeklyXp)
                .ThenByDescending(x => x.TotalXp)
                .ToList();

            // Assign ranks and tier badges
            for (int i = 0; i < allRanked.Count; i++)
            {
                var entry = allRanked[i];
                entry.Rank = i + 1;

                if (entry.Rank == 1)
                {
                    entry.TierBadge = "🥇 Sakura Champion";
                    entry.TierClass = "na-tier-gold";
                }
                else if (entry.Rank == 2)
                {
                    entry.TierBadge = "🥈 Silver Blossom";
                    entry.TierClass = "na-tier-silver";
                }
                else if (entry.Rank == 3)
                {
                    entry.TierBadge = "🥉 Bronze Petal";
                    entry.TierClass = "na-tier-bronze";
                }
                else if (entry.Rank <= 10)
                {
                    entry.TierBadge = "🌸 Sakura Elite";
                    entry.TierClass = "na-tier-top10";
                }
                else
                {
                    entry.TierBadge = "🌱 Rising Learner";
                    entry.TierClass = "na-tier-rising";
                }
            }

            var topEntries = allRanked.Take(top).ToList();
            var currentUserEntry = !string.IsNullOrEmpty(currentUserId)
                ? allRanked.FirstOrDefault(x => x.UserId == currentUserId)
                : null;

            int xpToNext = 0;
            if (currentUserEntry != null && currentUserEntry.Rank > 1)
            {
                var nextAbove = allRanked.ElementAtOrDefault(currentUserEntry.Rank - 2);
                if (nextAbove != null)
                {
                    xpToNext = Math.Max(1, (nextAbove.WeeklyXp - currentUserEntry.WeeklyXp) + 1);
                }
            }

            var summary = new UserXpSummaryViewModel
            {
                WeeklyXp = currentUserEntry?.WeeklyXp ?? 0,
                TotalXp = currentUserEntry?.TotalXp ?? 0,
                CurrentRank = currentUserEntry?.Rank ?? (allRanked.Count + 1),
                TotalParticipants = allRanked.Count,
                XpToNextRank = xpToNext,
                WeekEndsAt = weekEnd
            };

            return new LeaderboardPageViewModel
            {
                TopEntries = topEntries,
                CurrentUserEntry = currentUserEntry,
                UserSummary = summary,
                WeekStartsAt = weekStart,
                WeekEndsAt = weekEnd
            };
        }
    }
}
