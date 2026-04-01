namespace ERP.Shared.DTOs;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string? message = null) =>
        new() { IsSuccess = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message) =>
        new() { IsSuccess = false, Message = message };
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool? SortDescending { get; set; }
}

public class ErpDashboardData
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public decimal InventoryValue { get; set; }
    public int LowStockItems { get; set; }
    public decimal CashBalance { get; set; }
    public decimal AccountsReceivable { get; set; }
    public decimal AccountsPayable { get; set; }
    public int ActiveProjects { get; set; }
    public int OpenLeads { get; set; }
    public List<RevenueChartPoint> RevenueChart { get; set; } = [];
    public List<TopProductDto> TopProducts { get; set; } = [];
    public List<RecentOrderDto> RecentOrders { get; set; } = [];
}

public class RevenueChartPoint
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
}

public class TopProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class RecentOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
}

public class InventoryReportDto
{
    public decimal InventoryValue { get; set; }
    public int LowStockItems { get; set; }
    public int TotalProducts { get; set; }
}

public class ProjectReportDto
{
    public int ActiveProjects { get; set; }
    public decimal TotalHours { get; set; }
    public decimal BillableRevenue { get; set; }
}

public class SetupStatusDto
{
    public bool IsNew { get; set; }
    public bool HasDemoData { get; set; }
    public int CustomerCount { get; set; }
    public int ProductCount { get; set; }
}

public class CompanyBrandingDto
{
    public string CompanyName         { get; set; } = "ERP";
    public string CompanyTagline      { get; set; } = "Enterprise Resource Planning";
    public string LogoText            { get; set; } = "ERP";
    public string? LogoUrl            { get; set; }
    public string? FaviconUrl         { get; set; }
    public string AppTitle            { get; set; } = "Enterprise Resource Planning";
    public bool   TruncateSidebarText { get; set; } = false;
}

public class SystemSettingsDto
{
    public IdentitySettings Identity { get; set; } = new();
    public DatabaseSettings Database { get; set; } = new();
    public JobSettings Jobs { get; set; } = new();
    public EmailSettings Email { get; set; } = new();
    public StorageSettings Storage { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}

public class IdentitySettings
{
    public bool RequireDigit { get; set; }
    public int RequiredLength { get; set; }
    public bool RequireNonAlphanumeric { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireLowercase { get; set; }
    public int RequiredUniqueChars { get; set; }
    public string CookieName { get; set; } = string.Empty;
    public string LoginPath { get; set; } = string.Empty;
    public string LogoutPath { get; set; } = string.Empty;
    public string AccessDeniedPath { get; set; } = string.Empty;
    public int ExpireDays { get; set; }
    public string DefaultAdminEmail { get; set; } = string.Empty;
    public string DefaultAdminPassword { get; set; } = string.Empty;
}

public class DatabaseSettings
{
    public string Provider { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

public class JobSettings
{
    public bool EnableBackgroundJobs { get; set; }
    public int PayrollProcessingDay { get; set; }
}

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
}

public class StorageSettings
{
    public string AvatarPath { get; set; } = string.Empty;
    public long MaxFileSize { get; set; }
    public string AllowedExtensions { get; set; } = string.Empty;
}

public class LoggingSettings
{
    public string MinimumLevel { get; set; } = string.Empty;
    public bool EnableDatabaseLogging { get; set; }
    public bool EnableFileLogging { get; set; }
    public string LogFilePath { get; set; } = string.Empty;
}
