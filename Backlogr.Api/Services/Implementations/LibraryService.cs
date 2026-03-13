using Backlogr.Api.Data;
using Backlogr.Api.DTOs.Library;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Models.Enums;
using Backlogr.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backlogr.Api.Services.Implementations;

public sealed class LibraryService : ILibraryService
{
    private readonly ApplicationDbContext _dbContext;

    public LibraryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LibraryLogResponseDto>> GetLibraryAsync(Guid userId)
    {
        return await _dbContext.GameLogs
            .AsNoTracking()
            .Where(gl => gl.UserId == userId)
            .Include(gl => gl.Game)
            .OrderByDescending(gl => gl.UpdatedAt)
            .Select(gl => new LibraryLogResponseDto
            {
                GameLogId = gl.GameLogId,
                GameId = gl.GameId,
                GameTitle = gl.Game.Title,
                CoverImageUrl = gl.Game.CoverImageUrl,
                Status = gl.Status,
                Rating = gl.Rating,
                Platform = gl.Platform,
                Hours = gl.Hours,
                StartedAt = gl.StartedAt,
                FinishedAt = gl.FinishedAt,
                Notes = gl.Notes,
                UpdatedAt = gl.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<LibraryLogResponseDto> UpsertLibraryLogAsync(Guid userId, UpsertLibraryLogRequestDto dto)
    {
        await ValidateGameExistsAsync(dto.GameId);
        ValidateLibraryLog(dto);

        var existingLog = await _dbContext.GameLogs
            .Include(gl => gl.Game)
            .SingleOrDefaultAsync(gl => gl.UserId == userId && gl.GameId == dto.GameId);

        if (existingLog is null)
        {
            existingLog = new GameLog
            {
                GameLogId = Guid.NewGuid(),
                UserId = userId,
                GameId = dto.GameId,
                Status = dto.Status,
                Rating = dto.Rating,
                Platform = dto.Platform?.Trim(),
                Hours = dto.Hours,
                StartedAt = dto.StartedAt,
                FinishedAt = dto.FinishedAt,
                Notes = dto.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.GameLogs.Add(existingLog);
        }
        else
        {
            existingLog.Status = dto.Status;
            existingLog.Rating = dto.Rating;
            existingLog.Platform = dto.Platform?.Trim();
            existingLog.Hours = dto.Hours;
            existingLog.StartedAt = dto.StartedAt;
            existingLog.FinishedAt = dto.FinishedAt;
            existingLog.Notes = dto.Notes?.Trim();
            existingLog.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        if (existingLog.Game is null)
        {
            await _dbContext.Entry(existingLog)
                .Reference(gl => gl.Game)
                .LoadAsync();
        }

        var game = existingLog.Game
            ?? throw new InvalidOperationException("Game navigation was not loaded.");

        return new LibraryLogResponseDto
        {
            GameLogId = existingLog.GameLogId,
            GameId = existingLog.GameId,
            GameTitle = game.Title,
            CoverImageUrl = game.CoverImageUrl,
            Status = existingLog.Status,
            Rating = existingLog.Rating,
            Platform = existingLog.Platform,
            Hours = existingLog.Hours,
            StartedAt = existingLog.StartedAt,
            FinishedAt = existingLog.FinishedAt,
            Notes = existingLog.Notes,
            UpdatedAt = existingLog.UpdatedAt
        };
    }

    public async Task<bool> DeleteLibraryLogAsync(Guid userId, Guid gameId)
    {
        var existingLog = await _dbContext.GameLogs
            .SingleOrDefaultAsync(gl => gl.UserId == userId && gl.GameId == gameId);

        if (existingLog is null)
        {
            return false;
        }

        _dbContext.GameLogs.Remove(existingLog);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private async Task ValidateGameExistsAsync(Guid gameId)
    {
        var gameExists = await _dbContext.Games.AnyAsync(g => g.GameId == gameId);

        if (!gameExists)
        {
            throw new KeyNotFoundException("Game was not found.");
        }
    }

    private static void ValidateLibraryLog(UpsertLibraryLogRequestDto dto)
    {
        if (dto.GameId == Guid.Empty)
        {
            throw new ArgumentException("GameId is required.");
        }

        if (dto.Rating is < 0 or > 5)
        {
            throw new ArgumentException("Rating must be between 0.0 and 5.0.");
        }

        if (dto.Rating.HasValue)
        {
            var doubled = dto.Rating.Value * 2;
            if (doubled != decimal.Truncate(doubled))
            {
                throw new ArgumentException("Rating must be in 0.5 increments.");
            }
        }

        if (dto.Hours is < 0)
        {
            throw new ArgumentException("Hours cannot be negative.");
        }

        if (dto.FinishedAt.HasValue && dto.StartedAt.HasValue && dto.FinishedAt < dto.StartedAt)
        {
            throw new ArgumentException("FinishedAt cannot be earlier than StartedAt.");
        }

        if (dto.Status == LibraryStatus.Played && !dto.FinishedAt.HasValue)
        {
            throw new ArgumentException("FinishedAt is required when status is Played.");
        }
    }
}