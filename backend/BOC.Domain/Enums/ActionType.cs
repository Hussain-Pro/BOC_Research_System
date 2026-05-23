namespace BOC.Domain.Enums;

public enum ActionType
{
    CRUD,
    StatusChange,
    Login,
    Logout,
    FailedLogin,
    DelegationGranted,
    DelegationRevoked
}
