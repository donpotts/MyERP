using ERP.Shared.Models;

namespace ERP.Shared.DTOs;

public class CustomerDto
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
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public decimal TotalSales { get; set; }
    public int OrderCount { get; set; }
}

public class QuoteDto
{
    public int Id { get; set; }
    public string QuoteNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime QuoteDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public QuoteStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class SalesOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? QuoteId { get; set; }
    public string? QuoteNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public SalesOrderStatus Status { get; set; }
    public string? ShipToAddress { get; set; }
    public string? Notes { get; set; }
}

public class SalesInvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? SalesOrderId { get; set; }
    public string? SalesOrderNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue => TotalAmount - PaidAmount;
    public SalesInvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
}
