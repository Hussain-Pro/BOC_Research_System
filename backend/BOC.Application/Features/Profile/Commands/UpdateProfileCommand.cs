using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using BOC.Application.Common.Interfaces;

namespace BOC.Application.Features.Profile.Commands;

public record UpdateProfileCommand(Guid UserId, string Theme, string Language) : IRequest<Unit>;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Theme).NotEmpty();
        RuleFor(x => x.Language).NotEmpty();
    }
}

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Unit>
{
    private readonly IBOCDbContext _context;

    public UpdateProfileCommandHandler(IBOCDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        // Placeholder
        return Unit.Value;
    }
}
