using Microsoft.EntityFrameworkCore;
using ERP.Server.Data;
using ERP.Shared.DTOs;
using ERP.Shared.Models;

namespace ERP.Server.Endpoints;

public static class FinanceEndpoints
{
    public static void MapFinanceEndpoints(this IEndpointRouteBuilder app)
    {
        MapAccountEndpoints(app);
        MapJournalEndpoints(app);
        MapAPEndpoints(app);
        MapAREndpoints(app);
        MapBankEndpoints(app);
    }

    private static void MapAccountEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts").WithTags("Accounts").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.Accounts.AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(a => a.Name.Contains(req.Search) || a.Code.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderBy(a => a.Code)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(a => new AccountDto
                {
                    Id = a.Id, Code = a.Code, Name = a.Name, Type = a.Type, Status = a.Status,
                    ParentId = a.ParentId, Description = a.Description, Balance = a.Balance
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<AccountDto>>.Success(new PagedResult<AccountDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapGet("/all", async (AppDbContext db) =>
        {
            var items = await db.Accounts.OrderBy(a => a.Code).Select(a => new AccountDto { Id = a.Id, Code = a.Code, Name = a.Name, Type = a.Type }).ToListAsync();
            return Results.Ok(ApiResponse<List<AccountDto>>.Success(items));
        });

        group.MapPost("/", async (AccountDto dto, AppDbContext db) =>
        {
            var account = new Account { Code = dto.Code, Name = dto.Name, Type = dto.Type, ParentId = dto.ParentId, Description = dto.Description };
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
            dto.Id = account.Id;
            return Results.Ok(ApiResponse<AccountDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, AccountDto dto, AppDbContext db) =>
        {
            var entity = await db.Accounts.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.Code = dto.Code; entity.Name = dto.Name; entity.Type = dto.Type;
            entity.ParentId = dto.ParentId; entity.Description = dto.Description; entity.Status = dto.Status;
            entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<AccountDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Accounts.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapJournalEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/journal-entries").WithTags("Journal Entries").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.JournalEntries.AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(j => j.Reference.Contains(req.Search) || (j.Description != null && j.Description.Contains(req.Search)));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(j => j.EntryDate)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(j => new JournalEntryDto
                {
                    Id = j.Id, Reference = j.Reference, EntryDate = j.EntryDate,
                    Description = j.Description, Status = j.Status, TotalDebit = j.TotalDebit, TotalCredit = j.TotalCredit
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<JournalEntryDto>>.Success(new PagedResult<JournalEntryDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapPost("/", async (JournalEntryDto dto, AppDbContext db) =>
        {
            var entry = new JournalEntry
            {
                Reference = dto.Reference, EntryDate = dto.EntryDate, Description = dto.Description,
                Status = dto.Status, TotalDebit = dto.TotalDebit, TotalCredit = dto.TotalCredit
            };
            db.JournalEntries.Add(entry);
            await db.SaveChangesAsync();
            dto.Id = entry.Id;
            return Results.Ok(ApiResponse<JournalEntryDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.JournalEntries.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapAPEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ap-invoices").WithTags("Accounts Payable").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.APInvoices.Include(i => i.Vendor).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(i => i.InvoiceNumber.Contains(req.Search) || i.Vendor!.Name.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(i => i.InvoiceDate)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(i => new APInvoiceDto
                {
                    Id = i.Id, InvoiceNumber = i.InvoiceNumber, VendorId = i.VendorId,
                    VendorName = i.Vendor != null ? i.Vendor.Name : null,
                    InvoiceDate = i.InvoiceDate, DueDate = i.DueDate, SubTotal = i.SubTotal,
                    TaxAmount = i.TaxAmount, TotalAmount = i.TotalAmount, PaidAmount = i.PaidAmount,
                    Status = i.Status, Notes = i.Notes
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<APInvoiceDto>>.Success(new PagedResult<APInvoiceDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapPost("/", async (APInvoiceDto dto, AppDbContext db) =>
        {
            var invoice = new APInvoice
            {
                InvoiceNumber = dto.InvoiceNumber, VendorId = dto.VendorId, InvoiceDate = dto.InvoiceDate,
                DueDate = dto.DueDate, SubTotal = dto.SubTotal, TaxAmount = dto.TaxAmount,
                TotalAmount = dto.TotalAmount, Status = dto.Status, Notes = dto.Notes
            };
            db.APInvoices.Add(invoice);
            await db.SaveChangesAsync();
            dto.Id = invoice.Id;
            return Results.Ok(ApiResponse<APInvoiceDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, APInvoiceDto dto, AppDbContext db) =>
        {
            var entity = await db.APInvoices.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.InvoiceNumber = dto.InvoiceNumber; entity.VendorId = dto.VendorId;
            entity.InvoiceDate = dto.InvoiceDate; entity.DueDate = dto.DueDate;
            entity.SubTotal = dto.SubTotal; entity.TaxAmount = dto.TaxAmount;
            entity.TotalAmount = dto.TotalAmount; entity.PaidAmount = dto.PaidAmount;
            entity.Status = dto.Status; entity.Notes = dto.Notes; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<APInvoiceDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.APInvoices.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapAREndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ar-invoices").WithTags("Accounts Receivable").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.ARInvoices.Include(i => i.Customer).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(i => i.InvoiceNumber.Contains(req.Search) || i.Customer!.Name.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(i => i.InvoiceDate)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(i => new ARInvoiceDto
                {
                    Id = i.Id, InvoiceNumber = i.InvoiceNumber, CustomerId = i.CustomerId,
                    CustomerName = i.Customer != null ? i.Customer.Name : null,
                    InvoiceDate = i.InvoiceDate, DueDate = i.DueDate, SubTotal = i.SubTotal,
                    TaxAmount = i.TaxAmount, TotalAmount = i.TotalAmount, PaidAmount = i.PaidAmount,
                    Status = i.Status, Notes = i.Notes
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<ARInvoiceDto>>.Success(new PagedResult<ARInvoiceDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapPost("/", async (ARInvoiceDto dto, AppDbContext db) =>
        {
            var invoice = new ARInvoice
            {
                InvoiceNumber = dto.InvoiceNumber, CustomerId = dto.CustomerId, InvoiceDate = dto.InvoiceDate,
                DueDate = dto.DueDate, SubTotal = dto.SubTotal, TaxAmount = dto.TaxAmount,
                TotalAmount = dto.TotalAmount, Status = dto.Status, Notes = dto.Notes
            };
            db.ARInvoices.Add(invoice);
            await db.SaveChangesAsync();
            dto.Id = invoice.Id;
            return Results.Ok(ApiResponse<ARInvoiceDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.ARInvoices.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapBankEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bank-accounts").WithTags("Bank Accounts").RequireAuthorization();

        group.MapGet("/", async (AppDbContext db) =>
        {
            var items = await db.BankAccounts.Where(b => b.IsActive)
                .Select(b => new BankAccountDto { Id = b.Id, Name = b.Name, AccountNumber = b.AccountNumber, BankName = b.BankName, Currency = b.Currency, Balance = b.Balance, IsActive = b.IsActive })
                .ToListAsync();
            return Results.Ok(ApiResponse<List<BankAccountDto>>.Success(items));
        });

        group.MapGet("/{id:int}/transactions", async (int id, [AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.BankTransactions.Where(t => t.BankAccountId == id).AsQueryable();
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(t => t.TransactionDate)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(t => new BankTransactionDto
                {
                    Id = t.Id, BankAccountId = t.BankAccountId, TransactionDate = t.TransactionDate,
                    Reference = t.Reference, Description = t.Description, Type = t.Type.ToString(),
                    Amount = t.Amount, RunningBalance = t.RunningBalance, IsReconciled = t.IsReconciled
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<BankTransactionDto>>.Success(new PagedResult<BankTransactionDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapPost("/", async (BankAccountDto dto, AppDbContext db) =>
        {
            var account = new BankAccount { Name = dto.Name, AccountNumber = dto.AccountNumber, BankName = dto.BankName, Currency = dto.Currency, Balance = dto.Balance };
            db.BankAccounts.Add(account);
            await db.SaveChangesAsync();
            dto.Id = account.Id;
            return Results.Ok(ApiResponse<BankAccountDto>.Success(dto));
        });
    }
}
