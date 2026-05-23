using System;

namespace BOC.Domain.Exceptions;

public class FrozenMinutesException : Exception
{
    public Guid MinutesId { get; }

    public FrozenMinutesException(Guid minutesId)
        : base($"Cannot modify associated entities because the meeting minutes (ID: {minutesId}) are frozen.")
    {
        MinutesId = minutesId;
    }
}
