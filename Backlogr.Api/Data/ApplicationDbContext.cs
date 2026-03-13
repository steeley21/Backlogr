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
    }
}