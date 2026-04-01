using ERP.Shared.Models;

namespace ERP.Shared.DTOs;

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public ProjectStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal Budget { get; set; }
    public decimal ActualCost { get; set; }
    public string? ProjectManagerName { get; set; }
    public decimal CompletionPercent { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public decimal TotalHours { get; set; }
}

public class ProjectTaskDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public int? MilestoneId { get; set; }
    public string? MilestoneName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectTaskStatus Status { get; set; }
    public ProjectTaskPriority Priority { get; set; }
    public string? AssigneeName { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
}

public class MilestoneDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? AchievedDate { get; set; }
    public MilestoneStatus Status { get; set; }
}

public class TimeEntryDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public int? TaskId { get; set; }
    public string? TaskTitle { get; set; }
    public string? UserName { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public bool IsBillable { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal BillableAmount => IsBillable ? Hours * HourlyRate : 0;
}
