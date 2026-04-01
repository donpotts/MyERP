using ERP.Shared.Models;

namespace ERP.Shared.DTOs;

public class VendorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? TaxNumber { get; set; }
    public string? PaymentTerms { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalPurchases { get; set; }
}

public class PurchaseOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseOrderLineDto> Lines { get; set; } = [];
}

public class PurchaseOrderLineDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
    public decimal ReceivedQuantity { get; set; }
}

public class GoodsReceiptDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public int PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public DateTime ReceiptDate { get; set; }
    public string? Notes { get; set; }
    public GoodsReceiptStatus Status { get; set; }
    public List<GoodsReceiptLineDto> Lines { get; set; } = [];
}

public class GoodsReceiptLineDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }
}
