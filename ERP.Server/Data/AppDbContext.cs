using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ERP.Shared.Models;

namespace ERP.Server.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Finance
    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();
    public DbSet<APInvoice> APInvoices => Set<APInvoice>();
    public DbSet<APInvoiceLine> APInvoiceLines => Set<APInvoiceLine>();
    public DbSet<ARInvoice> ARInvoices => Set<ARInvoice>();
    public DbSet<ARInvoiceLine> ARInvoiceLines => Set<ARInvoiceLine>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();

    // Inventory
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    // Procurement
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<VendorContact> VendorContacts => Set<VendorContact>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptLine> GoodsReceiptLines => Set<GoodsReceiptLine>();

    // Sales
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteLine> QuoteLines => Set<QuoteLine>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceLine> SalesInvoiceLines => Set<SalesInvoiceLine>();

    // Projects
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    // CRM
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Activity> Activities => Set<Activity>();

    // Settings
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Account self-referential
        builder.Entity<Account>()
            .HasOne(a => a.Parent)
            .WithMany(a => a.Children)
            .HasForeignKey(a => a.ParentId)
            .OnDelete(DeleteBehavior.NoAction);

        // ProductCategory self-referential
        builder.Entity<ProductCategory>()
            .HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.NoAction);

        // PurchaseOrder -> Vendor (no cascade to avoid cycle)
        builder.Entity<APInvoice>()
            .HasOne(i => i.Vendor)
            .WithMany()
            .HasForeignKey(i => i.VendorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ARInvoice>()
            .HasOne(i => i.Customer)
            .WithMany()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<GoodsReceiptLine>()
            .HasOne(l => l.PurchaseOrderLine)
            .WithMany()
            .HasForeignKey(l => l.PurchaseOrderLineId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<GoodsReceiptLine>()
            .HasOne(l => l.GoodsReceipt)
            .WithMany(r => r.Lines)
            .HasForeignKey(l => l.GoodsReceiptId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<GoodsReceipt>()
            .HasOne(r => r.PurchaseOrder)
            .WithMany(o => o.GoodsReceipts)
            .HasForeignKey(r => r.PurchaseOrderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<SalesOrder>()
            .HasOne(o => o.Quote)
            .WithMany()
            .HasForeignKey(o => o.QuoteId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<SalesInvoice>()
            .HasOne(i => i.SalesOrder)
            .WithMany()
            .HasForeignKey(i => i.SalesOrderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<SalesInvoice>()
            .HasOne(i => i.Customer)
            .WithMany()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<SalesOrder>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Quote>()
            .HasOne(q => q.Customer)
            .WithMany()
            .HasForeignKey(q => q.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        // Opportunity FK
        builder.Entity<Opportunity>()
            .HasOne(o => o.Contact)
            .WithMany()
            .HasForeignKey(o => o.ContactId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Opportunity>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Activity>()
            .HasOne(a => a.Contact)
            .WithMany(c => c.Activities)
            .HasForeignKey(a => a.ContactId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Activity>()
            .HasOne(a => a.Opportunity)
            .WithMany(o => o.Activities)
            .HasForeignKey(a => a.OpportunityId)
            .OnDelete(DeleteBehavior.NoAction);

        // ProjectTask self-referential
        builder.Entity<ProjectTask>()
            .HasOne<ProjectTask>()
            .WithMany()
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Project>()
            .HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Contact>()
            .HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        // Soft delete filters
        builder.Entity<Account>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Vendor>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<SalesOrder>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<PurchaseOrder>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Lead>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Contact>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Opportunity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
