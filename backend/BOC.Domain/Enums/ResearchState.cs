namespace BOC.Domain.Enums;

public enum ResearchState
{
    Draft,
    Pending_Secretary_Screening,
    Non_Compliant_Returned,
    Incoming_Triage_Queue,
    Dispatched_To_Evaluators,
    Pending_Chairman_Grading,
    Substituted,
    Suspended_Plagiarism_Lockout,
    Force_Majeure_Retired,
    Force_Majeure_Deceased,
    Ministry_Batch_Transit,
    Pass_Approved,
    Fail_Rejected,
    Archived
}
