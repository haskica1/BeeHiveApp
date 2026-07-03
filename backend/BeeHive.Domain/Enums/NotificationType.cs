namespace BeeHive.Domain.Enums;

public enum NotificationType
{
    AccountCreated         = 1,
    OrganizationAssigned   = 2,
    OrganizationUnassigned = 3,
    ApiaryAssigned         = 4,
    ApiaryUnassigned       = 5,
    BeehiveAssigned        = 6,
    BeehiveUnassigned      = 7,
    BeehiveCreated         = 8,
    TodoCreated            = 9,

    // ── Smart alerts & weekly summary (SPEC-04) ──
    InspectionOverdue      = 10,
    HoneyLevelDrop         = 11,
    FrostWarning           = 12,
    OldQueen               = 13,
    WeeklySummary          = 14,
}
