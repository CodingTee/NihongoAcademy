using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NihongoAcademy.Models;
using System.Security.Cryptography;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, bool validateCurriculum = true)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Note: db.Database.Migrate() is already called in Program.cs, no need here

            await SeedRolesAsync(roleManager);
            if (environment.IsDevelopment())
                await SeedAdminUserAsync(userManager, configuration);
            await SeedCategoriesAsync(context);
            await SeedLessonsAsync(context);
            await SeedVocabularyAsync(context);
            await SeedLessonVocabularyAsync(context);
            await SeedHiraganaCurriculumAsync(context);
            await SeedAdvancedHiraganaCurriculumAsync(context);
            await SeedKatakanaCurriculumAsync(context);
            await SeedA1IntroductionUnitAsync(context);
            await SeedA1FamilyUnitAsync(context);
            await SeedRemainingA1CurriculumAsync(context);
            await SeedA1FoundationActivitiesAsync(context);
            await SeedA1GrammarPointsAsync(context);
            await SeedKanjiFoundationCurriculumAsync(context);
            await SeedVocabularyExampleReadingsAsync(context);
            await SeedA2CurriculumAsync(context);
            await SeedA2GrammarPointsAsync(context);
            await SeedReadingCurriculumAsync(context);
            await SeedWritingCurriculumAsync(context);
            await SeedSpeakingCurriculumAsync(context);
            await SeedListeningCurriculumAsync(context);
            await SeedLessonPrerequisitesAsync(context);
            if (environment.IsDevelopment())
                await SeedDemoProgressAsync(userManager, context);
            await SeedQuizzesAsync(context);
            await SeedA1FinalAssessmentAsync(context);
            await SeedKanjiFinalAssessmentAsync(context);
            await SeedA2FinalAssessmentAsync(context);

            if (validateCurriculum)
            {
                await ValidateHiraganaCurriculumAsync(context);
                await ValidateKatakanaCurriculumAsync(context);
                await ValidateA1CurriculumAsync(context);
                await ValidateKanjiCurriculumAsync(context);
                await ValidateA2CurriculumAsync(context);
                await ValidateGrammarCurriculumAsync(context);
                await ValidateIntegratedSkillsCurriculumAsync(context);
                await ValidateCompleteLearningPathAsync(context);
                await ValidateContentQualityAsync(context);
            }
            // Demo community accounts/posts are dev-only AND opt-in via
            // DatabaseStartup:SeedCommunityActivity. Production never seeds them
            // (IsDevelopment is false), and a developer can flip the flag to keep
            // their local DB clean. The same flag already gates the XP top-up in
            // XpService, so it is a single switch for all seed-community behaviour.
            if (environment.IsDevelopment() && configuration.GetValue("DatabaseStartup:SeedCommunityActivity", true))
            {
                await SeedCommunityUsersAsync(userManager, context);
                await SeedCommunityPostsAsync(context);
            }
        }

        private static async Task SeedLessonPrerequisitesAsync(ApplicationDbContext context)
        {
            var coreCategoryOrder = new[] { "Hiragana", "Katakana", "A1 Course", "Kanji", "A2 Course" };
            var lessons = await context.Lessons
                .Where(l => l.IsPublished && coreCategoryOrder.Contains(l.Category!.Name))
                .Include(l => l.Category)
                .ToListAsync();

            var ordered = lessons
                .OrderBy(l => Array.IndexOf(coreCategoryOrder, l.Category!.Name))
                .ThenBy(l => l.DisplayOrder)
                .ThenBy(l => l.Id)
                .ToList();

            var desiredPairs = ordered.Skip(1)
                .Select((lesson, index) => new { LessonId = lesson.Id, PrerequisiteId = ordered[index].Id })
                .ToList();
            var coreLessonIds = ordered.Select(l => l.Id).ToHashSet();
            var stale = await context.LessonPrerequisites
                // The guided path owns every prerequisite attached to a core
                // lesson. Remove legacy links even when their prerequisite is
                // an unpublished/retired lesson outside the current path.
                .Where(p => coreLessonIds.Contains(p.LessonId))
                .ToListAsync();
            context.LessonPrerequisites.RemoveRange(stale.Where(p =>
                !desiredPairs.Any(pair => pair.LessonId == p.LessonId && pair.PrerequisiteId == p.PrerequisiteLessonId)));

            for (var i = 1; i < ordered.Count; i++)
            {
                var lessonId = ordered[i].Id;
                var prerequisiteId = ordered[i - 1].Id;
                if (!await context.LessonPrerequisites.AnyAsync(p =>
                    p.LessonId == lessonId && p.PrerequisiteLessonId == prerequisiteId))
                {
                    context.LessonPrerequisites.Add(new LessonPrerequisite
                    {
                        LessonId = lessonId,
                        PrerequisiteLessonId = prerequisiteId
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "SuperAdmin", "Admin", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            // Seed-account passwords resolve from configuration (user-secrets / env / appsettings)
            // under "AdminSeed:*". No backdoor password travels with the source code. If a value is
            // not configured, a strong random password is generated and printed to the console once,
            // so a fresh developer still gets working accounts without a shared, guessable password.
            string ResolvePassword(string configKey, string accountLabel)
            {
                var configured = configuration[configKey];
                if (!string.IsNullOrWhiteSpace(configured))
                    return configured;

                var generated = GenerateStrongPassword();
                Console.WriteLine(
                    $"[DataSeeder] WARNING: '{configKey}' is not configured. " +
                    $"Generated a one-time random password for the '{accountLabel}' seed account: {generated}");
                return generated;
            }

            var adminEmail = "admin@nihongoacademy.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "+60123456789",
                    PhoneNumberConfirmed = true
                };
                await userManager.CreateAsync(admin, ResolvePassword("AdminSeed:AdminPassword", "admin"));
            }

            // 始终确保管理员账号拥有 SuperAdmin 角色（可管理角色账号的最高权限），
            // 避免历史数据中账号存在但角色缺失，导致登录后看不到管理面板
            // （/Admin 受 [Authorize(Roles="Admin,SuperAdmin")] 保护）。
            if (!await userManager.IsInRoleAsync(admin, "SuperAdmin"))
            {
                await userManager.AddToRoleAsync(admin, "SuperAdmin");
            }

            var studentEmail = "student@nihongoacademy.com";
            var student = await userManager.FindByEmailAsync(studentEmail);
            if (student == null)
            {
                student = new IdentityUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(student, ResolvePassword("AdminSeed:StudentPassword", "student"));
            }
            if (!await userManager.IsInRoleAsync(student, "Student"))
            {
                await userManager.AddToRoleAsync(student, "Student");
            }

            const string demoEmail = "demo@nihongoacademy.com";
            var demo = await userManager.FindByEmailAsync(demoEmail);
            if (demo == null)
            {
                demo = new IdentityUser { UserName = demoEmail, Email = demoEmail, EmailConfirmed = true };
                await userManager.CreateAsync(demo, ResolvePassword("AdminSeed:DemoPassword", "demo"));
            }
            if (!await userManager.IsInRoleAsync(demo, "Student"))
                await userManager.AddToRoleAsync(demo, "Student");
        }

        /// <summary>
        /// Generates a 16-character password satisfying ASP.NET Identity's default requirements
        /// (digit + lowercase + uppercase + non-alphanumeric). Used only as a fallback when a
        /// seed-account password is not explicitly configured.
        /// </summary>
        private static string GenerateStrongPassword()
        {
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string digit = "23456789";
            const string special = "!@#$%&*?";
            var all = lower + upper + digit + special;

            using var rng = RandomNumberGenerator.Create();
            int NextIndex(string set)
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return Math.Abs(BitConverter.ToInt32(bytes, 0)) % set.Length;
            }

            var chars = new List<char>
            {
                lower[NextIndex(lower)],
                upper[NextIndex(upper)],
                digit[NextIndex(digit)],
                special[NextIndex(special)]
            };
            for (var i = 0; i < 12; i++)
                chars.Add(all[NextIndex(all)]);

            // Fisher-Yates shuffle so the required characters are not always at the front.
            for (var i = chars.Count - 1; i > 0; i--)
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var j = Math.Abs(BitConverter.ToInt32(bytes, 0)) % (i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars.ToArray());
        }

        private static async Task SeedDemoProgressAsync(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            var demo = await userManager.FindByEmailAsync("demo@nihongoacademy.com");
            if (demo == null) return;

            if (!await context.UserProfiles.AnyAsync(profile => profile.UserId == demo.Id))
            {
                context.UserProfiles.Add(new UserProfile
                {
                    UserId = demo.Id,
                    CurrentLevel = "A1",
                    LearningGoal = "Everyday conversation",
                    AgeGroup = "18-24"
                });
            }

            var categoryOrder = new[] { "Hiragana", "Katakana", "A1 Course", "Kanji", "A2 Course" };
            var lessons = await context.Lessons.Include(lesson => lesson.Category)
                .Where(lesson => lesson.IsPublished && categoryOrder.Contains(lesson.Category!.Name)).ToListAsync();
            var ordered = lessons.OrderBy(lesson => Array.IndexOf(categoryOrder, lesson.Category!.Name))
                .ThenBy(lesson => lesson.DisplayOrder).ThenBy(lesson => lesson.Id).ToList();
            var targetIndex = ordered.FindIndex(lesson => lesson.Title == "A1: Shop for an item");
            if (targetIndex < 0) return;

            var selectedIds = ordered.Take(targetIndex + 1).Select(lesson => lesson.Id).ToHashSet();
            var existing = await context.LessonProgress.Where(progress => progress.UserId == demo.Id)
                .ToDictionaryAsync(progress => progress.LessonId);
            var now = DateTime.UtcNow;
            foreach (var lessonId in selectedIds)
            {
                if (existing.TryGetValue(lessonId, out var progress) && !progress.IsAdminSimulated) continue;
                var item = existing.GetValueOrDefault(lessonId) ?? new LessonProgress { UserId = demo.Id, LessonId = lessonId };
                item.IsCompleted = true;
                item.CompletionPercentage = 100;
                item.StartedDate = now.AddDays(-14);
                item.FirstMasteryPassedDate = now.AddDays(-10);
                item.MasteryReviewDueDate = now.AddDays(-9);
                item.CompletedDate = now.AddDays(-8);
                item.StableMasteryDate = now.AddDays(-8);
                item.IsAdminSimulated = true;
                if (!existing.ContainsKey(lessonId)) context.LessonProgress.Add(item);
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            if (await context.Categories.AnyAsync()) return;

            var categories = new[]
            {
                new Category { Name = "Hiragana", Description = "Learn the basic Japanese phonetic script used for native words", IconClass = "bi bi-translate", DisplayOrder = 1 },
                new Category { Name = "Katakana", Description = "Master the script used for foreign loanwords and names", IconClass = "bi bi-type", DisplayOrder = 2 },
                new Category { Name = "Vocabulary", Description = "Build your Japanese word bank with JLPT-aligned vocabulary", IconClass = "bi bi-book", DisplayOrder = 3 },
                new Category { Name = "Grammar", Description = "Understand Japanese sentence structure and grammar patterns", IconClass = "bi bi-diagram-3", DisplayOrder = 4 },
                new Category { Name = "Kanji", Description = "Learn Chinese characters used in Japanese writing", IconClass = "bi bi-pen", DisplayOrder = 5 },
                new Category { Name = "Culture", Description = "Explore Japanese culture, etiquette, and traditions", IconClass = "bi bi-globe-asia-australia", DisplayOrder = 6 }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        private static async Task SeedLessonsAsync(ApplicationDbContext context)
        {
            var existingLessons = await context.Lessons.ToDictionaryAsync(l => l.Title);

            var hiraganaId = await context.Categories.Where(c => c.Name == "Hiragana").Select(c => c.Id).FirstAsync();
            var katakanaId = await context.Categories.Where(c => c.Name == "Katakana").Select(c => c.Id).FirstAsync();
            var vocabId = await context.Categories.Where(c => c.Name == "Vocabulary").Select(c => c.Id).FirstAsync();
            var grammarId = await context.Categories.Where(c => c.Name == "Grammar").Select(c => c.Id).FirstAsync();
            var cultureId = await context.Categories.Where(c => c.Name == "Culture").Select(c => c.Id).FirstAsync();

            var lessons = new[]
            {
                new Lesson
                {
                    CategoryId = hiraganaId,
                    Title = "Hiragana Foundations: A-row",
                    JapaneseTitle = "ひらがな基礎：あ行",
                    Description = "Build accurate recognition, listening and active recall for あ・い・う・え・お before moving on.",
                    Content = "<h2>Can-do goal</h2><p>By the end of this lesson, you can hear, read and type the five Japanese vowels <strong>あ・い・う・え・お</strong> without relying on answer choices.</p><h2>Listen before memorising</h2><p>Japanese has five stable vowel sounds. Keep each sound short and clean: <strong>あ a</strong> (open), <strong>い i</strong> (like the vowel in 'machine'), <strong>う u</strong> (lightly rounded, not a strong English 'oo'), <strong>え e</strong> (like 'e' in 'bed'), and <strong>お o</strong> (a pure 'o', without an English-style glide). Click each character and imitate it.</p><div class='kana-chart'><div class='kana-row-label'>あ行</div><div class='kana-cell vowel'><strong>あ</strong><span class='kana-romaji'>a</span></div><div class='kana-cell vowel'><strong>い</strong><span class='kana-romaji'>i</span></div><div class='kana-cell vowel'><strong>う</strong><span class='kana-romaji'>u</span></div><div class='kana-cell vowel'><strong>え</strong><span class='kana-romaji'>e</span></div><div class='kana-cell vowel'><strong>お</strong><span class='kana-romaji'>o</span></div></div><h2>Form and handwriting</h2><ul><li><strong>あ</strong>: three strokes; keep the loop open and balanced.</li><li><strong>い</strong>: two separate strokes; the second is shorter.</li><li><strong>う</strong>: two strokes; do not confuse it with the katakana ウ.</li><li><strong>え</strong>: two strokes; the lower stroke changes direction smoothly.</li><li><strong>お</strong>: three strokes; distinguish it from あ by the right-hand finishing stroke.</li></ul><p>Printed fonts vary slightly. Learn the underlying stroke structure, not one exact computer font.</p><h2>Contrast the shapes</h2><p>Compare <strong>あ／お</strong> carefully. Cover the romaji, say each sound, then reverse the task: hear a sound and write the kana.</p><h2>Mastery task</h2><p>Open <strong>Practice This Lesson</strong>. Complete recognition, listening and recall. This lesson is mastered only after at least five attempts in every mode with 80% accuracy or better.</p><h2>What comes next</h2><p>Continue to the か行 only after you can retrieve all five vowels without looking at the chart. Review them again tomorrow to confirm long-term memory.</p>",
                    DifficultyLevel = 1,
                    EstimatedMinutes = 20,
                    DisplayOrder = 0
                },
                new Lesson
                {
                    CategoryId = hiraganaId,
                    Title = "Introduction to Hiragana",
                    JapaneseTitle = "ひらがな入門",
                    Description = "Learn all 46 basic hiragana characters with the complete gojuon chart, the foundation of Japanese writing.",
                    Content = "<h3>What is Hiragana?</h3><p>Hiragana (ひらがな) is the foundation of Japanese reading and writing. It represents the basic sounds of Japanese and is essential for learning vocabulary, grammar, and sentence structure. They follow a logical system called the <em>gojūon</em> (五十音, 'fifty sounds') chart: one row per consonant, and each consonant combines with the five vowels.</p><h3>Step 1 — The Five Vowels (あいうえお)</h3><p>Everything starts with these five sounds. Learn them first — every other character is simply a consonant plus one of these vowels. Click each character to hear its pronunciation.</p><div class='kana-chart'><div class='kana-row-label'>あ行</div><div class='kana-cell vowel'>あ<span class='kana-romaji'>a</span></div><div class='kana-cell vowel'>い<span class='kana-romaji'>i</span></div><div class='kana-cell vowel'>う<span class='kana-romaji'>u</span></div><div class='kana-cell vowel'>え<span class='kana-romaji'>e</span></div><div class='kana-cell vowel'>お<span class='kana-romaji'>o</span></div></div><h3>Step 2 — The Complete 46-Character Chart</h3><p>Here is everything. Click any character to hear its pronunciation. Work through it row by row and say each character out loud — exactly how Japanese schoolchildren learn it.</p><div class='kana-chart'><div class='kana-row-label'>あ行</div><div class='kana-cell'>あ<span class='kana-romaji'>a</span></div><div class='kana-cell'>い<span class='kana-romaji'>i</span></div><div class='kana-cell'>う<span class='kana-romaji'>u</span></div><div class='kana-cell'>え<span class='kana-romaji'>e</span></div><div class='kana-cell'>お<span class='kana-romaji'>o</span></div><div class='kana-row-label'>か行</div><div class='kana-cell'>か<span class='kana-romaji'>ka</span></div><div class='kana-cell'>き<span class='kana-romaji'>ki</span></div><div class='kana-cell'>く<span class='kana-romaji'>ku</span></div><div class='kana-cell'>け<span class='kana-romaji'>ke</span></div><div class='kana-cell'>こ<span class='kana-romaji'>ko</span></div><div class='kana-row-label'>さ行</div><div class='kana-cell'>さ<span class='kana-romaji'>sa</span></div><div class='kana-cell'>し<span class='kana-romaji'>shi</span></div><div class='kana-cell'>す<span class='kana-romaji'>su</span></div><div class='kana-cell'>せ<span class='kana-romaji'>se</span></div><div class='kana-cell'>そ<span class='kana-romaji'>so</span></div><div class='kana-row-label'>た行</div><div class='kana-cell'>た<span class='kana-romaji'>ta</span></div><div class='kana-cell'>ち<span class='kana-romaji'>chi</span></div><div class='kana-cell'>つ<span class='kana-romaji'>tsu</span></div><div class='kana-cell'>て<span class='kana-romaji'>te</span></div><div class='kana-cell'>と<span class='kana-romaji'>to</span></div><div class='kana-row-label'>な行</div><div class='kana-cell'>な<span class='kana-romaji'>na</span></div><div class='kana-cell'>に<span class='kana-romaji'>ni</span></div><div class='kana-cell'>ぬ<span class='kana-romaji'>nu</span></div><div class='kana-cell'>ね<span class='kana-romaji'>ne</span></div><div class='kana-cell'>の<span class='kana-romaji'>no</span></div><div class='kana-row-label'>は行</div><div class='kana-cell'>は<span class='kana-romaji'>ha</span></div><div class='kana-cell'>ひ<span class='kana-romaji'>hi</span></div><div class='kana-cell'>ふ<span class='kana-romaji'>fu</span></div><div class='kana-cell'>へ<span class='kana-romaji'>he</span></div><div class='kana-cell'>ほ<span class='kana-romaji'>ho</span></div><div class='kana-row-label'>ま行</div><div class='kana-cell'>ま<span class='kana-romaji'>ma</span></div><div class='kana-cell'>み<span class='kana-romaji'>mi</span></div><div class='kana-cell'>む<span class='kana-romaji'>mu</span></div><div class='kana-cell'>め<span class='kana-romaji'>me</span></div><div class='kana-cell'>も<span class='kana-romaji'>mo</span></div><div class='kana-row-label'>や行</div><div class='kana-cell'>や<span class='kana-romaji'>ya</span></div><div class='kana-cell'>ゆ<span class='kana-romaji'>yu</span></div><div class='kana-cell'>よ<span class='kana-romaji'>yo</span></div><div></div><div></div><div class='kana-row-label'>ら行</div><div class='kana-cell'>ら<span class='kana-romaji'>ra</span></div><div class='kana-cell'>り<span class='kana-romaji'>ri</span></div><div class='kana-cell'>る<span class='kana-romaji'>ru</span></div><div class='kana-cell'>れ<span class='kana-romaji'>re</span></div><div class='kana-cell'>ろ<span class='kana-romaji'>ro</span></div><div class='kana-row-label'>わ行</div><div class='kana-cell'>わ<span class='kana-romaji'>wa</span></div><div class='kana-cell'>を<span class='kana-romaji'>wo</span></div><div></div><div></div><div></div><div class='kana-row-label'>ん</div><div class='kana-cell'>ん<span class='kana-romaji'>n</span></div><div></div><div></div><div></div><div></div></div><h3>Special Readings to Remember</h3><p>Most hiragana sound exactly like they look, but four have irregular readings you must memorize:</p><ul><li><strong>し</strong> is read <em>shi</em> (not 'si')</li><li><strong>ち</strong> is read <em>chi</em> (not 'ti')</li><li><strong>つ</strong> is read <em>tsu</em> (not 'tu')</li><li><strong>ふ</strong> is read <em>fu</em> (not 'hu')</li><li><strong>を</strong> is written <em>wo</em>, but when used as a particle it is pronounced like <em>o</em></li></ul><h3>ん — The Character with No Vowel</h3><p>ん (n) is the only character that stands alone without a vowel. It is a nasal sound — like the 'n' at the end of ほん (hon, book) or the 'ng' in 'sing'.</p><h3>How to Memorize Them</h3><ul><li><strong>Row by row:</strong> chant あかさたなはまやらわ like a song — the chart order itself is a memory aid.</li><li><strong>Flashcards:</strong> click the <strong>Practice Flashcards</strong> button to drill every character until it sticks.</li><li><strong>Write them out:</strong> handwriting each character reinforces memory far better than just reading.</li><li><strong>Test yourself:</strong> finish with the Hiragana quiz to lock everything in.</li></ul><h3>What's Next?</h3><p>You now know all 46 basic hiragana! The next lesson shows how dakuten (が, ざ, だ, ば), handakuten (ぱ) and combination sounds (きゃ, しゅ, ちょ) build on these characters.</p>",
                    DifficultyLevel = 1,
                    EstimatedMinutes = 20,
                    DisplayOrder = 90,
                    IsPublished = false
                },
                new Lesson
                {
                    CategoryId = hiraganaId,
                    Title = "Hiragana: Dakuten and Combination Sounds",
                    JapaneseTitle = "濁音と組み合わせ音",
                    Description = "Master modified hiragana sounds including dakuten, handakuten, and yoon combinations.",
                    Content = "<h3>Dakuten (濁音)</h3><p>Adding two small dots (゛) to certain characters creates voiced sounds: か→が (ka→ga), さ→ざ (sa→za), た→だ (ta→da), は→ば (ha→ba).</p><h3>Handakuten (半濁音)</h3><p>Adding a small circle (゜) to h-series characters creates p-sounds: は→ぱ (ha→pa), ひ→ぴ (hi→pi), ふ→ぷ (fu→pu).</p><h3>Yoon (拗音) - Combination Sounds</h3><p>Combining a character with a small ya, yu, or yo creates contracted sounds: き+や→きゃ (kya), し+ゆ→しゅ (shu), ち+よ→ちょ (cho).</p>",
                    DifficultyLevel = 2,
                    EstimatedMinutes = 25,
                    DisplayOrder = 20
                },
                new Lesson
                {
                    CategoryId = katakanaId,
                    Title = "Introduction to Katakana",
                    JapaneseTitle = "カタカナ入門",
                    Description = "Learn all 46 katakana characters and how to read the foreign loanwords written with them.",
                    Content = "<h3>What is Katakana?</h3><p>Katakana (カタカナ) is used for foreign loanwords (gairaigo), scientific names, onomatopoeia and emphasis. If you see a word in katakana, it almost certainly came from another language. The good news: the <strong>46 basic katakana characters have the exact same sounds as hiragana</strong> — so if you already know hiragana, you only need to learn the new shapes.</p><h3>Step 1 — The Five Vowels (アイウエオ)</h3><p>Same five vowel sounds, new shapes. Memorize these first.</p><div class='kana-chart'><div class='kana-row-label'>ア行</div><div class='kana-cell vowel'>ア<span class='kana-romaji'>a</span></div><div class='kana-cell vowel'>イ<span class='kana-romaji'>i</span></div><div class='kana-cell vowel'>ウ<span class='kana-romaji'>u</span></div><div class='kana-cell vowel'>エ<span class='kana-romaji'>e</span></div><div class='kana-cell vowel'>オ<span class='kana-romaji'>o</span></div></div><h3>Step 2 — The Complete 46-Character Chart</h3><p>Same rows, same sounds, different shapes. Compare each katakana with its hiragana partner as you go.</p><div class='kana-chart'><div class='kana-row-label'>ア行</div><div class='kana-cell'>ア<span class='kana-romaji'>a</span></div><div class='kana-cell'>イ<span class='kana-romaji'>i</span></div><div class='kana-cell'>ウ<span class='kana-romaji'>u</span></div><div class='kana-cell'>エ<span class='kana-romaji'>e</span></div><div class='kana-cell'>オ<span class='kana-romaji'>o</span></div><div class='kana-row-label'>カ行</div><div class='kana-cell'>カ<span class='kana-romaji'>ka</span></div><div class='kana-cell'>キ<span class='kana-romaji'>ki</span></div><div class='kana-cell'>ク<span class='kana-romaji'>ku</span></div><div class='kana-cell'>ケ<span class='kana-romaji'>ke</span></div><div class='kana-cell'>コ<span class='kana-romaji'>ko</span></div><div class='kana-row-label'>サ行</div><div class='kana-cell'>サ<span class='kana-romaji'>sa</span></div><div class='kana-cell'>シ<span class='kana-romaji'>shi</span></div><div class='kana-cell'>ス<span class='kana-romaji'>su</span></div><div class='kana-cell'>セ<span class='kana-romaji'>se</span></div><div class='kana-cell'>ソ<span class='kana-romaji'>so</span></div><div class='kana-row-label'>タ行</div><div class='kana-cell'>タ<span class='kana-romaji'>ta</span></div><div class='kana-cell'>チ<span class='kana-romaji'>chi</span></div><div class='kana-cell'>ツ<span class='kana-romaji'>tsu</span></div><div class='kana-cell'>テ<span class='kana-romaji'>te</span></div><div class='kana-cell'>ト<span class='kana-romaji'>to</span></div><div class='kana-row-label'>ナ行</div><div class='kana-cell'>ナ<span class='kana-romaji'>na</span></div><div class='kana-cell'>ニ<span class='kana-romaji'>ni</span></div><div class='kana-cell'>ヌ<span class='kana-romaji'>nu</span></div><div class='kana-cell'>ネ<span class='kana-romaji'>ne</span></div><div class='kana-cell'>ノ<span class='kana-romaji'>no</span></div><div class='kana-row-label'>ハ行</div><div class='kana-cell'>ハ<span class='kana-romaji'>ha</span></div><div class='kana-cell'>ヒ<span class='kana-romaji'>hi</span></div><div class='kana-cell'>フ<span class='kana-romaji'>fu</span></div><div class='kana-cell'>ヘ<span class='kana-romaji'>he</span></div><div class='kana-cell'>ホ<span class='kana-romaji'>ho</span></div><div class='kana-row-label'>マ行</div><div class='kana-cell'>マ<span class='kana-romaji'>ma</span></div><div class='kana-cell'>ミ<span class='kana-romaji'>mi</span></div><div class='kana-cell'>ム<span class='kana-romaji'>mu</span></div><div class='kana-cell'>メ<span class='kana-romaji'>me</span></div><div class='kana-cell'>モ<span class='kana-romaji'>mo</span></div><div class='kana-row-label'>ヤ行</div><div class='kana-cell'>ヤ<span class='kana-romaji'>ya</span></div><div class='kana-cell'>ユ<span class='kana-romaji'>yu</span></div><div class='kana-cell'>ヨ<span class='kana-romaji'>yo</span></div><div></div><div></div><div class='kana-row-label'>ラ行</div><div class='kana-cell'>ラ<span class='kana-romaji'>ra</span></div><div class='kana-cell'>リ<span class='kana-romaji'>ri</span></div><div class='kana-cell'>ル<span class='kana-romaji'>ru</span></div><div class='kana-cell'>レ<span class='kana-romaji'>re</span></div><div class='kana-cell'>ロ<span class='kana-romaji'>ro</span></div><div class='kana-row-label'>ワ行</div><div class='kana-cell'>ワ<span class='kana-romaji'>wa</span></div><div class='kana-cell'>ヲ<span class='kana-romaji'>wo</span></div><div></div><div></div><div></div><div class='kana-row-label'>ン</div><div class='kana-cell'>ン<span class='kana-romaji'>n</span></div><div></div><div></div><div></div><div></div></div><h3>Watch Out for the Look-Alikes</h3><p>Katakana has four pairs that trip everyone up. Train your eye on the stroke direction:</p><ul><li><strong>シ (shi)</strong> vs <strong>ツ (tsu)</strong> — shi's dashes stack vertically, tsu's dashes sit horizontally</li><li><strong>ソ (so)</strong> vs <strong>ン (n)</strong> — so's dash is on the left and slants down, n's dash is higher and slants up</li><li><strong>ウ (u)</strong> vs <strong>ワ (wa)</strong> — u has a tiny hook on top, wa does not</li></ul><h3>Common Loanwords You Already Know</h3><p>コーヒー (kōhī, coffee), テレビ (terebi, TV), パン (pan, bread), インターネット (intānetto, internet), コンピューター (konpyūtā, computer), レストラン (resutoran, restaurant).</p><h3>How to Memorize Them</h3><ul><li><strong>Pair with hiragana:</strong> drill ア next to あ, カ next to か — you already know the sounds.</li><li><strong>Read real words:</strong> katakana is everywhere in menus and product names; try decoding テレビ, パソコン, カメラ.</li><li><strong>Flashcards:</strong> use the Practice Flashcards button to drill the shapes.</li></ul>",
                    DifficultyLevel = 2,
                    EstimatedMinutes = 20,
                    DisplayOrder = 1
                },
                new Lesson
                {
                    CategoryId = vocabId,
                    Title = "Essential Greetings and Expressions",
                    JapaneseTitle = "基本的な挨拶と表現",
                    Description = "Learn the most common Japanese greetings and everyday expressions.",
                    Content = "<h3>Daily Greetings</h3><p>おはようございます (Ohayou gozaimasu) - Good morning<br/>こんにちは (Konnichiwa) - Good afternoon<br/>こんばんは (Konbanwa) - Good evening<br/>おやすみなさい (Oyasuminasai) - Good night</p><h3>Polite Expressions</h3><p>ありがとうございます (Arigatou gozaimasu) - Thank you<br/>すみません (Sumimasen) - Excuse me / I'm sorry<br/>どういたしまして (Dou itashimashite) - You're welcome<br/>はじめまして (Hajimemashite) - Nice to meet you</p><h3>Useful Phrases</h3><p>わかりません (Wakarimasen) - I don't understand<br/>もういちどおねがいします (Mou ichido onegai shimasu) - Please say it again<br/>えいごをはなせますか (Eigo o hanasemasu ka) - Can you speak English?</p>",
                    DifficultyLevel = 1,
                    EstimatedMinutes = 15,
                    DisplayOrder = 1
                },
                new Lesson
                {
                    CategoryId = vocabId,
                    Title = "Numbers and Counting",
                    JapaneseTitle = "数と数え方",
                    Description = "Learn Japanese numbers from 1 to 100 and basic counting systems.",
                    Content = "<h3>Numbers 1-10</h3><p>1: 一 (ichi), 2: 二 (ni), 3: 三 (san), 4: 四 (yon/shi), 5: 五 (go), 6: 六 (roku), 7: 七 (nana/shichi), 8: 八 (hachi), 9: 九 (kyuu/ku), 10: 十 (juu)</p><h3>Counting to 100</h3><p>11 is 十一 (juu ichi) = ten-one. 20 is 二十 (ni juu) = two-ten. 100 is 百 (hyaku).</p><h3>Counter Words</h3><p>Japanese uses different counter words depending on what you're counting. 人 (nin) for people, 枚 (mai) for flat objects, 冊 (satsu) for books, 匹 (hiki) for small animals.</p>",
                    DifficultyLevel = 2,
                    EstimatedMinutes = 20,
                    DisplayOrder = 2
                },
                new Lesson
                {
                    CategoryId = grammarId,
                    Title = "Desu/Masu Form: Basic Sentence Structure",
                    JapaneseTitle = "です・ます形：基本文法",
                    Description = "Learn the polite form of Japanese verbs and the copula desu.",
                    Content = "<h3>The Copula: です (desu)</h3><p>Desu is the polite form of 'to be'. It equates the subject with a noun or adjective.<br/>わたしはがくせいです (Watashi wa gakusei desu) - I am a student.<br/>これはほんです (Kore wa hon desu) - This is a book.</p><h3>The Masu Form</h3><p>The masu form is the polite form of verbs. It's used in formal situations.<br/>たべます (tabemasu) - to eat<br/>のみます (nomimasu) - to drink<br/>いきます (ikimasu) - to go</p><h3>Negative Form</h3><p>To make desu negative: じゃないです (ja nai desu) or ではありません (dewa arimasen).<br/>To make masu negative: ません (masen). たべません (tabemasen) - do not eat.</p>",
                    DifficultyLevel = 2,
                    EstimatedMinutes = 30,
                    DisplayOrder = 1
                },
                new Lesson
                {
                    CategoryId = grammarId,
                    Title = "Particles: wa, ga, wo, ni, de",
                    JapaneseTitle = "助詞：は、が、を、に、で",
                    Description = "Master the five most essential Japanese grammar particles.",
                    Content = "<h3>は (wa) - Topic Marker</h3><p>Marks the topic of the sentence. わたしはがくせいです (Watashi wa gakusei desu) - As for me, I am a student.</p><h3>が (ga) - Subject Marker</h3><p>Marks the grammatical subject, often used for new information. だれがきましたか (Dare ga kimashita ka) - Who came?</p><h3>を (wo) - Object Marker</h3><p>Marks the direct object of a verb. パンをたべます (Pan wo tabemasu) - I eat bread.</p><h3>に (ni) - Direction/Time/Location</h3><p>Indicates destination, time, or location of existence. がっこうにいきます (Gakkou ni ikimasu) - I go to school.</p><h3>で (de) - Means/Location of Action</h3><p>Indicates where an action takes place or the means. としょかんでべんきょうします (Toshokan de benkyou shimasu) - I study at the library.</p>",
                    DifficultyLevel = 3,
                    EstimatedMinutes = 30,
                    DisplayOrder = 2
                },
                new Lesson
                {
                    CategoryId = cultureId,
                    Title = "Japanese Etiquette: Bowing and Politeness",
                    JapaneseTitle = "日本の礼儀：お辞儀と丁寧さ",
                    Description = "Understand the cultural significance of bowing and levels of politeness in Japan.",
                    Content = "<h3>The Art of Bowing (お辞儀)</h3><p>Bowing (ojigi) is the most common greeting in Japan. There are three main types:</p><ul><li><strong>Eshaku (会釈)</strong> - 15 degree bow, for casual greetings</li><li><strong>Keirei (敬礼)</strong> - 30 degree bow, for customers and superiors</li><li><strong>Saikeirei (最敬礼)</strong> - 45 degree bow, for deep gratitude or apology</li></ul><h3>Politeness Levels</h3><p>Japanese has three main politeness levels: plain form (casual), polite form (desu/masu), and keigo (honorific). Using the appropriate level shows respect and social awareness.</p><h3>Business Cards (名刺)</h3><p>Exchanging business cards (meishi) is a formal ritual. Present and receive with both hands, read the card carefully, and never write on it or put it in your pocket.</p>",
                    DifficultyLevel = 1,
                    EstimatedMinutes = 15,
                    DisplayOrder = 1
                }
            };

            foreach (var lesson in lessons)
            {
                if (existingLessons.TryGetValue(lesson.Title, out var dbLesson))
                {
                    dbLesson.CategoryId = lesson.CategoryId;
                    dbLesson.JapaneseTitle = lesson.JapaneseTitle;
                    dbLesson.Description = lesson.Description;
                    dbLesson.Content = lesson.Content;
                    dbLesson.DifficultyLevel = lesson.DifficultyLevel;
                    dbLesson.EstimatedMinutes = lesson.EstimatedMinutes;
                    dbLesson.DisplayOrder = lesson.DisplayOrder;
                    dbLesson.IsPublished = lesson.IsPublished;
                }
                else
                {
                    context.Lessons.Add(lesson);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedLessonVocabularyAsync(ApplicationDbContext context)
        {
            var lesson = await context.Lessons.FirstOrDefaultAsync(l => l.Title == "Hiragana Foundations: A-row");
            if (lesson == null) return;

            string[] vowelOrder = { "あ", "い", "う", "え", "お" };
            var vowels = await context.Vocabulary
                .Where(v => v.Category!.Name == "Hiragana" && vowelOrder.Contains(v.Japanese))
                .ToListAsync();
            var items = vowelOrder
                .Select(kana => vowels.Single(v => v.Japanese == kana))
                .ToList();
            await SyncLessonItems(context, lesson, items);
        }

        private static async Task SeedHiraganaCurriculumAsync(ApplicationDbContext context)
        {
            var hiraganaId = await context.Categories.Where(c => c.Name == "Hiragana").Select(c => c.Id).FirstAsync();
            var groups = new[]
            {
                new { Title = "Hiragana Foundations: K-row", JapaneseTitle = "ひらがな基礎：か行", Kana = new[] { "か", "き", "く", "け", "こ" }, Romaji = new[] { "ka", "ki", "ku", "ke", "ko" }, Order = 1, Note = "Keep the consonant light. き and さ will change shape slightly in some fonts and handwriting." },
                new { Title = "Hiragana Foundations: S-row", JapaneseTitle = "ひらがな基礎：さ行", Kana = new[] { "さ", "し", "す", "せ", "そ" }, Romaji = new[] { "sa", "shi", "su", "se", "so" }, Order = 2, Note = "し is shi, not si. The vowel in す is often lightly devoiced in natural speech." },
                new { Title = "Hiragana Foundations: T-row", JapaneseTitle = "ひらがな基礎：た行", Kana = new[] { "た", "ち", "つ", "て", "と" }, Romaji = new[] { "ta", "chi", "tsu", "te", "to" }, Order = 3, Note = "ち is chi and つ is tsu. Train them by listening, not by guessing from romaji." },
                new { Title = "Hiragana Foundations: N-row", JapaneseTitle = "ひらがな基礎：な行", Kana = new[] { "な", "に", "ぬ", "ね", "の" }, Romaji = new[] { "na", "ni", "nu", "ne", "no" }, Order = 4, Note = "Compare the loops and finishing strokes in ぬ, ね and め; shape contrast matters." },
                new { Title = "Hiragana Foundations: H-row", JapaneseTitle = "ひらがな基礎：は行", Kana = new[] { "は", "ひ", "ふ", "へ", "ほ" }, Romaji = new[] { "ha", "hi", "fu", "he", "ho" }, Order = 5, Note = "ふ uses a soft Japanese f sound. は is pronounced wa only when it functions as the topic particle." },
                new { Title = "Hiragana Foundations: M/Y-row", JapaneseTitle = "ひらがな基礎：ま行・や行", Kana = new[] { "ま", "み", "む", "め", "も", "や", "ゆ", "よ" }, Romaji = new[] { "ma", "mi", "mu", "me", "mo", "ya", "yu", "yo" }, Order = 6, Note = "The modern y-row contains only や, ゆ and よ. Notice the shape contrast between め and ぬ." },
                new { Title = "Hiragana Foundations: R/W-row and N", JapaneseTitle = "ひらがな基礎：ら行・わ行・ん", Kana = new[] { "ら", "り", "る", "れ", "ろ", "わ", "を", "ん" }, Romaji = new[] { "ra", "ri", "ru", "re", "ro", "wa", "wo", "n" }, Order = 7, Note = "The Japanese r is a light tap, not an English r. を is normally pronounced o as a particle; ん changes subtly before different sounds." }
            };

            foreach (var group in groups)
            {
                var lesson = await context.Lessons.FirstOrDefaultAsync(l => l.Title == group.Title);
                if (lesson == null)
                {
                    lesson = new Lesson
                    {
                        Title = group.Title,
                    };
                    context.Lessons.Add(lesson);
                }
                lesson.CategoryId = hiraganaId;
                lesson.JapaneseTitle = group.JapaneseTitle;
                lesson.DifficultyLevel = 1;
                lesson.EstimatedMinutes = group.Kana.Length <= 5 ? 20 : 25;
                lesson.DisplayOrder = group.Order;
                lesson.IsPublished = true;
                lesson.Description = $"Learn to hear, recognise and actively recall {string.Join("・", group.Kana)}.";
                lesson.Content = BuildKanaLessonContent(group.JapaneseTitle, group.Kana, group.Romaji, group.Note);
                await context.SaveChangesAsync();

                var lessonItems = new List<Vocabulary>();
                for (var i = 0; i < group.Kana.Length; i++)
                {
                    var kana = group.Kana[i];
                    var vocab = await context.Vocabulary.FirstOrDefaultAsync(v => v.CategoryId == hiraganaId && v.Japanese == kana);
                    if (vocab == null)
                    {
                        vocab = new Vocabulary
                        {
                            CategoryId = hiraganaId, Japanese = kana, Romaji = group.Romaji[i],
                            English = $"hiragana {group.Romaji[i]}", PartOfSpeech = "Hiragana",
                            DisplayOrder = group.Order * 10 + i, JlptLevel = 5
                        };
                        context.Vocabulary.Add(vocab);
                        await context.SaveChangesAsync();
                    }
                    vocab.Romaji = group.Romaji[i];
                    vocab.English = $"hiragana {group.Romaji[i]}";
                    vocab.PartOfSpeech = "Hiragana";
                    vocab.DisplayOrder = group.Order * 10 + i;
                    lessonItems.Add(vocab);
                }
                await SyncLessonItems(context, lesson, lessonItems);
            }
        }

        private static string BuildKanaLessonContent(string title, string[] kana, string[] romaji, string note)
        {
            var cells = string.Join("", kana.Select((k, i) => $"<div class='kana-cell vowel'><strong>{k}</strong><span class='kana-romaji'>{romaji[i]}</span></div>"));
            var handwriting = new Dictionary<string, string>
            {
                ["か"] = "keep the right vertical stroke separate from the left curve", ["き"] = "printed and handwritten forms may connect differently; preserve four clear stroke movements",
                ["く"] = "one angled stroke; do not round it into し", ["け"] = "keep the left pair separate from the long right stroke", ["こ"] = "two horizontal strokes with open space between them",
                ["さ"] = "the printed lower loop is often open in handwriting", ["し"] = "one rising curve from upper left to lower right", ["す"] = "centre the loop and let the final curve descend",
                ["せ"] = "the short vertical crosses the horizontal before the final stroke", ["そ"] = "learn the standard two-part structure despite font variation",
                ["た"] = "distinguish it from な by the left cross and right shape", ["ち"] = "the upper bar is separate from the long curved stroke", ["つ"] = "one broad curve; keep it wider than し",
                ["て"] = "one stroke that turns smoothly", ["と"] = "a short mark followed by a longer curved stroke",
                ["な"] = "four balanced strokes; keep the final loop compact", ["に"] = "left vertical plus two separate right strokes", ["ぬ"] = "retain the loop and finishing tail; compare with め",
                ["ね"] = "the right side loops and finishes outward", ["の"] = "one continuous rounded stroke",
                ["は"] = "left vertical plus a right section with a small loop", ["ひ"] = "one continuous symmetrical curve", ["ふ"] = "four separated marks; do not join them into one symbol",
                ["へ"] = "one peaked stroke with unequal slopes", ["ほ"] = "distinguish it from は by the extra right horizontal stroke",
                ["ま"] = "three horizontal/vertical movements followed by a loop", ["み"] = "keep the central curve open and the final mark distinct", ["む"] = "the loop sits low with a separate finishing mark",
                ["め"] = "the crossing curve has no final loop-tail like ぬ", ["も"] = "two short horizontals cross the long curved stroke",
                ["や"] = "three separated strokes with an open centre", ["ゆ"] = "the inner vertical sits inside the broad outer curve", ["よ"] = "two short upper strokes and a lower loop",
                ["ら"] = "a short top stroke precedes the lower curve", ["り"] = "two strokes; the right stroke descends farther", ["る"] = "finish with a small loop, unlike ろ",
                ["れ"] = "the right stroke travels outward without closing a loop", ["ろ"] = "one open curve; contrast the ending with る",
                ["わ"] = "the right curve stays open and flowing", ["を"] = "three strokes; it is mainly used as the object particle", ["ん"] = "one continuous nasal-sign curve; contrast it with そ"
            };
            var katakanaGuidance = new Dictionary<string, string>
            {
                ["ア"]="three strokes; the final diagonal sweeps down left", ["イ"]="two separate diagonal strokes", ["ウ"]="three strokes; retain the short top mark", ["エ"]="three strokes; keep the middle vertical centred", ["オ"]="three strokes; the last stroke sweeps left",
                ["カ"]="two strokes; keep the right stroke separate", ["キ"]="three strokes; two horizontals cross the long diagonal", ["ク"]="two strokes forming an open angle", ["ケ"]="three strokes; the extra short left stroke distinguishes it from ク", ["コ"]="two parallel cornered strokes",
                ["サ"]="three strokes; two upper strokes precede the long diagonal", ["シ"]="three strokes; two short marks descend vertically before the rising sweep", ["ス"]="two strokes; the second bends and finishes outward", ["セ"]="two crossing strokes with an angular turn", ["ソ"]="two strokes; the short mark and long stroke descend from above",
                ["タ"]="three strokes; build the top before the inner diagonal", ["チ"]="three strokes; two upper marks precede the curved lower stroke", ["ツ"]="three strokes; two short marks spread horizontally before the downward sweep", ["テ"]="three strokes; the lower stroke begins below two horizontals", ["ト"]="two strokes; place the short mark beside the vertical",
                ["ナ"]="two crossing strokes", ["ニ"]="two horizontal strokes", ["ヌ"]="two strokes; the final stroke crosses the curved first stroke", ["ネ"]="four strokes with a central crossing and finishing sweep", ["ノ"]="one clean diagonal stroke",
                ["ハ"]="two separated strokes that open downward", ["ヒ"]="two strokes; the long stroke bends beneath the short one", ["フ"]="one angular sweeping stroke", ["ヘ"]="one peaked stroke with unequal slopes", ["ホ"]="four strokes; centre the vertical through the two horizontals",
                ["マ"]="two strokes; the second diagonal cuts through the angled first stroke", ["ミ"]="three parallel strokes", ["ム"]="two strokes; the second creates the lower angled form", ["メ"]="two crossing diagonals", ["モ"]="three strokes; the long final stroke crosses two horizontals",
                ["ヤ"]="two strokes with a short upper mark", ["ユ"]="two strokes forming an open rectangular shape", ["ヨ"]="three strokes forming two interior spaces",
                ["ラ"]="two strokes; a short top line sits above the bent lower stroke", ["リ"]="two vertical strokes; the right descends farther", ["ル"]="two strokes with a hooked right finish", ["レ"]="one descending stroke that turns upward", ["ロ"]="three strokes forming a clean square",
                ["ワ"]="two strokes; unlike ウ, it has no separate short top mark", ["ヲ"]="three strokes with two horizontals and a final curve", ["ン"]="two strokes; the short mark begins low and the long stroke rises"
            };
            foreach (var (character, guidance) in katakanaGuidance) handwriting[character] = guidance;
            var formItems = string.Join("", kana.Select(k => $"<li><strong>{k}</strong>: {handwriting.GetValueOrDefault(k, "preserve the standard stroke direction and overall balance")}.</li>"));
            return $"<h2>Can-do goal</h2><p>You can hear, read and type <strong>{string.Join("・", kana)}</strong> without answer choices.</p>" +
                $"<h2>{title}</h2><div class='kana-chart'>{cells}</div><p>Click each character, listen, and imitate the sound. Then cover the romaji and retrieve it from memory.</p>" +
                $"<h2>Pronunciation focus</h2><p>{note}</p>" +
                $"<h2>Form and handwriting</h2><ul>{formItems}</ul><p>Write each character slowly three times, then once from memory. Compare structure and stroke direction rather than decorative font thickness.</p>" +
                "<h2>Mixed retrieval</h2><p>The practice screen mixes roughly 70% current targets with 30% characters from mastered earlier groups. Practise both directions: character to sound and sound to character.</p>" +
                "<h2>Mastery task</h2><p>Complete recognition, listening and active recall practice. Each mode requires at least five attempts and 80% accuracy. Review the group again tomorrow before treating it as stable knowledge.</p>";
        }

        private static async Task SeedAdvancedHiraganaCurriculumAsync(ApplicationDbContext context)
        {
            var hiraganaId = await context.Categories.Where(c => c.Name == "Hiragana").Select(c => c.Id).FirstAsync();
            var modules = new[]
            {
                new {
                    Title = "Hiragana: Voiced and P-sounds", JapaneseTitle = "濁音・半濁音",
                    Items = new[] { "が", "ぎ", "ぐ", "げ", "ご", "ざ", "じ", "ず", "ぜ", "ぞ", "だ", "ぢ", "づ", "で", "ど", "ば", "び", "ぶ", "べ", "ぼ", "ぱ", "ぴ", "ぷ", "ぺ", "ぽ" },
                    Readings = new[] { "ga", "gi", "gu", "ge", "go", "za", "ji", "zu", "ze", "zo", "da", "ji", "zu", "de", "do", "ba", "bi", "bu", "be", "bo", "pa", "pi", "pu", "pe", "po" },
                    Order = 8,
                    Intro = "Dakuten ゛changes k→g, s→z, t→d and h→b. Handakuten ゜changes the h-row to p. ぢ and づ are uncommon and are usually pronounced like じ and ず in standard Japanese.",
                    Contrast = "Compare か／が, さ／ざ, た／だ, は／ば／ぱ. The marks are part of the character, so missing them can change the word."
                },
                new {
                    Title = "Hiragana: Contracted sounds", JapaneseTitle = "拗音",
                    Items = new[] { "きゃ", "きゅ", "きょ", "しゃ", "しゅ", "しょ", "ちゃ", "ちゅ", "ちょ", "にゃ", "にゅ", "にょ", "ひゃ", "ひゅ", "ひょ", "みゃ", "みゅ", "みょ", "りゃ", "りゅ", "りょ", "ぎゃ", "ぎゅ", "ぎょ", "じゃ", "じゅ", "じょ", "びゃ", "びゅ", "びょ", "ぴゃ", "ぴゅ", "ぴょ" },
                    Readings = new[] { "kya", "kyu", "kyo", "sha", "shu", "sho", "cha", "chu", "cho", "nya", "nyu", "nyo", "hya", "hyu", "hyo", "mya", "myu", "myo", "rya", "ryu", "ryo", "gya", "gyu", "gyo", "ja", "ju", "jo", "bya", "byu", "byo", "pya", "pyu", "pyo" },
                    Order = 9,
                    Intro = "A small ゃ, ゅ or ょ combines with an i-row kana to form one contracted beat. きゃ is kya, not ki-ya.",
                    Contrast = "Character size changes rhythm and meaning. Compare きや (ki-ya, two beats) with きゃ (kya, one beat)."
                },
                new {
                    Title = "Hiragana: Small tsu and consonant doubling", JapaneseTitle = "促音・小さいっ",
                    Items = new[] { "きて", "きって", "さか", "さっか", "かこ", "かっこ", "もっと", "ちょっと" },
                    Readings = new[] { "kite", "kitte", "saka", "sakka", "kako", "kakko", "motto", "chotto" },
                    Order = 10,
                    Intro = "Small っ creates a one-beat pause before the next consonant. Do not pronounce it as tsu. Hold the closure briefly: きって = ki-(pause)-te.",
                    Contrast = "The pause can distinguish words: きて (come/wear and...) versus きって (stamp/cut and...). Listen for timing, not volume."
                },
                new {
                    Title = "Hiragana: Long vowels", JapaneseTitle = "長音",
                    Items = new[] { "おばさん", "おばあさん", "おじさん", "おじいさん", "ここ", "こうこう", "ゆき", "ゆうき", "え", "ええ" },
                    Readings = new[] { "obasan", "obaasan", "ojisan", "ojiisan", "koko", "koukou", "yuki", "yuuki", "e", "ee" },
                    Order = 11,
                    Intro = "A long vowel lasts two beats. In hiragana, long a/i/u often add the same vowel; long e commonly uses い and long o commonly uses う.",
                    Contrast = "Length can change meaning: おばさん is aunt or middle-aged woman; おばあさん is grandmother or elderly woman. Count beats rather than stretching randomly."
                },
                new {
                    Title = "Hiragana: Integrated reading", JapaneseTitle = "ひらがな総合読み",
                    Items = new[] { "あさ", "いえ", "うえ", "えき", "おと", "かさ", "すし", "ねこ", "いぬ", "みず", "やま", "くるま", "さかな", "たまご", "ひらがな" },
                    Readings = new[] { "asa", "ie", "ue", "eki", "oto", "kasa", "sushi", "neko", "inu", "mizu", "yama", "kuruma", "sakana", "tamago", "hiragana" },
                    Order = 12,
                    Intro = "Now move from isolated symbols to whole-word decoding. Read in beats, keep vowels stable and avoid spelling through English letter names.",
                    Contrast = "First read for accuracy, then reread smoothly. Hide romaji as soon as possible and use audio only after your own attempt."
                }
            };

            foreach (var module in modules)
            {
                var lesson = await context.Lessons.FirstOrDefaultAsync(l => l.Title == module.Title);
                if (lesson == null)
                {
                    lesson = new Lesson { Title = module.Title };
                    context.Lessons.Add(lesson);
                }
                lesson.CategoryId = hiraganaId;
                lesson.JapaneseTitle = module.JapaneseTitle;
                lesson.DifficultyLevel = 2;
                lesson.EstimatedMinutes = 25;
                lesson.DisplayOrder = module.Order;
                lesson.IsPublished = true;
                lesson.Description = $"Train accurate perception and recall of {module.JapaneseTitle}.";
                lesson.Content = BuildAdvancedKanaContent(module.JapaneseTitle, module.Items, module.Readings, module.Intro, module.Contrast);
                await context.SaveChangesAsync();

                var lessonItems = new List<Vocabulary>();
                var meanings = new Dictionary<string, string>
                {
                    ["きて"] = "come; wear (te-form)", ["きって"] = "postage stamp; cut (te-form)", ["さか"] = "slope", ["さっか"] = "author",
                    ["かこ"] = "past", ["かっこ"] = "appearance; parentheses", ["もっと"] = "more", ["ちょっと"] = "a little",
                    ["おばさん"] = "aunt; middle-aged woman", ["おばあさん"] = "grandmother; elderly woman", ["おじさん"] = "uncle; middle-aged man", ["おじいさん"] = "grandfather; elderly man",
                    ["ここ"] = "here", ["こうこう"] = "high school", ["ゆき"] = "snow", ["ゆうき"] = "courage", ["え"] = "picture", ["ええ"] = "yes",
                    ["あさ"] = "morning", ["いえ"] = "house", ["うえ"] = "above", ["えき"] = "station", ["おと"] = "sound", ["かさ"] = "umbrella",
                    ["すし"] = "sushi", ["ねこ"] = "cat", ["いぬ"] = "dog", ["みず"] = "water", ["やま"] = "mountain", ["くるま"] = "car",
                    ["さかな"] = "fish", ["たまご"] = "egg", ["ひらがな"] = "hiragana"
                };
                for (var i = 0; i < module.Items.Length; i++)
                {
                    var text = module.Items[i];
                    var item = await context.Vocabulary.FirstOrDefaultAsync(v => v.CategoryId == hiraganaId && v.Japanese == text);
                    if (item == null)
                    {
                        item = new Vocabulary { CategoryId = hiraganaId, Japanese = text, Romaji = module.Readings[i], English = meanings.GetValueOrDefault(text, $"hiragana pattern: {module.Readings[i]}"), PartOfSpeech = "Hiragana pattern", DisplayOrder = module.Order * 100 + i, JlptLevel = 5 };
                        context.Vocabulary.Add(item);
                        await context.SaveChangesAsync();
                    }
                    // Basic single kana (e.g. え in the long-vowel module) already exist as foundation
                    // vocabulary; never overwrite their identity fields or ordering.
                    const string BasicKana = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん";
                    var reusedBasicCharacter = text.Length == 1 && BasicKana.Contains(text);
                    if (!reusedBasicCharacter)
                    {
                        item.Romaji = module.Readings[i];
                        item.English = meanings.GetValueOrDefault(text, item.English);
                        item.PartOfSpeech = "Hiragana pattern";
                        item.DisplayOrder = module.Order * 100 + i;
                    }
                    lessonItems.Add(item);
                }
                await SyncLessonItems(context, lesson, lessonItems);
            }

            // Replaced by the focused modules above.
            var legacy = await context.Lessons.FirstOrDefaultAsync(l => l.Title == "Hiragana: Dakuten and Combination Sounds");
            if (legacy != null) { legacy.IsPublished = false; await context.SaveChangesAsync(); }
            var legacyIntroduction = await context.Lessons.FirstOrDefaultAsync(l => l.Title == "Introduction to Hiragana");
            if (legacyIntroduction != null) { legacyIntroduction.IsPublished = false; await context.SaveChangesAsync(); }
        }

        private static string BuildAdvancedKanaContent(string title, string[] items, string[] readings, string intro, string contrast)
        {
            var cells = string.Join("", items.Select((item, i) => $"<div class='kana-cell'><strong>{item}</strong><span class='kana-romaji'>{readings[i]}</span></div>"));
            return $"<h2>Can-do goal</h2><p>You can hear, distinguish, read and type the target patterns in <strong>{title}</strong>.</p>" +
                $"<h2>How it works</h2><p>{intro}</p><div class='kana-chart'>{cells}</div>" +
                $"<h2>Meaningful contrast</h2><p>{contrast}</p>" +
                "<h2>Practice sequence</h2><ol><li>Listen without looking.</li><li>Imitate the timing aloud.</li><li>Read the displayed form.</li><li>Type it from sound or romaji without choices.</li><li>Mix it with earlier material.</li></ol>" +
                "<h2>Mastery task</h2><p>Complete recognition, listening and active recall with at least five attempts and 80% accuracy in each mode. Recheck the contrasts on a later day.</p>";
        }

        private static async Task SyncLessonItems(ApplicationDbContext context, Lesson lesson, IReadOnlyList<Vocabulary> items)
        {
            var desiredIds = items.Select(x => x.Id).ToHashSet();
            var existing = await context.LessonVocabulary.Where(x => x.LessonId == lesson.Id).ToListAsync();
            context.LessonVocabulary.RemoveRange(existing.Where(x => !desiredIds.Contains(x.VocabularyId)));

            for (var i = 0; i < items.Count; i++)
            {
                var mapping = existing.FirstOrDefault(x => x.VocabularyId == items[i].Id);
                if (mapping == null)
                    context.LessonVocabulary.Add(new LessonVocabulary { LessonId = lesson.Id, VocabularyId = items[i].Id, DisplayOrder = i });
                else
                    mapping.DisplayOrder = i;
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedKatakanaCurriculumAsync(ApplicationDbContext context)
        {
            var katakanaId = await context.Categories.Where(c => c.Name == "Katakana").Select(c => c.Id).FirstAsync();
            var groups = new[]
            {
                new { Title = "Katakana Foundations: A-row", Jp = "カタカナ基礎：ア行", Items = new[] { "ア", "イ", "ウ", "エ", "オ" }, Readings = new[] { "a", "i", "u", "e", "o" }, Order = 0, Note = "The sounds match hiragana exactly; only the shapes change. Compare ア／あ, イ／い and ウ／う." },
                new { Title = "Katakana Foundations: K-row", Jp = "カタカナ基礎：カ行", Items = new[] { "カ", "キ", "ク", "ケ", "コ" }, Readings = new[] { "ka", "ki", "ku", "ke", "ko" }, Order = 1, Note = "Notice the extra lower stroke in ケ compared with ク. Keep the angular printed forms clear." },
                new { Title = "Katakana Foundations: S-row", Jp = "カタカナ基礎：サ行", Items = new[] { "サ", "シ", "ス", "セ", "ソ" }, Readings = new[] { "sa", "shi", "su", "se", "so" }, Order = 2, Note = "シ has two short strokes arranged more vertically; ソ has one short stroke and a different finishing direction." },
                new { Title = "Katakana Foundations: T-row", Jp = "カタカナ基礎：タ行", Items = new[] { "タ", "チ", "ツ", "テ", "ト" }, Readings = new[] { "ta", "chi", "tsu", "te", "to" }, Order = 3, Note = "ツ has short strokes arranged more horizontally than シ. Use stroke direction, not only dot position, to distinguish them." },
                new { Title = "Katakana Foundations: N-row", Jp = "カタカナ基礎：ナ行", Items = new[] { "ナ", "ニ", "ヌ", "ネ", "ノ" }, Readings = new[] { "na", "ni", "nu", "ne", "no" }, Order = 4, Note = "Compare ヌ with ス and ネ. The internal crossing and final direction identify the character." },
                new { Title = "Katakana Foundations: H-row", Jp = "カタカナ基礎：ハ行", Items = new[] { "ハ", "ヒ", "フ", "ヘ", "ホ" }, Readings = new[] { "ha", "hi", "fu", "he", "ho" }, Order = 5, Note = "Katakana ヘ and hiragana へ are almost identical. フ, ウ and ワ need deliberate contrast practice." },
                new { Title = "Katakana Foundations: M/Y-row", Jp = "カタカナ基礎：マ行・ヤ行", Items = new[] { "マ", "ミ", "ム", "メ", "モ", "ヤ", "ユ", "ヨ" }, Readings = new[] { "ma", "mi", "mu", "me", "mo", "ya", "yu", "yo" }, Order = 6, Note = "Distinguish マ／ム and メ／ヌ by their intersections and finishing strokes." },
                new { Title = "Katakana Foundations: R/W-row and N", Jp = "カタカナ基礎：ラ行・ワ行・ン", Items = new[] { "ラ", "リ", "ル", "レ", "ロ", "ワ", "ヲ", "ン" }, Readings = new[] { "ra", "ri", "ru", "re", "ro", "wa", "wo", "n" }, Order = 7, Note = "ン rises from lower left toward upper right; ソ is written with a different stroke direction. ヲ is uncommon outside special stylistic uses." }
            };

            foreach (var group in groups)
                await UpsertKanaModule(context, katakanaId, group.Title, group.Jp, group.Items, group.Readings, group.Order, group.Note, false);

            var advanced = new[]
            {
                new { Title = "Katakana: Voiced and semi-voiced sounds", Jp = "濁音・半濁音", Items = new[] { "ガ", "ギ", "グ", "ゲ", "ゴ", "ザ", "ジ", "ズ", "ゼ", "ゾ", "ダ", "ヂ", "ヅ", "デ", "ド", "バ", "ビ", "ブ", "ベ", "ボ", "パ", "ピ", "プ", "ペ", "ポ" }, Readings = new[] { "ga", "gi", "gu", "ge", "go", "za", "ji", "zu", "ze", "zo", "da", "ji", "zu", "de", "do", "ba", "bi", "bu", "be", "bo", "pa", "pi", "pu", "pe", "po" }, Order = 8, Note = "Dakuten changes k→g, s→z, t→d and h→b; handakuten changes h→p. ヂ and ヅ are uncommon and usually sound like ジ and ズ in standard Japanese." },
                new { Title = "Katakana: Contracted sounds", Jp = "拗音", Items = new[] { "キャ", "キュ", "キョ", "シャ", "シュ", "ショ", "チャ", "チュ", "チョ", "ニャ", "ニュ", "ニョ", "ヒャ", "ヒュ", "ヒョ", "ミャ", "ミュ", "ミョ", "リャ", "リュ", "リョ", "ギャ", "ギュ", "ギョ", "ジャ", "ジュ", "ジョ", "ビャ", "ビュ", "ビョ", "ピャ", "ピュ", "ピョ" }, Readings = new[] { "kya", "kyu", "kyo", "sha", "shu", "sho", "cha", "chu", "cho", "nya", "nyu", "nyo", "hya", "hyu", "hyo", "mya", "myu", "myo", "rya", "ryu", "ryo", "gya", "gyu", "gyo", "ja", "ju", "jo", "bya", "byu", "byo", "pya", "pyu", "pyo" }, Order = 9, Note = "A small ャ・ュ・ョ combines with an i-row character into one beat. Character size matters: キヤ is ki-ya, while キャ is kya." },
                new { Title = "Katakana: Long vowels and small tsu", Jp = "長音符号・促音", Items = new[] { "コート", "コットン", "ビル", "ビール", "サッカー", "ベッド", "ケーキ", "カップ", "ノート", "メール" }, Readings = new[] { "kooto", "kotton", "biru", "biiru", "sakkaa", "beddo", "keeki", "kappu", "nooto", "meeru" }, Order = 10, Note = "The mark ー extends the previous vowel by one beat. Small ッ creates a silent beat before a doubled consonant. Both can change what word is understood." },
                new { Title = "Katakana: Extended foreign sounds", Jp = "外来音", Items = new[] { "ティ", "ディ", "ファ", "フィ", "フェ", "フォ", "ウィ", "ウェ", "ウォ", "チェ", "ジェ", "シェ", "ヴァ", "ヴィ", "ヴ", "ヴェ", "ヴォ" }, Readings = new[] { "ti", "di", "fa", "fi", "fe", "fo", "wi", "we", "wo", "che", "je", "she", "va", "vi", "vu", "ve", "vo" }, Order = 11, Note = "Modern loanwords use small vowel characters to approximate sounds not found in the traditional kana table. Pronunciation is adapted to Japanese rhythm." },
                new { Title = "Katakana: Real-world loanwords", Jp = "外来語を読む", Items = new[] { "コーヒー", "テレビ", "ホテル", "レストラン", "スーパー", "カメラ", "タクシー", "コンピューター", "スマートフォン", "インターネット", "メニュー", "チョコレート" }, Readings = new[] { "koohii", "terebi", "hoteru", "resutoran", "suupaa", "kamera", "takushii", "konpyuutaa", "sumaatofon", "intaanetto", "menyuu", "chokoreeto" }, Order = 12, Note = "Read by Japanese sound and rhythm rather than reconstructing the English spelling. Loanword meaning and pronunciation may differ from the source language." },
                new { Title = "Katakana: Look-alike mastery", Jp = "似ている文字", Items = new[] { "シ", "ツ", "ソ", "ン", "ク", "ケ", "ウ", "ワ", "フ", "ヌ", "ス", "マ", "ム" }, Readings = new[] { "shi", "tsu", "so", "n", "ku", "ke", "u", "wa", "fu", "nu", "su", "ma", "mu" }, Order = 13, Note = "Printed typefaces change thickness and corner style, but stroke direction and structure remain reliable. Practise the contrasts in random order, not as fixed pairs." }
            };

            foreach (var module in advanced)
                await UpsertKanaModule(context, katakanaId, module.Title, module.Jp, module.Items, module.Readings, module.Order, module.Note, true);

            var legacy = await context.Lessons.FirstOrDefaultAsync(l => l.Title == "Introduction to Katakana");
            if (legacy != null) { legacy.IsPublished = false; await context.SaveChangesAsync(); }
            var combinedLegacy = await context.Lessons.FirstOrDefaultAsync(l => l.Title == "Katakana: Voiced and contracted sounds");
            if (combinedLegacy != null) { combinedLegacy.IsPublished = false; await context.SaveChangesAsync(); }
        }

        private static async Task UpsertKanaModule(ApplicationDbContext context, int categoryId, string title, string japaneseTitle, string[] items, string[] readings, int order, string note, bool advanced)
        {
            var meanings = new Dictionary<string, string>
            {
                ["コート"] = "coat", ["コットン"] = "cotton", ["ビル"] = "building", ["ビール"] = "beer",
                ["サッカー"] = "soccer", ["ベッド"] = "bed", ["ケーキ"] = "cake", ["カップ"] = "cup",
                ["ノート"] = "notebook", ["メール"] = "email", ["コーヒー"] = "coffee", ["テレビ"] = "television",
                ["ホテル"] = "hotel", ["レストラン"] = "restaurant", ["スーパー"] = "supermarket",
                ["カメラ"] = "camera", ["タクシー"] = "taxi", ["コンピューター"] = "computer",
                ["スマートフォン"] = "smartphone", ["インターネット"] = "internet", ["メニュー"] = "menu",
                ["チョコレート"] = "chocolate"
            };
            var lesson = await context.Lessons.FirstOrDefaultAsync(l => l.Title == title);
            if (lesson == null)
            {
                lesson = new Lesson { CategoryId = categoryId, Title = title, JapaneseTitle = japaneseTitle, IsPublished = true };
                context.Lessons.Add(lesson);
            }
            lesson.Description = $"Learn to hear, distinguish, read and actively recall {string.Join("・", items)}.";
            lesson.DifficultyLevel = advanced ? 2 : 1;
            lesson.EstimatedMinutes = items.Length > 15 ? 30 : 22;
            lesson.DisplayOrder = order;
            lesson.Content = advanced
                ? BuildAdvancedKanaContent(japaneseTitle, items, readings, note, "Pay attention to character size, stroke direction and mora timing; these features can change the word.")
                : BuildKanaLessonContent(japaneseTitle, items, readings, note + " Compare standard print with careful handwriting; decorative font details are not new strokes.");
            await context.SaveChangesAsync();

            for (var i = 0; i < items.Length; i++)
            {
                var text = items[i];
                var vocab = await context.Vocabulary.FirstOrDefaultAsync(v => v.CategoryId == categoryId && v.Japanese == text);
                if (vocab == null)
                {
                    vocab = new Vocabulary { CategoryId = categoryId, Japanese = text, Romaji = readings[i], English = meanings.GetValueOrDefault(text, $"katakana reading: {readings[i]}"), PartOfSpeech = advanced ? "Katakana pattern" : "Katakana", DisplayOrder = order * 100 + i, JlptLevel = 5 };
                    context.Vocabulary.Add(vocab);
                    await context.SaveChangesAsync();
                }
                vocab.Romaji = readings[i];
                var reusedBasicCharacter = advanced && vocab.PartOfSpeech == "Katakana";
                if (!reusedBasicCharacter)
                {
                    vocab.English = meanings.GetValueOrDefault(text, advanced ? $"katakana pattern: {readings[i]}" : $"katakana {readings[i]}");
                    vocab.PartOfSpeech = advanced ? "Katakana pattern" : "Katakana";
                    vocab.DisplayOrder = order * 100 + i;
                }
            }
            await context.SaveChangesAsync();
            var lessonItems = await context.Vocabulary.Where(v => v.CategoryId == categoryId && items.Contains(v.Japanese))
                .OrderBy(v => v.DisplayOrder).ToListAsync();
            await SyncLessonItems(context, lesson, items.Select(item => lessonItems.Single(v => v.Japanese == item)).ToList());
        }

        private static async Task SeedA1IntroductionUnitAsync(ApplicationDbContext context)
        {
            var vocabularyId = await context.Categories.Where(c => c.Name == "Vocabulary").Select(c => c.Id).FirstAsync();
            var grammarId = await context.Categories.Where(c => c.Name == "Grammar").Select(c => c.Id).FirstAsync();

            var entries = new[]
            {
                new { Japanese = "おはようございます", Romaji = "ohayou gozaimasu", English = "good morning (polite)", Kind = "Greeting" },
                new { Japanese = "こんにちは", Romaji = "konnichiwa", English = "hello / good afternoon", Kind = "Greeting" },
                new { Japanese = "こんばんは", Romaji = "konbanwa", English = "good evening", Kind = "Greeting" },
                new { Japanese = "はじめまして", Romaji = "hajimemashite", English = "nice to meet you (first greeting)", Kind = "Expression" },
                new { Japanese = "よろしくおねがいします", Romaji = "yoroshiku onegai shimasu", English = "a polite phrase used at the end of an introduction", Kind = "Expression" },
                new { Japanese = "ありがとうございます", Romaji = "arigatou gozaimasu", English = "thank you (polite)", Kind = "Expression" },
                new { Japanese = "すみません", Romaji = "sumimasen", English = "excuse me / sorry", Kind = "Expression" },
                new { Japanese = "わたし", Romaji = "watashi", English = "I / me", Kind = "Pronoun" },
                new { Japanese = "なまえ", Romaji = "namae", English = "name", Kind = "Noun" },
                new { Japanese = "がくせい", Romaji = "gakusei", English = "student", Kind = "Noun" },
                new { Japanese = "せんせい", Romaji = "sensei", English = "teacher; title for a teacher", Kind = "Noun" },
                new { Japanese = "かいしゃいん", Romaji = "kaishain", English = "company employee", Kind = "Noun" },
                new { Japanese = "にほん", Romaji = "nihon", English = "Japan", Kind = "Country" },
                new { Japanese = "マレーシア", Romaji = "mareeshia", English = "Malaysia", Kind = "Country" },
                new { Japanese = "ちゅうごく", Romaji = "chuugoku", English = "China", Kind = "Country" },
                new { Japanese = "どちら", Romaji = "dochira", English = "where / which direction (polite)", Kind = "Question word" }
            };

            var vocab = new List<Vocabulary>();
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var item = await context.Vocabulary.FirstOrDefaultAsync(v => v.CategoryId == vocabularyId && v.Japanese == entry.Japanese);
                if (item == null)
                {
                    item = new Vocabulary { CategoryId = vocabularyId, Japanese = entry.Japanese, Romaji = entry.Romaji, English = entry.English, PartOfSpeech = entry.Kind, DisplayOrder = 100 + i, JlptLevel = 5 };
                    context.Vocabulary.Add(item);
                }
                else
                {
                    item.Romaji = entry.Romaji; item.English = entry.English; item.PartOfSpeech = entry.Kind;
                }
                vocab.Add(item);
            }
            await context.SaveChangesAsync();

            var greetings = await UpsertA1Lesson(context, vocabularyId,
                "A1: Greetings for the situation", "場面に合ったあいさつ", 1, 25,
                "I can choose and use a basic greeting appropriate to the time and situation.",
                "<h2>Scene first</h2><p>Imagine meeting a teacher in the morning, entering a shop, or meeting someone for the first time. Japanese greetings depend on time, relationship and situation.</p>" +
                "<h2>Core expressions</h2><ul><li><strong>おはようございます</strong> — polite morning greeting</li><li><strong>こんにちは</strong> — daytime greeting</li><li><strong>こんばんは</strong> — evening greeting</li><li><strong>はじめまして</strong> — used only when meeting for the first time</li><li><strong>よろしくおねがいします</strong> — closes an introduction and expresses goodwill</li></ul>" +
                "<h2>Politeness and culture</h2><p>おはよう is casual; おはようございます is safer with teachers, colleagues and unfamiliar adults. こんにちは is not normally used as a telephone greeting. A small bow may accompany a greeting, but the language choice matters more than memorising a fixed bow angle.</p>" +
                "<h2>Can-do task</h2><p>Choose suitable expressions for morning, daytime, evening and a first meeting. Then say a two-line first greeting aloud without reading.</p>");

            var selfIntro = await UpsertA1Lesson(context, grammarId,
                "A1: Introduce yourself", "自己紹介をする", 2, 35,
                "I can state my name, country and role, and ask for the same basic information.",
                "<h2>Model dialogue</h2><p><strong>A:</strong> はじめまして。リーです。マレーシアからきました。がくせいです。よろしくおねがいします。<br/><strong>B:</strong> はじめまして。たなかです。よろしくおねがいします。</p>" +
                "<h2>Notice the pattern</h2><ul><li><strong>A は B です</strong> identifies or describes A.</li><li><strong>A は B ですか</strong> asks a polite question.</li><li><strong>A は B じゃありません</strong> gives a polite conversational negative.</li><li><strong>も</strong> means 'also' when replacing は.</li></ul>" +
                "<h2>Natural omission</h2><p>Japanese often omits information that is obvious. Instead of repeatedly saying わたしは, a natural introduction may simply be リーです。がくせいです。</p>" +
                "<h2>Useful questions</h2><p>おなまえは？ asks someone's name in a conversational way. どちらからですか asks where someone is from politely. Avoid attaching あなたは to every question.</p>" +
                "<h2>Output task</h2><p>Create a four-part introduction: first-meeting greeting, name, country or hometown, and role. Record or say it once with support, then again without reading.</p>" +
                "<h2>Success check</h2><p>Your message is understandable, uses です consistently, and suits a first meeting. Small pronunciation errors are acceptable if they do not block understanding.</p>");

            await MapLessonItems(context, greetings, vocab.Where(v => new[] { "おはようございます", "こんにちは", "こんばんは", "はじめまして", "よろしくおねがいします", "ありがとうございます", "すみません" }.Contains(v.Japanese)).ToList());
            await MapLessonItems(context, selfIntro, vocab.Where(v => new[] { "はじめまして", "よろしくおねがいします", "わたし", "なまえ", "がくせい", "せんせい", "かいしゃいん", "にほん", "マレーシア", "ちゅうごく", "どちら" }.Contains(v.Japanese)).ToList());

            var legacyGreetings = await context.Lessons.FirstOrDefaultAsync(l => l.Title == "Essential Greetings and Expressions");
            var legacyGrammar = await context.Lessons.FirstOrDefaultAsync(l => l.Title == "Desu/Masu Form: Basic Sentence Structure");
            if (legacyGreetings != null) legacyGreetings.IsPublished = false;
            if (legacyGrammar != null) legacyGrammar.IsPublished = false;
            await context.SaveChangesAsync();
        }

        private static async Task<Lesson> UpsertA1Lesson(ApplicationDbContext context, int categoryId, string title, string japaneseTitle, int order, int minutes, string canDo, string content)
        {
            var lesson = await context.Lessons.FirstOrDefaultAsync(l => l.Title == title);
            if (lesson == null) { lesson = new Lesson { Title = title }; context.Lessons.Add(lesson); }
            lesson.CategoryId = categoryId; lesson.JapaneseTitle = japaneseTitle; lesson.DisplayOrder = order;
            lesson.EstimatedMinutes = minutes; lesson.DifficultyLevel = 1; lesson.IsPublished = true;
            lesson.Description = canDo; lesson.Content = $"<h2>Can-do goal</h2><p>{canDo}</p>" + content;
            await context.SaveChangesAsync();
            return lesson;
        }

        private static async Task MapLessonItems(ApplicationDbContext context, Lesson lesson, List<Vocabulary> items)
        {
            for (var i = 0; i < items.Count; i++)
                if (!await context.LessonVocabulary.AnyAsync(x => x.LessonId == lesson.Id && x.VocabularyId == items[i].Id))
                    context.LessonVocabulary.Add(new LessonVocabulary { LessonId = lesson.Id, VocabularyId = items[i].Id, DisplayOrder = i });
            await context.SaveChangesAsync();
        }

        private static async Task SeedA1FamilyUnitAsync(ApplicationDbContext context)
        {
            var a1Category = await context.Categories.FirstOrDefaultAsync(c => c.Name == "A1 Course");
            if (a1Category == null)
            {
                a1Category = new Category
                {
                    Name = "A1 Course", Description = "Build practical beginner Japanese through real-life Can-do tasks",
                    IconClass = "bi bi-signpost-split", DisplayOrder = 3
                };
                context.Categories.Add(a1Category);
                await context.SaveChangesAsync();
            }

            var orderMap = new Dictionary<string, int> { ["Hiragana"] = 1, ["Katakana"] = 2, ["A1 Course"] = 3, ["Vocabulary"] = 4, ["Grammar"] = 5, ["Kanji"] = 6, ["Culture"] = 7 };
            foreach (var category in await context.Categories.ToListAsync())
                if (orderMap.TryGetValue(category.Name, out var categoryOrder)) category.DisplayOrder = categoryOrder;

            var existingA1 = await context.Lessons.Where(l => l.Title.StartsWith("A1:")).ToListAsync();
            foreach (var lesson in existingA1) lesson.CategoryId = a1Category.Id;
            await context.SaveChangesAsync();

            var entries = new[]
            {
                new { Japanese = "かぞく", Romaji = "kazoku", English = "family", Kind = "Noun" },
                new { Japanese = "ちち", Romaji = "chichi", English = "my father", Kind = "Family" },
                new { Japanese = "はは", Romaji = "haha", English = "my mother", Kind = "Family" },
                new { Japanese = "おとうさん", Romaji = "otousan", English = "someone else's father / dad", Kind = "Family" },
                new { Japanese = "おかあさん", Romaji = "okaasan", English = "someone else's mother / mom", Kind = "Family" },
                new { Japanese = "あに", Romaji = "ani", English = "my older brother", Kind = "Family" },
                new { Japanese = "あね", Romaji = "ane", English = "my older sister", Kind = "Family" },
                new { Japanese = "おにいさん", Romaji = "oniisan", English = "someone else's older brother", Kind = "Family" },
                new { Japanese = "おねえさん", Romaji = "oneesan", English = "someone else's older sister", Kind = "Family" },
                new { Japanese = "おとうと", Romaji = "otouto", English = "younger brother", Kind = "Family" },
                new { Japanese = "いもうと", Romaji = "imouto", English = "younger sister", Kind = "Family" },
                new { Japanese = "りょうしん", Romaji = "ryoushin", English = "parents", Kind = "Family" },
                new { Japanese = "きょうだい", Romaji = "kyoudai", English = "siblings", Kind = "Family" },
                new { Japanese = "ともだち", Romaji = "tomodachi", English = "friend", Kind = "Noun" },
                new { Japanese = "だれ", Romaji = "dare", English = "who", Kind = "Question word" },
                new { Japanese = "ひと", Romaji = "hito", English = "person", Kind = "Noun" },
                new { Japanese = "やさしい", Romaji = "yasashii", English = "kind / gentle", Kind = "I-adjective" },
                new { Japanese = "げんき", Romaji = "genki", English = "healthy / energetic", Kind = "Na-adjective" },
                new { Japanese = "しずか", Romaji = "shizuka", English = "quiet", Kind = "Na-adjective" }
            };

            var items = new List<Vocabulary>();
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var item = await context.Vocabulary.FirstOrDefaultAsync(v => v.CategoryId == a1Category.Id && v.Japanese == entry.Japanese);
                if (item == null)
                {
                    item = new Vocabulary { CategoryId = a1Category.Id, Japanese = entry.Japanese, Romaji = entry.Romaji, English = entry.English, PartOfSpeech = entry.Kind, DisplayOrder = 200 + i, JlptLevel = 5 };
                    context.Vocabulary.Add(item);
                }
                items.Add(item);
            }
            await context.SaveChangesAsync();

            var familyTerms = await UpsertA1Lesson(context, a1Category.Id,
                "A1: Talk about your family", "家族について話す", 3, 35,
                "I can identify close family members and introduce one member of my family.",
                "<h2>Scene first</h2><p>You are showing a family photo to a new friend. The goal is not to recite a family-word list; it is to identify people naturally.</p>" +
                "<h2>Inside and outside language</h2><p>When speaking to people outside your family, Japanese commonly distinguishes your own family from someone else's family.</p><table class='table'><thead><tr><th>My family</th><th>Someone else's family / direct address</th></tr></thead><tbody><tr><td>ちち</td><td>おとうさん</td></tr><tr><td>はは</td><td>おかあさん</td></tr><tr><td>あに</td><td>おにいさん</td></tr><tr><td>あね</td><td>おねえさん</td></tr></tbody></table>" +
                "<h2>Core pattern</h2><p><strong>これは わたしの ははです。</strong> means 'This is my mother.' The particle の links two nouns and often shows belonging or relationship.</p>" +
                "<h2>Ask and answer</h2><p><strong>このひとは だれですか。</strong> — Who is this person?<br/><strong>あねです。</strong> — She is my older sister.</p>" +
                "<h2>Output task</h2><p>Choose a real or imaginary photo. Identify at least three people and introduce one person using の and です.</p>");

            var describePeople = await UpsertA1Lesson(context, a1Category.Id,
                "A1: Describe people simply", "人を簡単に説明する", 4, 35,
                "I can give two simple facts about a family member or friend.",
                "<h2>Model</h2><p>これは わたしの ともだちです。マリアさんです。がくせいです。やさしいです。</p>" +
                "<h2>Add information</h2><p>Put short statements together rather than forcing one long English-style sentence: <strong>あには かいしゃいんです。げんきです。</strong></p>" +
                "<h2>Use も for shared information</h2><p><strong>わたしは がくせいです。いもうとも がくせいです。</strong> Here も replaces は and means 'also'.</p>" +
                "<h2>Adjective note</h2><p>い-adjectives can appear directly before です: やさしいです。The word げんき behaves as a な-adjective: げんきです in this sentence pattern. Do not add な before です.</p>" +
                "<h2>Output task</h2><p>Introduce one person with their relationship or name, role, and one quality. Then ask your partner このひとは だれですか。</p>");

            await MapLessonItems(context, familyTerms, items.Where(v => v.PartOfSpeech == "Family" || new[] { "かぞく", "だれ", "ひと" }.Contains(v.Japanese)).ToList());
            await MapLessonItems(context, describePeople, items.Where(v => new[] { "ともだち", "だれ", "ひと", "やさしい", "げんき", "しずか" }.Contains(v.Japanese)).Concat(items.Where(v => v.PartOfSpeech == "Family").Take(4)).ToList());
            await context.SaveChangesAsync();
        }

        private static async Task SeedVocabularyAsync(ApplicationDbContext context)
        {
            if (await context.Vocabulary.AnyAsync()) return;

            var hiraganaId = await context.Categories.Where(c => c.Name == "Hiragana").Select(c => c.Id).FirstAsync();
            var katakanaId = await context.Categories.Where(c => c.Name == "Katakana").Select(c => c.Id).FirstAsync();
            var vocabId = await context.Categories.Where(c => c.Name == "Vocabulary").Select(c => c.Id).FirstAsync();
            var grammarId = await context.Categories.Where(c => c.Name == "Grammar").Select(c => c.Id).FirstAsync();

            var vocabulary = new[]
            {
                new Vocabulary { CategoryId = hiraganaId, Japanese = "あ", Romaji = "a", English = "letter a", PartOfSpeech = "Hiragana", DisplayOrder = 1, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "い", Romaji = "i", English = "letter i", PartOfSpeech = "Hiragana", DisplayOrder = 2, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "う", Romaji = "u", English = "letter u", PartOfSpeech = "Hiragana", DisplayOrder = 3, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "え", Romaji = "e", English = "letter e", PartOfSpeech = "Hiragana", DisplayOrder = 4, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "お", Romaji = "o", English = "letter o", PartOfSpeech = "Hiragana", DisplayOrder = 5, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "か", Romaji = "ka", English = "letter ka", PartOfSpeech = "Hiragana", DisplayOrder = 6, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "き", Romaji = "ki", English = "letter ki", PartOfSpeech = "Hiragana", DisplayOrder = 7, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "く", Romaji = "ku", English = "letter ku", PartOfSpeech = "Hiragana", DisplayOrder = 8, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "け", Romaji = "ke", English = "letter ke", PartOfSpeech = "Hiragana", DisplayOrder = 9, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "こ", Romaji = "ko", English = "letter ko", PartOfSpeech = "Hiragana", DisplayOrder = 10, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "さ", Romaji = "sa", English = "letter sa", PartOfSpeech = "Hiragana", DisplayOrder = 11, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "し", Romaji = "shi", English = "letter shi", PartOfSpeech = "Hiragana", DisplayOrder = 12, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "す", Romaji = "su", English = "letter su", PartOfSpeech = "Hiragana", DisplayOrder = 13, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "せ", Romaji = "se", English = "letter se", PartOfSpeech = "Hiragana", DisplayOrder = 14, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "そ", Romaji = "so", English = "letter so", PartOfSpeech = "Hiragana", DisplayOrder = 15, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "た", Romaji = "ta", English = "letter ta", PartOfSpeech = "Hiragana", DisplayOrder = 16, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "な", Romaji = "na", English = "letter na", PartOfSpeech = "Hiragana", DisplayOrder = 17, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "は", Romaji = "ha", English = "letter ha", PartOfSpeech = "Hiragana", DisplayOrder = 18, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "ま", Romaji = "ma", English = "letter ma", PartOfSpeech = "Hiragana", DisplayOrder = 19, JlptLevel = 5 },
                new Vocabulary { CategoryId = hiraganaId, Japanese = "や", Romaji = "ya", English = "letter ya", PartOfSpeech = "Hiragana", DisplayOrder = 20, JlptLevel = 5 },
                new Vocabulary { CategoryId = katakanaId, Japanese = "ア", Romaji = "a", English = "katakana a", PartOfSpeech = "Katakana", DisplayOrder = 1, JlptLevel = 5 },
                new Vocabulary { CategoryId = katakanaId, Japanese = "カ", Romaji = "ka", English = "katakana ka", PartOfSpeech = "Katakana", DisplayOrder = 2, JlptLevel = 5 },
                new Vocabulary { CategoryId = katakanaId, Japanese = "サ", Romaji = "sa", English = "katakana sa", PartOfSpeech = "Katakana", DisplayOrder = 3, JlptLevel = 5 },
                new Vocabulary { CategoryId = katakanaId, Japanese = "タ", Romaji = "ta", English = "katakana ta", PartOfSpeech = "Katakana", DisplayOrder = 4, JlptLevel = 5 },
                new Vocabulary { CategoryId = katakanaId, Japanese = "ナ", Romaji = "na", English = "katakana na", PartOfSpeech = "Katakana", DisplayOrder = 5, JlptLevel = 5 },
                new Vocabulary { CategoryId = vocabId, Japanese = "こんにちは", Romaji = "konnichiwa", English = "Hello / Good afternoon", PartOfSpeech = "Greeting", ExampleJapanese = "こんにちは、はじめまして。", ExampleEnglish = "Hello, nice to meet you.", DisplayOrder = 1, JlptLevel = 5 },
                new Vocabulary { CategoryId = vocabId, Japanese = "ありがとう", Romaji = "arigatou", English = "Thank you", PartOfSpeech = "Expression", ExampleJapanese = "ありがとうございます。", ExampleEnglish = "Thank you very much.", DisplayOrder = 2, JlptLevel = 5 },
                new Vocabulary { CategoryId = vocabId, Japanese = "すみません", Romaji = "sumimasen", English = "Excuse me / I'm sorry", PartOfSpeech = "Expression", ExampleJapanese = "すみません、えきはどこですか。", ExampleEnglish = "Excuse me, where is the station?", DisplayOrder = 3, JlptLevel = 5 },
                new Vocabulary { CategoryId = vocabId, Japanese = "がくせい", Romaji = "gakusei", English = "student", PartOfSpeech = "Noun", ExampleJapanese = "わたしはがくせいです。", ExampleEnglish = "I am a student.", DisplayOrder = 4, JlptLevel = 5 },
                new Vocabulary { CategoryId = vocabId, Japanese = "せんせい", Romaji = "sensei", English = "teacher", PartOfSpeech = "Noun", ExampleJapanese = "たなかせんせいはやさしいです。", ExampleEnglish = "Tanaka-sensei is kind.", DisplayOrder = 5, JlptLevel = 5 },
                new Vocabulary { CategoryId = vocabId, Japanese = "ほん", Romaji = "hon", English = "book", PartOfSpeech = "Noun", ExampleJapanese = "これはほんです。", ExampleEnglish = "This is a book.", DisplayOrder = 6, JlptLevel = 5 },
                new Vocabulary { CategoryId = vocabId, Japanese = "みず", Romaji = "mizu", English = "water", PartOfSpeech = "Noun", ExampleJapanese = "みずをのみます。", ExampleEnglish = "I drink water.", DisplayOrder = 7, JlptLevel = 5 },
                new Vocabulary { CategoryId = vocabId, Japanese = "たべる", Romaji = "taberu", English = "to eat", PartOfSpeech = "Verb", ExampleJapanese = "パンをたべます。", ExampleEnglish = "I eat bread.", DisplayOrder = 8, JlptLevel = 5 },
                new Vocabulary { CategoryId = vocabId, Japanese = "のむ", Romaji = "nomu", English = "to drink", PartOfSpeech = "Verb", ExampleJapanese = "コーヒーをのみます。", ExampleEnglish = "I drink coffee.", DisplayOrder = 9, JlptLevel = 5 },
                new Vocabulary { CategoryId = vocabId, Japanese = "いく", Romaji = "iku", English = "to go", PartOfSpeech = "Verb", ExampleJapanese = "がっこうにいきます。", ExampleEnglish = "I go to school.", DisplayOrder = 10, JlptLevel = 5 },
                new Vocabulary { CategoryId = vocabId, Japanese = "おおきい", Romaji = "ookii", English = "big", PartOfSpeech = "Adjective", ExampleJapanese = "おおきいねこです。", ExampleEnglish = "It is a big cat.", DisplayOrder = 11, JlptLevel = 5 },
                new Vocabulary { CategoryId = vocabId, Japanese = "ちいさい", Romaji = "chiisai", English = "small", PartOfSpeech = "Adjective", ExampleJapanese = "ちいさいいぬです。", ExampleEnglish = "It is a small dog.", DisplayOrder = 12, JlptLevel = 5 },
                new Vocabulary { CategoryId = grammarId, Japanese = "は", Romaji = "wa", English = "topic marker particle", PartOfSpeech = "Particle", ExampleJapanese = "わたしはがくせいです。", ExampleEnglish = "As for me, I am a student.", DisplayOrder = 1, JlptLevel = 5 },
                new Vocabulary { CategoryId = grammarId, Japanese = "が", Romaji = "ga", English = "subject marker particle", PartOfSpeech = "Particle", ExampleJapanese = "だれがきましたか。", ExampleEnglish = "Who came?", DisplayOrder = 2, JlptLevel = 5 },
                new Vocabulary { CategoryId = grammarId, Japanese = "を", Romaji = "wo", English = "object marker particle", PartOfSpeech = "Particle", ExampleJapanese = "パンをたべます。", ExampleEnglish = "I eat bread.", DisplayOrder = 3, JlptLevel = 5 },
                new Vocabulary { CategoryId = grammarId, Japanese = "に", Romaji = "ni", English = "direction/time particle", PartOfSpeech = "Particle", ExampleJapanese = "がっこうにいきます。", ExampleEnglish = "I go to school.", DisplayOrder = 4, JlptLevel = 5 },
                new Vocabulary { CategoryId = grammarId, Japanese = "で", Romaji = "de", English = "means/location particle", PartOfSpeech = "Particle", ExampleJapanese = "バスでいきます。", ExampleEnglish = "I go by bus.", DisplayOrder = 5, JlptLevel = 5 },
            };

            context.Vocabulary.AddRange(vocabulary);
            await context.SaveChangesAsync();
        }

        private static async Task SeedQuizzesAsync(ApplicationDbContext context)
        {
            if (await context.Quizzes.AnyAsync()) return;

            var hiraganaId = await context.Categories.Where(c => c.Name == "Hiragana").Select(c => c.Id).FirstAsync();
            var vocabId = await context.Categories.Where(c => c.Name == "Vocabulary").Select(c => c.Id).FirstAsync();
            var grammarId = await context.Categories.Where(c => c.Name == "Grammar").Select(c => c.Id).FirstAsync();

            var quiz1 = new Quiz
            {
                CategoryId = hiraganaId,
                Title = "Hiragana Basic Characters",
                JapaneseTitle = "ひらがなテスト",
                Description = "Test your knowledge of the basic hiragana characters.",
                TimeLimitMinutes = 10,
                PassingScore = 70,
                DisplayOrder = 1
            };
            context.Quizzes.Add(quiz1);
            await context.SaveChangesAsync();

            var quiz1Questions = new[]
            {
                new QuizQuestion { QuizId = quiz1.Id, QuestionText = "Which hiragana character is 'a'?", OptionA = "あ", OptionB = "い", OptionC = "う", OptionD = "え", CorrectOption = 1, Explanation = "あ is the hiragana character for 'a'.", DisplayOrder = 1 },
                new QuizQuestion { QuizId = quiz1.Id, QuestionText = "Which hiragana character is 'ki'?", OptionA = "か", OptionB = "き", OptionC = "く", OptionD = "け", CorrectOption = 2, Explanation = "き is the hiragana character for 'ki'.", DisplayOrder = 2 },
                new QuizQuestion { QuizId = quiz1.Id, QuestionText = "What sound does さ make?", OptionA = "ka", OptionB = "ki", OptionC = "sa", OptionD = "shi", CorrectOption = 3, Explanation = "さ is pronounced 'sa'.", DisplayOrder = 3 },
                new QuizQuestion { QuizId = quiz1.Id, QuestionText = "Which character represents 'to'?", OptionA = "た", OptionB = "ち", OptionC = "つ", OptionD = "と", CorrectOption = 4, Explanation = "と is the hiragana for 'to'.", DisplayOrder = 4 },
                new QuizQuestion { QuizId = quiz1.Id, QuestionText = "What sound does ぬ make?", OptionA = "na", OptionB = "nu", OptionC = "ne", OptionD = "no", CorrectOption = 2, Explanation = "ぬ is pronounced 'nu'.", DisplayOrder = 5 },
                new QuizQuestion { QuizId = quiz1.Id, QuestionText = "Which hiragana is 'ha'?", OptionA = "は", OptionB = "ひ", OptionC = "ふ", OptionD = "ほ", CorrectOption = 1, Explanation = "は is the hiragana for 'ha'.", DisplayOrder = 6 },
                new QuizQuestion { QuizId = quiz1.Id, QuestionText = "What sound does も make?", OptionA = "ma", OptionB = "mi", OptionC = "mu", OptionD = "mo", CorrectOption = 4, Explanation = "も is pronounced 'mo'.", DisplayOrder = 7 },
                new QuizQuestion { QuizId = quiz1.Id, QuestionText = "Which character is 'yu'?", OptionA = "や", OptionB = "ゆ", OptionC = "よ", OptionD = "わ", CorrectOption = 2, Explanation = "ゆ is the hiragana for 'yu'.", DisplayOrder = 8 },
            };
            context.QuizQuestions.AddRange(quiz1Questions);

            var quiz2 = new Quiz
            {
                CategoryId = vocabId,
                Title = "Essential Vocabulary Quiz",
                JapaneseTitle = "語彙テスト",
                Description = "Test your knowledge of basic Japanese vocabulary words.",
                TimeLimitMinutes = 15,
                PassingScore = 60,
                DisplayOrder = 1
            };
            context.Quizzes.Add(quiz2);
            await context.SaveChangesAsync();

            var quiz2Questions = new[]
            {
                new QuizQuestion { QuizId = quiz2.Id, QuestionText = "What does 'ありがとう' mean?", OptionA = "Good morning", OptionB = "Thank you", OptionC = "Goodbye", OptionD = "Sorry", CorrectOption = 2, Explanation = "ありがとう means 'Thank you'.", DisplayOrder = 1 },
                new QuizQuestion { QuizId = quiz2.Id, QuestionText = "What does 'がくせい' mean?", OptionA = "Teacher", OptionB = "School", OptionC = "Student", OptionD = "Book", CorrectOption = 3, Explanation = "がくせい (gakusei) means 'student'.", DisplayOrder = 2 },
                new QuizQuestion { QuizId = quiz2.Id, QuestionText = "What does 'たべる' mean?", OptionA = "To drink", OptionB = "To eat", OptionC = "To go", OptionD = "To read", CorrectOption = 2, Explanation = "たべる (taberu) means 'to eat'.", DisplayOrder = 3 },
                new QuizQuestion { QuizId = quiz2.Id, QuestionText = "What does 'みず' mean?", OptionA = "Water", OptionB = "Fire", OptionC = "Earth", OptionD = "Wind", CorrectOption = 1, Explanation = "みず (mizu) means 'water'.", DisplayOrder = 4 },
                new QuizQuestion { QuizId = quiz2.Id, QuestionText = "How do you say 'Hello' in Japanese?", OptionA = "さようなら", OptionB = "こんにちは", OptionC = "ありがとう", OptionD = "すみません", CorrectOption = 2, Explanation = "こんにちは (konnichiwa) means 'Hello'.", DisplayOrder = 5 },
                new QuizQuestion { QuizId = quiz2.Id, QuestionText = "What does 'おおきい' mean?", OptionA = "Small", OptionB = "Big", OptionC = "Old", OptionD = "New", CorrectOption = 2, Explanation = "おおきい (ookii) means 'big'.", DisplayOrder = 6 },
                new QuizQuestion { QuizId = quiz2.Id, QuestionText = "What does 'せんせい' mean?", OptionA = "Student", OptionB = "Doctor", OptionC = "Teacher", OptionD = "Friend", CorrectOption = 3, Explanation = "せんせい (sensei) means 'teacher'.", DisplayOrder = 7 },
                new QuizQuestion { QuizId = quiz2.Id, QuestionText = "What does 'のむ' mean?", OptionA = "To eat", OptionB = "To drink", OptionC = "To sleep", OptionD = "To walk", CorrectOption = 2, Explanation = "のむ (nomu) means 'to drink'.", DisplayOrder = 8 },
            };
            context.QuizQuestions.AddRange(quiz2Questions);

            var quiz3 = new Quiz
            {
                CategoryId = grammarId,
                Title = "Basic Grammar Particles",
                JapaneseTitle = "文法テスト",
                Description = "Test your understanding of essential Japanese grammar particles.",
                TimeLimitMinutes = 15,
                PassingScore = 60,
                DisplayOrder = 1
            };
            context.Quizzes.Add(quiz3);
            await context.SaveChangesAsync();

            var quiz3Questions = new[]
            {
                new QuizQuestion { QuizId = quiz3.Id, QuestionText = "Which particle marks the topic of a sentence?", OptionA = "が", OptionB = "を", OptionC = "は", OptionD = "に", CorrectOption = 3, Explanation = "は (wa) marks the topic of the sentence.", DisplayOrder = 1 },
                new QuizQuestion { QuizId = quiz3.Id, QuestionText = "Which particle marks the direct object?", OptionA = "は", OptionB = "を", OptionC = "が", OptionD = "で", CorrectOption = 2, Explanation = "を (wo) marks the direct object of a verb.", DisplayOrder = 2 },
                new QuizQuestion { QuizId = quiz3.Id, QuestionText = "Fill in: パン___たべます (I eat bread)", OptionA = "は", OptionB = "が", OptionC = "を", OptionD = "に", CorrectOption = 3, Explanation = "パンをたべます - 'wo' marks 'bread' as the object.", DisplayOrder = 3 },
                new QuizQuestion { QuizId = quiz3.Id, QuestionText = "Fill in: がっこう___いきます (I go to school)", OptionA = "で", OptionB = "を", OptionC = "は", OptionD = "に", CorrectOption = 4, Explanation = "がっこうにいきます - 'ni' indicates direction.", DisplayOrder = 4 },
                new QuizQuestion { QuizId = quiz3.Id, QuestionText = "Which particle indicates the means or location of an action?", OptionA = "で", OptionB = "に", OptionC = "が", OptionD = "を", CorrectOption = 1, Explanation = "で (de) indicates means or location of action.", DisplayOrder = 5 },
                new QuizQuestion { QuizId = quiz3.Id, QuestionText = "Fill in: わたし___がくせいです (I am a student)", OptionA = "を", OptionB = "は", OptionC = "で", OptionD = "に", CorrectOption = 2, Explanation = "わたしはがくせいです - 'wa' marks the topic.", DisplayOrder = 6 },
            };
            context.QuizQuestions.AddRange(quiz3Questions);

            await context.SaveChangesAsync();
        }

        // ============================================================
        //                  COMMUNITY SEED DATA
        // ============================================================
        // 16 IdentityUsers + UserProfiles (with diverse AvatarJson)
        // + 48 CommunityPosts split evenly across 4 categories (12 each)
        // + realistic likes, comments, and timestamps.

        /// <summary>
        /// Ensures the 16 seed users + their UserProfile rows exist.
        /// Handles partial creation gracefully — creates any missing users.
        /// </summary>
        private static async Task SeedCommunityUsersAsync(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            var seedUsers = SeedUsers();
            var seedProfiles = SeedUserProfiles();

            // Fast check: are all 16 seed users already present?
            var seedEmails = seedUsers.Values.Select(v => v.Email).ToHashSet();
            var existingEmails = (await context.Users
                .Where(u => seedEmails.Contains(u.Email!))
                .Select(u => u.Email!)
                .ToListAsync())
                .ToHashSet();

            // Only create users that are actually missing
            foreach (var u in seedUsers.Values)
            {
                if (existingEmails.Contains(u.Email)) continue;

                var iu = new IdentityUser
                {
                    UserName = u.UserName,
                    Email = u.Email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(iu, "SeedUser@123");
                if (!result.Succeeded) continue;
            }

            // Batch check: which UserProfiles already exist?
            var allSeedUserIds = (await context.Users
                .Where(u => seedEmails.Contains(u.Email!))
                .Select(u => u.Id)
                .ToListAsync())
                .ToHashSet();

            var existingProfileUserIds = (await context.UserProfiles
                .Where(p => allSeedUserIds.Contains(p.UserId))
                .Select(p => p.UserId)
                .ToListAsync())
                .ToHashSet();

            // Only create missing profiles
            var emailToId = await context.Users
                .Where(u => seedEmails.Contains(u.Email!))
                .ToDictionaryAsync(u => u.Email!, u => u.Id);

            foreach (var p in seedProfiles.Values)
            {
                if (!emailToId.TryGetValue(p.Email, out var userId)) continue;
                if (existingProfileUserIds.Contains(userId)) continue;

                context.UserProfiles.Add(new UserProfile
                {
                    UserId = userId,
                    AvatarJson = p.AvatarJson,
                    CreatedDate = DateTime.UtcNow.AddMonths(-3)
                });
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedCommunityPostsAsync(ApplicationDbContext context)
        {
            // Collect ALL user Ids by email (seed users + admin + student)
            var users = await context.Users
                .ToDictionaryAsync(u => u.Email ?? u.UserName!, u => u.Id);

            if (users.Count == 0) return;

            // Count existing seed posts (posts by seed users)
            var seedEmails = SeedUsers().Values.Select(v => v.Email).ToHashSet();
            var existingSeedPostCount = await context.CommunityPosts
                .Where(p => seedEmails.Contains(p.User!.Email!))
                .CountAsync();

            // Idempotency: skip if CommunityPosts is already fully populated
            if (existingSeedPostCount >= 45) return;

            // If partial seed exists, wipe community data and re-seed cleanly
            if (existingSeedPostCount > 0 && existingSeedPostCount < 45)
            {
                // Delete in correct order (likes first due to FK)
                var allSeedPostIds = await context.CommunityPosts
                    .Where(p => seedEmails.Contains(p.User!.Email!))
                    .Select(p => p.Id)
                    .ToListAsync();

                context.CommunityLikes.RemoveRange(
                    context.CommunityLikes.Where(l => allSeedPostIds.Contains(l.PostId)));
                context.CommunityComments.RemoveRange(
                    context.CommunityComments.Where(c => allSeedPostIds.Contains(c.PostId)));
                context.CommunityPosts.RemoveRange(
                    context.CommunityPosts.Where(p => allSeedPostIds.Contains(p.Id)));
                await context.SaveChangesAsync();
            }

            var now = DateTime.UtcNow;
            var postSeeds = BuildCommunityPostSeeds(now);

            // Build a full like-pool from all available users (seed + admin + student)
            var likePool = users.Keys.Where(k => k.Contains("@")).ToList();
            var maxLikesPerPost = likePool.Count;

            foreach (var ps in postSeeds)
            {
                if (!users.TryGetValue(ps.AuthorEmail, out var uid)) continue;

                try
                {
                    var post = new CommunityPost
                    {
                        UserId = uid,
                        Title = ps.Title,
                        Content = ps.Content,
                        Category = ps.Category,
                        FeedbackWanted = ps.FeedbackWanted,
                        CreatedDate = ps.CreatedUtc
                    };
                    context.CommunityPosts.Add(post);
                    await context.SaveChangesAsync(); // need Id for likes/comments

                    // Likes — cap to available unique users, randomize selection
                    var actualLikeCount = Math.Min(ps.LikeCount, maxLikesPerPost);
                    var shuffled = likePool.OrderBy(_ => Guid.NewGuid()).Take(actualLikeCount).ToList();
                    var likeIdx = 0;
                    foreach (var likerEmail in shuffled)
                    {
                        if (!users.TryGetValue(likerEmail, out var likerId)) continue;
                        if (likerId == uid && actualLikeCount < maxLikesPerPost) continue; // author can like own post, but skip for variety
                        context.CommunityLikes.Add(new CommunityLike
                        {
                            PostId = post.Id,
                            UserId = likerId,
                            CreatedDate = ps.CreatedUtc.AddMinutes(likeIdx * 2)
                        });
                        likeIdx++;
                    }

                    // Comments
                    foreach (var (commenterEmail, body, isReply, ageOffset) in ps.Comments)
                    {
                        if (!users.TryGetValue(commenterEmail, out var cid)) continue;
                        var commentDate = ps.CreatedUtc.Add(ageOffset);

                        int? parentId = null;
                        if (isReply && post.Id > 0)
                        {
                            var existingTop = await context.CommunityComments
                                .Where(c => c.PostId == post.Id && c.ParentCommentId == null)
                                .OrderBy(c => c.Id)
                                .FirstOrDefaultAsync();
                            parentId = existingTop?.Id;
                        }
                        context.CommunityComments.Add(new CommunityComment
                        {
                            PostId = post.Id,
                            UserId = cid,
                            Content = body,
                            ParentCommentId = parentId,
                            CreatedDate = commentDate
                        });
                    }
                    await context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    // If one post fails, continue with the rest
                    // Detach any tracked entities to avoid state pollution
                    foreach (var entry in context.ChangeTracker.Entries().ToList())
                    {
                        entry.State = EntityState.Detached;
                    }
                }
            }
        }

        // ---- Helpers: avatar JSON for each seed user ----

        private static Dictionary<string, (string Email, string UserName)> SeedUsers() => new()
        {
            { "miku",   ("miku@nihongoseed.community",   "MikuStudy")       },
            { "sakura", ("sakura@nihongoseed.community", "Sakura_Dream")    },
            { "haru",   ("haru@nihongoseed.community",   "HaruNeko")        },
            { "kenji",  ("kenji@nihongoseed.community",  "Kenji_Learner")   },
            { "kenta",  ("kenta@nihongoseed.community",  "KentaLearns")     },
            { "yume",   ("yume@nihongoseed.community",   "Yume_22")         },
            { "sora",   ("sora@nihongoseed.community",   "SoraStudy")       },
            { "aoi",    ("aoi@nihongoseed.community",    "AoiChan")         },
            { "neko",   ("neko@nihongoseed.community",   "NekoSensei")      },
            { "mina",   ("mina@nihongoseed.community",   "MinaN5")          },
            { "yuki",   ("yuki@nihongoseed.community",   "YukiN5")          },
            { "hana",   ("hana@nihongoseed.community",   "HanaStudy")       },
            { "ren",    ("ren@nihongoseed.community",    "RenLearnsJP")     },
            { "minoru", ("minoru@nihongoseed.community", "MinoruStudy")     },
            { "tomo",   ("tomo@nihongoseed.community",   "TomoJourney")     },
            { "rika",   ("rika@nihongoseed.community",   "RikaN4")          },
        };

        private static List<string> AllSeedLikeUsers => new()
        {
            "miku@nihongoseed.community",
            "sakura@nihongoseed.community",
            "haru@nihongoseed.community",
            "kenji@nihongoseed.community",
            "kenta@nihongoseed.community",
            "yume@nihongoseed.community",
            "sora@nihongoseed.community",
            "aoi@nihongoseed.community",
            "neko@nihongoseed.community",
            "mina@nihongoseed.community",
            "yuki@nihongoseed.community",
            "hana@nihongoseed.community",
            "ren@nihongoseed.community",
            "minoru@nihongoseed.community",
            "tomo@nihongoseed.community",
            "rika@nihongoseed.community",
        };

        private static Dictionary<string, (string Email, string AvatarJson)> SeedUserProfiles()
        {
            // Mix of hairstyles, hair colors, skin tones, expressions, accessories, backgrounds
            // so the timeline looks like a real community. Each profile is its own
            // varied combination — no two are identical.
            string A(string s, string h, string k, string e, string a, string b = "none") =>
                "{\"skinTone\":\"" + s + "\",\"hairColor\":\"" + h + "\",\"hairStyle\":\"" + k + "\",\"expression\":\"" + e + "\",\"accessory\":\"" + a + "\",\"background\":\"" + b + "\"}";

            return new Dictionary<string, (string, string)>
            {
                { "miku@nihongoseed.community",   ("miku@nihongoseed.community",   A("#F5C4B3", "#4A3426", "long-wavy", "happy",      "headphones", "#FFF0F3"))   },
                { "sakura@nihongoseed.community", ("sakura@nihongoseed.community", A("#E5C2A8", "#222222", "ponytail",  "wink",       "bow-tie",    "#EDE7F6"))   },
                { "haru@nihongoseed.community",   ("haru@nihongoseed.community",   A("#C9966B", "#A0522D", "spiky",     "calm",       "glasses",    "#E3F2FD"))   },
                { "kenji@nihongoseed.community",  ("kenji@nihongoseed.community",  A("#F5C4B3", "#4A3426", "mens-spiky","smile",      "none",       "#E3F2FD"))   },
                { "kenta@nihongoseed.community",  ("kenta@nihongoseed.community",  A("#C9966B", "#222222", "buzz",      "wink",       "headphones", "#FFF8E1"))   },
                { "yume@nihongoseed.community",   ("yume@nihongoseed.community",   A("#E5C2A8", "#F0C040", "short-bob", "happy",      "sakura-pin", "#FFF0F3"))   },
                { "sora@nihongoseed.community",   ("sora@nihongoseed.community",   A("#F5C4B3", "#4A3426", "long-wavy", "smile",      "bow-tie",    "#E8F5E9"))   },
                { "aoi@nihongoseed.community",    ("aoi@nihongoseed.community",    A("#E5C2A8", "#C84B31", "ponytail",  "surprised",  "none",       "#FFF8E1"))   },
                { "neko@nihongoseed.community",   ("neko@nihongoseed.community",   A("#C9966B", "#222222", "spiky",     "calm",       "glasses",    "#F5EDE0"))   },
                { "mina@nihongoseed.community",   ("mina@nihongoseed.community",   A("#F5C4B3", "#A0522D", "short-bob", "surprised",  "glasses",    "#FFF8E1"))   },
                { "yuki@nihongoseed.community",   ("yuki@nihongoseed.community",   A("#F5C4B3", "#FFFFFF", "long-wavy", "calm",       "none",       "#EDE7F6"))   },
                { "hana@nihongoseed.community",   ("hana@nihongoseed.community",   A("#E5C2A8", "#222222", "ponytail",  "happy",      "sakura-pin", "#E8F5E9"))   },
                { "ren@nihongoseed.community",    ("ren@nihongoseed.community",    A("#C9966B", "#F0C040", "mens-spiky","smile",      "glasses",    "#F5EDE0"))   },
                { "minoru@nihongoseed.community", ("minoru@nihongoseed.community", A("#8D5E3C", "#222222", "buzz",      "smile",      "none",       "#E8F5E9"))   },
                { "tomo@nihongoseed.community",   ("tomo@nihongoseed.community",   A("#F5C4B3", "#4A3426", "spiky",     "happy",      "headphones", "#F5EDE0"))   },
                { "rika@nihongoseed.community",   ("rika@nihongoseed.community",   A("#E5C2A8", "#FFFFFF", "short-bob", "wink",       "sakura-pin", "#FFF0F3"))   },
            };
        }

        /// <summary>
        /// 32 community posts (8 per category), with realistic likes,
        /// sample comments and varied timestamps.
        /// </summary>
        private record PostSeed(
            string AuthorEmail,
            string Title,
            string Content,
            CommunityCategory Category,
            bool FeedbackWanted,
            int LikeCount,
            DateTime CreatedUtc,
            List<(string Email, string Body, bool IsReply, TimeSpan AgeOffset)> Comments
        );

        private static List<PostSeed> BuildCommunityPostSeeds(DateTime now)
        {
            // Helpers
            DateTime T(double minutesAgo) => now.AddMinutes(-minutesAgo);
            DateTime H(double hoursAgo)   => now.AddHours(-hoursAgo);
            DateTime D(double daysAgo)    => now.AddDays(-daysAgo);

            var list = new List<PostSeed>();

            // ============== 1. Community (12 posts) ==============
            list.Add(new PostSeed(
                "miku@nihongoseed.community",
                "How is everyone's Japanese study going lately?",
                "I've been learning for about 2 months now. Hiragana is finally getting easier but katakana is still confusing me 😭 What are you guys working on recently?",
                CommunityCategory.General, false, 34, T(8),
                new() {
                    ("kenji@nihongoseed.community", "N5 grammar right now. Particles are killing me 😂", false, TimeSpan.FromMinutes(5)),
                    ("sakura@nihongoseed.community", "Same! シ and ツ still get me every time.", false, TimeSpan.FromMinutes(12)),
                    ("haru@nihongoseed.community", "Slowly, but it's going. That's what counts 🐢", false, TimeSpan.FromMinutes(40)),
                }
            ));

            list.Add(new PostSeed(
                "sakura@nihongoseed.community",
                "Finally memorised all 46 Hiragana!! 🎉",
                "It took me almost two weeks but I can finally recognise all of them without checking the chart. Next stop: Katakana!",
                CommunityCategory.General, false, 51, H(2),
                new() {
                    ("haru@nihongoseed.community", "Congrats!! Katakana next 💪", false, TimeSpan.FromMinutes(30)),
                    ("miku@nihongoseed.community", "This motivates me so much haha", false, TimeSpan.FromMinutes(50)),
                    ("yume@nihongoseed.community", "How did you practise? Any tips?", false, TimeSpan.FromMinutes(75)),
                }
            ));

            list.Add(new PostSeed(
                "haru@nihongoseed.community",
                "What are you studying today? ☕",
                "I'm doing 20 minutes of vocabulary and then trying a short listening exercise.",
                CommunityCategory.General, false, 16, T(45),
                new() {
                    ("yuki@nihongoseed.community", "Grammar revision today!", false, TimeSpan.FromMinutes(20)),
                    ("aoi@nihongoseed.community", "I'm practising speaking 🙋", false, TimeSpan.FromMinutes(25)),
                    ("neko@nihongoseed.community", "Audio only today — no flashcards!", false, TimeSpan.FromMinutes(40)),
                }
            ));

            list.Add(new PostSeed(
                "kenta@nihongoseed.community",
                "Anyone wants to be study buddies?",
                "I'm around beginner/N5 level and usually study at night. Would be nice to keep each other motivated.",
                CommunityCategory.General, false, 21, H(5),
                new() {
                    ("mina@nihongoseed.community", "I'm N5 too!", false, TimeSpan.FromHours(1)),
                    ("sora@nihongoseed.community", "What timezone are you in?", false, TimeSpan.FromHours(2)),
                    ("aoi@nihongoseed.community", "Count me in 🎉", false, TimeSpan.FromHours(4)),
                }
            ));

            list.Add(new PostSeed(
                "yume@nihongoseed.community",
                "Japanese song recommendations? 🎧",
                "I want to listen to more Japanese music while studying. Prefer something not too fast so I can try to catch the lyrics.",
                CommunityCategory.General, false, 28, H(8),
                new() {
                    ("hana@nihongoseed.community", "Try Fujii Kaze!", false, TimeSpan.FromHours(2)),
                    ("ren@nihongoseed.community", "Yorushika has some nice slower songs too.", false, TimeSpan.FromHours(3)),
                    ("miku@nihongoseed.community", "Yoasobi is great if you don't mind the speed.", false, TimeSpan.FromHours(5)),
                }
            ));

            list.Add(new PostSeed(
                "sora@nihongoseed.community",
                "I studied Japanese for 7 days in a row 🔥",
                "Small achievement but I usually give up after 3 days, so I'm happy haha.",
                CommunityCategory.General, false, 43, D(1),
                new() {
                    ("miku@nihongoseed.community", "A win is a win!!", false, TimeSpan.FromHours(6)),
                    ("kenji@nihongoseed.community", "Keep going 🔥", false, TimeSpan.FromHours(10)),
                }
            ));

            list.Add(new PostSeed(
                "aoi@nihongoseed.community",
                "Does anyone else accidentally use Japanese words in real life? 😂",
                "I almost said \"daijoubu\" to my friend today without thinking.",
                CommunityCategory.General, false, 62, D(2),
                new() {
                    ("haru@nihongoseed.community", "I keep saying \"eto…\" 😭", false, TimeSpan.FromHours(8)),
                    ("yume@nihongoseed.community", "SAME HAHAHA", false, TimeSpan.FromHours(9)),
                    ("sakura@nihongoseed.community", "I bowed reflexively to a stranger once 💀", false, TimeSpan.FromHours(10)),
                }
            ));

            list.Add(new PostSeed(
                "neko@nihongoseed.community",
                "What made you start learning Japanese?",
                "For me it was anime at first, but now I really want to travel around Japan and speak comfortably.",
                CommunityCategory.General, false, 39, D(3),
                new() {
                    ("ren@nihongoseed.community", "Japanese games for me.", false, TimeSpan.FromHours(6)),
                    ("sakura@nihongoseed.community", "I started because of J-pop!", false, TimeSpan.FromHours(8)),
                    ("hana@nihongoseed.community", "Wanted to read manga without translations.", false, TimeSpan.FromHours(12)),
                }
            ));

            list.Add(new PostSeed(
                "minoru@nihongoseed.community",
                "Day 15 check-in: still going strong 💪",
                "Halfway through my 30-day study challenge. Today I reviewed all the katakana I learned last week and started on basic greetings. Feeling pretty good!",
                CommunityCategory.General, false, 22, H(1),
                new() {
                    ("kenta@nihongoseed.community", "You got this!", false, TimeSpan.FromMinutes(30)),
                    ("tomo@nihongoseed.community", "Keep it up! What's your next goal?", false, TimeSpan.FromHours(1)),
                }
            ));

            list.Add(new PostSeed(
                "tomo@nihongoseed.community",
                "Found an amazing Japanese learning podcast 🎙️",
                "It's called Nihongo con Teppei — short episodes perfect for beginners. Each one is only about 5 minutes. Highly recommend for commute study!",
                CommunityCategory.General, false, 35, H(4),
                new() {
                    ("sora@nihongoseed.community", "Love that one! Teppei-san is so encouraging.", false, TimeSpan.FromHours(1)),
                    ("mina@nihongoseed.community", "Adding it to my playlist, thanks!", false, TimeSpan.FromHours(2)),
                    ("yume@nihongoseed.community", "5 minutes is perfect for my attention span 😂", false, TimeSpan.FromHours(3)),
                }
            ));

            list.Add(new PostSeed(
                "rika@nihongoseed.community",
                "Hit my 100-day streak today!! 🏆",
                "Never thought I'd make it this far. Some days I only studied 5 minutes but I showed up every single day. Consistency > intensity.",
                CommunityCategory.General, false, 75, D(1.5),
                new() {
                    ("miku@nihongoseed.community", "100 days!! That's incredible 🎉🎉", false, TimeSpan.FromHours(4)),
                    ("sakura@nihongoseed.community", "You're an inspiration honestly.", false, TimeSpan.FromHours(5)),
                    ("neko@nihongoseed.community", "Even 5 minutes counts. Great mindset.", false, TimeSpan.FromHours(7)),
                }
            ));

            list.Add(new PostSeed(
                "yuki@nihongoseed.community",
                "My cat sits with me while I study 🐱",
                "Every time I open my Japanese textbook she jumps on my lap. I think she's secretly learning too.",
                CommunityCategory.General, false, 48, D(2.5),
                new() {
                    ("aoi@nihongoseed.community", "Cats + studying = perfect combo 😻", false, TimeSpan.FromHours(3)),
                    ("haru@nihongoseed.community", "My cat does the same! Must be a universal thing.", false, TimeSpan.FromHours(5)),
                }
            ));

            // ============== 2. Study (12 posts) ==============
            list.Add(new PostSeed(
                "kenji@nihongoseed.community",
                "A trick that helped me remember Katakana",
                "Instead of memorising every symbol separately, I grouped similar-looking characters together and made silly stories for them. It helped a lot with シ / ツ and ソ / ン.",
                CommunityCategory.Study, false, 46, T(15),
                new() {
                    ("miku@nihongoseed.community", "Please share the シ/ツ trick 😭", false, TimeSpan.FromMinutes(10)),
                    ("kenji@nihongoseed.community", "I'll make another post!", true, TimeSpan.FromMinutes(25)),
                    ("yuki@nihongoseed.community", "Stories really do help me too.", false, TimeSpan.FromMinutes(40)),
                }
            ));

            list.Add(new PostSeed(
                "mina@nihongoseed.community",
                "How I review vocabulary without forgetting everything",
                "I review new words after 1 day, 3 days, 7 days and then again after two weeks. It's slower but I remember much more now.",
                CommunityCategory.Study, false, 58, H(3),
                new() {
                    ("aoi@nihongoseed.community", "Going to try this!", false, TimeSpan.FromMinutes(20)),
                    ("haru@nihongoseed.community", "Basically spaced repetition 👍", false, TimeSpan.FromMinutes(45)),
                    ("ren@nihongoseed.community", "I use Anki with custom intervals. Same idea.", false, TimeSpan.FromHours(1)),
                }
            ));

            list.Add(new PostSeed(
                "sakura@nihongoseed.community",
                "My 30-day vocabulary challenge",
                "I learned 10 new words every day for 30 days. I didn't remember all 300 perfectly, but my reading feels much easier now.",
                CommunityCategory.Study, false, 71, H(6),
                new() {
                    ("ren@nihongoseed.community", "300 is still impressive!", false, TimeSpan.FromHours(1)),
                    ("yuki@nihongoseed.community", "How long did you study per day?", false, TimeSpan.FromHours(2)),
                    ("miku@nihongoseed.community", "Do you have a list?", false, TimeSpan.FromHours(3)),
                }
            ));

            list.Add(new PostSeed(
                "yuki@nihongoseed.community",
                "N5 grammar checklist that I'm using",
                "My current focus: particles, adjective conjugation, 〜たい, 〜てください and basic verb forms. Slowly getting there!",
                CommunityCategory.Study, false, 32, D(1),
                new() {
                    ("kenta@nihongoseed.community", "Saving this 👀", false, TimeSpan.FromHours(3)),
                    ("mina@nihongoseed.community", "Don't forget 〜てもいいです!", false, TimeSpan.FromHours(5)),
                }
            ));

            list.Add(new PostSeed(
                "hana@nihongoseed.community",
                "Shadowing actually helped my speaking",
                "I started repeating short audio clips immediately after the speaker. At first I sounded terrible, but after a week my pronunciation felt smoother.",
                CommunityCategory.Study, false, 49, D(1.5),
                new() {
                    ("sora@nihongoseed.community", "What audio do you use?", false, TimeSpan.FromHours(2)),
                    ("hana@nihongoseed.community", "Mostly beginner podcasts.", true, TimeSpan.FromHours(3)),
                    ("aoi@nihongoseed.community", "I keep giving up on shadowing... maybe I'll try again.", false, TimeSpan.FromHours(8)),
                }
            ));

            list.Add(new PostSeed(
                "ren@nihongoseed.community",
                "Reading manga without translating every sentence",
                "I'm trying to understand from context first and only look up words that keep appearing. It makes reading much less tiring.",
                CommunityCategory.Study, false, 37, D(2),
                new() {
                    ("aoi@nihongoseed.community", "I need to stop checking every word 😭", false, TimeSpan.FromHours(4)),
                    ("ren@nihongoseed.community", "Same, that was my biggest problem.", true, TimeSpan.FromHours(5)),
                }
            ));

            list.Add(new PostSeed(
                "aoi@nihongoseed.community",
                "My current daily routine",
                "10 min flashcards → 15 min grammar → 10 min listening. On weekends I try to write a short diary entry.",
                CommunityCategory.Study, false, 29, D(2.5),
                new() {
                    ("miku@nihongoseed.community", "This actually looks manageable.", false, TimeSpan.FromHours(2)),
                    ("haru@nihongoseed.community", "Stealing this routine 😂", false, TimeSpan.FromHours(8)),
                }
            ));

            list.Add(new PostSeed(
                "sora@nihongoseed.community",
                "今日の日記 🌸",
                "今日は大学で日本語を勉強しました。新しい単語を十個覚えました。でも、文法はまだ難しいです。",
                CommunityCategory.Study, true, 25, H(4),
                new() {
                    ("neko@nihongoseed.community", "Very natural! You could also write 「10個」.", false, TimeSpan.FromHours(1)),
                    ("sora@nihongoseed.community", "ありがとうございます！", true, TimeSpan.FromHours(2)),
                    ("haru@nihongoseed.community", "明日も頑張ってください 👍", false, TimeSpan.FromHours(3)),
                }
            ));

            list.Add(new PostSeed(
                "tomo@nihongoseed.community",
                "Best free resources for N5 listening practice",
                "I've been using NHK Easy News, JapanesePod101 free episodes, and a few YouTube channels. What else is good for beginner listening?",
                CommunityCategory.Study, false, 41, H(2.5),
                new() {
                    ("neko@nihongoseed.community", "Comprehensible Japanese on YouTube is great!", false, TimeSpan.FromMinutes(30)),
                    ("yuki@nihongoseed.community", "Also try the Tobira beginner audio.", false, TimeSpan.FromHours(1)),
                    ("kenji@nihongoseed.community", "For anime, watch with JP subtitles.", false, TimeSpan.FromHours(2)),
                }
            ));

            list.Add(new PostSeed(
                "rika@nihongoseed.community",
                "How I passed N4 — my 6-month plan",
                "Month 1-2: Finish N5 grammar + 800 vocab. Month 3-4: N4 grammar + 1500 vocab + daily reading. Month 5-6: Past papers + listening drills. It's doable if you're consistent!",
                CommunityCategory.Study, false, 66, D(1),
                new() {
                    ("sora@nihongoseed.community", "This is super helpful, thank you!", false, TimeSpan.FromHours(3)),
                    ("mina@nihongoseed.community", "6 months is motivating. I was thinking it'd take years.", false, TimeSpan.FromHours(5)),
                    ("neko@nihongoseed.community", "Solid plan. Past papers are key.", false, TimeSpan.FromHours(7)),
                }
            ));

            list.Add(new PostSeed(
                "minoru@nihongoseed.community",
                "Writing kanji by hand vs typing",
                "Do you actually practice writing kanji by hand? I feel like I can recognise them but can't write most from memory. Should I bother learning to write?",
                CommunityCategory.Study, false, 33, D(3),
                new() {
                    ("neko@nihongoseed.community", "Recognition > production for most learners. Writing is nice but not essential unless you live in Japan.", false, TimeSpan.FromHours(4)),
                    ("hana@nihongoseed.community", "I practice writing the JLPT ones only.", false, TimeSpan.FromHours(6)),
                }
            ));

            list.Add(new PostSeed(
                "tomo@nihongoseed.community",
                "今日の日本語日記 ✏️",
                "週末に友達と一緒に日本の映画を見ました。映画は面白かったです。でも、全部はわかりませんでした。もっと勉強しなきゃ！",
                CommunityCategory.Study, true, 19, D(2),
                new() {
                    ("neko@nihongoseed.community", "Good effort! 「全部はわかりませんでした」→ 「全部はわかりませんでした」is fine, or you could say 「全部わからなかった」for casual style.", false, TimeSpan.FromHours(3)),
                    ("tomo@nihongoseed.community", "Thank you for the correction! 😊", true, TimeSpan.FromHours(4)),
                }
            ));

            // ============== 3. Questions (12 posts) ==============
            list.Add(new PostSeed(
                "miku@nihongoseed.community",
                "What's the difference between は and が?",
                "I understand the basic explanation but when I see actual sentences I still don't know which one to choose.",
                CommunityCategory.Questions, false, 18, T(20),
                new() {
                    ("neko@nihongoseed.community", "Think of は as introducing the topic, while が often identifies/emphasises the subject.", false, TimeSpan.FromMinutes(8)),
                    ("kenji@nihongoseed.community", "Context matters a lot too.", false, TimeSpan.FromMinutes(15)),
                    ("hana@nihongoseed.community", "は = topic, が = new info. That's how I remember it.", false, TimeSpan.FromMinutes(25)),
                }
            ));

            list.Add(new PostSeed(
                "haru@nihongoseed.community",
                "When should I use です vs ます?",
                "I know both are polite, but why do some sentences end with です and others with ます?",
                CommunityCategory.Questions, false, 11, T(35),
                new() {
                    ("yuki@nihongoseed.community", "です usually follows nouns/adjectives, while ます is a polite verb ending.", false, TimeSpan.FromMinutes(10)),
                    ("neko@nihongoseed.community", "Think: nouns → です, actions → ます.", false, TimeSpan.FromMinutes(18)),
                }
            ));

            list.Add(new PostSeed(
                "yume@nihongoseed.community",
                "Is かわいい only used for girls/things?",
                "Can you call a guy かわいい too or would that sound weird?",
                CommunityCategory.Questions, false, 20, H(1),
                new() {
                    ("aoi@nihongoseed.community", "You can! Depends on context though.", false, TimeSpan.FromMinutes(15)),
                    ("ren@nihongoseed.community", "Definitely used for guys too.", false, TimeSpan.FromMinutes(30)),
                    ("neko@nihongoseed.community", "Especially for younger guys or anime characters.", false, TimeSpan.FromMinutes(45)),
                }
            ));

            list.Add(new PostSeed(
                "kenta@nihongoseed.community",
                "Why is 今日 sometimes きょう and sometimes こんにち?",
                "I see both in subtitles and I'm not sure which one is right.",
                CommunityCategory.Questions, false, 13, H(2),
                new() {
                    ("neko@nihongoseed.community", "今日 is usually きょう. こんにち appears in expressions like こんにちは.", false, TimeSpan.FromMinutes(30)),
                    ("miku@nihongoseed.community", "Ahh that makes sense, thanks!", false, TimeSpan.FromMinutes(60)),
                }
            ));

            list.Add(new PostSeed(
                "sakura@nihongoseed.community",
                "Can someone check my sentence?",
                "「昨日、友達と映画を見に行きました。そしてラーメンを食べました。」Does this sound natural?",
                CommunityCategory.Questions, false, 17, H(7),
                new() {
                    ("hana@nihongoseed.community", "Yes! Very natural 👍", false, TimeSpan.FromHours(1)),
                    ("neko@nihongoseed.community", "You could omit そして too, but it's correct.", false, TimeSpan.FromHours(2)),
                }
            ));

            list.Add(new PostSeed(
                "ren@nihongoseed.community",
                "Difference between すみません and ごめんなさい?",
                "Are they interchangeable or are there situations where one sounds more natural?",
                CommunityCategory.Questions, false, 24, D(1),
                new() {
                    ("mina@nihongoseed.community", "すみません can also mean \"excuse me\".", false, TimeSpan.FromHours(3)),
                    ("yuki@nihongoseed.community", "ごめんなさい feels more directly apologetic.", false, TimeSpan.FromHours(4)),
                }
            ));

            list.Add(new PostSeed(
                "aoi@nihongoseed.community",
                "How do you pronounce つ properly? 😭",
                "Mine keeps sounding like \"su\". Any pronunciation tips?",
                CommunityCategory.Questions, false, 19, D(2),
                new() {
                    ("hana@nihongoseed.community", "Try starting with a tiny \"t\" before \"su\".", false, TimeSpan.FromHours(4)),
                    ("sora@nihongoseed.community", "Shadowing helped me with this.", false, TimeSpan.FromHours(8)),
                }
            ));

            list.Add(new PostSeed(
                "mina@nihongoseed.community",
                "Is 私 always necessary?",
                "Japanese sentences seem to leave out \"I\" all the time. How do people know who the sentence is about?",
                CommunityCategory.Questions, false, 27, D(3),
                new() {
                    ("neko@nihongoseed.community", "Context does most of the work. Japanese omits subjects very often when they're obvious.", false, TimeSpan.FromHours(5)),
                    ("yuki@nihongoseed.community", "Took me a while to stop adding 私 in every sentence.", false, TimeSpan.FromHours(8)),
                }
            ));

            list.Add(new PostSeed(
                "minoru@nihongoseed.community",
                "What's the difference between いる and ある?",
                "I know one is for living things and one for non-living, but are there exceptions or tricky cases?",
                CommunityCategory.Questions, false, 15, H(1.5),
                new() {
                    ("neko@nihongoseed.community", "いる = animate (people, animals). ある = inanimate (objects, concepts). Robots and AI can be tricky — usually いる in casual speech.", false, TimeSpan.FromMinutes(20)),
                    ("yuki@nihongoseed.community", "Plants use ある, not いる. That tripped me up.", false, TimeSpan.FromMinutes(45)),
                }
            ));

            list.Add(new PostSeed(
                "tomo@nihongoseed.community",
                "How do you know when to use Onyomi vs Kunyomi?",
                "Some kanji have like 5+ readings and I never know which one to use. Is there a pattern?",
                CommunityCategory.Questions, false, 22, D(1.5),
                new() {
                    ("neko@nihongoseed.community", "Rough rule: Onyomi in compound words (漢語), Kunyomi when the kanji stands alone or with okurigana.", false, TimeSpan.FromHours(2)),
                    ("ren@nihongoseed.community", "You just have to learn it word by word eventually.", false, TimeSpan.FromHours(4)),
                }
            ));

            list.Add(new PostSeed(
                "rika@nihongoseed.community",
                "Why does 日本 sometimes read as にほん and sometimes にっぽん?",
                "I hear both on TV and in lessons. Which one is correct?",
                CommunityCategory.Questions, false, 14, D(2),
                new() {
                    ("neko@nihongoseed.community", "Both are correct. にほん is more common in everyday speech. にっぽん sounds more formal/official.", false, TimeSpan.FromHours(3)),
                    ("sora@nihongoseed.community", "Sports events and official stuff tend to use にっぽん.", false, TimeSpan.FromHours(5)),
                }
            ));

            list.Add(new PostSeed(
                "hana@nihongoseed.community",
                "Can someone explain 〜てしまう?",
                "I keep seeing this pattern but the textbook explanation is confusing. What does it actually mean?",
                CommunityCategory.Questions, false, 23, D(3),
                new() {
                    ("neko@nihongoseed.community", "Two main uses: 1) Completion (did something completely), 2) Regret (did something unfortunately). Context tells you which.", false, TimeSpan.FromHours(4)),
                    ("yuki@nihongoseed.community", "食べてしまった = I ate it all (completion) or I accidentally ate it (regret).", false, TimeSpan.FromHours(6)),
                    ("aoi@nihongoseed.community", "Casual form 〜ちゃう is super common too!", false, TimeSpan.FromHours(8)),
                }
            ));

            // ============== 4. Japan (12 posts) ==============
            list.Add(new PostSeed(
                "sakura@nihongoseed.community",
                "If you could visit one place in Japan right now, where would you go? 🇯🇵",
                "I'd choose Kyoto in autumn. I really want to see the temples surrounded by red leaves 🍁",
                CommunityCategory.Japan, false, 55, T(30),
                new() {
                    ("haru@nihongoseed.community", "Hokkaido in winter! ❄️", false, TimeSpan.FromMinutes(10)),
                    ("miku@nihongoseed.community", "Osaka for food 😂", false, TimeSpan.FromMinutes(20)),
                    ("yume@nihongoseed.community", "Naoshima — I want to see the art island.", false, TimeSpan.FromMinutes(40)),
                }
            ));

            list.Add(new PostSeed(
                "haru@nihongoseed.community",
                "What Japanese food should everyone try at least once?",
                "Not counting sushi or ramen!",
                CommunityCategory.Japan, false, 42, H(1.5),
                new() {
                    ("yuki@nihongoseed.community", "Okonomiyaki!", false, TimeSpan.FromMinutes(20)),
                    ("aoi@nihongoseed.community", "Takoyaki 🐙", false, TimeSpan.FromMinutes(25)),
                    ("ren@nihongoseed.community", "Unagi!", false, TimeSpan.FromMinutes(45)),
                    ("hana@nihongoseed.community", "Tonkatsu if you want something filling.", false, TimeSpan.FromHours(1)),
                }
            ));

            list.Add(new PostSeed(
                "mina@nihongoseed.community",
                "Why do people bow so much in Japan?",
                "I know it's used for greetings, but does the angle really mean different things?",
                CommunityCategory.Japan, false, 31, H(3),
                new() {
                    ("neko@nihongoseed.community", "Yes, generally deeper bows show more respect or apology.", false, TimeSpan.FromMinutes(30)),
                    ("sora@nihongoseed.community", "There's even a 45-degree one for serious apologies.", false, TimeSpan.FromHours(1)),
                }
            ));

            list.Add(new PostSeed(
                "ren@nihongoseed.community",
                "Kansai-ben sounds so different 😂",
                "I heard someone say \"ほんま?\" instead of \"ほんとう?\" and now I want to learn more Kansai expressions.",
                CommunityCategory.Japan, false, 38, H(5),
                new() {
                    ("haru@nihongoseed.community", "おおきに is another famous one!", false, TimeSpan.FromHours(1)),
                    ("yume@nihongoseed.community", "Kansai-ben sounds so friendly.", false, TimeSpan.FromHours(2)),
                }
            ));

            list.Add(new PostSeed(
                "aoi@nihongoseed.community",
                "Japanese convenience stores look dangerous for my wallet",
                "Every video I see has so many snacks, desserts and seasonal drinks 😭 What should I try first?",
                CommunityCategory.Japan, false, 67, D(1),
                new() {
                    ("miku@nihongoseed.community", "Egg sandwiches!!", false, TimeSpan.FromHours(2)),
                    ("kenta@nihongoseed.community", "FamilyMart fried chicken 😂", false, TimeSpan.FromHours(3)),
                    ("sakura@nihongoseed.community", "Onigiri, the classic.", false, TimeSpan.FromHours(4)),
                }
            ));

            list.Add(new PostSeed(
                "yuki@nihongoseed.community",
                "Would you rather visit Tokyo or Osaka first?",
                "Tokyo seems more iconic, but Osaka looks more relaxed and food-focused.",
                CommunityCategory.Japan, false, 36, D(2),
                new() {
                    ("sakura@nihongoseed.community", "Osaka first!", false, TimeSpan.FromHours(3)),
                    ("ren@nihongoseed.community", "Tokyo if it's your first ever Japan trip.", false, TimeSpan.FromHours(4)),
                    ("aoi@nihongoseed.community", "Tokyo. Easier to navigate too.", false, TimeSpan.FromHours(8)),
                }
            ));

            list.Add(new PostSeed(
                "hana@nihongoseed.community",
                "What's something about Japanese etiquette foreigners often misunderstand?",
                "I've watched several videos and feel like I'm always missing something 😅",
                CommunityCategory.Japan, false, 45, D(2.5),
                new() {
                    ("neko@nihongoseed.community", "Talking loudly on trains is a big one.", false, TimeSpan.FromHours(3)),
                    ("mina@nihongoseed.community", "Also eating while walking depends on the situation/place.", false, TimeSpan.FromHours(5)),
                }
            ));

            list.Add(new PostSeed(
                "kenta@nihongoseed.community",
                "Which Japanese festival would you most like to experience? 🎆",
                "I really want to see a summer matsuri with fireworks and food stalls.",
                CommunityCategory.Japan, false, 48, D(3),
                new() {
                    ("aoi@nihongoseed.community", "Tanabata! 🎋", false, TimeSpan.FromHours(3)),
                    ("haru@nihongoseed.community", "Sapporo Snow Festival for me.", false, TimeSpan.FromHours(8)),
                    ("yume@nihongoseed.community", "Gion Matsuri in Kyoto 🌸", false, TimeSpan.FromHours(12)),
                }
            ));

            list.Add(new PostSeed(
                "minoru@nihongoseed.community",
                "Japanese train culture is fascinating 🚃",
                "The punctuality, the silence, the way everyone queues. I watched a video about the station attendants who push people into trains. Japan really runs on precision!",
                CommunityCategory.Japan, false, 52, H(6),
                new() {
                    ("tomo@nihongoseed.community", "The pushers (押し屋) are wild 😂", false, TimeSpan.FromHours(1)),
                    ("sora@nihongoseed.community", "Don't forget the jingles at each station — every line has its own melody!", false, TimeSpan.FromHours(3)),
                    ("rika@nihongoseed.community", "The Suica/Pasmo cards are so convenient too.", false, TimeSpan.FromHours(5)),
                }
            ));

            list.Add(new PostSeed(
                "tomo@nihongoseed.community",
                "Japanese vending machines are next level 🥤",
                "Hot and cold drinks, soup, ramen, ice cream — all from a machine! Why doesn't every country have these?",
                CommunityCategory.Japan, false, 44, D(1),
                new() {
                    ("kenta@nihongoseed.community", "The hot canned coffee in winter is life-changing.", false, TimeSpan.FromHours(2)),
                    ("aoi@nihongoseed.community", "I saw one that sells bananas! 🍌", false, TimeSpan.FromHours(4)),
                    ("yume@nihongoseed.community", "Apparently there are over 5 million vending machines in Japan.", false, TimeSpan.FromHours(6)),
                }
            ));

            list.Add(new PostSeed(
                "rika@nihongoseed.community",
                "Best Japanese snacks to try? 🍬",
                "I'm making a list for when I visit. So far I have Pocky, KitKats (matcha flavor), and mochi. What else?",
                CommunityCategory.Japan, false, 40, D(2),
                new() {
                    ("sakura@nihongoseed.community", "Hi-Chew! So many flavors.", false, TimeSpan.FromHours(2)),
                    ("haru@nihongoseed.community", "Taiyaki if you want something warm.", false, TimeSpan.FromHours(4)),
                    ("minoru@nihongoseed.community", "Don't sleep on senbei (rice crackers).", false, TimeSpan.FromHours(6)),
                }
            ));

            list.Add(new PostSeed(
                "minoru@nihongoseed.community",
                "Japanese public baths (銭湯) — would you try one?",
                "The idea of being naked with strangers terrifies me but everyone says it's amazing and very relaxing. Has anyone here actually been?",
                CommunityCategory.Japan, false, 30, D(3),
                new() {
                    ("neko@nihongoseed.community", "It's amazing once you get past the initial awkwardness. The hot baths are incredible.", false, TimeSpan.FromHours(4)),
                    ("rika@nihongoseed.community", "I tried an onsen in Hakone. Life-changing!", false, TimeSpan.FromHours(6)),
                    ("sora@nihongoseed.community", "Just remember: no tattoos in most public baths.", false, TimeSpan.FromHours(8)),
                }
            ));

            return list;
        }
    }
}
