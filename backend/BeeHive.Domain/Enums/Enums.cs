namespace BeeHive.Domain.Enums;

public enum BeehiveType
{
    Langstroth = 1,
    DadantBlatt = 2,
    Warré = 3,
    TopBar = 4,
    Other = 5
}

public enum BeehiveMaterial
{
    Wood = 1,
    Plastic = 2,
    Polystyrene = 3
}

public enum HoneyLevel
{
    Low = 1,
    Medium = 2,
    High = 3
}

public enum TodoPriority
{
    Low    = 1,
    Medium = 2,
    High   = 3,
}

public enum DietStatus
{
    NotStarted  = 1,
    InProgress  = 2,
    Completed   = 3,
    StoppedEarly = 4,
}

public enum FeedingEntryStatus
{
    Pending   = 1,
    Completed = 2,
}

public enum DietReason
{
    LackOfFood               = 1,
    WinterFeeding            = 2,
    SpringStimulation        = 3,
    NewSwarmSupport          = 4,
    PostHarvestRecovery      = 5,
    DroughtConditions        = 6,
    WeakColonySupport        = 7,
    QueenIntroductionSupport = 8,
    Custom                   = 9,
}

public enum FoodType
{
    SugarSyrup      = 1,
    Fondant         = 2,
    Pollen          = 3,
    ProteinPatties  = 4,
    Custom          = 5,
}

public enum UserRole
{
    Admin       = 1,
    SystemAdmin = 2,
}
