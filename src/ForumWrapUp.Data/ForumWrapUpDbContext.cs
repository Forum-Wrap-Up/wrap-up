using ForumWrapUp.Data.Entities;
using ForumWrapUp.Data.Entities.Raw;
using Microsoft.EntityFrameworkCore;

namespace ForumWrapUp.Data;

public sealed class ForumWrapUpDbContext : DbContext
{
    public ForumWrapUpDbContext(DbContextOptions<ForumWrapUpDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public DbSet<RegistrationAttempt> RegistrationAttempts => Set<RegistrationAttempt>();

    public DbSet<ForumInstance> ForumInstances => Set<ForumInstance>();

    public DbSet<ForumSection> ForumSections => Set<ForumSection>();

    public DbSet<ForumTopic> ForumTopics => Set<ForumTopic>();

    public DbSet<ForumPost> ForumPosts => Set<ForumPost>();

    public DbSet<ImportOperation> ImportOperations => Set<ImportOperation>();

    public DbSet<WrapUpYear> WrapUpYears => Set<WrapUpYear>();

    public DbSet<AnnualWrapUp> AnnualWrapUps => Set<AnnualWrapUp>();

    public DbSet<DailyPostStatistic> DailyPostStatistics => Set<DailyPostStatistic>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("user_accounts");
            entity.HasKey(account => account.Mid);
            entity.Property(account => account.Mid).ValueGeneratedNever();
            entity.Property(account => account.Nickname).HasMaxLength(100);
            entity.Property(account => account.AvatarUrl).HasMaxLength(2048);
            entity.Property(account => account.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(account => account.CreatedAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(account => account.ForumRegisteredAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(account => account.ProfileVerifiedAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(account => account.LastLoginAtUtc).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<RegistrationAttempt>(entity =>
        {
            entity.ToTable("registration_attempts");
            entity.HasKey(attempt => attempt.Id);
            entity.Property(attempt => attempt.VerificationCode).HasMaxLength(36).IsRequired();
            entity.Property(attempt => attempt.CreatedAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(attempt => attempt.ExpiresAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(attempt => attempt.ProfileVerifiedAtUtc).HasColumnType("timestamp with time zone");
            entity.HasIndex(attempt => attempt.Mid).IsUnique();
            entity.HasIndex(attempt => attempt.ExpiresAtUtc);
        });

        modelBuilder.Entity<ForumInstance>(entity =>
        {
            entity.ToTable("forum_instances", "raw");
            entity.HasKey(forum => forum.ApiForumId);
            entity.Property(forum => forum.ApiForumId).ValueGeneratedNever();
        });

        modelBuilder.Entity<ForumSection>(entity =>
        {
            entity.ToTable("forum_sections", "raw");
            entity.HasKey(section => section.Id);
            entity.Property(section => section.Name).HasMaxLength(500);
            entity.HasIndex(section => new { section.ApiForumId, section.ExternalId }).IsUnique();
            entity.HasOne(section => section.ForumInstance)
                .WithMany(forum => forum.Sections)
                .HasForeignKey(section => section.ApiForumId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ForumTopic>(entity =>
        {
            entity.ToTable("forum_topics", "raw");
            entity.HasKey(topic => topic.Id);
            entity.Property(topic => topic.TitleHtml).HasColumnType("text");
            entity.HasIndex(topic => new { topic.ApiForumId, topic.ExternalId }).IsUnique();
            entity.HasOne(topic => topic.ForumInstance)
                .WithMany(forum => forum.Topics)
                .HasForeignKey(topic => topic.ApiForumId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ForumPost>(entity =>
        {
            entity.ToTable("forum_posts", "raw");
            entity.HasKey(post => post.Id);
            entity.Property(post => post.PublishedAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(post => post.RawPoints).HasMaxLength(64);
            entity.Property(post => post.ContentHtml).HasColumnType("text");
            entity.HasIndex(post => new { post.ApiForumId, post.ExternalId }).IsUnique();
            entity.HasIndex(post => new { post.AuthorMid, post.PublishedAtUtc });
            entity.HasIndex(post => post.TopicId);
            entity.HasIndex(post => post.SectionId);
            entity.HasOne(post => post.ForumInstance)
                .WithMany(forum => forum.Posts)
                .HasForeignKey(post => post.ApiForumId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(post => post.Author)
                .WithMany(account => account.Posts)
                .HasForeignKey(post => post.AuthorMid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(post => post.Topic)
                .WithMany(topic => topic.Posts)
                .HasForeignKey(post => post.TopicId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(post => post.Section)
                .WithMany(section => section.Posts)
                .HasForeignKey(post => post.SectionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ImportOperation>(entity =>
        {
            entity.ToTable("import_operations", table =>
                table.HasCheckConstraint(
                    "ck_import_operations_date_range",
                    "\"FromDate\" <= \"ToDate\""));
            entity.HasKey(operation => operation.Id);
            entity.Property(operation => operation.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(operation => operation.RequestedAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(operation => operation.StartedAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(operation => operation.CompletedAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(operation => operation.ErrorMessage).HasMaxLength(2000);
            entity.HasIndex(operation => new { operation.Mid, operation.RequestedAtUtc });
            entity.HasOne(operation => operation.User)
                .WithMany(account => account.ImportOperations)
                .HasForeignKey(operation => operation.Mid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WrapUpYear>(entity =>
        {
            entity.ToTable("wrap_up_years");
            entity.HasKey(year => year.Year);
            entity.Property(year => year.Year).ValueGeneratedNever();
            entity.Property(year => year.CreatedAtUtc).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<AnnualWrapUp>(entity =>
        {
            entity.ToTable("annual_wrap_ups");
            entity.HasKey(summary => new { summary.Mid, summary.Year });
            entity.Property(summary => summary.TopTopicTitleHtml).HasColumnType("text");
            entity.Property(summary => summary.MostSuccessfulPostContentHtml).HasColumnType("text");
            entity.Property(summary => summary.MostSuccessfulPostDateUtc).HasColumnType("timestamp with time zone");
            entity.Property(summary => summary.CalculatedAtUtc).HasColumnType("timestamp with time zone");
            entity.HasOne(summary => summary.User)
                .WithMany(account => account.AnnualWrapUps)
                .HasForeignKey(summary => summary.Mid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(summary => summary.WrapUpYear)
                .WithMany(year => year.AnnualWrapUps)
                .HasForeignKey(summary => summary.Year)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DailyPostStatistic>(entity =>
        {
            entity.ToTable("daily_post_statistics");
            entity.HasKey(statistic => new { statistic.Mid, statistic.Year, statistic.Day });
            entity.Property(statistic => statistic.Day).HasColumnType("date");
            entity.HasOne(statistic => statistic.AnnualWrapUp)
                .WithMany(summary => summary.DailyPostStatistics)
                .HasForeignKey(statistic => new { statistic.Mid, statistic.Year })
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
