using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Models;

namespace NihongoAcademy.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Vocabulary> Vocabulary { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<LessonProgress> LessonProgress { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<FlashcardProgress> FlashcardProgress { get; set; }
        public DbSet<LessonVocabulary> LessonVocabulary { get; set; }
        public DbSet<PracticeAttempt> PracticeAttempts { get; set; }
        public DbSet<LessonPrerequisite> LessonPrerequisites { get; set; }
        public DbSet<LessonActivity> LessonActivities { get; set; }
        public DbSet<GrammarPoint> GrammarPoints { get; set; }
        public DbSet<LessonGrammarPoint> LessonGrammarPoints { get; set; }
        public DbSet<ReadingPassage> ReadingPassages { get; set; }
        public DbSet<ReadingQuestion> ReadingQuestions { get; set; }
        public DbSet<ReadingPassageVocabulary> ReadingPassageVocabulary { get; set; }
        public DbSet<ReadingAttempt> ReadingAttempts { get; set; }
        public DbSet<WritingTask> WritingTasks { get; set; }
        public DbSet<WritingSubmission> WritingSubmissions { get; set; }
        public DbSet<SpeakingTask> SpeakingTasks { get; set; }
        public DbSet<SpeakingAttempt> SpeakingAttempts { get; set; }
        public DbSet<ListeningTask> ListeningTasks { get; set; }
        public DbSet<ListeningQuestion> ListeningQuestions { get; set; }
        public DbSet<ListeningAttempt> ListeningAttempts { get; set; }
        public DbSet<CommunityPost> CommunityPosts { get; set; }
        public DbSet<CommunityComment> CommunityComments { get; set; }
        public DbSet<CommunityLike> CommunityLikes { get; set; }
        public DbSet<CommunityBookmark> CommunityBookmarks { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<UserXpLog> UserXpLogs { get; set; }
        public DbSet<UserRelationship> UserRelationships { get; set; }
        public DbSet<DirectMessage> DirectMessages { get; set; }

        // Moderation & audit
        public DbSet<ContentReport> ContentReports { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }
        public DbSet<OperationLog> OperationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            });

            builder.Entity<Lesson>(entity =>
            {
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Lessons)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CategoryId);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DifficultyLevel).HasDefaultValue(1);
                entity.Property(e => e.IsPublished).HasDefaultValue(true);
            });

            builder.Entity<Vocabulary>(entity =>
            {
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Vocabulary)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CategoryId);
                entity.Property(e => e.Japanese).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Romaji).IsRequired().HasMaxLength(50);
                entity.Property(e => e.English).IsRequired().HasMaxLength(100);
            });

            builder.Entity<Quiz>(entity =>
            {
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Quizzes)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CategoryId);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            });

            builder.Entity<QuizQuestion>(entity =>
            {
                entity.HasOne(e => e.Quiz)
                    .WithMany(q => q.Questions)
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.QuizId);
                entity.Property(e => e.QuestionText).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CorrectOption).IsRequired();
            });

            builder.Entity<LessonProgress>(entity =>
            {
                entity.HasOne(e => e.Lesson)
                    .WithMany(l => l.ProgressRecords)
                    .HasForeignKey(e => e.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.LessonId }).IsUnique();
            });

            builder.Entity<QuizAttempt>(entity =>
            {
                entity.HasOne(e => e.Quiz)
                    .WithMany(q => q.Attempts)
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.QuizId);
            });

            builder.Entity<UserProfile>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.AvatarJson).HasMaxLength(4000);
                entity.Property(e => e.AvatarMode).HasMaxLength(20).HasDefaultValue("generated");
                entity.Property(e => e.AvatarImagePath).HasMaxLength(256);
                entity.Property(e => e.DisplayName).HasMaxLength(50);
                entity.Property(e => e.Bio).HasMaxLength(240);
                entity.Property(e => e.NativeLanguage).HasMaxLength(30);
                entity.Property(e => e.Country).HasMaxLength(50);
                entity.Property(e => e.ThemePreference).HasMaxLength(10);
                entity.Property(e => e.LearningGoal).HasMaxLength(200);
                entity.Property(e => e.CurrentLevel).HasMaxLength(50);
                entity.Property(e => e.AgeGroup).HasMaxLength(30);
            });

            builder.Entity<FlashcardProgress>(entity =>
            {
                entity.HasOne(e => e.Vocabulary)
                    .WithMany()
                    .HasForeignKey(e => e.VocabularyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.VocabularyId }).IsUnique();
                entity.HasIndex(e => e.NextReview);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            });

            builder.Entity<LessonVocabulary>(entity =>
            {
                entity.HasKey(e => new { e.LessonId, e.VocabularyId });
                entity.HasOne(e => e.Lesson).WithMany(l => l.LessonVocabulary)
                    .HasForeignKey(e => e.LessonId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Vocabulary).WithMany(v => v.LessonVocabulary)
                    .HasForeignKey(e => e.VocabularyId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<PracticeAttempt>(entity =>
            {
                entity.HasOne(e => e.Lesson).WithMany().HasForeignKey(e => e.LessonId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Vocabulary).WithMany().HasForeignKey(e => e.VocabularyId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.LessonActivity).WithMany().HasForeignKey(e => e.LessonActivityId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.GrammarPoint).WithMany().HasForeignKey(e => e.GrammarPointId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.UserId, e.LessonId, e.ExerciseType });
                entity.HasIndex(e => new { e.UserId, e.GrammarPointId, e.ExerciseType });
            });

            builder.Entity<LessonActivity>(entity =>
            {
                entity.HasOne(e => e.Lesson).WithMany(l => l.Activities)
                    .HasForeignKey(e => e.LessonId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.LessonId, e.DisplayOrder });
                entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Prompt).IsRequired().HasMaxLength(500);
                entity.Property(e => e.SlowAudioUrl).HasMaxLength(300);
                entity.Property(e => e.NaturalAudioUrl).HasMaxLength(300);
            });

            builder.Entity<GrammarPoint>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Code).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Pattern).IsRequired().HasMaxLength(120);
                entity.Property(e => e.Level).IsRequired().HasMaxLength(10);
            });

            builder.Entity<LessonGrammarPoint>(entity =>
            {
                entity.HasKey(e => new { e.LessonId, e.GrammarPointId });
                entity.HasOne(e => e.Lesson).WithMany(l => l.LessonGrammarPoints)
                    .HasForeignKey(e => e.LessonId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.GrammarPoint).WithMany(g => g.Lessons)
                    .HasForeignKey(e => e.GrammarPointId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.LessonId, e.DisplayOrder });
            });

            builder.Entity<ReadingPassage>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Code).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Level).IsRequired().HasMaxLength(10);
                entity.Property(e => e.TextType).IsRequired().HasMaxLength(30);
                entity.Property(e => e.JapaneseText).IsRequired();
                entity.HasOne(e => e.Lesson).WithMany().HasForeignKey(e => e.LessonId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.GrammarPoint).WithMany().HasForeignKey(e => e.GrammarPointId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<ReadingQuestion>(entity =>
            {
                entity.HasOne(e => e.ReadingPassage).WithMany(p => p.Questions)
                    .HasForeignKey(e => e.ReadingPassageId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.ReadingPassageId, e.DisplayOrder });
            });

            builder.Entity<ReadingPassageVocabulary>(entity =>
            {
                entity.HasKey(e => new { e.ReadingPassageId, e.VocabularyId });
                entity.HasOne(e => e.ReadingPassage).WithMany(p => p.Vocabulary)
                    .HasForeignKey(e => e.ReadingPassageId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Vocabulary).WithMany()
                    .HasForeignKey(e => e.VocabularyId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ReadingAttempt>(entity =>
            {
                entity.HasOne(e => e.ReadingPassage).WithMany().HasForeignKey(e => e.ReadingPassageId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ReadingQuestion).WithMany().HasForeignKey(e => e.ReadingQuestionId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.UserId, e.ReadingPassageId, e.AttemptedAt });
            });

            builder.Entity<WritingTask>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Code).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
                entity.HasOne(e => e.Lesson).WithMany().HasForeignKey(e => e.LessonId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.GrammarPoint).WithMany().HasForeignKey(e => e.GrammarPointId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<WritingSubmission>(entity =>
            {
                entity.HasOne(e => e.WritingTask).WithMany(t => t.Submissions).HasForeignKey(e => e.WritingTaskId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.PreviousSubmission).WithMany().HasForeignKey(e => e.PreviousSubmissionId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.UserId, e.WritingTaskId, e.SubmittedAt });
            });

            builder.Entity<SpeakingTask>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Code).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
                entity.HasOne(e => e.Lesson).WithMany().HasForeignKey(e => e.LessonId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<SpeakingAttempt>(entity =>
            {
                entity.HasOne(e => e.SpeakingTask).WithMany(t => t.Attempts).HasForeignKey(e => e.SpeakingTaskId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.UserId, e.SpeakingTaskId, e.AttemptedAt });
            });

            builder.Entity<ListeningTask>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Code).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
                entity.HasOne(e => e.Lesson).WithMany().HasForeignKey(e => e.LessonId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<ListeningQuestion>(entity =>
            {
                entity.HasOne(e => e.ListeningTask).WithMany(t => t.Questions).HasForeignKey(e => e.ListeningTaskId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.ListeningTaskId, e.DisplayOrder });
            });

            builder.Entity<ListeningAttempt>(entity =>
            {
                entity.HasOne(e => e.ListeningTask).WithMany().HasForeignKey(e => e.ListeningTaskId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ListeningQuestion).WithMany().HasForeignKey(e => e.ListeningQuestionId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.UserId, e.ListeningTaskId, e.AttemptedAt });
            });

            builder.Entity<LessonPrerequisite>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_LessonPrerequisite_NotSelf", "[LessonId] <> [PrerequisiteLessonId]"));
                entity.HasKey(e => new { e.LessonId, e.PrerequisiteLessonId });
                entity.HasOne(e => e.Lesson)
                    .WithMany(l => l.Prerequisites)
                    .HasForeignKey(e => e.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.PrerequisiteLesson)
                    .WithMany(l => l.RequiredByLessons)
                    .HasForeignKey(e => e.PrerequisiteLessonId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CommunityPost>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.CreatedDate);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);

                // Moderation: when a report is approved the post is taken down for rectification.
                entity.Property(e => e.IsTakenDown).HasDefaultValue(false);
                entity.HasIndex(e => e.IsTakenDown);
                entity.Property(e => e.TakenDownByUserId).HasMaxLength(450);
                entity.Property(e => e.TakenDownReason).HasMaxLength(500);
            });

            builder.Entity<CommunityComment>(entity =>
            {
                entity.HasOne(e => e.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(e => e.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.PostId);
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);

                // Moderation: when a report is approved the comment is taken down for rectification.
                entity.Property(e => e.IsTakenDown).HasDefaultValue(false);
                entity.HasIndex(e => e.IsTakenDown);
                entity.Property(e => e.TakenDownByUserId).HasMaxLength(450);
                entity.Property(e => e.TakenDownReason).HasMaxLength(500);
            });

            builder.Entity<CommunityLike>(entity =>
            {
                entity.HasOne(e => e.Post)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.UserId, e.PostId }).IsUnique();
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            });

            builder.Entity<CommunityBookmark>(entity =>
            {
                entity.HasOne(e => e.Post)
                    .WithMany()
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.UserId, e.PostId }).IsUnique();
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            });

            builder.Entity<UserXpLog>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.EarnedAt);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
            });

            builder.Entity<UserRelationship>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.TargetUserId }).IsUnique();
                entity.HasIndex(e => e.TargetUserId);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.TargetUserId).IsRequired().HasMaxLength(450);
            });

            builder.Entity<DirectMessage>(entity =>
            {
                entity.HasIndex(e => new { e.SenderUserId, e.ReceiverUserId });
                entity.HasIndex(e => new { e.ReceiverUserId, e.IsRead });
                entity.Property(e => e.SenderUserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ReceiverUserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
            });

            builder.Entity<ContentReport>(entity =>
            {
                entity.HasOne(e => e.ReporterUser)
                    .WithMany()
                    .HasForeignKey(e => e.ReporterUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.TargetType);
                entity.HasIndex(e => e.CreatedDate);
                entity.Property(e => e.ReporterUserId).IsRequired().HasMaxLength(450);
            });

            builder.Entity<LoginLog>(entity =>
            {
                entity.HasIndex(e => e.LoginTime);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Succeeded);
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.Email).HasMaxLength(256);
            });

            builder.Entity<OperationLog>(entity =>
            {
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Action);
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.UserEmail).HasMaxLength(256);
            });
        }
    }
}
