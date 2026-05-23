using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BOC.Application.Common.Interfaces;
using BOC.Domain.Entities;
using BOC.Domain.Enums;

namespace BOC.Application.Features.Auth.Commands;

public record VerifyTwoFactorCommand(
    string Email,
    string Code,
    string DeviceFingerprint) : IRequest<VerifyTwoFactorResultDto>;

public class VerifyTwoFactorResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpires { get; set; }
}

public class VerifyTwoFactorCommandValidator : AbstractValidator<VerifyTwoFactorCommand>
{
    public VerifyTwoFactorCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .Length(6).WithMessage("TOTP code must be 6 digits.");
    }
}

public class VerifyTwoFactorCommandHandler : IRequestHandler<VerifyTwoFactorCommand, VerifyTwoFactorResultDto>
{
    private readonly IBOCDbContext _context;
    private readonly ITotpService _totpService;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public VerifyTwoFactorCommandHandler(
        IBOCDbContext context,
        ITotpService totpService,
        IJwtTokenGenerator tokenGenerator)
    {
        _context = context;
        _totpService = totpService;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<VerifyTwoFactorResultDto> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user == null)
        {
            throw new ValidationException("User not found.");
        }

        if (string.IsNullOrEmpty(user.TwoFactorSecret))
        {
            throw new ValidationException("2FA has not been initialized for this account.");
        }

        var isValid = _totpService.VerifyCode(user.TwoFactorSecret, request.Code);
        if (!isValid)
        {
            throw new ValidationException("Invalid two-factor authentication code.");
        }

        // If this is the initial verification/setup, mark it enabled
        if (!user.TwoFactorEnabled)
        {
            user.TwoFactorEnabled = true;
        }

        var tokenResult = _tokenGenerator.GenerateTokens(user);

        user.LastLoginAt = DateTime.UtcNow;
        user.DeviceFingerprint = request.DeviceFingerprint;
        user.RefreshTokenHash = tokenResult.RefreshToken;
        user.RefreshTokenExpires = tokenResult.RefreshTokenExpires;

        await _context.SaveChangesAsync(cancellationToken);

        return new VerifyTwoFactorResultDto
        {
            AccessToken = tokenResult.AccessToken,
            RefreshToken = tokenResult.RefreshToken,
            RefreshTokenExpires = tokenResult.RefreshTokenExpires
        };
    }
}
