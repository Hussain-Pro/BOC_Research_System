using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BOC.Application.Common.Interfaces;
using BOC.Domain.Entities;

namespace BOC.Application.Features.Auth.Commands;

// ─────────────────────────────────────────────────────────────────────────────
// Command
// ─────────────────────────────────────────────────────────────────────────────

public record ForgotPasswordCommand(string Email) : IRequest<Unit>;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Handler
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Initiates the password reset flow:
///   1. Finds user by email (silent success even if email not found — anti-enumeration).
///   2. Generates a cryptographically secure reset token (stored hashed, 15-min expiry).
///   3. Sends a reset-link email via IEmailSender.
/// Blueprint Reference: Section 12 — ForgotPassword; 15-min token; no enumeration disclosure.
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IBOCDbContext   _context;
    private readonly IEmailSender    _emailSender;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration  _configuration;

    public ForgotPasswordCommandHandler(
        IBOCDbContext   context,
        IEmailSender    emailSender,
        IPasswordHasher passwordHasher,
        IConfiguration  configuration)
    {
        _context         = context;
        _emailSender     = emailSender;
        _passwordHasher  = passwordHasher;
        _configuration   = configuration;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Anti-enumeration: never reveal whether email exists — always return success
        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user == null)
            return Unit.Value; // Silent — do not reveal email existence

        // Generate raw token (URL-safe) and its hash for DB storage
        var rawToken   = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64))
                               .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        var tokenHash  = _passwordHasher.HashPassword(rawToken);
        var expiresAt  = DateTime.UtcNow.AddMinutes(15);

        // Persist (one active token per user — remove old ones)
        var existingTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.Id)
            .ToListAsync(cancellationToken);
        _context.PasswordResetTokens.RemoveRange(existingTokens);

        _context.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId    = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt
        });

        await _context.SaveChangesAsync(cancellationToken);

        // Build reset link
        var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:4200";
        var resetLink       = $"{frontendBaseUrl}/auth/reset-password?token={rawToken}&email={Uri.EscapeDataString(user.Email)}";

        // Send email
        var subject = "إعادة تعيين كلمة المرور — نظام تقييم البحوث";
        var body    = BuildResetEmailBody(user.FullName, resetLink);

        await _emailSender.SendEmailAsync(user.Email, subject, body, cancellationToken);

        return Unit.Value;
    }

    private static string BuildResetEmailBody(string fullName, string resetLink) => $@"
<html>
<body dir='rtl' style='font-family: Tajawal, Arial, sans-serif; color: #0F2A38;'>
  <div style='max-width:600px; margin:0 auto; padding:24px; border:1px solid #e0e0e0; border-radius:6px;'>
    <h2 style='color:#163E54;'>إعادة تعيين كلمة المرور</h2>
    <p>السيد/ة <strong>{fullName}</strong>،</p>
    <p>تلقّينا طلباً لإعادة تعيين كلمة المرور الخاصة بحسابك في نظام تقييم بحوث شركة نفط البصرة.</p>
    <p>الرجاء النقر على الزر أدناه لإعادة تعيين كلمة المرور. صلاحية هذا الرابط <strong>15 دقيقة فقط</strong>.</p>
    <div style='text-align:center; margin:32px 0;'>
      <a href='{resetLink}'
         style='background:#163E54; color:#fff; padding:12px 28px; text-decoration:none;
                border-radius:6px; font-size:16px; display:inline-block;'>
        إعادة تعيين كلمة المرور
      </a>
    </div>
    <p style='color:#d9534f; font-size:13px;'>
      إذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذه الرسالة.
    </p>
    <hr style='border-color:#163E54;'/>
    <p style='font-size:12px; color:#4A607A;'>
      هذه رسالة آلية من نظام إدارة وتقييم البحوث — شركة نفط البصرة.
    </p>
  </div>
</body>
</html>";
}
