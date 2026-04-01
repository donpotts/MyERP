using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Shared.Models;

public enum QuoteStatus { Draft, Sent, Accepted, Rejected, Expired }
public enum SalesOrderStatus { Draft, Confirmed, Processing, Shipped, Delivered, Cancelled }
public enum SalesInvoiceStatus { Draft, Sent, Paid, PartiallyPaid, Overdue, Cancelled }

public class Customer : BaseEntity
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
    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; } = true;
    [MaxLength(500)]
    public string? Notes { get; set; }
    public List<CustomerContact> Contacts { get; set; } = [];
}

public class CustomerContact : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
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

public class Quote : BaseEntity
{
    [Required, MaxLength(50)]
    public string QuoteNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime QuoteDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
    [MaxLength(1000)]
    public string? Notes { get; set; }
    public List<QuoteLine> Lines { get; set; } = [];
}

public class QuoteLine : BaseEntity
{
    public int QuoteId { get; set; }
    public Quote? Quote { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountPercent { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxRate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }
}

public class SalesOrder : BaseEntity
{
    [Required, MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? QuoteId { get; set; }
    public Quote? Quote { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    [MaxLength(200)]
    public string? ShipToAddress { get; set; }
    [MaxLength(1000)]
    public string? Notes { get; set; }
    public List<SalesOrderLine> Lines { get; set; } = [];
}

public class SalesOrderLine : BaseEntity
{
    public int SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountPercent { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxRate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippedQuantity { get; set; }
}

public class SalesInvoice : BaseEntity
{
    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; }
    public SalesInvoiceStatus Status { get; set; } = SalesInvoiceStatus.Draft;
    [MaxLength(1000)]
    public string? Notes { get; set; }
    public List<SalesInvoiceLine> Lines { get; set; } = [];
}

public class SalesInvoiceLine : BaseEntity
{
    public int SalesInvoiceId { get; set; }
    public SalesInvoice? SalesInvoice { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountPercent { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxRate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }
}
