using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Shared.Models;

public enum StockMovementType { Receipt, Issue, Adjustment, Transfer, Return }
public enum ProductStatus { Active, Inactive, Discontinued }

public class UnitOfMeasure : BaseEntity
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(10)]
    public string Abbreviation { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class ProductCategory : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public ProductCategory? Parent { get; set; }
    public List<ProductCategory> Children { get; set; } = [];
    public List<Product> Products { get; set; } = [];
}

public class Product : BaseEntity
{
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public ProductCategory? Category { get; set; }
    public int? UnitOfMeasureId { get; set; }
    public UnitOfMeasure? UnitOfMeasure { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal SalePrice { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ReorderPoint { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ReorderQuantity { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public bool IsTracked { get; set; } = true;
    [MaxLength(100)]
    public string? Barcode { get; set; }
    public List<StockLevel> StockLevels { get; set; } = [];
}

public class Warehouse : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(200)]
    public string? Address { get; set; }
    [MaxLength(100)]
    public string? City { get; set; }
    [MaxLength(100)]
    public string? Country { get; set; }
    [MaxLength(50)]
    public string? ManagerName { get; set; }
    public bool IsActive { get; set; } = true;
    public List<StockLevel> StockLevels { get; set; } = [];
}

public class StockLevel : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityOnHand { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityReserved { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;
}

public class StockMovement : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public StockMovementType MovementType { get; set; }
    public DateTime MovementDate { get; set; }
    [MaxLength(50)]
    public string Reference { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceAfter { get; set; }
}
