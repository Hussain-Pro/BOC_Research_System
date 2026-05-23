using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BOC.Application.Common.Interfaces;
using BOC.Domain.Entities;
using BOC.Domain.Enums;

namespace BOC.Application.Features.Auth.Commands;

public record LoginUserCommand(
    string Email,
    string Password,
    string DeviceFingerprint) : IRequest<LoginResultDto>;

public class LoginResultDto
{
    public bool RequiresTwoFactorVerification { get; set; }
    public bool RequiresTwoFactorSetup { get; set; }
    public string? TwoFactorSecret { get; set; }
    public string? TwoFactorQrCodeUrl { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpires { get; set; }
}

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResultDto>
{
    private readonly IBOCDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ITotpService _totpService;

    public LoginUserCommandHandler(
        IBOCDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        ITotpService totpService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _totpService = totpService;
    }

    public async Task<LoginResultDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash ?? string.Empty))
        {
            throw new ValidationException("Invalid email or password.");
        }

        if (user.AccountStatus == AccountStatus.Pending_HR_Verification)
        {
            throw new ValidationException("Your account is pending HR verification and cannot log in yet.");
        }

        if (user.AccountStatus == AccountStatus.Locked)
        {
            throw new ValidationException("Your account has been locked. Please contact administration.");
        }

        // Check if 2FA is required for this role
        var requires2Fa = user.Role.Name switch
        {
            "Admin" => true,
            "Committee Chairman" => true,
            "Deputy Chairman" => true,
            "Committee Secretary" => true,
            _ => false
        };

        if (requires2Fa)
        {
            if (!user.TwoFactorEnabled)
            {
                // Generate secret and QR code URL for first-time setup
                var secret = user.TwoFactorSecret;
                if (string.IsNullOrEmpty(secret))
                {
                    secret = _totpService.GenerateSecret();
                    user.TwoFactorSecret = secret;
                    await _context.SaveChangesAsync(cancellationToken);
                }

                var qrUrl = _totpService.GetQrCodeUrl(user.Email, secret);

                return new LoginResultDto
                {
                    RequiresTwoFactorSetup = true,
                    TwoFactorSecret = secret,
                    TwoFactorQrCodeUrl = qrUrl
                };
            }

            return new LoginResultDto
            {
                RequiresTwoFactorVerification = true
            };
        }

        // If no 2FA required, proceed to issue tokens
        var tokenResult = _tokenGenerator.GenerateTokens(user);

        // Update login stats & device fingerprint
        user.LastLoginAt = DateTime.UtcNow;
        user.DeviceFingerprint = request.DeviceFingerprint;
        user.RefreshTokenHash = tokenResult.RefreshToken; // In production we can hash it, or store plain base64
        user.RefreshTokenExpires = tokenResult.RefreshTokenExpires;

        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResultDto
        {
            AccessToken = tokenResult.AccessToken,
            RefreshToken = tokenResult.RefreshToken,
            RefreshTokenExpires = tokenResult.RefreshTokenExpires
        };
    }
}
