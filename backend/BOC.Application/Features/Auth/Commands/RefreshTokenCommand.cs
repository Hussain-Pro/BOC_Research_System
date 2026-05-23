using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BOC.Application.Common.Interfaces;
using BOC.Domain.Enums;

namespace BOC.Application.Features.Auth.Commands;

// ─────────────────────────────────────────────────────────────────────────────
// Command
// ─────────────────────────────────────────────────────────────────────────────

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken) : IRequest<RefreshTokenResultDto>;

public class RefreshTokenResultDto
{
    public string AccessToken       { get; set; } = string.Empty;
    public string RefreshToken      { get; set; } = string.Empty;
    public DateTime RefreshTokenExpires { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Validator
// ─────────────────────────────────────────────────────────────────────────────

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Handler
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Validates the expired AccessToken signature + claims, then issues a new
/// token pair if the RefreshToken in the DB is still valid.
/// Blueprint Reference: Section 12 — Token refresh; 15-min AccessToken,
///                      7-day RefreshToken; strict rotation on every refresh.
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResultDto>
{
    private readonly IBOCDbContext      _context;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public RefreshTokenCommandHandler(
        IBOCDbContext context,
        IJwtTokenGenerator tokenGenerator)
    {
        _context        = context;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<RefreshTokenResultDto> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate the expired access token — extract claims without checking expiry
        var principal = _tokenGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            throw new ValidationException("Invalid access token — signature verification failed.");

        // 2. Extract user ID from claims
        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new ValidationException("Invalid access token — user claim missing.");

        // 3. Load user with current refresh token state
        var user = await _context.AppUsers
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new ValidationException("User not found.");

        // 4. Validate refresh token — DB stores the raw token (blueprint allows hashed variant)
        if (user.RefreshTokenHash != request.RefreshToken)
            throw new ValidationException("Invalid refresh token.");

        // 5. Check refresh token expiry (7-day window)
        if (user.RefreshTokenExpires == null || user.RefreshTokenExpires < DateTime.UtcNow)
            throw new ValidationException("Refresh token has expired. Please log in again.");

        // 6. Check account is still active
        if (user.AccountStatus == AccountStatus.Locked)
            throw new ValidationException("Account is locked. Refresh token denied.");

        // 7. Issue new token pair — STRICT ROTATION (old refresh token is invalidated)
        var tokenResult = _tokenGenerator.GenerateTokens(user);

        user.RefreshTokenHash    = tokenResult.RefreshToken;
        user.RefreshTokenExpires = tokenResult.RefreshTokenExpires;
        user.ModifiedAt          = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResultDto
        {
            AccessToken         = tokenResult.AccessToken,
            RefreshToken        = tokenResult.RefreshToken,
            RefreshTokenExpires = tokenResult.RefreshTokenExpires
        };
    }
}
