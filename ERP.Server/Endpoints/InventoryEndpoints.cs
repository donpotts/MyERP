using Microsoft.EntityFrameworkCore;
using ERP.Server.Data;
using ERP.Shared.DTOs;
using ERP.Shared.Models;

namespace ERP.Server.Endpoints;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        MapProductEndpoints(app);
        MapCategoryEndpoints(app);
        MapWarehouseEndpoints(app);
        MapStockEndpoints(app);
        MapStockMovementEndpoints(app);
    }

    private static void MapProductEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.Products.Include(p => p.Category).Include(p => p.UnitOfMeasure).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(p => p.Name.Contains(req.Search) || p.Code.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderBy(p => p.Code)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id, Code = p.Code, Name = p.Name, Description = p.Description,
                    CategoryId = p.CategoryId, CategoryName = p.Category != null ? p.Category.Name : null,
                    UnitOfMeasureId = p.UnitOfMeasureId, UnitOfMeasure = p.UnitOfMeasure != null ? p.UnitOfMeasure.Name : null,
                    CostPrice = p.CostPrice, SalePrice = p.SalePrice, ReorderPoint = p.ReorderPoint,
                    ReorderQuantity = p.ReorderQuantity, Status = p.Status, IsTracked = p.IsTracked, Barcode = p.Barcode,
                    TotalStock = db.StockLevels.Where(sl => sl.ProductId == p.Id).Sum(sl => sl.QuantityOnHand)
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<ProductDto>>.Success(new PagedResult<ProductDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapGet("/all", async (AppDbContext db) =>
        {
            var items = await db.Products.Where(p => p.Status == ProductStatus.Active)
                .Select(p => new ProductDto { Id = p.Id, Code = p.Code, Name = p.Name, SalePrice = p.SalePrice, CostPrice = p.CostPrice })
                .ToListAsync();
            return Results.Ok(ApiResponse<List<ProductDto>>.Success(items));
        });

        group.MapPost("/", async (ProductDto dto, AppDbContext db) =>
        {
            var product = new Product
            {
                Code = dto.Code, Name = dto.Name, Description = dto.Description,
                CategoryId = dto.CategoryId, UnitOfMeasureId = dto.UnitOfMeasureId,
                CostPrice = dto.CostPrice, SalePrice = dto.SalePrice,
                ReorderPoint = dto.ReorderPoint, ReorderQuantity = dto.ReorderQuantity,
                Status = dto.Status, IsTracked = dto.IsTracked, Barcode = dto.Barcode
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();
            dto.Id = product.Id;
            return Results.Ok(ApiResponse<ProductDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, ProductDto dto, AppDbContext db) =>
        {
            var entity = await db.Products.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.Code = dto.Code; entity.Name = dto.Name; entity.Description = dto.Description;
            entity.CategoryId = dto.CategoryId; entity.UnitOfMeasureId = dto.UnitOfMeasureId;
            entity.CostPrice = dto.CostPrice; entity.SalePrice = dto.SalePrice;
            entity.ReorderPoint = dto.ReorderPoint; entity.ReorderQuantity = dto.ReorderQuantity;
            entity.Status = dto.Status; entity.Barcode = dto.Barcode; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<ProductDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Products.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true; entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapCategoryEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/product-categories").WithTags("Product Categories").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.ProductCategories.AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(c => c.Name.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderBy(c => c.Name)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(c => new ProductCategoryDto
                {
                    Id = c.Id, Name = c.Name, Description = c.Description, ParentId = c.ParentId,
                    ProductCount = db.Products.Count(p => p.CategoryId == c.Id)
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<ProductCategoryDto>>.Success(new PagedResult<ProductCategoryDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapGet("/all", async (AppDbContext db) =>
        {
            var items = await db.ProductCategories.Select(c => new ProductCategoryDto { Id = c.Id, Name = c.Name }).ToListAsync();
            return Results.Ok(ApiResponse<List<ProductCategoryDto>>.Success(items));
        });

        group.MapPost("/", async (ProductCategoryDto dto, AppDbContext db) =>
        {
            var category = new ProductCategory { Name = dto.Name, Description = dto.Description, ParentId = dto.ParentId };
            db.ProductCategories.Add(category);
            await db.SaveChangesAsync();
            dto.Id = category.Id;
            return Results.Ok(ApiResponse<ProductCategoryDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, ProductCategoryDto dto, AppDbContext db) =>
        {
            var entity = await db.ProductCategories.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.Name = dto.Name; entity.Description = dto.Description; entity.ParentId = dto.ParentId;
            entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<ProductCategoryDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.ProductCategories.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapWarehouseEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/warehouses").WithTags("Warehouses").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.Warehouses.AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(w => w.Name.Contains(req.Search) || (w.City != null && w.City.Contains(req.Search)));
            var total = await query.CountAsync();
            var items = await query.OrderBy(w => w.Name)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(w => new WarehouseDto
                {
                    Id = w.Id, Name = w.Name, Address = w.Address, City = w.City, Country = w.Country,
                    ManagerName = w.ManagerName, IsActive = w.IsActive,
                    ProductCount = db.StockLevels.Count(sl => sl.WarehouseId == w.Id && sl.QuantityOnHand > 0),
                    TotalValue = db.StockLevels
                        .Where(sl => sl.WarehouseId == w.Id)
                        .Join(db.Products, sl => sl.ProductId, p => p.Id, (sl, p) => sl.QuantityOnHand * p.CostPrice)
                        .Sum()
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<WarehouseDto>>.Success(new PagedResult<WarehouseDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });

        group.MapGet("/all", async (AppDbContext db) =>
        {
            var items = await db.Warehouses.Where(w => w.IsActive).Select(w => new WarehouseDto { Id = w.Id, Name = w.Name }).ToListAsync();
            return Results.Ok(ApiResponse<List<WarehouseDto>>.Success(items));
        });

        group.MapPost("/", async (WarehouseDto dto, AppDbContext db) =>
        {
            var warehouse = new Warehouse { Name = dto.Name, Address = dto.Address, City = dto.City, Country = dto.Country, ManagerName = dto.ManagerName };
            db.Warehouses.Add(warehouse);
            await db.SaveChangesAsync();
            dto.Id = warehouse.Id;
            return Results.Ok(ApiResponse<WarehouseDto>.Success(dto));
        });

        group.MapPut("/{id:int}", async (int id, WarehouseDto dto, AppDbContext db) =>
        {
            var entity = await db.Warehouses.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.Name = dto.Name; entity.Address = dto.Address; entity.City = dto.City;
            entity.Country = dto.Country; entity.ManagerName = dto.ManagerName; entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<WarehouseDto>.Success(dto));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var entity = await db.Warehouses.FindAsync(id);
            if (entity is null) return Results.NotFound();
            entity.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.Ok(ApiResponse<string>.Success("Deleted"));
        });
    }

    private static void MapStockEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stock-levels").WithTags("Stock Levels").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.StockLevels
                .Include(sl => sl.Product).Include(sl => sl.Warehouse).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(sl => sl.Product!.Name.Contains(req.Search) || sl.Warehouse!.Name.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderBy(sl => sl.Product!.Code)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(sl => new StockLevelDto
                {
                    Id = sl.Id, ProductId = sl.ProductId, ProductCode = sl.Product != null ? sl.Product.Code : null,
                    ProductName = sl.Product != null ? sl.Product.Name : null,
                    WarehouseId = sl.WarehouseId, WarehouseName = sl.Warehouse != null ? sl.Warehouse.Name : null,
                    QuantityOnHand = sl.QuantityOnHand, QuantityReserved = sl.QuantityReserved,
                    QuantityAvailable = sl.QuantityOnHand - sl.QuantityReserved,
                    ReorderPoint = sl.Product != null ? sl.Product.ReorderPoint : 0,
                    IsLowStock = sl.QuantityOnHand <= (sl.Product != null ? sl.Product.ReorderPoint : 0)
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<StockLevelDto>>.Success(new PagedResult<StockLevelDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });
    }

    private static void MapStockMovementEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stock-movements").WithTags("Stock Movements").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
        {
            var query = db.StockMovements
                .Include(m => m.Product).Include(m => m.Warehouse).AsQueryable();
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(m => m.Product!.Name.Contains(req.Search) || m.Reference.Contains(req.Search));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(m => m.MovementDate)
                .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
                .Select(m => new StockMovementDto
                {
                    Id = m.Id, ProductId = m.ProductId,
                    ProductName = m.Product != null ? m.Product.Name : null,
                    WarehouseId = m.WarehouseId,
                    WarehouseName = m.Warehouse != null ? m.Warehouse.Name : null,
                    MovementType = m.MovementType.ToString(),
                    MovementDate = m.MovementDate, Reference = m.Reference,
                    Description = m.Description, Quantity = m.Quantity,
                    UnitCost = m.UnitCost, TotalCost = m.TotalCost, BalanceAfter = m.BalanceAfter
                }).ToListAsync();
            return Results.Ok(ApiResponse<PagedResult<StockMovementDto>>.Success(
                new PagedResult<StockMovementDto> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize }));
        });
    }
}
