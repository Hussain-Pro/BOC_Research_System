using System;
using System.Collections.Generic;

namespace BOC.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    private readonly List<object> _domainEvents = new();
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(object domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
