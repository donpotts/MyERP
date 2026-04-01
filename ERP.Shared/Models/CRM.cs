using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Shared.Models;

public enum LeadStatus { New, Contacted, Qualified, Proposal, Negotiation, Won, Lost }
public enum LeadSource { Website, Referral, Email, Phone, Social, Event, Other }
public enum OpportunityStage { Prospecting, Qualification, Proposal, Negotiation, ClosedWon, ClosedLost }
public enum ActivityType { Call, Email, Meeting, Task, Note }
public enum ActivityStatus { Planned, Completed, Cancelled }

public class Lead : BaseEntity
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? LastName { get; set; }
    [MaxLength(200)]
    public string? Email { get; set; }
    [MaxLength(50)]
    public string? Phone { get; set; }
    [MaxLength(200)]
    public string? Company { get; set; }
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public LeadSource Source { get; set; } = LeadSource.Website;
    [MaxLength(100)]
    public string? OwnerId { get; set; }
    [MaxLength(100)]
    public string? OwnerName { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? EstimatedValue { get; set; }
    public DateTime? LastContactDate { get; set; }
    [MaxLength(1000)]
    public string? Notes { get; set; }
    public int? ConvertedContactId { get; set; }
    public bool IsConverted { get; set; }
}

public class Contact : BaseEntity
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? LastName { get; set; }
    [MaxLength(200)]
    public string? Email { get; set; }
    [MaxLength(50)]
    public string? Phone { get; set; }
    [MaxLength(50)]
    public string? Mobile { get; set; }
    [MaxLength(200)]
    public string? Company { get; set; }
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    [MaxLength(300)]
    public string? Address { get; set; }
    [MaxLength(100)]
    public string? City { get; set; }
    [MaxLength(100)]
    public string? Country { get; set; }
    [MaxLength(100)]
    public string? OwnerId { get; set; }
    [MaxLength(100)]
    public string? OwnerName { get; set; }
    [MaxLength(1000)]
    public string? Notes { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public List<Activity> Activities { get; set; } = [];
}

public class Opportunity : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public int? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public OpportunityStage Stage { get; set; } = OpportunityStage.Prospecting;
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal Probability { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ExpectedRevenue => Amount * Probability / 100;
    public DateTime? CloseDate { get; set; }
    [MaxLength(100)]
    public string? OwnerId { get; set; }
    [MaxLength(100)]
    public string? OwnerName { get; set; }
    [MaxLength(1000)]
    public string? Description { get; set; }
    public List<Activity> Activities { get; set; } = [];
}

public class Activity : BaseEntity
{
    public ActivityType Type { get; set; }
    [Required, MaxLength(300)]
    public string Subject { get; set; } = string.Empty;
    [MaxLength(2000)]
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public ActivityStatus Status { get; set; } = ActivityStatus.Planned;
    [MaxLength(100)]
    public string? AssigneeId { get; set; }
    [MaxLength(100)]
    public string? AssigneeName { get; set; }
    public int? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public int? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
}
