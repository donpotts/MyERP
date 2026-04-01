using ERP.Shared.Models;

namespace ERP.Shared.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? UnitOfMeasureId { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal ReorderQuantity { get; set; }
    public ProductStatus Status { get; set; }
    public bool IsTracked { get; set; }
    public string? Barcode { get; set; }
    public decimal TotalStock { get; set; }
}

public class ProductCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int ProductCount { get; set; }
}

public class WarehouseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ManagerName { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public decimal TotalValue { get; set; }
}

public class StockLevelDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }
    public decimal ReorderPoint { get; set; }
    public bool IsLowStock { get; set; }
}

public class StockMovementDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public DateTime MovementDate { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal BalanceAfter { get; set; }
}
