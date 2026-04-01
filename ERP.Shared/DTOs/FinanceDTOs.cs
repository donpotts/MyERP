using ERP.Shared.Models;

namespace ERP.Shared.DTOs;

public class AccountDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public AccountStatus Status { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string? Description { get; set; }
    public decimal Balance { get; set; }
}

public class JournalEntryDto
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string? Description { get; set; }
    public JournalStatus Status { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public List<JournalLineDto> Lines { get; set; } = [];
}

public class JournalLineDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class APInvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue => TotalAmount - PaidAmount;
    public InvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class ARInvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue => TotalAmount - PaidAmount;
    public InvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class BankAccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
}

public class BankTransactionDto
{
    public int Id { get; set; }
    public int BankAccountId { get; set; }
    public string? BankAccountName { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal RunningBalance { get; set; }
    public bool IsReconciled { get; set; }
}
