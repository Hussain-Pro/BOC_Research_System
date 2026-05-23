using System;
using BOC.Domain.Enums;

namespace BOC.Domain.Exceptions;

public class InvalidStateTransitionException : Exception
{
    public ResearchState From { get; }
    public ResearchState To { get; }

    public InvalidStateTransitionException(ResearchState from, ResearchState to)
        : base($"Invalid transition of research paper state from '{from}' to '{to}'.")
    {
        From = from;
        To = to;
    }
}
