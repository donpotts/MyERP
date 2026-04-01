using Microsoft.EntityFrameworkCore;
using ERP.Server.Data;
using ERP.Shared.DTOs;
using ERP.Shared.Models;

namespace ERP.Server.Endpoints;

public static class ProjectsEndpoints
{
    public static void MapProjectsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.Projects.Include(p => p.Customer).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(p => p.Name.Contains(req.Search) || (p.Code != null && p.Code.Contains(req.Search)));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(p => p.StartDate)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(p => new ProjectDto
                {
                    Id = p.Id, Name = p.Name, Code = p.Code, Description = p.Description,
                    CustomerId = p.CustomerId, CustomerName = p.Customer != null ? p.Customer.Name : null,
                    Status = p.Status, StartDate = p.StartDate, EndDate = p.EndDate, ActualEndDate = p.ActualEndDate,
                    Budget = p.Budget, ActualCost = p.ActualCost, ProjectManagerName = p.ProjectManagerName,
                    CompletionPercent = p.CompletionPercent,
                    TaskCount = db.ProjectTasks.Count(t => t.ProjectId == p.Id),
                    CompletedTaskCount = db.ProjectTasks.Count(t => t.ProjectId == p.Id && t.Status == ProjectTaskStatus.Done),
                    TotalHours = db.TimeEntries.Where(te => te.ProjectId == p.Id).Sum(te => te.Hours)
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<ProjectDto>>.Success(new PagedResult<ProjectDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapPost("/", async (ProjectDto dto, AppDbContext db) =>
        {
            var project = new Project
            {
                Name = dto.Name, Code = dto.Code, Description = dto.Description, CustomerId = dto.CustomerId,
                Status = dto.Status, StartDate = dto.StartDate, EndDate = dto.EndDate,
                Budget = dto.Budget, ProjectManagerName = dto.ProjectManagerName
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            dto.Id = project.Id;
            return Results.Ok(ApiResponse<ProjectDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, ProjectDto dto, AppDbContext db) =>
        {
            var entity = await db.Projects.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.Name = dto.Name; entity.Code = dto.Code; entity.Description = dto.Description;
            entity.CustomerId = dto.CustomerId; entity.Status = dto.Status; entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate; entity.Budget = dto.Budget; entity.ProjectManagerName = dto.ProjectManagerName;
            entity.CompletionPercent = dto.CompletionPercent; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<ProjectDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Projects.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });

        // Tasks
        var tasksGroup = app.MapGroup("/api/project-tasks").WithTags("Project Tasks").RequireAuthorization();

        tasksGroup.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.ProjectTasks.Include(t => t.Project).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(t => t.Title.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(t => t.CreatedAt)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(t => new ProjectTaskDto
                {
                    Id = t.Id, ProjectId = t.ProjectId, ProjectName = t.Project != null ? t.Project.Name : null,
                    Title = t.Title, Description = t.Description, Status = t.Status, Priority = t.Priority,
                    AssigneeName = t.AssigneeName, DueDate = t.DueDate, CompletedDate = t.CompletedDate,
                    EstimatedHours = t.EstimatedHours, ActualHours = t.ActualHours
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<ProjectTaskDto>>.Success(new PagedResult<ProjectTaskDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        tasksGroup.MapPost("/", async (ProjectTaskDto dto, AppDbContext db) =>
        {
            var task = new ProjectTask
            {
                ProjectId = dto.ProjectId, MilestoneId = dto.MilestoneId, Title = dto.Title,
                Description = dto.Description, Status = dto.Status, Priority = dto.Priority,
                AssigneeName = dto.AssigneeName, DueDate = dto.DueDate, EstimatedHours = dto.EstimatedHours
            };
            db.ProjectTasks.Add(task);
            await db.SaveChangesAsync();
            dto.Id = task.Id;
            return Results.Ok(ApiResponse<ProjectTaskDto>.Success(dto));
        });

        tasksGroup.MapPut("/{id:int}", async (int id, ProjectTaskDto dto, AppDbContext db) =>
        {
            var entity = await db.ProjectTasks.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.Title = dto.Title; entity.Description = dto.Description; entity.Status = dto.Status;
            entity.Priority = dto.Priority; entity.AssigneeName = dto.AssigneeName; entity.DueDate = dto.DueDate;
            entity.EstimatedHours = dto.EstimatedHours; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<ProjectTaskDto>.Success(dto));
        });

        // Time Entries
        var timeGroup = app.MapGroup("/api/time-entries").WithTags("Time Entries").RequireAuthorization();

        timeGroup.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.TimeEntries.Include(te => te.Project).Include(te => te.Task).AsQueryable();
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(te => te.EntryDate)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(te => new TimeEntryDto
                {
                    Id = te.Id, ProjectId = te.ProjectId, ProjectName = te.Project != null ? te.Project.Name : null,
                    TaskId = te.TaskId, TaskTitle = te.Task != null ? te.Task.Title : null,
                    UserName = te.UserName, EntryDate = te.EntryDate, Hours = te.Hours,
                    Description = te.Description, IsBillable = te.IsBillable, HourlyRate = te.HourlyRate
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<TimeEntryDto>>.Success(new PagedResult<TimeEntryDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        timeGroup.MapPost("/", async (TimeEntryDto dto, AppDbContext db) =>
        {
            var entry = new TimeEntry
            {
                ProjectId = dto.ProjectId, TaskId = dto.TaskId, UserId = "current-user",
                UserName = dto.UserName, EntryDate = dto.EntryDate, Hours = dto.Hours,
                Description = dto.Description, IsBillable = dto.IsBillable, HourlyRate = dto.HourlyRate
            };
            db.TimeEntries.Add(entry);
            await db.SaveChangesAsync();
            dto.Id = entry.Id;
            return Results.Ok(ApiResponse<TimeEntryDto>.Success(dto));
        });
    }
}
