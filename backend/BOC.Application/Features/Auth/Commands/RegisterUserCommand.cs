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

public record RegisterUserCommand(
    string EmployeeID,
    string NationalID,
    string FullName,
    string Email,
    string Password,
    Guid RoleId,
    Guid? DepartmentId,
    Guid? DirectorateId,
    DateOnly BirthDate) : IRequest<Guid>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.EmployeeID)
            .NotEmpty().WithMessage("Employee ID is required.")
            .MaximumLength(50);

        RuleFor(x => x.NationalID)
            .NotEmpty().WithMessage("National ID is required.")
            .MaximumLength(50);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role is required.");

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth date is required.");
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IBOCDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IBOCDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check uniqueness of Email, EmployeeID, and NationalID
        if (await _context.AppUsers.AnyAsync(u => u.Email == request.Email.Trim(), cancellationToken))
            throw new ValidationException("A user with this Email already exists.");

        if (await _context.AppUsers.AnyAsync(u => u.EmployeeID == request.EmployeeID.Trim(), cancellationToken))
            throw new ValidationException("A user with this Employee ID already exists.");

        if (await _context.AppUsers.AnyAsync(u => u.NationalID == request.NationalID.Trim(), cancellationToken))
            throw new ValidationException("A user with this National ID already exists.");

        var role = await _context.AppRoles.FindAsync(new object[] { request.RoleId }, cancellationToken)
            ?? throw new ValidationException("Selected Role was not found.");

        var user = new AppUser
            {
                Id = Guid.NewGuid(),
                EmployeeID = request.EmployeeID.Trim(),
                NationalID = request.NationalID.Trim(),
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                RoleId = request.RoleId,
                DepartmentId = request.DepartmentId,
                DirectorateId = request.DirectorateId,
                BirthDate = request.BirthDate,
                EvaluatorStatus = EvaluatorStatus.Active,
                AccountStatus = AccountStatus.Pending_HR_Verification,
                IsEmailConfirmed = false,
                AccessFailedCount = 0,
                TwoFactorEnabled = false, // Enrolled on first login for administrative tiers
                CreatedAt = DateTime.UtcNow
            };

        _context.AppUsers.Add(user);

        // Put in HR Verification Queue
        var queueEntry = new HRVerificationQueue
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EmployeeID = user.EmployeeID,
            NationalID = user.NationalID,
            FullName = user.FullName,
            Status = HRVerificationStatus.Pending,
            SubmittedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.HRVerificationQueue.Add(queueEntry);

        await _context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
