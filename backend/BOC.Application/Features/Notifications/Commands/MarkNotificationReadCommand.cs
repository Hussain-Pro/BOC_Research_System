using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using BOC.Application.Common.Interfaces;

namespace BOC.Application.Features.Notifications.Commands;

public record MarkNotificationReadCommand(Guid UserId, Guid NotificationId) : IRequest<Unit>;

public class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NotificationId).NotEmpty();
    }
}

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Unit>
{
    private readonly IBOCDbContext _context;

    public MarkNotificationReadCommandHandler(IBOCDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        return Unit.Value; // Placeholder
    }
}
