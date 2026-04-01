using Microsoft.EntityFrameworkCore;
using ERP.Server.Data;
using ERP.Shared.DTOs;
using ERP.Shared.Models;

namespace ERP.Server.Endpoints;

public static class ProcurementEndpoints
{
    public static void MapProcurementEndpoints(this IEndpointRouteBuilder app)
    {
        MapVendorEndpoints(app);
        MapPurchaseOrderEndpoints(app);
        MapGoodsReceiptEndpoints(app);
    }

    private static void MapVendorEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vendors").WithTags("Vendors").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.Vendors.AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(v => v.Name.Contains(req.Search) || (v.Code != null && v.Code.Contains(req.Search)));
            var total = await query.CountAsync();
            var items = await query.OrderBy(v => v.Name)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(v => new VendorDto
                {
                    Id = v.Id, Name = v.Name, Code = v.Code, Email = v.Email, Phone = v.Phone,
                    Address = v.Address, City = v.City, Country = v.Country,
                    TaxNumber = v.TaxNumber, PaymentTerms = v.PaymentTerms, IsActive = v.IsActive, Notes = v.Notes,
                    OrderCount = db.PurchaseOrders.Count(po => po.VendorId == v.Id)
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<VendorDto>>.Success(new PagedResult<VendorDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapGet("/all", async (AppDbContext db) =>
        {
            var items = await db.Vendors.Where(v => v.IsActive).Select(v => new VendorDto { Id = v.Id, Name = v.Name, Code = v.Code }).ToListAsync();
            return Results.Ok(ApiResponse<List<VendorDto>>.Success(items));
        });

        group.MapPost("/", async (VendorDto dto, AppDbContext db) =>
        {
            var vendor = new Vendor
            {
                Name = dto.Name, Code = dto.Code, Email = dto.Email, Phone = dto.Phone,
                Address = dto.Address, City = dto.City, Country = dto.Country,
                TaxNumber = dto.TaxNumber, PaymentTerms = dto.PaymentTerms, Notes = dto.Notes
            };
            db.Vendors.Add(vendor);
            await db.SaveChangesAsync();
            dto.Id = vendor.Id;
            return Results.Ok(ApiResponse<VendorDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, VendorDto dto, AppDbContext db) =>
        {
            var entity = await db.Vendors.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.Name = dto.Name; entity.Code = dto.Code; entity.Email = dto.Email; entity.Phone = dto.Phone;
            entity.Address = dto.Address; entity.City = dto.City; entity.Country = dto.Country;
            entity.TaxNumber = dto.TaxNumber; entity.PaymentTerms = dto.PaymentTerms;
            entity.IsActive = dto.IsActive; entity.Notes = dto.Notes; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<VendorDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Vendors.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapPurchaseOrderEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/purchase-orders").WithTags("Purchase Orders").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.PurchaseOrders.Include(po => po.Vendor).Include(po => po.Warehouse).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(po => po.OrderNumber.Contains(req.Search) || po.Vendor!.Name.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(po => po.OrderDate)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(po => new PurchaseOrderDto
                {
                    Id = po.Id, OrderNumber = po.OrderNumber, VendorId = po.VendorId,
                    VendorName = po.Vendor != null ? po.Vendor.Name : null,
                    WarehouseId = po.WarehouseId, WarehouseName = po.Warehouse != null ? po.Warehouse.Name : null,
                    OrderDate = po.OrderDate, ExpectedDate = po.ExpectedDate,
                    SubTotal = po.SubTotal, TaxAmount = po.TaxAmount, TotalAmount = po.TotalAmount,
                    Status = po.Status, Notes = po.Notes
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<PurchaseOrderDto>>.Success(new PagedResult<PurchaseOrderDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapPost("/", async (PurchaseOrderDto dto, AppDbContext db) =>
        {
            var po = new PurchaseOrder
            {
                OrderNumber = dto.OrderNumber, VendorId = dto.VendorId, WarehouseId = dto.WarehouseId,
                OrderDate = dto.OrderDate, ExpectedDate = dto.ExpectedDate,
                SubTotal = dto.SubTotal, TaxAmount = dto.TaxAmount, TotalAmount = dto.TotalAmount,
                Status = dto.Status, Notes = dto.Notes
            };
            db.PurchaseOrders.Add(po);
            await db.SaveChangesAsync();
            dto.Id = po.Id;
            return Results.Ok(ApiResponse<PurchaseOrderDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, PurchaseOrderDto dto, AppDbContext db) =>
        {
            var entity = await db.PurchaseOrders.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.OrderNumber = dto.OrderNumber; entity.VendorId = dto.VendorId; entity.WarehouseId = dto.WarehouseId;
            entity.OrderDate = dto.OrderDate; entity.ExpectedDate = dto.ExpectedDate;
            entity.SubTotal = dto.SubTotal; entity.TaxAmount = dto.TaxAmount; entity.TotalAmount = dto.TotalAmount;
            entity.Status = dto.Status; entity.Notes = dto.Notes; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<PurchaseOrderDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.PurchaseOrders.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapGoodsReceiptEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/goods-receipts").WithTags("Goods Receipts").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.GoodsReceipts.Include(gr => gr.PurchaseOrder).Include(gr => gr.Warehouse).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(gr => gr.ReceiptNumber.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(gr => gr.ReceiptDate)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(gr => new GoodsReceiptDto
                {
                    Id = gr.Id, ReceiptNumber = gr.ReceiptNumber, PurchaseOrderId = gr.PurchaseOrderId,
                    PurchaseOrderNumber = gr.PurchaseOrder != null ? gr.PurchaseOrder.OrderNumber : null,
                    WarehouseId = gr.WarehouseId, WarehouseName = gr.Warehouse != null ? gr.Warehouse.Name : null,
                    ReceiptDate = gr.ReceiptDate, Notes = gr.Notes, Status = gr.Status
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<GoodsReceiptDto>>.Success(new PagedResult<GoodsReceiptDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapPost("/", async (GoodsReceiptDto dto, AppDbContext db) =>
        {
            var gr = new GoodsReceipt
            {
                ReceiptNumber = dto.ReceiptNumber, PurchaseOrderId = dto.PurchaseOrderId,
                WarehouseId = dto.WarehouseId, ReceiptDate = dto.ReceiptDate,
                Notes = dto.Notes, Status = dto.Status
            };
            db.GoodsReceipts.Add(gr);
            await db.SaveChangesAsync();
            dto.Id = gr.Id;
            return Results.Ok(ApiResponse<GoodsReceiptDto>.Success(dto));
        });
    }
}
