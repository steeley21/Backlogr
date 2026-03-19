using Backlogr.Api.Common;
using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Admin;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Models.Enums;
using Backlogr.Api.Options;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backlogr.Api.Services.Implementations;

public sealed class DemoSeedService : IDemoSeedService
{
    private const string DemoPassword = "Backlogr123!";

    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IIgdbService _igdbService;
    private readonly IAiSearchSyncService _aiSearchSyncService;
    private readonly DemoSeedOptions _options;
    private readonly ILogger<DemoSeedService> _logger;

    public DemoSeedService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IIgdbService igdbService,
        IAiSearchSyncService aiSearchSyncService,
        IOptions<DemoSeedOptions> options,
        ILogger<DemoSeedService> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _igdbService = igdbService;
        _aiSearchSyncService = aiSearchSyncService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AdminDemoSeedResponseDto> SeedAsync(
        Guid currentUserId,
        AdminDemoSeedRequestDto? request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_options.Enabled)
        {
            throw new InvalidOperationException(
                "Demo seeding is disabled. Enable DemoSeed:Enabled before running this endpoint.");
        }

        var normalizedRequest = request ?? new AdminDemoSeedRequestDto();
        var summary = new AdminDemoSeedResponseDto
        {
            DemoAdminUserName = "steeley",
            DemoAdminEmail = "kate.demo@backlogr.app",
            Notes =
            [
                "Use username 'steeley' to log in. The UI will display it as '@steeley'.",
                $"The seeded demo password for the demo accounts is '{DemoPassword}'.",
                "The demo viewer account is seeded as Admin (not SuperAdmin).",
            ]
        };

        var currentUser = await _userManager.FindByIdAsync(currentUserId.ToString());
        if (currentUser is null)
        {
            throw new InvalidOperationException("The current admin user could not be found.");
        }

        var userSpecs = GetUserSeeds();
        var usersByUserName = new Dictionary<string, ApplicationUser>(StringComparer.OrdinalIgnoreCase);

        foreach (var userSpec in userSpecs)
        {
            var user = await CreateOrUpdateUserAsync(userSpec, summary, cancellationToken);
            usersByUserName[userSpec.UserName] = user;
        }

        summary.SeededUsers = userSpecs
            .Select(spec => spec.UserName)
            .ToList();

        var requestedGameTitles = GetRequestedGameTitles();
        var gamesByRequestedTitle = new Dictionary<string, Game>(StringComparer.OrdinalIgnoreCase);

        foreach (var requestedTitle in requestedGameTitles)
        {
            var game = await EnsureGameAsync(requestedTitle, summary, cancellationToken);
            gamesByRequestedTitle[requestedTitle] = game;
        }

        summary.SeededGames = requestedGameTitles.ToList();

        foreach (var followSeed in GetFollowSeeds())
        {
            await CreateFollowIfMissingAsync(
                usersByUserName[followSeed.FollowerUserName],
                usersByUserName[followSeed.FollowingUserName],
                followSeed.CreatedAtUtc,
                summary,
                cancellationToken);
        }

        foreach (var logSeed in GetLogSeeds())
        {
            await CreateOrUpdateLogAsync(
                usersByUserName[logSeed.UserName],
                gamesByRequestedTitle[logSeed.RequestedGameTitle],
                logSeed,
                summary,
                cancellationToken);
        }

        foreach (var reviewSeed in GetReviewSeeds())
        {
            await CreateOrUpdateReviewAsync(
                usersByUserName[reviewSeed.UserName],
                gamesByRequestedTitle[reviewSeed.RequestedGameTitle],
                reviewSeed,
                summary,
                cancellationToken);
        }

        foreach (var likeSeed in GetLikeSeeds())
        {
            await CreateLikeIfMissingAsync(
                usersByUserName[likeSeed.UserName],
                usersByUserName[likeSeed.ReviewOwnerUserName],
                gamesByRequestedTitle[likeSeed.RequestedGameTitle],
                likeSeed.CreatedAtUtc,
                summary,
                cancellationToken);
        }

        foreach (var commentSeed in GetCommentSeeds())
        {
            await CreateCommentIfMissingAsync(
                usersByUserName[commentSeed.UserName],
                usersByUserName[commentSeed.ReviewOwnerUserName],
                gamesByRequestedTitle[commentSeed.RequestedGameTitle],
                commentSeed.Text,
                commentSeed.CreatedAtUtc,
                summary,
                cancellationToken);
        }

        if (normalizedRequest.RunAiBackfill)
        {
            await _aiSearchSyncService.BackfillGamesAsync(cancellationToken);
            summary.AiBackfillRun = true;
        }

        _logger.LogInformation(
            "Demo seed completed by admin {AdminUserName}. UsersCreated={UsersCreated}, GamesImported={GamesImported}, LogsCreated={LogsCreated}, ReviewsCreated={ReviewsCreated}",
            currentUser.UserName,
            summary.UsersCreated,
            summary.GamesImported,
            summary.LogsCreated,
            summary.ReviewsCreated);

        return summary;
    }

    private async Task<ApplicationUser> CreateOrUpdateUserAsync(
        DemoUserSeed seed,
        AdminDemoSeedResponseDto summary,
        CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByNameAsync(seed.UserName);

        if (existingUser is null)
        {
            var existingByEmail = await _userManager.FindByEmailAsync(seed.Email);
            if (existingByEmail is not null)
            {
                throw new InvalidOperationException(
                    $"Cannot seed demo user '{seed.UserName}' because the email '{seed.Email}' is already used by another account.");
            }

            var newUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = seed.UserName,
                Email = seed.Email,
                DisplayName = seed.DisplayName,
                Bio = seed.Bio,
                AvatarUrl = seed.AvatarUrl,
                CreatedAt = seed.CreatedAtUtc,
                EmailConfirmed = true,
            };

            var createResult = await _userManager.CreateAsync(newUser, seed.Password);
            ThrowIfIdentityFailed(createResult, $"Failed to create demo user '{seed.UserName}'.");

            var addRolesResult = await _userManager.AddToRolesAsync(newUser, seed.Roles);
            ThrowIfIdentityFailed(addRolesResult, $"Failed to assign roles for demo user '{seed.UserName}'.");

            summary.UsersCreated++;
            return newUser;
        }

        existingUser.Email = seed.Email;
        existingUser.DisplayName = seed.DisplayName;
        existingUser.Bio = seed.Bio;
        existingUser.AvatarUrl = seed.AvatarUrl;
        existingUser.EmailConfirmed = true;

        var updateResult = await _userManager.UpdateAsync(existingUser);
        ThrowIfIdentityFailed(updateResult, $"Failed to update demo user '{seed.UserName}'.");

        await EnsurePasswordAsync(existingUser, seed.Password);
        await SyncRolesAsync(existingUser, seed.Roles);

        summary.UsersUpdated++;
        return existingUser;
    }

    private async Task EnsurePasswordAsync(ApplicationUser user, string password)
    {
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            var addPasswordResult = await _userManager.AddPasswordAsync(user, password);
            ThrowIfIdentityFailed(addPasswordResult, $"Failed to add a password for '{user.UserName}'.");
            return;
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, password);
        ThrowIfIdentityFailed(resetResult, $"Failed to reset the password for '{user.UserName}'.");
    }

    private async Task SyncRolesAsync(ApplicationUser user, IReadOnlyCollection<string> desiredRoles)
    {
        var currentRoles = await _userManager.GetRolesAsync(user);

        var rolesToRemove = currentRoles
            .Except(desiredRoles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (rolesToRemove.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            ThrowIfIdentityFailed(removeResult, $"Failed to remove stale roles for '{user.UserName}'.");
        }

        var rolesToAdd = desiredRoles
            .Except(currentRoles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (rolesToAdd.Length > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            ThrowIfIdentityFailed(addResult, $"Failed to add required roles for '{user.UserName}'.");
        }
    }

    private async Task<Game> EnsureGameAsync(
        string requestedTitle,
        AdminDemoSeedResponseDto summary,
        CancellationToken cancellationToken)
    {
        var existingGame = await _dbContext.Games
            .FirstOrDefaultAsync(game => game.Title == requestedTitle, cancellationToken);

        if (existingGame is not null)
        {
            summary.GamesMatchedExistingCatalog++;
            return existingGame;
        }

        var searchResults = await _igdbService.SearchGamesAsync(requestedTitle, 10);
        var bestMatch = SelectBestGameMatch(requestedTitle, searchResults);

        if (bestMatch is null)
        {
            throw new InvalidOperationException($"Unable to find an IGDB match for demo game '{requestedTitle}'.");
        }

        var importedGame = await _igdbService.ImportGameAsync(bestMatch.IgdbId);
        summary.GamesImported++;

        return await _dbContext.Games
            .SingleAsync(game => game.GameId == importedGame.GameId, cancellationToken);
    }

    private async Task CreateFollowIfMissingAsync(
        ApplicationUser follower,
        ApplicationUser following,
        DateTime createdAtUtc,
        AdminDemoSeedResponseDto summary,
        CancellationToken cancellationToken)
    {
        if (follower.Id == following.Id)
        {
            return;
        }

        var existing = await _dbContext.Follows
            .AnyAsync(
                follow => follow.FollowerId == follower.Id && follow.FollowingId == following.Id,
                cancellationToken);

        if (existing)
        {
            return;
        }

        _dbContext.Follows.Add(new Follow
        {
            FollowId = Guid.NewGuid(),
            FollowerId = follower.Id,
            FollowingId = following.Id,
            CreatedAt = createdAtUtc,
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        summary.FollowsCreated++;
    }

    private async Task CreateOrUpdateLogAsync(
        ApplicationUser user,
        Game game,
        DemoLogSeed seed,
        AdminDemoSeedResponseDto summary,
        CancellationToken cancellationToken)
    {
        var existing = await _dbContext.GameLogs
            .SingleOrDefaultAsync(
                log => log.UserId == user.Id && log.GameId == game.GameId,
                cancellationToken);

        if (existing is null)
        {
            _dbContext.GameLogs.Add(new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = user.Id,
                GameId = game.GameId,
                Status = seed.Status,
                Rating = seed.Rating,
                Platform = seed.Platform,
                Hours = seed.Hours,
                StartedAt = seed.StartedAtUtc,
                FinishedAt = seed.FinishedAtUtc,
                Notes = seed.Notes,
                CreatedAt = seed.CreatedAtUtc,
                UpdatedAt = seed.UpdatedAtUtc,
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            summary.LogsCreated++;
            return;
        }

        existing.Status = seed.Status;
        existing.Rating = seed.Rating;
        existing.Platform = seed.Platform;
        existing.Hours = seed.Hours;
        existing.StartedAt = seed.StartedAtUtc;
        existing.FinishedAt = seed.FinishedAtUtc;
        existing.Notes = seed.Notes;
        existing.UpdatedAt = seed.UpdatedAtUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);
        summary.LogsUpdated++;
    }

    private async Task CreateOrUpdateReviewAsync(
        ApplicationUser user,
        Game game,
        DemoReviewSeed seed,
        AdminDemoSeedResponseDto summary,
        CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Reviews
            .SingleOrDefaultAsync(
                review => review.UserId == user.Id && review.GameId == game.GameId,
                cancellationToken);

        if (existing is null)
        {
            _dbContext.Reviews.Add(new Review
            {
                ReviewId = Guid.NewGuid(),
                UserId = user.Id,
                GameId = game.GameId,
                Text = seed.Text,
                HasSpoilers = seed.HasSpoilers,
                CreatedAt = seed.CreatedAtUtc,
                UpdatedAt = seed.UpdatedAtUtc,
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            summary.ReviewsCreated++;
            return;
        }

        existing.Text = seed.Text;
        existing.HasSpoilers = seed.HasSpoilers;
        existing.UpdatedAt = seed.UpdatedAtUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);
        summary.ReviewsUpdated++;
    }

    private async Task CreateLikeIfMissingAsync(
        ApplicationUser likingUser,
        ApplicationUser reviewOwner,
        Game game,
        DateTime createdAtUtc,
        AdminDemoSeedResponseDto summary,
        CancellationToken cancellationToken)
    {
        var reviewId = await _dbContext.Reviews
            .Where(review => review.UserId == reviewOwner.Id && review.GameId == game.GameId)
            .Select(review => (Guid?)review.ReviewId)
            .SingleOrDefaultAsync(cancellationToken);

        if (reviewId is null)
        {
            throw new InvalidOperationException(
                $"Cannot seed a like because review '{reviewOwner.UserName}/{game.Title}' was not found.");
        }

        var existing = await _dbContext.ReviewLikes
            .AnyAsync(
                like => like.UserId == likingUser.Id && like.ReviewId == reviewId.Value,
                cancellationToken);

        if (existing)
        {
            return;
        }

        _dbContext.ReviewLikes.Add(new ReviewLike
        {
            ReviewLikeId = Guid.NewGuid(),
            UserId = likingUser.Id,
            ReviewId = reviewId.Value,
            CreatedAt = createdAtUtc,
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        summary.LikesCreated++;
    }

    private async Task CreateCommentIfMissingAsync(
        ApplicationUser commentingUser,
        ApplicationUser reviewOwner,
        Game game,
        string text,
        DateTime createdAtUtc,
        AdminDemoSeedResponseDto summary,
        CancellationToken cancellationToken)
    {
        var reviewId = await _dbContext.Reviews
            .Where(review => review.UserId == reviewOwner.Id && review.GameId == game.GameId)
            .Select(review => (Guid?)review.ReviewId)
            .SingleOrDefaultAsync(cancellationToken);

        if (reviewId is null)
        {
            throw new InvalidOperationException(
                $"Cannot seed a comment because review '{reviewOwner.UserName}/{game.Title}' was not found.");
        }

        var existing = await _dbContext.ReviewComments
            .AnyAsync(
                comment =>
                    comment.UserId == commentingUser.Id &&
                    comment.ReviewId == reviewId.Value &&
                    comment.Text == text,
                cancellationToken);

        if (existing)
        {
            return;
        }

        _dbContext.ReviewComments.Add(new ReviewComment
        {
            ReviewCommentId = Guid.NewGuid(),
            UserId = commentingUser.Id,
            ReviewId = reviewId.Value,
            Text = text,
            CreatedAt = createdAtUtc,
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        summary.CommentsCreated++;
    }

    private static void ThrowIfIdentityFailed(IdentityResult result, string prefixMessage)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{prefixMessage} Errors: {errors}");
    }

    private static Backlogr.Api.DTOs.Igdb.IgdbGameSearchResultDto? SelectBestGameMatch(
        string requestedTitle,
        IReadOnlyList<Backlogr.Api.DTOs.Igdb.IgdbGameSearchResultDto> searchResults)
    {
        return searchResults
            .OrderBy(result => GetTitleMatchScore(requestedTitle, result.Title))
            .FirstOrDefault();
    }

    private static int GetTitleMatchScore(string requestedTitle, string candidateTitle)
    {
        if (string.Equals(requestedTitle, candidateTitle, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (candidateTitle.StartsWith(requestedTitle, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (requestedTitle.StartsWith(candidateTitle, StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (candidateTitle.Contains(requestedTitle, StringComparison.OrdinalIgnoreCase))
        {
            return 3;
        }

        return 4;
    }

    private static IReadOnlyList<DemoUserSeed> GetUserSeeds()
    {
        var createdAt = UtcAt(15, 17);

        return
        [
            new DemoUserSeed(
                "steeley",
                "Kate",
                "kate.demo@backlogr.app",
                DemoPassword,
                [RoleNames.User, RoleNames.Admin],
                "[DEMO] Backlogr walkthrough account. Loves clean UI, sharp review blurbs, cozy sims, and recommendation features that actually feel personalized.",
                "https://api.dicebear.com/9.x/pixel-art/svg?seed=steeley",
                createdAt),
            new DemoUserSeed(
                "hyrule_hermit",
                "Hyrule Hermit",
                "hyrule.hermit@backlogr.app",
                DemoPassword,
                [RoleNames.User],
                "[DEMO] Climbs every tower, opens every chest, and somehow still says they're just 'casually exploring.'",
                "https://api.dicebear.com/9.x/pixel-art/svg?seed=hyrule-hermit",
                createdAt.AddMinutes(3)),
            new DemoUserSeed(
                "chocobo_commuter",
                "Chocobo Commuter",
                "chocobo.commuter@backlogr.app",
                DemoPassword,
                [RoleNames.User],
                "[DEMO] JRPG enjoyer. Will absolutely spend forty minutes optimizing a party before the first real boss.",
                "https://api.dicebear.com/9.x/pixel-art/svg?seed=chocobo-commuter",
                createdAt.AddMinutes(6)),
            new DemoUserSeed(
                "raccooncity_hr",
                "Raccoon City HR",
                "raccooncity.hr@backlogr.app",
                DemoPassword,
                [RoleNames.User],
                "[DEMO] Horror and survival specialist. Rates games on atmosphere, inventory stress, and door-opening anxiety.",
                "https://api.dicebear.com/9.x/pixel-art/svg?seed=raccooncity-hr",
                createdAt.AddMinutes(9)),
            new DemoUserSeed(
                "pelicantown_nightshift",
                "PelicanTown Night Shift",
                "pelicantown.nightshift@backlogr.app",
                DemoPassword,
                [RoleNames.User],
                "[DEMO] Cozy-game evangelist. Waters crops, decorates cabins, and pretends min-maxing friendship routes is relaxing.",
                "https://api.dicebear.com/9.x/pixel-art/svg?seed=pelicantown-nightshift",
                createdAt.AddMinutes(12)),
            new DemoUserSeed(
                "spartan_snackbreak",
                "Spartan Snackbreak",
                "spartan.snackbreak@backlogr.app",
                DemoPassword,
                [RoleNames.User],
                "[DEMO] Drops into shooters for one quick match and somehow looks up two hours later wondering what happened.",
                "https://api.dicebear.com/9.x/pixel-art/svg?seed=spartan-snackbreak",
                createdAt.AddMinutes(15)),
            new DemoUserSeed(
                "soulslike_susan",
                "Soulslike Susan",
                "soulslike.susan@backlogr.app",
                DemoPassword,
                [RoleNames.User],
                "[DEMO] Thinks repeated failure is character building and calls every impossible encounter 'a learning opportunity.'",
                "https://api.dicebear.com/9.x/pixel-art/svg?seed=soulslike-susan",
                createdAt.AddMinutes(18)),
        ];
    }

    private static IReadOnlyList<string> GetRequestedGameTitles()
    {
        return
        [
            "Stardew Valley",
            "The Legend of Zelda: Breath of the Wild",
            "Mario Kart 8 Deluxe",
            "Persona 5 Royal",
            "Final Fantasy VII Remake",
            "Resident Evil 4",
            "Dead Space",
            "Halo Infinite",
            "Hades",
            "Elden Ring",
            "Baldur's Gate 3",
            "Animal Crossing: New Horizons",
        ];
    }

    private static IReadOnlyList<DemoFollowSeed> GetFollowSeeds()
    {
        return
        [
            new DemoFollowSeed("steeley", "hyrule_hermit", UtcAt(12, 19)),
            new DemoFollowSeed("steeley", "chocobo_commuter", UtcAt(12, 19).AddMinutes(1)),
            new DemoFollowSeed("steeley", "raccooncity_hr", UtcAt(12, 19).AddMinutes(2)),
            new DemoFollowSeed("steeley", "pelicantown_nightshift", UtcAt(12, 19).AddMinutes(3)),
            new DemoFollowSeed("steeley", "spartan_snackbreak", UtcAt(12, 19).AddMinutes(4)),
            new DemoFollowSeed("steeley", "soulslike_susan", UtcAt(12, 19).AddMinutes(5)),
            new DemoFollowSeed("hyrule_hermit", "chocobo_commuter", UtcAt(10, 20)),
            new DemoFollowSeed("chocobo_commuter", "pelicantown_nightshift", UtcAt(9, 21)),
            new DemoFollowSeed("spartan_snackbreak", "steeley", UtcAt(8, 22)),
            new DemoFollowSeed("soulslike_susan", "steeley", UtcAt(8, 22).AddMinutes(7)),
            new DemoFollowSeed("raccooncity_hr", "soulslike_susan", UtcAt(7, 23)),
        ];
    }

    private static IReadOnlyList<DemoLogSeed> GetLogSeeds()
    {
        return
        [
            new DemoLogSeed("hyrule_hermit", "The Legend of Zelda: Breath of the Wild", LibraryStatus.Played, 5.0m, "Nintendo Switch", 78m, UtcAt(14, 18), UtcAt(11, 19), "Still one of the best 'pick a direction and just go' games.", UtcAt(11, 19), UtcAt(11, 19)),
            new DemoLogSeed("hyrule_hermit", "Mario Kart 8 Deluxe", LibraryStatus.Playing, 4.0m, "Nintendo Switch", 12m, UtcAt(4, 20), null, "Good for one race. Never actually just one race.", UtcAt(4, 20), UtcAt(2, 21)),
            new DemoLogSeed("chocobo_commuter", "Persona 5 Royal", LibraryStatus.Played, 5.0m, "PlayStation 5", 102m, UtcAt(13, 18), UtcAt(9, 20), "Ridiculous style. Great soundtrack. Zero regrets.", UtcAt(9, 20), UtcAt(9, 20)),
            new DemoLogSeed("chocobo_commuter", "Final Fantasy VII Remake", LibraryStatus.Played, 4.5m, "PlayStation 5", 36m, UtcAt(10, 18), UtcAt(7, 19), "The boss fights carried exactly the amount of drama I wanted.", UtcAt(7, 19), UtcAt(7, 19)),
            new DemoLogSeed("raccooncity_hr", "Resident Evil 4", LibraryStatus.Played, 4.5m, "PlayStation 5", 19m, UtcAt(9, 21), UtcAt(6, 21), "Perfect briefcase management simulator with occasional panic.", UtcAt(6, 21), UtcAt(6, 21)),
            new DemoLogSeed("raccooncity_hr", "Dead Space", LibraryStatus.Played, 4.0m, "Xbox Series X", 14m, UtcAt(7, 19), UtcAt(5, 22), "Industrial sci-fi dread still works every single time.", UtcAt(5, 22), UtcAt(5, 22)),
            new DemoLogSeed("pelicantown_nightshift", "Stardew Valley", LibraryStatus.Played, 5.0m, "PC", 84m, UtcAt(14, 17), UtcAt(10, 18), "Accidentally optimized an entire year and had a great time doing it.", UtcAt(10, 18), UtcAt(10, 18)),
            new DemoLogSeed("pelicantown_nightshift", "Animal Crossing: New Horizons", LibraryStatus.Playing, 4.5m, "Nintendo Switch", 67m, UtcAt(8, 18), null, "Extremely good at turning 'quick check-in' into island renovation.", UtcAt(8, 18), UtcAt(3, 18)),
            new DemoLogSeed("spartan_snackbreak", "Halo Infinite", LibraryStatus.Playing, 3.5m, "Xbox Series X", 16m, UtcAt(6, 20), null, "Gunfeel is still strong. I just want one more map rotation surprise.", UtcAt(6, 20), UtcAt(1, 18)),
            new DemoLogSeed("soulslike_susan", "Elden Ring", LibraryStatus.Played, 5.0m, "PlayStation 5", 121m, UtcAt(12, 16), UtcAt(8, 17), "A huge map full of beautiful danger and terrible decisions.", UtcAt(8, 17), UtcAt(8, 17)),
            new DemoLogSeed("soulslike_susan", "Hades", LibraryStatus.Played, 4.5m, "Steam Deck", 27m, UtcAt(5, 19), UtcAt(3, 20), "Fast runs, sharp writing, and absolutely lethal 'one more run' energy.", UtcAt(3, 20), UtcAt(3, 20)),
            new DemoLogSeed("steeley", "Hades", LibraryStatus.Played, 4.5m, "Nintendo Switch", 18m, UtcAt(6, 19), UtcAt(5, 21), "Great pick-up-and-play flow. Easy to demo and easy to talk about.", UtcAt(5, 21), UtcAt(5, 21)),
            new DemoLogSeed("steeley", "Stardew Valley", LibraryStatus.Playing, 4.0m, "PC", 32m, UtcAt(10, 17), null, "A perfect 'just one more day' game when I need something mellow.", UtcAt(10, 17), UtcAt(2, 19)),
            new DemoLogSeed("steeley", "Baldur's Gate 3", LibraryStatus.Backlog, null, "PC", null, null, null, "Queued up for the next long weekend when I can actually commit to it.", UtcAt(1, 17), UtcAt(1, 17)),
        ];
    }

    private static IReadOnlyList<DemoReviewSeed> GetReviewSeeds()
    {
        return
        [
            new DemoReviewSeed("hyrule_hermit", "The Legend of Zelda: Breath of the Wild", "I love how generous this world feels. Every hill, shrine, and weird detour rewards curiosity instead of punishing it.", false, UtcAt(8, 20), UtcAt(8, 20)),
            new DemoReviewSeed("chocobo_commuter", "Persona 5 Royal", "An absurd amount of style packed into every menu, battle, and social link. Long, yes. Worth it, also yes.", false, UtcAt(7, 21), UtcAt(7, 21)),
            new DemoReviewSeed("raccooncity_hr", "Resident Evil 4", "The pacing is almost rude in how efficiently it keeps throwing new problems at you. Great combat, great tension, great inventory stress.", false, UtcAt(4, 19), UtcAt(4, 19)),
            new DemoReviewSeed("pelicantown_nightshift", "Stardew Valley", "A farming game that somehow makes spreadsheets feel cozy. It is impossible for me to play this casually.", false, UtcAt(3, 21), UtcAt(3, 21)),
            new DemoReviewSeed("soulslike_susan", "Elden Ring", "It feels huge without feeling empty. Even when it flattens you, it usually leaves you curious enough to go right back in.", false, UtcAt(2, 20), UtcAt(2, 20)),
            new DemoReviewSeed("spartan_snackbreak", "Halo Infinite", "Moment-to-moment combat still rules. I just wish the live-service side kept pace with how good the sandbox feels.", false, UtcAt(1, 20), UtcAt(1, 20)),
            new DemoReviewSeed("steeley", "Hades", "This is such an easy recommendation because everything is readable, stylish, and polished. Every run gives me something useful back.", false, UtcAt(1, 22), UtcAt(1, 22)),
        ];
    }

    private static IReadOnlyList<DemoLikeSeed> GetLikeSeeds()
    {
        return
        [
            new DemoLikeSeed("steeley", "hyrule_hermit", "The Legend of Zelda: Breath of the Wild", UtcAt(7, 8)),
            new DemoLikeSeed("steeley", "pelicantown_nightshift", "Stardew Valley", UtcAt(3, 22)),
            new DemoLikeSeed("steeley", "soulslike_susan", "Elden Ring", UtcAt(2, 21)),
            new DemoLikeSeed("steeley", "spartan_snackbreak", "Halo Infinite", UtcAt(1, 21)),
            new DemoLikeSeed("hyrule_hermit", "steeley", "Hades", UtcAt(1, 23)),
            new DemoLikeSeed("chocobo_commuter", "steeley", "Hades", UtcAt(1, 23).AddMinutes(8)),
            new DemoLikeSeed("soulslike_susan", "raccooncity_hr", "Resident Evil 4", UtcAt(4, 21)),
            new DemoLikeSeed("raccooncity_hr", "soulslike_susan", "Elden Ring", UtcAt(2, 22)),
            new DemoLikeSeed("pelicantown_nightshift", "steeley", "Hades", UtcAt(1, 23).AddMinutes(16)),
        ];
    }

    private static IReadOnlyList<DemoCommentSeed> GetCommentSeeds()
    {
        return
        [
            new DemoCommentSeed("steeley", "pelicantown_nightshift", "Stardew Valley", "'Spreadsheets feel cozy' is unfortunately extremely true.", UtcAt(3, 22).AddMinutes(20)),
            new DemoCommentSeed("chocobo_commuter", "steeley", "Hades", "That is exactly why I keep recommending it to people who want something polished right away.", UtcAt(1, 23).AddMinutes(25)),
            new DemoCommentSeed("soulslike_susan", "raccooncity_hr", "Resident Evil 4", "Inventory stress is the correct review metric, honestly.", UtcAt(4, 21).AddMinutes(30)),
            new DemoCommentSeed("hyrule_hermit", "steeley", "Hades", "It really is one of the easiest 'you should try this' games.", UtcAt(1, 23).AddMinutes(40)),
            new DemoCommentSeed("pelicantown_nightshift", "chocobo_commuter", "Persona 5 Royal", "Long games with this much style get a lot more grace from me.", UtcAt(7, 22)),
            new DemoCommentSeed("raccooncity_hr", "soulslike_susan", "Elden Ring", "'Beautiful danger' is a pretty solid elevator pitch.", UtcAt(2, 21).AddMinutes(12)),
        ];
    }

    private static DateTime UtcAt(int daysAgo, int hour)
    {
        return DateTime.UtcNow.Date.AddDays(-daysAgo).AddHours(hour);
    }

    private sealed record DemoUserSeed(
        string UserName,
        string DisplayName,
        string Email,
        string Password,
        IReadOnlyCollection<string> Roles,
        string Bio,
        string? AvatarUrl,
        DateTime CreatedAtUtc);

    private sealed record DemoFollowSeed(
        string FollowerUserName,
        string FollowingUserName,
        DateTime CreatedAtUtc);

    private sealed record DemoLogSeed(
        string UserName,
        string RequestedGameTitle,
        LibraryStatus Status,
        decimal? Rating,
        string? Platform,
        decimal? Hours,
        DateTime? StartedAtUtc,
        DateTime? FinishedAtUtc,
        string? Notes,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);

    private sealed record DemoReviewSeed(
        string UserName,
        string RequestedGameTitle,
        string Text,
        bool HasSpoilers,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);

    private sealed record DemoLikeSeed(
        string UserName,
        string ReviewOwnerUserName,
        string RequestedGameTitle,
        DateTime CreatedAtUtc);

    private sealed record DemoCommentSeed(
        string UserName,
        string ReviewOwnerUserName,
        string RequestedGameTitle,
        string Text,
        DateTime CreatedAtUtc);
}
