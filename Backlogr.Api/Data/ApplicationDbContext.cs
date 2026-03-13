using Backlogr.Api.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Data;

public sealed class ApplicationDbContext
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Game> Games => Set<Game>();

    public DbSet<GameLog> GameLogs => Set<GameLog>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<ReviewLike> ReviewLikes => Set<ReviewLike>();

    public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();

    public DbSet<Follow> Follows => Set<Follow>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.DisplayName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.FirstName)
                .HasMaxLength(100);

            entity.Property(u => u.LastName)
                .HasMaxLength(100);

            entity.Property(u => u.AvatarUrl)
                .HasMaxLength(500);

            entity.Property(u => u.Bio)
                .HasMaxLength(1000);
        });

        builder.Entity<Game>(entity =>
        {
            entity.HasKey(g => g.GameId);

            entity.Property(g => g.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(g => g.Slug)
                .HasMaxLength(200);

            entity.Property(g => g.Summary)
                .HasMaxLength(4000);

            entity.Property(g => g.CoverImageUrl)
                .HasMaxLength(500);

            entity.Property(g => g.Developer)
                .HasMaxLength(200);

            entity.Property(g => g.Publisher)
                .HasMaxLength(200);

            entity.Property(g => g.Platforms)
                .HasMaxLength(1000);

            entity.Property(g => g.Genres)
                .HasMaxLength(1000);

            entity.HasIndex(g => g.IgdbId)
                .IsUnique()
                .HasFilter("[IgdbId] IS NOT NULL");
        });

        builder.Entity<GameLog>(entity =>
        {
            entity.HasKey(gl => gl.GameLogId);

            entity.Property(gl => gl.Rating)
                .HasPrecision(3, 1);

            entity.Property(gl => gl.Platform)
                .HasMaxLength(100);

            entity.Property(gl => gl.Hours)
                .HasPrecision(6, 1);

            entity.Property(gl => gl.Notes)
                .HasMaxLength(2000);

            entity.HasIndex(gl => new { gl.UserId, gl.GameId })
                .IsUnique();

            entity.HasOne(gl => gl.User)
                .WithMany(u => u.GameLogs)
                .HasForeignKey(gl => gl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gl => gl.Game)
                .WithMany(g => g.GameLogs)
                .HasForeignKey(gl => gl.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.ReviewId);

            entity.Property(r => r.Text)
                .HasMaxLength(4000)
                .IsRequired();

            entity.HasIndex(r => new { r.UserId, r.GameId })
                .IsUnique();

            entity.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Game)
                .WithMany(g => g.Reviews)
                .HasForeignKey(r => r.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ReviewLike>(entity =>
        {
            entity.HasKey(rl => rl.ReviewLikeId);

            entity.HasIndex(rl => new { rl.UserId, rl.ReviewId })
                .IsUnique();

            entity.HasOne(rl => rl.User)
                .WithMany(u => u.ReviewLikes)
                .HasForeignKey(rl => rl.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(rl => rl.Review)
                .WithMany(r => r.ReviewLikes)
                .HasForeignKey(rl => rl.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ReviewComment>(entity =>
        {
            entity.HasKey(rc => rc.ReviewCommentId);

            entity.Property(rc => rc.Text)
                .HasMaxLength(2000)
                .IsRequired();

            entity.HasOne(rc => rc.User)
                .WithMany(u => u.ReviewComments)
                .HasForeignKey(rc => rc.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(rc => rc.Review)
                .WithMany(r => r.ReviewComments)
                .HasForeignKey(rc => rc.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Follow>(entity =>
        {
            entity.HasKey(f => f.FollowId);

            entity.HasIndex(f => new { f.FollowerId, f.FollowingId })
                .IsUnique();

            entity.HasOne(f => f.Follower)
                .WithMany(u => u.FollowingUsers)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(f => f.Following)
                .WithMany(u => u.FollowerUsers)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}