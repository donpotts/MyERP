using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ERP.Shared.Models;

namespace ERP.Server.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<AppDbContext>();
        var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var config = sp.GetRequiredService<IConfiguration>();

        await db.Database.EnsureCreatedAsync();
        await SeedRolesAsync(roleMgr);
        await SeedAdminAsync(userMgr, roleMgr, config);
        await SeedDemoDataAsync(sp);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleMgr)
    {
        string[] roles = ["Super Admin", "Admin", "Finance Manager", "Sales Manager", "Inventory Manager", "Employee"];
        foreach (var role in roles)
            if (!await roleMgr.RoleExistsAsync(role))
                await roleMgr.CreateAsync(new IdentityRole(role));
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userMgr, RoleManager<IdentityRole> roleMgr, IConfiguration config)
    {
        var adminEmail = config["DefaultAdmin:Email"] ?? "admin@root.com";
        var adminPass = config["DefaultAdmin:Password"] ?? "admin";

        if (await userMgr.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail, Email = adminEmail,
                FirstName = "Super", LastName = "Admin",
                EmailConfirmed = true, IsActive = true
            };
            await userMgr.CreateAsync(admin, adminPass);
            await userMgr.AddToRoleAsync(admin, "Super Admin");
        }

        // Demo users
        await EnsureUserAsync(userMgr, "finance@root.com", "fin1234", "Finance", "Manager", "Finance Manager");
        await EnsureUserAsync(userMgr, "sales@root.com", "sales123", "Sales", "Manager", "Sales Manager");
        await EnsureUserAsync(userMgr, "inventory@root.com", "inv1234", "Inventory", "Manager", "Inventory Manager");
        await EnsureUserAsync(userMgr, "emp@root.com", "emp123", "John", "Employee", "Employee");
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> userMgr, string email, string pass, string first, string last, string role)
    {
        if (await userMgr.FindByEmailAsync(email) is null)
        {
            var user = new ApplicationUser
            {
                UserName = email, Email = email,
                FirstName = first, LastName = last,
                EmailConfirmed = true, IsActive = true
            };
            await userMgr.CreateAsync(user, pass);
            await userMgr.AddToRoleAsync(user, role);
        }
    }

    public static async Task SeedDemoDataAsync(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<AppDbContext>();

        if (await db.Customers.AnyAsync()) return; // already seeded

        // Seed Chart of Accounts
        var accounts = new List<Account>
        {
            new() { Code = "1000", Name = "Cash and Bank", Type = AccountType.Asset, Balance = 250000 },
            new() { Code = "1100", Name = "Accounts Receivable", Type = AccountType.Asset, Balance = 185000 },
            new() { Code = "1200", Name = "Inventory", Type = AccountType.Asset, Balance = 320000 },
            new() { Code = "1300", Name = "Prepaid Expenses", Type = AccountType.Asset, Balance = 15000 },
            new() { Code = "1500", Name = "Fixed Assets", Type = AccountType.Asset, Balance = 450000 },
            new() { Code = "2000", Name = "Accounts Payable", Type = AccountType.Liability, Balance = 95000 },
            new() { Code = "2100", Name = "Accrued Liabilities", Type = AccountType.Liability, Balance = 42000 },
            new() { Code = "2200", Name = "Long-term Debt", Type = AccountType.Liability, Balance = 200000 },
            new() { Code = "3000", Name = "Common Stock", Type = AccountType.Equity, Balance = 500000 },
            new() { Code = "3100", Name = "Retained Earnings", Type = AccountType.Equity, Balance = 383000 },
            new() { Code = "4000", Name = "Sales Revenue", Type = AccountType.Revenue, Balance = 1250000 },
            new() { Code = "4100", Name = "Service Revenue", Type = AccountType.Revenue, Balance = 320000 },
            new() { Code = "5000", Name = "Cost of Goods Sold", Type = AccountType.Expense, Balance = 720000 },
            new() { Code = "5100", Name = "Salaries Expense", Type = AccountType.Expense, Balance = 180000 },
            new() { Code = "5200", Name = "Rent Expense", Type = AccountType.Expense, Balance = 48000 },
            new() { Code = "5300", Name = "Marketing Expense", Type = AccountType.Expense, Balance = 35000 },
            new() { Code = "5400", Name = "Utilities Expense", Type = AccountType.Expense, Balance = 12000 },
        };
        db.Accounts.AddRange(accounts);

        // Bank Accounts
        var bankAccounts = new List<BankAccount>
        {
            new() { Name = "Main Operating Account", AccountNumber = "1234567890", BankName = "First National Bank", Currency = "USD", Balance = 150000 },
            new() { Name = "Payroll Account", AccountNumber = "0987654321", BankName = "City Bank", Currency = "USD", Balance = 75000 },
            new() { Name = "Savings Account", AccountNumber = "1122334455", BankName = "First National Bank", Currency = "USD", Balance = 25000 },
        };
        db.BankAccounts.AddRange(bankAccounts);

        // Units of Measure
        var uoms = new List<UnitOfMeasure>
        {
            new() { Name = "Each", Abbreviation = "EA" },
            new() { Name = "Kilogram", Abbreviation = "KG" },
            new() { Name = "Liter", Abbreviation = "L" },
            new() { Name = "Meter", Abbreviation = "M" },
            new() { Name = "Box", Abbreviation = "BOX" },
            new() { Name = "Dozen", Abbreviation = "DZ" },
        };
        db.UnitsOfMeasure.AddRange(uoms);

        // Product Categories
        var categories = new List<ProductCategory>
        {
            new() { Name = "Electronics", Description = "Electronic components and devices" },
            new() { Name = "Office Supplies", Description = "Office consumables and equipment" },
            new() { Name = "Software", Description = "Software licenses and subscriptions" },
            new() { Name = "Hardware", Description = "Computer hardware and peripherals" },
            new() { Name = "Furniture", Description = "Office furniture and fixtures" },
        };
        db.ProductCategories.AddRange(categories);
        await db.SaveChangesAsync();

        // Products
        var products = new List<Product>
        {
            new() { Code = "PROD-001", Name = "Dell Laptop 15\"", CategoryId = categories[0].Id, CostPrice = 850, SalePrice = 1299, ReorderPoint = 5, ReorderQuantity = 20, Barcode = "884116000000" },
            new() { Code = "PROD-002", Name = "HP Monitor 27\"", CategoryId = categories[0].Id, CostPrice = 280, SalePrice = 449, ReorderPoint = 3, ReorderQuantity = 10 },
            new() { Code = "PROD-003", Name = "Wireless Keyboard", CategoryId = categories[0].Id, CostPrice = 45, SalePrice = 79, ReorderPoint = 10, ReorderQuantity = 30 },
            new() { Code = "PROD-004", Name = "USB-C Hub", CategoryId = categories[0].Id, CostPrice = 25, SalePrice = 49, ReorderPoint = 15, ReorderQuantity = 50 },
            new() { Code = "PROD-005", Name = "A4 Paper (500 sheets)", CategoryId = categories[1].Id, CostPrice = 4, SalePrice = 8, ReorderPoint = 50, ReorderQuantity = 200 },
            new() { Code = "PROD-006", Name = "Ballpoint Pen Box", CategoryId = categories[1].Id, CostPrice = 5, SalePrice = 12, ReorderPoint = 20, ReorderQuantity = 100 },
            new() { Code = "PROD-007", Name = "Microsoft Office License", CategoryId = categories[2].Id, CostPrice = 150, SalePrice = 249, ReorderPoint = 2, ReorderQuantity = 10 },
            new() { Code = "PROD-008", Name = "Antivirus Software", CategoryId = categories[2].Id, CostPrice = 30, SalePrice = 59, ReorderPoint = 5, ReorderQuantity = 20 },
            new() { Code = "PROD-009", Name = "Gaming Mouse", CategoryId = categories[3].Id, CostPrice = 35, SalePrice = 69, ReorderPoint = 8, ReorderQuantity = 25 },
            new() { Code = "PROD-010", Name = "Office Chair", CategoryId = categories[4].Id, CostPrice = 180, SalePrice = 299, ReorderPoint = 2, ReorderQuantity = 8 },
        };
        db.Products.AddRange(products);

        // Warehouses
        var warehouses = new List<Warehouse>
        {
            new() { Name = "Main Warehouse", Address = "100 Industrial Blvd", City = "Chicago", Country = "USA", ManagerName = "Mike Johnson" },
            new() { Name = "East Coast Hub", Address = "50 Commerce St", City = "New York", Country = "USA", ManagerName = "Sarah Davis" },
            new() { Name = "West Coast Depot", Address = "200 Harbor Dr", City = "Los Angeles", Country = "USA", ManagerName = "Tom Chen" },
        };
        db.Warehouses.AddRange(warehouses);
        await db.SaveChangesAsync();

        // Stock Levels
        var stockLevels = new List<StockLevel>();
        var random = new Random(42);
        foreach (var product in products)
        {
            stockLevels.Add(new StockLevel
            {
                ProductId = product.Id,
                WarehouseId = warehouses[0].Id,
                QuantityOnHand = random.Next(5, 100),
                QuantityReserved = random.Next(0, 5)
            });
        }
        db.StockLevels.AddRange(stockLevels);

        // Vendors
        var vendors = new List<Vendor>
        {
            new() { Name = "TechSupply Co.", Code = "VEN-001", Email = "orders@techsupply.com", Phone = "+1-555-0100", City = "San Francisco", Country = "USA", PaymentTerms = "Net 30" },
            new() { Name = "OfficeWorld Inc.", Code = "VEN-002", Email = "procurement@officeworld.com", Phone = "+1-555-0101", City = "Chicago", Country = "USA", PaymentTerms = "Net 15" },
            new() { Name = "SoftwarePros Ltd.", Code = "VEN-003", Email = "sales@softwarepros.com", Phone = "+1-555-0102", City = "Austin", Country = "USA", PaymentTerms = "Net 30" },
            new() { Name = "Global Electronics", Code = "VEN-004", Email = "b2b@globalelec.com", Phone = "+1-555-0103", City = "Seattle", Country = "USA", PaymentTerms = "Net 45" },
            new() { Name = "FurniturePlus", Code = "VEN-005", Email = "trade@furnitureplus.com", Phone = "+1-555-0104", City = "Dallas", Country = "USA", PaymentTerms = "Net 30" },
        };
        db.Vendors.AddRange(vendors);

        // Customers
        var customers = new List<Customer>
        {
            new() { Name = "Acme Corporation", Code = "CUST-001", Email = "procurement@acme.com", Phone = "+1-555-1000", City = "New York", Country = "USA", PaymentTerms = "Net 30", CreditLimit = 100000 },
            new() { Name = "Global Dynamics", Code = "CUST-002", Email = "purchasing@globaldyn.com", Phone = "+1-555-1001", City = "Chicago", Country = "USA", PaymentTerms = "Net 45", CreditLimit = 250000 },
            new() { Name = "Summit Tech", Code = "CUST-003", Email = "orders@summittech.com", Phone = "+1-555-1002", City = "San Francisco", Country = "USA", PaymentTerms = "Net 30", CreditLimit = 75000 },
            new() { Name = "Horizon Industries", Code = "CUST-004", Email = "supply@horizon.com", Phone = "+1-555-1003", City = "Boston", Country = "USA", PaymentTerms = "Net 15", CreditLimit = 50000 },
            new() { Name = "Pacific Ventures", Code = "CUST-005", Email = "buy@pacificv.com", Phone = "+1-555-1004", City = "Seattle", Country = "USA", PaymentTerms = "Net 30", CreditLimit = 150000 },
            new() { Name = "Metro Solutions", Code = "CUST-006", Email = "info@metrosol.com", Phone = "+1-555-1005", City = "Dallas", Country = "USA", PaymentTerms = "Net 30", CreditLimit = 80000 },
            new() { Name = "Apex Industries", Code = "CUST-007", Email = "orders@apexind.com", Phone = "+1-555-1006", City = "Miami", Country = "USA", PaymentTerms = "Net 45", CreditLimit = 200000 },
            new() { Name = "DataCore Systems", Code = "CUST-008", Email = "buy@datacore.com", Phone = "+1-555-1007", City = "Austin", Country = "USA", PaymentTerms = "Net 30", CreditLimit = 120000 },
        };
        db.Customers.AddRange(customers);
        await db.SaveChangesAsync();

        // Purchase Orders
        var pos = new List<PurchaseOrder>
        {
            new() { OrderNumber = "PO-2024-001", VendorId = vendors[0].Id, WarehouseId = warehouses[0].Id, OrderDate = DateTime.UtcNow.AddDays(-30), ExpectedDate = DateTime.UtcNow.AddDays(-15), SubTotal = 8500, TaxAmount = 850, TotalAmount = 9350, Status = PurchaseOrderStatus.FullyReceived },
            new() { OrderNumber = "PO-2024-002", VendorId = vendors[1].Id, WarehouseId = warehouses[0].Id, OrderDate = DateTime.UtcNow.AddDays(-20), ExpectedDate = DateTime.UtcNow.AddDays(-5), SubTotal = 240, TaxAmount = 24, TotalAmount = 264, Status = PurchaseOrderStatus.FullyReceived },
            new() { OrderNumber = "PO-2024-003", VendorId = vendors[2].Id, WarehouseId = warehouses[1].Id, OrderDate = DateTime.UtcNow.AddDays(-10), ExpectedDate = DateTime.UtcNow.AddDays(5), SubTotal = 1500, TaxAmount = 150, TotalAmount = 1650, Status = PurchaseOrderStatus.Approved },
            new() { OrderNumber = "PO-2024-004", VendorId = vendors[3].Id, WarehouseId = warehouses[0].Id, OrderDate = DateTime.UtcNow.AddDays(-5), ExpectedDate = DateTime.UtcNow.AddDays(10), SubTotal = 5600, TaxAmount = 560, TotalAmount = 6160, Status = PurchaseOrderStatus.Submitted },
            new() { OrderNumber = "PO-2024-005", VendorId = vendors[0].Id, WarehouseId = warehouses[2].Id, OrderDate = DateTime.UtcNow.AddDays(-2), SubTotal = 3200, TaxAmount = 320, TotalAmount = 3520, Status = PurchaseOrderStatus.Draft },
        };
        db.PurchaseOrders.AddRange(pos);

        // Sales Orders
        var salesOrders = new List<SalesOrder>
        {
            new() { OrderNumber = "SO-2024-001", CustomerId = customers[0].Id, OrderDate = DateTime.UtcNow.AddDays(-25), SubTotal = 12990, TaxAmount = 1299, TotalAmount = 14289, Status = SalesOrderStatus.Delivered },
            new() { OrderNumber = "SO-2024-002", CustomerId = customers[1].Id, OrderDate = DateTime.UtcNow.AddDays(-18), SubTotal = 7840, TaxAmount = 784, TotalAmount = 8624, Status = SalesOrderStatus.Shipped },
            new() { OrderNumber = "SO-2024-003", CustomerId = customers[2].Id, OrderDate = DateTime.UtcNow.AddDays(-12), SubTotal = 4970, TaxAmount = 497, TotalAmount = 5467, Status = SalesOrderStatus.Processing },
            new() { OrderNumber = "SO-2024-004", CustomerId = customers[3].Id, OrderDate = DateTime.UtcNow.AddDays(-7), SubTotal = 1196, TaxAmount = 120, TotalAmount = 1316, Status = SalesOrderStatus.Confirmed },
            new() { OrderNumber = "SO-2024-005", CustomerId = customers[4].Id, OrderDate = DateTime.UtcNow.AddDays(-4), SubTotal = 8960, TaxAmount = 896, TotalAmount = 9856, Status = SalesOrderStatus.Confirmed },
            new() { OrderNumber = "SO-2024-006", CustomerId = customers[5].Id, OrderDate = DateTime.UtcNow.AddDays(-2), SubTotal = 3590, TaxAmount = 359, TotalAmount = 3949, Status = SalesOrderStatus.Draft },
            new() { OrderNumber = "SO-2024-007", CustomerId = customers[6].Id, OrderDate = DateTime.UtcNow.AddDays(-1), SubTotal = 14990, TaxAmount = 1499, TotalAmount = 16489, Status = SalesOrderStatus.Draft },
        };
        db.SalesOrders.AddRange(salesOrders);

        // Sales Invoices (AR)
        var salesInvoices = new List<SalesInvoice>
        {
            new() { InvoiceNumber = "INV-2024-001", CustomerId = customers[0].Id, SalesOrderId = null, InvoiceDate = DateTime.UtcNow.AddDays(-25), DueDate = DateTime.UtcNow.AddDays(5), SubTotal = 12990, TaxAmount = 1299, TotalAmount = 14289, PaidAmount = 14289, Status = SalesInvoiceStatus.Paid },
            new() { InvoiceNumber = "INV-2024-002", CustomerId = customers[1].Id, InvoiceDate = DateTime.UtcNow.AddDays(-18), DueDate = DateTime.UtcNow.AddDays(12), SubTotal = 7840, TaxAmount = 784, TotalAmount = 8624, PaidAmount = 4000, Status = SalesInvoiceStatus.PartiallyPaid },
            new() { InvoiceNumber = "INV-2024-003", CustomerId = customers[2].Id, InvoiceDate = DateTime.UtcNow.AddDays(-5), DueDate = DateTime.UtcNow.AddDays(25), SubTotal = 4970, TaxAmount = 497, TotalAmount = 5467, PaidAmount = 0, Status = SalesInvoiceStatus.Sent },
            new() { InvoiceNumber = "INV-2024-004", CustomerId = customers[3].Id, InvoiceDate = DateTime.UtcNow.AddDays(-45), DueDate = DateTime.UtcNow.AddDays(-15), SubTotal = 2500, TaxAmount = 250, TotalAmount = 2750, PaidAmount = 0, Status = SalesInvoiceStatus.Overdue },
            new() { InvoiceNumber = "INV-2024-005", CustomerId = customers[4].Id, InvoiceDate = DateTime.UtcNow.AddDays(-3), DueDate = DateTime.UtcNow.AddDays(27), SubTotal = 8960, TaxAmount = 896, TotalAmount = 9856, PaidAmount = 0, Status = SalesInvoiceStatus.Draft },
        };
        db.SalesInvoices.AddRange(salesInvoices);

        // AP Invoices
        var apInvoices = new List<APInvoice>
        {
            new() { InvoiceNumber = "AP-2024-001", VendorId = vendors[0].Id, InvoiceDate = DateTime.UtcNow.AddDays(-28), DueDate = DateTime.UtcNow.AddDays(2), SubTotal = 8500, TaxAmount = 850, TotalAmount = 9350, PaidAmount = 9350, Status = InvoiceStatus.Paid },
            new() { InvoiceNumber = "AP-2024-002", VendorId = vendors[1].Id, InvoiceDate = DateTime.UtcNow.AddDays(-18), DueDate = DateTime.UtcNow.AddDays(12), SubTotal = 1200, TaxAmount = 120, TotalAmount = 1320, PaidAmount = 0, Status = InvoiceStatus.Approved },
            new() { InvoiceNumber = "AP-2024-003", VendorId = vendors[2].Id, InvoiceDate = DateTime.UtcNow.AddDays(-10), DueDate = DateTime.UtcNow.AddDays(20), SubTotal = 3000, TaxAmount = 300, TotalAmount = 3300, PaidAmount = 1500, Status = InvoiceStatus.PartiallyPaid },
            new() { InvoiceNumber = "AP-2024-004", VendorId = vendors[3].Id, InvoiceDate = DateTime.UtcNow.AddDays(-50), DueDate = DateTime.UtcNow.AddDays(-20), SubTotal = 4500, TaxAmount = 450, TotalAmount = 4950, PaidAmount = 0, Status = InvoiceStatus.Overdue },
        };
        db.APInvoices.AddRange(apInvoices);

        // Projects
        var projects = new List<Project>
        {
            new() { Name = "ERP Implementation", Code = "PROJ-001", Description = "Full ERP system rollout", CustomerId = customers[0].Id, Status = ProjectStatus.Active, StartDate = DateTime.UtcNow.AddMonths(-3), EndDate = DateTime.UtcNow.AddMonths(3), Budget = 150000, ActualCost = 62000, ProjectManagerName = "Alice Smith", CompletionPercent = 45 },
            new() { Name = "Website Redesign", Code = "PROJ-002", Description = "Corporate website overhaul", CustomerId = customers[1].Id, Status = ProjectStatus.Active, StartDate = DateTime.UtcNow.AddMonths(-1), EndDate = DateTime.UtcNow.AddMonths(2), Budget = 45000, ActualCost = 18000, ProjectManagerName = "Bob Johnson", CompletionPercent = 30 },
            new() { Name = "Mobile App Dev", Code = "PROJ-003", Description = "Customer mobile application", CustomerId = customers[2].Id, Status = ProjectStatus.Planning, StartDate = DateTime.UtcNow.AddMonths(1), EndDate = DateTime.UtcNow.AddMonths(5), Budget = 80000, ActualCost = 0, ProjectManagerName = "Carol White", CompletionPercent = 0 },
            new() { Name = "Data Migration", Code = "PROJ-004", Description = "Legacy data migration", CustomerId = customers[3].Id, Status = ProjectStatus.Completed, StartDate = DateTime.UtcNow.AddMonths(-6), EndDate = DateTime.UtcNow.AddMonths(-1), ActualEndDate = DateTime.UtcNow.AddMonths(-1), Budget = 25000, ActualCost = 23500, ProjectManagerName = "David Brown", CompletionPercent = 100 },
        };
        db.Projects.AddRange(projects);

        // Leads
        var leads = new List<Lead>
        {
            new() { FirstName = "Emma", LastName = "Wilson", Email = "emma@techstart.com", Phone = "+1-555-2001", Company = "TechStart Inc.", JobTitle = "CEO", Status = LeadStatus.Qualified, Source = LeadSource.Website, OwnerName = "Sales Team", EstimatedValue = 50000 },
            new() { FirstName = "James", LastName = "Brown", Email = "james@growthco.com", Phone = "+1-555-2002", Company = "GrowthCo", JobTitle = "CTO", Status = LeadStatus.Contacted, Source = LeadSource.Referral, OwnerName = "Sales Team", EstimatedValue = 30000 },
            new() { FirstName = "Olivia", LastName = "Davis", Email = "olivia@nexthorizon.com", Phone = "+1-555-2003", Company = "Next Horizon", JobTitle = "IT Director", Status = LeadStatus.New, Source = LeadSource.Email, OwnerName = "Sales Team", EstimatedValue = 75000 },
            new() { FirstName = "Noah", LastName = "Martin", Email = "noah@cloudpeak.com", Phone = "+1-555-2004", Company = "CloudPeak", JobTitle = "VP Operations", Status = LeadStatus.Proposal, Source = LeadSource.Event, OwnerName = "Sales Team", EstimatedValue = 120000 },
            new() { FirstName = "Ava", LastName = "Thompson", Email = "ava@digitalnow.com", Phone = "+1-555-2005", Company = "Digital Now", JobTitle = "CFO", Status = LeadStatus.Won, Source = LeadSource.Social, OwnerName = "Sales Team", EstimatedValue = 85000, IsConverted = true },
            new() { FirstName = "Liam", LastName = "Garcia", Email = "liam@innovatek.com", Phone = "+1-555-2006", Company = "InnovaTek", JobTitle = "Product Manager", Status = LeadStatus.Lost, Source = LeadSource.Phone, OwnerName = "Sales Team", EstimatedValue = 40000 },
        };
        db.Leads.AddRange(leads);

        // CRM Contacts
        var contacts = new List<Contact>
        {
            new() { FirstName = "Robert", LastName = "Anderson", Email = "robert@acme.com", Phone = "+1-555-3001", Company = "Acme Corporation", JobTitle = "Procurement Manager", CustomerId = customers[0].Id },
            new() { FirstName = "Jennifer", LastName = "Taylor", Email = "jennifer@globaldyn.com", Phone = "+1-555-3002", Company = "Global Dynamics", JobTitle = "VP Purchasing", CustomerId = customers[1].Id },
            new() { FirstName = "Michael", LastName = "Jackson", Email = "michael@summittech.com", Phone = "+1-555-3003", Company = "Summit Tech", JobTitle = "IT Manager", CustomerId = customers[2].Id },
            new() { FirstName = "Sarah", LastName = "White", Email = "sarah@horizon.com", Phone = "+1-555-3004", Company = "Horizon Industries", JobTitle = "Operations Director", CustomerId = customers[3].Id },
        };
        db.Contacts.AddRange(contacts);

        // Opportunities
        var opportunities = new List<Opportunity>
        {
            new() { Name = "Enterprise Software Suite - Acme", CustomerId = customers[0].Id, Stage = OpportunityStage.Proposal, Amount = 85000, Probability = 60, CloseDate = DateTime.UtcNow.AddDays(30), OwnerName = "Alice Smith" },
            new() { Name = "Hardware Refresh - Global Dynamics", CustomerId = customers[1].Id, Stage = OpportunityStage.Negotiation, Amount = 120000, Probability = 80, CloseDate = DateTime.UtcNow.AddDays(15), OwnerName = "Bob Johnson" },
            new() { Name = "Cloud Migration - Summit Tech", CustomerId = customers[2].Id, Stage = OpportunityStage.Qualification, Amount = 45000, Probability = 40, CloseDate = DateTime.UtcNow.AddDays(45), OwnerName = "Carol White" },
            new() { Name = "Network Upgrade - Horizon", CustomerId = customers[3].Id, Stage = OpportunityStage.Prospecting, Amount = 30000, Probability = 25, CloseDate = DateTime.UtcNow.AddDays(60), OwnerName = "David Brown" },
            new() { Name = "Annual Maintenance Contract", CustomerId = customers[4].Id, Stage = OpportunityStage.ClosedWon, Amount = 25000, Probability = 100, CloseDate = DateTime.UtcNow.AddDays(-5), OwnerName = "Alice Smith" },
            new() { Name = "Security Audit - Pacific", CustomerId = customers[4].Id, Stage = OpportunityStage.ClosedLost, Amount = 15000, Probability = 0, CloseDate = DateTime.UtcNow.AddDays(-10), OwnerName = "Bob Johnson" },
        };
        db.Opportunities.AddRange(opportunities);

        // System Logs
        var logEntries = new List<SystemLog>
        {
            new() { Level = "Information", Message = "Application started successfully", Source = "ERP.Server", Timestamp = DateTime.UtcNow.AddMinutes(-30) },
            new() { Level = "Information", Message = "Demo data seeded successfully", Source = "SeedData", Timestamp = DateTime.UtcNow.AddMinutes(-29) },
            new() { Level = "Information", Message = "User admin@root.com logged in", Source = "AuthEndpoints", Timestamp = DateTime.UtcNow.AddMinutes(-25) },
            new() { Level = "Warning", Message = "Low stock detected for 3 products", Source = "InventoryService", Timestamp = DateTime.UtcNow.AddMinutes(-20) },
            new() { Level = "Information", Message = "Purchase order PO-2024-005 created", Source = "ProcurementEndpoints", Timestamp = DateTime.UtcNow.AddMinutes(-15) },
            new() { Level = "Information", Message = "Sales invoice INV-2024-005 generated", Source = "SalesEndpoints", Timestamp = DateTime.UtcNow.AddMinutes(-10) },
        };
        db.SystemLogs.AddRange(logEntries);

        await db.SaveChangesAsync();
    }

    public static async Task ResetAsync(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await InitializeAsync(sp);
    }

    public static async Task ResetWithDemoAsync(IServiceProvider sp)
    {
        await ResetAsync(sp);
    }
}
