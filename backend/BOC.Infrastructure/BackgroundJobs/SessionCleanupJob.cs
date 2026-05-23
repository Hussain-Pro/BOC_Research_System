using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using BOC.Infrastructure.Persistence;

namespace BOC.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs daily at 01:00 UTC.
/// Responsibilities:
///   1. Revoke expired refresh tokens (clear RefreshTokenHash + RefreshTokenExpires on AppUsers).
///   2. Purge expired PasswordResetTokens (expiry &gt; 15 minutes per blueprint).
///   3. Purge expired FileAccessTokens (expiry &gt; 24 hours per blueprint).
///   4. Clear orphaned device fingerprints for users whose tokens have expired.
/// Blueprint Reference: Section 8 — SessionCleanupJob, Daily 01:00 UTC.
///                      Section 17 — PasswordResetTokens retain 15 min; FileAccessTokens retain 24h.
/// </summary>
[DisallowConcurrentExecution]
public sealed class SessionCleanupJob : IJob
{
    private readonly BOCDbContext _dbContext;
    private readonly ILogger<SessionCleanupJob> _logger;

    public SessionCleanupJob(BOCDbContext dbContext, ILogger<SessionCleanupJob> logger)
    {
        _dbContext = dbContext;
        _logger    = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[SessionCleanupJob] Starting session cleanup...");

        var now               = DateTime.UtcNow;
        var cancellationToken = context.CancellationToken;
        var totalCleaned      = 0;

        // ── 1. Expire refresh tokens ──────────────────────────────────────────
        var usersWithExpiredTokens = await _dbContext.AppUsers
            .Where(u =>
                u.RefreshTokenHash != null &&
                u.RefreshTokenExpires != null &&
                u.RefreshTokenExpires < now)
            .ToListAsync(cancellationToken);

        foreach (var user in usersWithExpiredTokens)
        {
            user.RefreshTokenHash    = null;
            user.RefreshTokenExpires = null;
            user.DeviceFingerprint   = null; // Clear orphaned device fingerprint
            user.ModifiedAt          = now;
        }

        if (usersWithExpiredTokens.Count > 0)
        {
            _logger.LogInformation(
                "[SessionCleanupJob] Revoked {Count} expired refresh token(s).",
                usersWithExpiredTokens.Count);
            totalCleaned += usersWithExpiredTokens.Count;
        }

        // ── 2. Purge expired PasswordResetTokens (> 15 minutes) ──────────────
        var expiredPasswordTokens = await _dbContext.PasswordResetTokens
            .Where(t => t.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        if (expiredPasswordTokens.Count > 0)
        {
            _dbContext.PasswordResetTokens.RemoveRange(expiredPasswordTokens);
            _logger.LogInformation(
                "[SessionCleanupJob] Purged {Count} expired password reset token(s).",
                expiredPasswordTokens.Count);
            totalCleaned += expiredPasswordTokens.Count;
        }

        // ── 3. Purge expired FileAccessTokens (> 24 hours) ───────────────────
        var expiredFileTokens = await _dbContext.FileAccessTokens
            .Where(t => t.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        if (expiredFileTokens.Count > 0)
        {
            _dbContext.FileAccessTokens.RemoveRange(expiredFileTokens);
            _logger.LogInformation(
                "[SessionCleanupJob] Purged {Count} expired file access token(s).",
                expiredFileTokens.Count);
            totalCleaned += expiredFileTokens.Count;
        }

        if (totalCleaned > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "[SessionCleanupJob] Completed. Total records cleaned: {Total}.",
            totalCleaned);
    }
}
