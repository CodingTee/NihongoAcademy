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
        public DbSet<CommunityPost> CommunityPosts { get; set; }
        public DbSet<CommunityComment> CommunityComments { get; set; }
        public DbSet<CommunityLike> CommunityLikes { get; set; }

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
                entity.HasIndex(e => new { e.UserId, e.LessonId, e.ExerciseType });
            });

            builder.Entity<LessonActivity>(entity =>
            {
                entity.HasOne(e => e.Lesson).WithMany(l => l.Activities)
                    .HasForeignKey(e => e.LessonId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.LessonId, e.DisplayOrder });
                entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Prompt).IsRequired().HasMaxLength(500);
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
        }
    }
}
