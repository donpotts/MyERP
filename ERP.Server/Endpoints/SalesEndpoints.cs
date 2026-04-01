using Microsoft.EntityFrameworkCore;
using ERP.Server.Data;
using ERP.Shared.DTOs;
using ERP.Shared.Models;

namespace ERP.Server.Endpoints;

public static class SalesEndpoints
{
    public static void MapSalesEndpoints(this IEndpointRouteBuilder app)
    {
        MapCustomerEndpoints(app);
        MapQuoteEndpoints(app);
        MapSalesOrderEndpoints(app);
        MapSalesInvoiceEndpoints(app);
    }

    private static void MapCustomerEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.Customers.AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(c => c.Name.Contains(req.Search) || (c.Code != null && c.Code.Contains(req.Search)) || (c.Email != null && c.Email.Contains(req.Search)));
            var total = await query.CountAsync();
            var items = await query.OrderBy(c => c.Name)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(c => new CustomerDto
                {
                    Id = c.Id, Name = c.Name, Code = c.Code, Email = c.Email, Phone = c.Phone,
                    Address = c.Address, City = c.City, Country = c.Country,
                    TaxNumber = c.TaxNumber, PaymentTerms = c.PaymentTerms, CreditLimit = c.CreditLimit,
                    IsActive = c.IsActive, Notes = c.Notes,
                    OrderCount = db.SalesOrders.Count(so => so.CustomerId == c.Id),
                    TotalSales = db.SalesInvoices.Where(si => si.CustomerId == c.Id).Sum(si => si.PaidAmount)
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<CustomerDto>>.Success(new PagedResult<CustomerDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapGet("/all", async (AppDbContext db) =>
        {
            var items = await db.Customers.Where(c => c.IsActive).Select(c => new CustomerDto { Id = c.Id, Name = c.Name, Code = c.Code }).ToListAsync();
            return Results.Ok(ApiResponse<List<CustomerDto>>.Success(items));
        });

        group.MapPost("/", async (CustomerDto dto, AppDbContext db) =>
        {
            var customer = new Customer
            {
                Name = dto.Name, Code = dto.Code, Email = dto.Email, Phone = dto.Phone,
                Address = dto.Address, City = dto.City, Country = dto.Country,
                TaxNumber = dto.TaxNumber, PaymentTerms = dto.PaymentTerms, CreditLimit = dto.CreditLimit, Notes = dto.Notes
            };
            db.Customers.Add(customer);
            await db.SaveChangesAsync();
            dto.Id = customer.Id;
            return Results.Ok(ApiResponse<CustomerDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, CustomerDto dto, AppDbContext db) =>
        {
            var entity = await db.Customers.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.Name = dto.Name; entity.Code = dto.Code; entity.Email = dto.Email; entity.Phone = dto.Phone;
            entity.Address = dto.Address; entity.City = dto.City; entity.Country = dto.Country;
            entity.TaxNumber = dto.TaxNumber; entity.PaymentTerms = dto.PaymentTerms;
            entity.CreditLimit = dto.CreditLimit; entity.IsActive = dto.IsActive; entity.Notes = dto.Notes;
            entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<CustomerDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Customers.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapQuoteEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quotes").WithTags("Quotes").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.Quotes.Include(q => q.Customer).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(q => q.QuoteNumber.Contains(req.Search) || q.Customer!.Name.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(q => q.QuoteDate)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(q => new QuoteDto
                {
                    Id = q.Id, QuoteNumber = q.QuoteNumber, CustomerId = q.CustomerId,
                    CustomerName = q.Customer != null ? q.Customer.Name : null,
                    QuoteDate = q.QuoteDate, ExpiryDate = q.ExpiryDate,
                    SubTotal = q.SubTotal, TaxAmount = q.TaxAmount, DiscountAmount = q.DiscountAmount,
                    TotalAmount = q.TotalAmount, Status = q.Status, Notes = q.Notes
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<QuoteDto>>.Success(new PagedResult<QuoteDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapPost("/", async (QuoteDto dto, AppDbContext db) =>
        {
            var quote = new Quote
            {
                QuoteNumber = dto.QuoteNumber, CustomerId = dto.CustomerId, QuoteDate = dto.QuoteDate,
                ExpiryDate = dto.ExpiryDate, SubTotal = dto.SubTotal, TaxAmount = dto.TaxAmount,
                DiscountAmount = dto.DiscountAmount, TotalAmount = dto.TotalAmount, Status = dto.Status, Notes = dto.Notes
            };
            db.Quotes.Add(quote);
            await db.SaveChangesAsync();
            dto.Id = quote.Id;
            return Results.Ok(ApiResponse<QuoteDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, QuoteDto dto, AppDbContext db) =>
        {
            var entity = await db.Quotes.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.QuoteNumber = dto.QuoteNumber; entity.CustomerId = dto.CustomerId;
            entity.QuoteDate = dto.QuoteDate; entity.ExpiryDate = dto.ExpiryDate;
            entity.SubTotal = dto.SubTotal; entity.TaxAmount = dto.TaxAmount;
            entity.DiscountAmount = dto.DiscountAmount; entity.TotalAmount = dto.TotalAmount;
            entity.Status = dto.Status; entity.Notes = dto.Notes; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<QuoteDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Quotes.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapSalesOrderEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sales-orders").WithTags("Sales Orders").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.SalesOrders.Include(o => o.Customer).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(o => o.OrderNumber.Contains(req.Search) || o.Customer!.Name.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(o => o.OrderDate)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(o => new SalesOrderDto
                {
                    Id = o.Id, OrderNumber = o.OrderNumber, CustomerId = o.CustomerId,
                    CustomerName = o.Customer != null ? o.Customer.Name : null,
                    OrderDate = o.OrderDate, RequiredDate = o.RequiredDate,
                    SubTotal = o.SubTotal, TaxAmount = o.TaxAmount, DiscountAmount = o.DiscountAmount,
                    TotalAmount = o.TotalAmount, Status = o.Status, Notes = o.Notes
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<SalesOrderDto>>.Success(new PagedResult<SalesOrderDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapPost("/", async (SalesOrderDto dto, AppDbContext db) =>
        {
            var order = new SalesOrder
            {
                OrderNumber = dto.OrderNumber, CustomerId = dto.CustomerId, QuoteId = dto.QuoteId,
                OrderDate = dto.OrderDate, RequiredDate = dto.RequiredDate,
                SubTotal = dto.SubTotal, TaxAmount = dto.TaxAmount, DiscountAmount = dto.DiscountAmount,
                TotalAmount = dto.TotalAmount, Status = dto.Status, Notes = dto.Notes
            };
            db.SalesOrders.Add(order);
            await db.SaveChangesAsync();
            dto.Id = order.Id;
            return Results.Ok(ApiResponse<SalesOrderDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, SalesOrderDto dto, AppDbContext db) =>
        {
            var entity = await db.SalesOrders.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.OrderNumber = dto.OrderNumber; entity.CustomerId = dto.CustomerId;
            entity.OrderDate = dto.OrderDate; entity.RequiredDate = dto.RequiredDate;
            entity.SubTotal = dto.SubTotal; entity.TaxAmount = dto.TaxAmount;
            entity.DiscountAmount = dto.DiscountAmount; entity.TotalAmount = dto.TotalAmount;
            entity.Status = dto.Status; entity.Notes = dto.Notes; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<SalesOrderDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.SalesOrders.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapSalesInvoiceEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sales-invoices").WithTags("Sales Invoices").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.SalesInvoices.Include(i => i.Customer).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(i => i.InvoiceNumber.Contains(req.Search) || i.Customer!.Name.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(i => i.InvoiceDate)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(i => new SalesInvoiceDto
                {
                    Id = i.Id, InvoiceNumber = i.InvoiceNumber, CustomerId = i.CustomerId,
                    CustomerName = i.Customer != null ? i.Customer.Name : null,
                    InvoiceDate = i.InvoiceDate, DueDate = i.DueDate,
                    SubTotal = i.SubTotal, TaxAmount = i.TaxAmount, DiscountAmount = i.DiscountAmount,
                    TotalAmount = i.TotalAmount, PaidAmount = i.PaidAmount, Status = i.Status, Notes = i.Notes
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<SalesInvoiceDto>>.Success(new PagedResult<SalesInvoiceDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapPost("/", async (SalesInvoiceDto dto, AppDbContext db) =>
        {
            var invoice = new SalesInvoice
            {
                InvoiceNumber = dto.InvoiceNumber, CustomerId = dto.CustomerId, SalesOrderId = dto.SalesOrderId,
                InvoiceDate = dto.InvoiceDate, DueDate = dto.DueDate,
                SubTotal = dto.SubTotal, TaxAmount = dto.TaxAmount, DiscountAmount = dto.DiscountAmount,
                TotalAmount = dto.TotalAmount, Status = dto.Status, Notes = dto.Notes
            };
            db.SalesInvoices.Add(invoice);
            await db.SaveChangesAsync();
            dto.Id = invoice.Id;
            return Results.Ok(ApiResponse<SalesInvoiceDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.SalesInvoices.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }
}
