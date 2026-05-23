using System;

namespace BOC.Domain.Events;

public class MinutesFrozenEvent
{
    public Guid MinutesId { get; }
    public Guid FrozenById { get; }

    public MinutesFrozenEvent(Guid minutesId, Guid frozenById)
    {
        MinutesId = minutesId;
        FrozenById = frozenById;
    }
}
