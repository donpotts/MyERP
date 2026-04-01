using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Shared.Models;

public enum PurchaseOrderStatus { Draft, Submitted, Approved, PartiallyReceived, FullyReceived, Cancelled }
public enum GoodsReceiptStatus { Draft, Confirmed, Cancelled }

public class Vendor : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? Code { get; set; }
    [MaxLength(200)]
    public string? Email { get; set; }
    [MaxLength(50)]
    public string? Phone { get; set; }
    [MaxLength(300)]
    public string? Address { get; set; }
    [MaxLength(100)]
    public string? City { get; set; }
    [MaxLength(100)]
    public string? Country { get; set; }
    [MaxLength(100)]
    public string? TaxNumber { get; set; }
    [MaxLength(50)]
    public string? PaymentTerms { get; set; }
    public bool IsActive { get; set; } = true;
    [MaxLength(500)]
    public string? Notes { get; set; }
    public List<VendorContact> Contacts { get; set; } = [];
    public List<PurchaseOrder> PurchaseOrders { get; set; } = [];
}

public class VendorContact : BaseEntity
{
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    [MaxLength(200)]
    public string? Email { get; set; }
    [MaxLength(50)]
    public string? Phone { get; set; }
    public bool IsPrimary { get; set; }
}

public class PurchaseOrder : BaseEntity
{
    [Required, MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    [MaxLength(1000)]
    public string? Notes { get; set; }
    public List<PurchaseOrderLine> Lines { get; set; } = [];
    public List<GoodsReceipt> GoodsReceipts { get; set; } = [];
}

public class PurchaseOrderLine : BaseEntity
{
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxRate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ReceivedQuantity { get; set; }
}

public class GoodsReceipt : BaseEntity
{
    [Required, MaxLength(50)]
    public string ReceiptNumber { get; set; } = string.Empty;
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public DateTime ReceiptDate { get; set; }
    [MaxLength(1000)]
    public string? Notes { get; set; }
    public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Draft;
    public List<GoodsReceiptLine> Lines { get; set; } = [];
}

public class GoodsReceiptLine : BaseEntity
{
    public int GoodsReceiptId { get; set; }
    public GoodsReceipt? GoodsReceipt { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int PurchaseOrderLineId { get; set; }
    public PurchaseOrderLine? PurchaseOrderLine { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal OrderedQuantity { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ReceivedQuantity { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; }
    [MaxLength(500)]
    public string? Notes { get; set; }
}
