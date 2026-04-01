using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Shared.Models;

public enum AccountType { Asset, Liability, Equity, Revenue, Expense }
public enum AccountStatus { Active, Inactive }
public enum JournalStatus { Draft, Posted, Voided }
public enum InvoiceStatus { Draft, Pending, Approved, Paid, PartiallyPaid, Overdue, Cancelled }
public enum BankTransactionType { Deposit, Withdrawal, Transfer }
public enum FiscalPeriodStatus { Open, Closed }

public class FiscalYear : BaseEntity
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
    public List<FiscalPeriod> Periods { get; set; } = [];
}

public class FiscalPeriod : BaseEntity
{
    public int FiscalYearId { get; set; }
    public FiscalYear? FiscalYear { get; set; }
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public FiscalPeriodStatus Status { get; set; } = FiscalPeriodStatus.Open;
}

public class TaxRate : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;
    [Column(TypeName = "decimal(5,2)")]
    public decimal Rate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Account : BaseEntity
{
    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public int? ParentId { get; set; }
    public Account? Parent { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }
    public bool IsSystem { get; set; }
    public List<Account> Children { get; set; } = [];
}

public class JournalEntry : BaseEntity
{
    [Required, MaxLength(50)]
    public string Reference { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    public JournalStatus Status { get; set; } = JournalStatus.Draft;
    public int? FiscalPeriodId { get; set; }
    public FiscalPeriod? FiscalPeriod { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDebit { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCredit { get; set; }
    public List<JournalLine> Lines { get; set; } = [];
}

public class JournalLine : BaseEntity
{
    public int JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
    public int AccountId { get; set; }
    public Account? Account { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Debit { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Credit { get; set; }
}

public class APInvoice : BaseEntity
{
    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    [MaxLength(1000)]
    public string? Notes { get; set; }
    public List<APInvoiceLine> Lines { get; set; } = [];
}

public class APInvoiceLine : BaseEntity
{
    public int APInvoiceId { get; set; }
    public APInvoice? APInvoice { get; set; }
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxRate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
}

public class ARInvoice : BaseEntity
{
    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    [MaxLength(1000)]
    public string? Notes { get; set; }
    public List<ARInvoiceLine> Lines { get; set; } = [];
}

public class ARInvoiceLine : BaseEntity
{
    public int ARInvoiceId { get; set; }
    public ARInvoice? ARInvoice { get; set; }
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxRate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
}

public class BankAccount : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string AccountNumber { get; set; } = string.Empty;
    [MaxLength(100)]
    public string BankName { get; set; } = string.Empty;
    [MaxLength(20)]
    public string Currency { get; set; } = "USD";
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }
    public bool IsActive { get; set; } = true;
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    public List<BankTransaction> Transactions { get; set; } = [];
}

public class BankTransaction : BaseEntity
{
    public int BankAccountId { get; set; }
    public BankAccount? BankAccount { get; set; }
    public DateTime TransactionDate { get; set; }
    [MaxLength(50)]
    public string Reference { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    public BankTransactionType Type { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal RunningBalance { get; set; }
    public bool IsReconciled { get; set; }
}
