using ERP.Shared.Models;

namespace ERP.Shared.DTOs;

public class LeadDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public LeadStatus Status { get; set; }
    public LeadSource Source { get; set; }
    public string? OwnerName { get; set; }
    public decimal? EstimatedValue { get; set; }
    public DateTime? LastContactDate { get; set; }
    public string? Notes { get; set; }
    public bool IsConverted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ContactDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? OwnerName { get; set; }
    public string? Notes { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
}

public class OpportunityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public OpportunityStage Stage { get; set; }
    public decimal Amount { get; set; }
    public decimal Probability { get; set; }
    public decimal ExpectedRevenue { get; set; }
    public DateTime? CloseDate { get; set; }
    public string? OwnerName { get; set; }
    public string? Description { get; set; }
}

public class ActivityDto
{
    public int Id { get; set; }
    public ActivityType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public ActivityStatus Status { get; set; }
    public string? AssigneeName { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public int? OpportunityId { get; set; }
    public string? OpportunityName { get; set; }
}

public class PipelineStageDto
{
    public string Stage { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OpportunityDto> Opportunities { get; set; } = [];
}
