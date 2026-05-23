using System.Collections.Generic;
using BOC.Domain.Enums;
using BOC.Domain.Exceptions;

namespace BOC.Domain.Fsm;

public static class ResearchStateMachine
{
    private static readonly Dictionary<ResearchState, HashSet<ResearchState>> AllowedTransitions = new()
    {
        {
            ResearchState.Draft,
            new() { ResearchState.Pending_Secretary_Screening, ResearchState.Force_Majeure_Retired, ResearchState.Force_Majeure_Deceased }
        },
        {
            ResearchState.Pending_Secretary_Screening,
            new() { ResearchState.Non_Compliant_Returned, ResearchState.Incoming_Triage_Queue, ResearchState.Force_Majeure_Retired, ResearchState.Force_Majeure_Deceased }
        },
        {
            ResearchState.Non_Compliant_Returned,
            new() { ResearchState.Draft, ResearchState.Force_Majeure_Retired, ResearchState.Force_Majeure_Deceased }
        },
        {
            ResearchState.Incoming_Triage_Queue,
            new() { ResearchState.Dispatched_To_Evaluators, ResearchState.Substituted, ResearchState.Force_Majeure_Retired, ResearchState.Force_Majeure_Deceased }
        },
        {
            ResearchState.Dispatched_To_Evaluators,
            new() { ResearchState.Pending_Chairman_Grading, ResearchState.Suspended_Plagiarism_Lockout, ResearchState.Force_Majeure_Retired, ResearchState.Force_Majeure_Deceased }
        },
        {
            ResearchState.Pending_Chairman_Grading,
            new() { ResearchState.Pass_Approved, ResearchState.Fail_Rejected, ResearchState.Substituted, ResearchState.Force_Majeure_Retired, ResearchState.Force_Majeure_Deceased }
        },
        {
            ResearchState.Substituted,
            new() { ResearchState.Incoming_Triage_Queue, ResearchState.Force_Majeure_Retired, ResearchState.Force_Majeure_Deceased }
        },
        {
            ResearchState.Suspended_Plagiarism_Lockout,
            new() { ResearchState.Fail_Rejected, ResearchState.Incoming_Triage_Queue, ResearchState.Force_Majeure_Retired, ResearchState.Force_Majeure_Deceased }
        },
        {
            ResearchState.Pass_Approved,
            new() { ResearchState.Archived, ResearchState.Ministry_Batch_Transit, ResearchState.Force_Majeure_Retired, ResearchState.Force_Majeure_Deceased }
        },
        {
            ResearchState.Fail_Rejected,
            new() { ResearchState.Archived, ResearchState.Force_Majeure_Retired, ResearchState.Force_Majeure_Deceased }
        },
        {
            ResearchState.Force_Majeure_Retired,
            new() { ResearchState.Archived }
        },
        {
            ResearchState.Force_Majeure_Deceased,
            new() { ResearchState.Archived }
        },
        {
            ResearchState.Ministry_Batch_Transit,
            new() { ResearchState.Pass_Approved, ResearchState.Fail_Rejected, ResearchState.Force_Majeure_Retired, ResearchState.Force_Majeure_Deceased }
        },
        {
            ResearchState.Archived,
            new() // Immutable terminal state. No transitions permitted.
        }
    };

    public static bool CanTransition(ResearchState from, ResearchState to)
    {
        if (AllowedTransitions.TryGetValue(from, out var toStates))
        {
            return toStates.Contains(to);
        }
        return false;
    }

    public static void ValidateTransition(ResearchState from, ResearchState to)
    {
        if (!CanTransition(from, to))
        {
            throw new InvalidStateTransitionException(from, to);
        }
    }
}
