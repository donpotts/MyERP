using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Shared.Models;

public enum ProjectStatus { Planning, Active, OnHold, Completed, Cancelled }
public enum ProjectTaskStatus { Todo, InProgress, Review, Done, Cancelled }
public enum ProjectTaskPriority { Low, Medium, High, Critical }
public enum MilestoneStatus { Pending, Achieved, Missed }

public class Project : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string? Code { get; set; }
    [MaxLength(2000)]
    public string? Description { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Budget { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ActualCost { get; set; }
    [MaxLength(100)]
    public string? ProjectManagerId { get; set; }
    [MaxLength(100)]
    public string? ProjectManagerName { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal CompletionPercent { get; set; }
    public List<ProjectMember> Members { get; set; } = [];
    public List<Milestone> Milestones { get; set; } = [];
    public List<ProjectTask> Tasks { get; set; } = [];
}

public class ProjectMember : BaseEntity
{
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    [Required, MaxLength(100)]
    public string UserId { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? UserName { get; set; }
    [MaxLength(100)]
    public string? Role { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "decimal(5,2)")]
    public decimal AllocationPercent { get; set; } = 100;
}

public class Milestone : BaseEntity
{
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? AchievedDate { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;
}

public class ProjectTask : BaseEntity
{
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public int? MilestoneId { get; set; }
    public Milestone? Milestone { get; set; }
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(2000)]
    public string? Description { get; set; }
    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.Todo;
    public ProjectTaskPriority Priority { get; set; } = ProjectTaskPriority.Medium;
    [MaxLength(100)]
    public string? AssigneeId { get; set; }
    [MaxLength(100)]
    public string? AssigneeName { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    [Column(TypeName = "decimal(8,2)")]
    public decimal EstimatedHours { get; set; }
    [Column(TypeName = "decimal(8,2)")]
    public decimal ActualHours { get; set; }
    public int? ParentTaskId { get; set; }
    public List<TimeEntry> TimeEntries { get; set; } = [];
}

public class TimeEntry : BaseEntity
{
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public int? TaskId { get; set; }
    public ProjectTask? Task { get; set; }
    [Required, MaxLength(100)]
    public string UserId { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? UserName { get; set; }
    public DateTime EntryDate { get; set; }
    [Column(TypeName = "decimal(8,2)")]
    public decimal Hours { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    public bool IsBillable { get; set; } = true;
    [Column(TypeName = "decimal(18,2)")]
    public decimal HourlyRate { get; set; }
}
