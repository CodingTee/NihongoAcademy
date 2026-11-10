# NihongoAcademy

A Japanese learning web application built with ASP.NET Core 8 MVC. It covers a structured
lesson curriculum, spaced-repetition flashcards, skill practice, quizzes, progress tracking,
a community area and an admin workbench for the content behind all of it.

No external service, API key or mail server is needed to run it.

## Features

### Learning

- Lessons organised into eight categories: Hiragana, Katakana, Vocabulary, Grammar, Kanji,
  Culture, A1 Course and A2 Course. Vocabulary, grammar points, reading passages,
  listening, speaking and writing activities are attached where relevant.
- Mastery is gated on evidence, not on a checkbox. `LessonMasteryPolicy` requires
  `max(5, vocabulary count)` attempts per core exercise type, full vocabulary coverage,
  at least 80% accuracy and all activities completed before a lesson passes. Production
  exercises use a different rule: at least one attempt with 100% accuracy.
  `IntegratedSkillMasteryPolicy` adds a requirement for correct answers on two different days.
- SM-2 spaced repetition for flashcards (`FlashcardsController`): due cards first, per-card
  ease factor, repetition count and interval.
- Free-text answers are normalised before comparison (`AnswerEvaluationPolicy`), so
  whitespace, Japanese punctuation and casing do not change the result.
- Quizzes with per-question attempts, passing scores, history and category assessments.

### Progress and community

- Dashboard with five-skill bars, streaks, JLPT milestones, achievements, XP and a weekly
  leaderboard.
- Community posts, comments, likes, bookmarks, direct messages, notifications, content
  reports and a moderation queue.
- Explore Japan: region and culture pages with quizzes and background audio.

### Administration

- `AdminContentController` is a two-page surface (list and editor). The `type` and `mode`
  parameters select lessons, vocabulary, quizzes or categories, while reusing the existing
  CRUD views and POST handlers.
- Roles: SuperAdmin, Admin, Student. Login and operation audit logs, account management,
  announcements, audio coverage checks and progress testing tools.

## Stack

| Layer | Choice |
| --- | --- |
| Framework | ASP.NET Core 8 MVC, C# 12, Razor Views, one ViewComponent |
| Data | EF Core 8 Code-First, SQL Server / LocalDB, 52 migrations, 37 DbSets, Fluent API plus data annotations |
| Auth | ASP.NET Core Identity with roles, security-stamp validation on every request |
| Frontend | Bootstrap 5, vanilla JS, hand-written CSS. No jQuery, chart library or icon font |
| Tests | xUnit unit tests, Playwright and NUnit smoke tests |
| NuGet | 4 first-party Microsoft packages only |

The web project runs on those 4 packages (the two test projects add their own). The
dashboard charts are plain CSS bars, and the `bi-*` icon classes are mapped to emoji in
`wwwroot/css/bi-emoji.css` instead of shipping an icon font.

## Getting started

Prerequisites: .NET 8 SDK and SQL Server Express LocalDB (Windows). Any SQL Server instance
works if you override the connection string.

After cloning, run from the project root:

```bash
dotnet restore
dotnet user-secrets set "AdminSeed:AdminPassword" "YourStrongPw1!"
dotnet run --launch-profile https
```

Open `https://localhost:7271`, or `http://localhost:5023` with the `http` launch profile.
Startup applies pending migrations automatically, creates the three roles and seeds the full
curriculum when the database is empty. The first run is slower because of that seed.

Development seed accounts, created only when `ASPNETCORE_ENVIRONMENT=Development`:

| Role | Email |
| --- | --- |
| SuperAdmin | `admin@nihongoacademy.com` |
| Student | `student@nihongoacademy.com` |
| Student with sample progress | `demo@nihongoacademy.com` |

Passwords come from `AdminSeed:AdminPassword`, `AdminSeed:StudentPassword` and
`AdminSeed:DemoPassword`. If one is unset, a random password is generated and printed once
to the console as a `[DataSeeder] WARNING:` line. No password is stored in the repository.

To point at another SQL Server, change `ConnectionStrings:DefaultConnection` in
`appsettings.Development.json`, or set the environment variable
`ConnectionStrings__DefaultConnection`.

## Configuration

| Key | Default | Purpose |
| --- | --- | --- |
| `DatabaseStartup:ApplyMigrations` | `true` | Apply pending EF migrations at startup |
| `DatabaseStartup:SeedOnStartup` | `false` | Force a curriculum reseed |
| `DatabaseStartup:ValidateCurriculumOnStartup` | `false` | Validate curriculum integrity on seed |
| `DatabaseStartup:SeedCommunityActivity` | `false` | Seed sample community activity |
| `Identity:RequireConfirmedAccount` | `false` | Require confirmed email before sign-in |
| `App:TimeZoneId` | `Singapore Standard Time` | Time zone for streaks and daily reviews |
| `AdminSeed:*` | empty | Passwords for the three dev accounts |

## Project layout

| Path | Contents |
| --- | --- |
| `Controllers/` | 21 MVC controllers for learning, community and admin areas |
| `Services/` | 16 services: mastery policies, XP, achievements, caching, clock, audio |
| `Data/` | `ApplicationDbContext`, `DataSeeder`, 52 EF migrations |
| `Models/` | 41 domain and view models |
| `Views/` | 99 Razor views, plus the Identity pages under `Areas/Identity/` |
| `wwwroot/` | CSS, vanilla JS, Bootstrap, fonts, images and audio assets |
| `NihongoAcademy.Tests/` | xUnit tests (5 tests, needs LocalDB) |
| `NihongoAcademy.UITests/` | Playwright smoke tests |
| `scripts/MasteryPolicyChecks/` | Small console project for checking mastery policy rules |

## Tests

```bash
dotnet test NihongoAcademy.Tests/NihongoAcademy.Tests.csproj
APP_BASE_URL=https://localhost:7271 dotnet test NihongoAcademy.UITests/NihongoAcademy.UITests.csproj
```

The UI suite needs a running instance and a Chromium build. Set `CHROMIUM_PATH` to an
existing `chrome.exe` to skip the Playwright browser download.

## Engineering notes

- Static assets are served with Brotli or Gzip and a 7-day cache header, paired with
  `asp-append-version` on every reference so edits appear immediately.
- Public catalogue data and navbar summaries use `IMemoryCache` with a 60-second TTL.
  Read-only queries use `AsNoTracking()`, and the lesson, practice and navbar list queries
  were rewritten to single round trips instead of N+1.
- The 5.2 MB background track uses `preload="none"`, so it loads only when playback starts.
- Security answers are stored as SHA-256 hashes. State-changing endpoints use antiforgery
  tokens, admin areas are role-gated, and the security stamp is revalidated on every request
  so a role change or password reset ends existing sessions immediately.
- Email delivery was removed. A no-op `IEmailSender` keeps the scaffolded Identity pages
  working, and `RequireConfirmedAccount` stays off because no mail server is registered.

## Known limitations

- The default connection string targets LocalDB, which is Windows only. On Linux or macOS,
  supply a SQL Server connection string.
- `.config/dotnet-tools.json` pins `dotnet-ef` 10.0.11 while the app targets EF Core 8.0.29.
  Startup migrations work; install a matching 8.x tool when authoring new migrations.
- Email confirmation and mail-based password reset are unavailable by design.
- The UI tests need a browser installed and a server already running.
