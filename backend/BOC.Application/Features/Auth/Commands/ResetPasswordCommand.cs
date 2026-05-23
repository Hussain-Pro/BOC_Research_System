using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BOC.Application.Common.Interfaces;

namespace BOC.Application.Features.Auth.Commands;

// ─────────────────────────────────────────────────────────────────────────────
// Command
// ─────────────────────────────────────────────────────────────────────────────

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmPassword) : IRequest<Unit>;

// ─────────────────────────────────────────────────────────────────────────────
// Validator
// ─────────────────────────────────────────────────────────────────────────────

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(12).WithMessage("New password must be at least 12 characters.")
            .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("New password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("New password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Handler
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Completes the password reset flow.
/// Blueprint Reference: Section 12 — Verifies token expiry and hash, applies
/// new password, and clears all sessions.
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IBOCDbContext   _context;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IBOCDbContext   context,
        IPasswordHasher passwordHasher)
    {
        _context        = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user == null)
            throw new ValidationException("Invalid email or token.");

        // Check if there is a reset token for this user
        var resetTokenRecord = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id, cancellationToken);

        if (resetTokenRecord == null || resetTokenRecord.ExpiresAt < DateTime.UtcNow)
        {
            if (resetTokenRecord != null)
            {
                _context.PasswordResetTokens.Remove(resetTokenRecord);
                await _context.SaveChangesAsync(cancellationToken);
            }
            throw new ValidationException("Token is invalid or has expired.");
        }

        // Verify the provided raw token against the stored hash
        if (!_passwordHasher.VerifyPassword(request.Token, resetTokenRecord.TokenHash))
        {
            throw new ValidationException("Invalid token.");
        }

        // Token is valid; apply new password
        user.PasswordHash      = _passwordHasher.HashPassword(request.NewPassword);
        user.ModifiedAt        = DateTime.UtcNow;

        // Force re-login: clear all refresh tokens and sessions
        user.RefreshTokenHash    = null;
        user.RefreshTokenExpires = null;
        user.SecurityStamp       = Guid.NewGuid().ToString();

        // Consume (delete) the token so it cannot be reused
        _context.PasswordResetTokens.Remove(resetTokenRecord);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
