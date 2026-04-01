using Microsoft.EntityFrameworkCore;
using ERP.Server.Data;
using ERP.Shared.DTOs;
using ERP.Shared.Models;

namespace ERP.Server.Endpoints;

public static class CRMEndpoints
{
    public static void MapCRMEndpoints(this IEndpointRouteBuilder app)
    {
        MapLeadEndpoints(app);
        MapContactEndpoints(app);
        MapOpportunityEndpoints(app);
        MapPipelineEndpoints(app);
    }

    private static void MapLeadEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/leads").WithTags("Leads").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.Leads.AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(l => l.FirstName.Contains(req.Search) || (l.LastName != null && l.LastName.Contains(req.Search)) || (l.Company != null && l.Company.Contains(req.Search)));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(l => l.CreatedAt)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(l => new LeadDto
                {
                    Id = l.Id, FirstName = l.FirstName, LastName = l.LastName, Email = l.Email, Phone = l.Phone,
                    Company = l.Company, JobTitle = l.JobTitle, Status = l.Status, Source = l.Source,
                    OwnerName = l.OwnerName, EstimatedValue = l.EstimatedValue, LastContactDate = l.LastContactDate,
                    Notes = l.Notes, IsConverted = l.IsConverted, CreatedAt = l.CreatedAt
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<LeadDto>>.Success(new PagedResult<LeadDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapPost("/", async (LeadDto dto, AppDbContext db) =>
        {
            var lead = new Lead
            {
                FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email, Phone = dto.Phone,
                Company = dto.Company, JobTitle = dto.JobTitle, Status = dto.Status, Source = dto.Source,
                OwnerName = dto.OwnerName, EstimatedValue = dto.EstimatedValue, Notes = dto.Notes
            };
            db.Leads.Add(lead);
            await db.SaveChangesAsync();
            dto.Id = lead.Id;
            return Results.Ok(ApiResponse<LeadDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, LeadDto dto, AppDbContext db) =>
        {
            var entity = await db.Leads.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.FirstName = dto.FirstName; entity.LastName = dto.LastName; entity.Email = dto.Email;
            entity.Phone = dto.Phone; entity.Company = dto.Company; entity.JobTitle = dto.JobTitle;
            entity.Status = dto.Status; entity.Source = dto.Source; entity.OwnerName = dto.OwnerName;
            entity.EstimatedValue = dto.EstimatedValue; entity.Notes = dto.Notes; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<LeadDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Leads.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapContactEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/crm-contacts").WithTags("CRM Contacts").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.Contacts.Include(c => c.Customer).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(c => c.FirstName.Contains(req.Search) || (c.LastName != null && c.LastName.Contains(req.Search)) || (c.Company != null && c.Company.Contains(req.Search)));
            var total = await query.CountAsync();
            var items = await query.OrderBy(c => c.FirstName)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(c => new ContactDto
                {
                    Id = c.Id, FirstName = c.FirstName, LastName = c.LastName, Email = c.Email,
                    Phone = c.Phone, Mobile = c.Mobile, Company = c.Company, JobTitle = c.JobTitle,
                    Address = c.Address, City = c.City, Country = c.Country, OwnerName = c.OwnerName,
                    Notes = c.Notes, CustomerId = c.CustomerId,
                    CustomerName = c.Customer != null ? c.Customer.Name : null
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<ContactDto>>.Success(new PagedResult<ContactDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapPost("/", async (ContactDto dto, AppDbContext db) =>
        {
            var contact = new Contact
            {
                FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email,
                Phone = dto.Phone, Mobile = dto.Mobile, Company = dto.Company, JobTitle = dto.JobTitle,
                Address = dto.Address, City = dto.City, Country = dto.Country,
                OwnerName = dto.OwnerName, Notes = dto.Notes, CustomerId = dto.CustomerId
            };
            db.Contacts.Add(contact);
            await db.SaveChangesAsync();
            dto.Id = contact.Id;
            return Results.Ok(ApiResponse<ContactDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, ContactDto dto, AppDbContext db) =>
        {
            var entity = await db.Contacts.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.FirstName = dto.FirstName; entity.LastName = dto.LastName; entity.Email = dto.Email;
            entity.Phone = dto.Phone; entity.Mobile = dto.Mobile; entity.Company = dto.Company;
            entity.JobTitle = dto.JobTitle; entity.Notes = dto.Notes; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<ContactDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Contacts.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapOpportunityEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/opportunities").WithTags("Opportunities").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.Opportunities.Include(o => o.Customer).Include(o => o.Contact).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(o => o.Name.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(o => o.CreatedAt)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(o => new OpportunityDto
                {
                    Id = o.Id, Name = o.Name, ContactId = o.ContactId,
                    ContactName = o.Contact != null ? o.Contact.FirstName + " " + o.Contact.LastName : null,
                    CustomerId = o.CustomerId, CustomerName = o.Customer != null ? o.Customer.Name : null,
                    Stage = o.Stage, Amount = o.Amount, Probability = o.Probability,
                    ExpectedRevenue = o.Amount * o.Probability / 100,
                    CloseDate = o.CloseDate, OwnerName = o.OwnerName, Description = o.Description
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<OpportunityDto>>.Success(new PagedResult<OpportunityDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapPost("/", async (OpportunityDto dto, AppDbContext db) =>
        {
            var opp = new Opportunity
            {
                Name = dto.Name, ContactId = dto.ContactId, CustomerId = dto.CustomerId,
                Stage = dto.Stage, Amount = dto.Amount, Probability = dto.Probability,
                CloseDate = dto.CloseDate, OwnerName = dto.OwnerName, Description = dto.Description
            };
            db.Opportunities.Add(opp);
            await db.SaveChangesAsync();
            dto.Id = opp.Id;
            return Results.Ok(ApiResponse<OpportunityDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, OpportunityDto dto, AppDbContext db) =>
        {
            var entity = await db.Opportunities.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.Name = dto.Name; entity.Stage = dto.Stage; entity.Amount = dto.Amount;
            entity.Probability = dto.Probability; entity.CloseDate = dto.CloseDate;
            entity.OwnerName = dto.OwnerName; entity.Description = dto.Description; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<OpportunityDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Opportunities.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapPipelineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/pipeline", async (AppDbContext db) =>
        {
            var opps = await db.Opportunities.Include(o => o.Customer)
                .OrderBy(o => o.Stage).ThenByDescending(o => o.Amount)
                .Select(o => new OpportunityDto
                {
                    Id = o.Id, Name = o.Name, CustomerId = o.CustomerId,
                    CustomerName = o.Customer != null ? o.Customer.Name : null,
                    Stage = o.Stage, Amount = o.Amount, Probability = o.Probability,
                    ExpectedRevenue = o.Amount * o.Probability / 100,
                    CloseDate = o.CloseDate, OwnerName = o.OwnerName
                }).ToListAsync();

            var stages = Enum.GetValues<OpportunityStage>()
                .Select(stage => new PipelineStageDto
                {
                    Stage = stage.ToString(),
                    Opportunities = opps.Where(o => o.Stage == stage).ToList(),
                    Count = opps.Count(o => o.Stage == stage),
                    TotalAmount = opps.Where(o => o.Stage == stage).Sum(o => o.Amount)
                }).ToList();

            return Results.Ok(ApiResponse<List<PipelineStageDto>>.Success(stages));
        }).WithTags("Pipeline").RequireAuthorization();
    }
}
