namespace Api.Models;

/// <summary>What a record in an inspection card is about.</summary>
public enum InspectionKind
{
    Inspection,
    Failure,
}

/// <summary>Records entered by a technician wait for an admin; admin entries are approved at once.</summary>
public enum ReviewStatus
{
    Pending,
    Approved,
    Rejected,
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical,
}

public enum TicketStatus
{
    New,
    InProgress,
    Resolved,
    Closed,
}
