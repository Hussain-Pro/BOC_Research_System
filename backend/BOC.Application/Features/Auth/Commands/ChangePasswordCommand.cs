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

public record ChangePasswordCommand(
    Guid   UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword) : IRequest<Unit>;

// ─────────────────────────────────────────────────────────────────────────────
// Validator
// ─────────────────────────────────────────────────────────────────────────────

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

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
/// Changes an authenticated user's password.
/// Verifies current password before accepting the new one.
/// Forces re-login by clearing the refresh token after change.
/// Blueprint Reference: Section 12 — Password policy: 12+ chars, upper+lower+digit+special.
///                      Section 12 — Force re-login on password change (clear refresh token).
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IBOCDbContext  _context;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(IBOCDbContext context, IPasswordHasher passwordHasher)
    {
        _context        = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new ValidationException("User not found.");

        // Verify current password
        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash ?? string.Empty))
            throw new ValidationException("Current password is incorrect.");

        // Ensure new password is different from current
        if (_passwordHasher.VerifyPassword(request.NewPassword, user.PasswordHash ?? string.Empty))
            throw new ValidationException("New password must be different from the current password.");

        // Apply new password
        user.PasswordHash        = _passwordHasher.HashPassword(request.NewPassword);
        user.ModifiedAt          = DateTime.UtcNow;

        // Force re-login — invalidate all existing sessions
        user.RefreshTokenHash    = null;
        user.RefreshTokenExpires = null;
        user.SecurityStamp       = Guid.NewGuid().ToString(); // Invalidates all JWT tokens

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
