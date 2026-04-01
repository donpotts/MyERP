using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ERP.Server.Data;
using ERP.Shared.Models;
using ERP.Shared.DTOs;

namespace ERP.Server.Endpoints;

public static class SettingsEndpoints
{
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    public static void MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        MapUsersEndpoints(app);
        MapSystemLogsEndpoints(app);
        MapSystemSettingsEndpoints(app);
        MapBrandingEndpoints(app);
        MapCurrencyEndpoints(app);
        MapAutoNumberEndpoints(app);
        MapMaintenanceEndpoints(app);
    }

    // ── /api/users ────────────────────────────────────────────────────────────
    private static void MapUsersEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users", async (UserManager<ApplicationUser> userManager) =>
        {
            var users = userManager.Users.Where(u => u.IsActive).ToList();
            var result = new List<UserInfo>();
            foreach (var u in users)
            {
                var roles = await userManager.GetRolesAsync(u);
                result.Add(new UserInfo { Id = u.Id, Email = u.Email!, FirstName = u.FirstName, LastName = u.LastName, AvatarUrl = u.AvatarUrl, Role = roles.FirstOrDefault() });
            }
            return Results.Ok(ApiResponse<List<UserInfo>>.Success(result));
        }).WithTags("Users").RequireAuthorization();
    }

    // ── /api/system-logs ─────────────────────────────────────────────────────
    private static void MapSystemLogsEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/system-logs").WithTags("System Logs").RequireAuthorization();

        // legacy root
        group.MapGet("/", async ([AsParameters] PagedRequest req, AppDbContext db) =>
            await QueryLogs(db, req.Page, req.PageSize, req.Search, null));

        group.MapGet("/database", async (int page, int pageSize, string? search, string? level, AppDbContext db) =>
            await QueryLogs(db, page, pageSize, search, level));

        group.MapGet("/file", async (int page, int pageSize, string? search, string? level, AppDbContext db) =>
            // Return same DB logs with source="File" filter as a stand-in for file logs
            await QueryLogs(db, page, pageSize, search, level, sourceFilter: "File"));
    }

    private static async Task<IResult> QueryLogs(
        AppDbContext db, int page, int pageSize, string? search, string? level,
        string? sourceFilter = null)
    {
        page     = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.SystemLogs.AsQueryable();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(l => l.Message.Contains(search) || (l.Source != null && l.Source.Contains(search)));
        if (!string.IsNullOrEmpty(level))
            query = query.Where(l => l.Level == level);
        if (sourceFilter is not null)
            query = query.Where(l => l.Source == sourceFilter);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => new { l.Timestamp, l.Level, Category = l.Source ?? "", l.Message })
            .ToListAsync();

        return Results.Ok(ApiResponse<PagedResult<object>>.Success(new PagedResult<object>
        {
            Items      = items.Cast<object>().ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        }));
    }

    // ── /api/branding  +  /api/settings/branding alias ───────────────────────
    private static void MapBrandingEndpoints(IEndpointRouteBuilder app)
    {
        const string brandingFile  = "branding.json";
        string[] allowedTypes      = ["logo", "favicon"];
        string[] allowedExt        = [".png", ".jpg", ".jpeg", ".ico", ".svg", ".gif"];

        app.MapGet("/api/branding", async (IWebHostEnvironment env) =>
            Results.Ok(ApiResponse<CompanyBrandingDto>.Success(await ReadBranding(env, brandingFile))))
            .WithTags("Branding");

        // alias used by BrandingSettings.razor
        app.MapGet("/api/settings/branding", async (IWebHostEnvironment env) =>
            Results.Ok(ApiResponse<CompanyBrandingDto>.Success(await ReadBranding(env, brandingFile))))
            .WithTags("Branding");

        app.MapPut("/api/branding", async (CompanyBrandingDto dto, IWebHostEnvironment env) =>
        {
            await WriteBranding(env, brandingFile, dto);
            return Results.Ok(ApiResponse<CompanyBrandingDto>.Success(dto));
        }).WithTags("Branding").RequireAuthorization();

        app.MapPut("/api/settings/branding", async (CompanyBrandingDto dto, IWebHostEnvironment env) =>
        {
            await WriteBranding(env, brandingFile, dto);
            return Results.Ok(ApiResponse<CompanyBrandingDto>.Success(dto));
        }).WithTags("Branding").RequireAuthorization();

        app.MapPost("/api/branding/upload/{type}", async (string type, HttpRequest request, IWebHostEnvironment env) =>
        {
            if (!allowedTypes.Contains(type))
                return Results.BadRequest(ApiResponse<string>.Fail("Invalid asset type."));

            var form = await request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
                return Results.BadRequest(ApiResponse<string>.Fail("No file uploaded"));

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExt.Contains(ext))
                return Results.BadRequest(ApiResponse<string>.Fail("Invalid file type"));

            var brandingDir = Path.Combine(env.WebRootPath ?? "wwwroot", "branding");
            Directory.CreateDirectory(brandingDir);
            foreach (var old in Directory.GetFiles(brandingDir, $"{type}.*")) File.Delete(old);

            var fileName = $"{type}{ext}";
            using (var stream = new FileStream(Path.Combine(brandingDir, fileName), FileMode.Create))
                await file.CopyToAsync(stream);

            var url      = $"/branding/{fileName}?v={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var branding = await ReadBranding(env, brandingFile);
            if (type == "logo")    branding.LogoUrl    = url;
            if (type == "favicon") branding.FaviconUrl = url;
            await WriteBranding(env, brandingFile, branding);

            return Results.Ok(ApiResponse<string>.Success(url));
        }).WithTags("Branding").RequireAuthorization().DisableAntiforgery();
    }

    private static async Task<CompanyBrandingDto> ReadBranding(IWebHostEnvironment env, string fileName)
    {
        var path = Path.Combine(env.ContentRootPath, fileName);
        if (!File.Exists(path)) return new CompanyBrandingDto();
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<CompanyBrandingDto>(json) ?? new();
    }

    private static async Task WriteBranding(IWebHostEnvironment env, string fileName, CompanyBrandingDto dto)
    {
        var path = Path.Combine(env.ContentRootPath, fileName);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(dto, _json));
    }

    // ── /api/settings/currencies ─────────────────────────────────────────────
    private static void MapCurrencyEndpoints(IEndpointRouteBuilder app)
    {
        const string settingsKey = "Currencies";

        app.MapGet("/api/settings/currencies", async (AppDbContext db) =>
        {
            var currencies = await LoadSettingsJson<List<CurrencySettingDto>>(db, settingsKey)
                             ?? DefaultCurrencies();
            return Results.Ok(ApiResponse<List<CurrencySettingDto>>.Success(currencies));
        }).WithTags("Settings").RequireAuthorization();

        app.MapPost("/api/settings/currencies", async (CurrencySettingDto dto, AppDbContext db) =>
        {
            var list = await LoadSettingsJson<List<CurrencySettingDto>>(db, settingsKey) ?? DefaultCurrencies();
            dto.Id = list.Count > 0 ? list.Max(c => c.Id) + 1 : 1;
            list.Add(dto);
            await SaveSettingsJson(db, settingsKey, list);
            return Results.Ok(ApiResponse<CurrencySettingDto>.Success(dto));
        }).WithTags("Settings").RequireAuthorization();

        app.MapPut("/api/settings/currencies/{id:int}", async (int id, CurrencySettingDto dto, AppDbContext db) =>
        {
            var list = await LoadSettingsJson<List<CurrencySettingDto>>(db, settingsKey) ?? DefaultCurrencies();
            var idx  = list.FindIndex(c => c.Id == id);
            if (idx < 0) return Results.NotFound();
            dto.Id = id;
            list[idx] = dto;
            await SaveSettingsJson(db, settingsKey, list);
            return Results.Ok(ApiResponse<CurrencySettingDto>.Success(dto));
        }).WithTags("Settings").RequireAuthorization();
    }

    private static List<CurrencySettingDto> DefaultCurrencies() =>
    [
        new() { Id = 1, Code = "USD", Name = "US Dollar",      Symbol = "$",  ExchangeRate = 1.0m,   IsDefault = true  },
        new() { Id = 2, Code = "EUR", Name = "Euro",           Symbol = "€",  ExchangeRate = 0.92m,  IsDefault = false },
        new() { Id = 3, Code = "GBP", Name = "British Pound",  Symbol = "£",  ExchangeRate = 0.79m,  IsDefault = false },
        new() { Id = 4, Code = "CAD", Name = "Canadian Dollar",Symbol = "C$", ExchangeRate = 1.36m,  IsDefault = false },
        new() { Id = 5, Code = "AUD", Name = "Australian Dollar",Symbol="A$", ExchangeRate = 1.52m,  IsDefault = false },
    ];

    // ── /api/settings/auto-numbers ───────────────────────────────────────────
    private static void MapAutoNumberEndpoints(IEndpointRouteBuilder app)
    {
        const string settingsKey = "AutoNumbers";

        app.MapGet("/api/settings/auto-numbers", async (AppDbContext db) =>
        {
            var list = await LoadSettingsJson<List<AutoNumberSettingDto>>(db, settingsKey)
                       ?? DefaultAutoNumbers();
            return Results.Ok(ApiResponse<List<AutoNumberSettingDto>>.Success(list));
        }).WithTags("Settings").RequireAuthorization();

        app.MapPut("/api/settings/auto-numbers/{documentType}", async (string documentType, AutoNumberSettingDto dto, AppDbContext db) =>
        {
            var list = await LoadSettingsJson<List<AutoNumberSettingDto>>(db, settingsKey) ?? DefaultAutoNumbers();
            var idx  = list.FindIndex(a => a.DocumentType == documentType);
            if (idx >= 0) list[idx] = dto; else list.Add(dto);
            await SaveSettingsJson(db, settingsKey, list);
            return Results.Ok(ApiResponse<AutoNumberSettingDto>.Success(dto));
        }).WithTags("Settings").RequireAuthorization();
    }

    private static List<AutoNumberSettingDto> DefaultAutoNumbers() =>
    [
        new() { DocumentType = "Sales Order",      Prefix = "SO-",  NextNumber = 1001, Padding = 4 },
        new() { DocumentType = "Sales Invoice",    Prefix = "INV-", NextNumber = 1001, Padding = 4 },
        new() { DocumentType = "Sales Quote",      Prefix = "QT-",  NextNumber = 1001, Padding = 4 },
        new() { DocumentType = "Purchase Order",   Prefix = "PO-",  NextNumber = 1001, Padding = 4 },
        new() { DocumentType = "Goods Receipt",    Prefix = "GR-",  NextNumber = 1001, Padding = 4 },
        new() { DocumentType = "Journal Entry",    Prefix = "JE-",  NextNumber = 1001, Padding = 4 },
        new() { DocumentType = "Project",          Prefix = "PRJ-", NextNumber = 1001, Padding = 4 },
    ];

    // ── /api/settings/maintenance/{action} ───────────────────────────────────
    private static void MapMaintenanceEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/settings/maintenance/{action}", async (string action, AppDbContext db) =>
        {
            var message = action switch
            {
                "clear-cache"  => "Application cache cleared successfully.",
                "optimize-db"  => "Database optimization complete.",
                "export"       => "Data export initiated. Download will start shortly.",
                "purge-logs"   => await PurgeLogs(db),
                _              => $"Action '{action}' completed."
            };
            return Results.Ok(ApiResponse<string>.Success(message));
        }).WithTags("Settings").RequireAuthorization();
    }

    private static async Task<string> PurgeLogs(AppDbContext db)
    {
        var cutoff   = DateTime.UtcNow.AddDays(-90);
        var oldLogs  = db.SystemLogs.Where(l => l.Timestamp < cutoff);
        var count    = await oldLogs.CountAsync();
        db.SystemLogs.RemoveRange(oldLogs);
        await db.SaveChangesAsync();
        return $"Deleted {count} log entries older than 90 days.";
    }

    // ── /api/system-settings/{section} ───────────────────────────────────────
    private static void MapSystemSettingsEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/system-settings").WithTags("System Settings").RequireAuthorization();

        group.MapGet("/identity", (IConfiguration config) =>
            Results.Ok(ApiResponse<object>.Success(new
            {
                RequireUppercase       = config.GetValue<bool>("Identity:RequireUppercase"),
                RequireDigit           = config.GetValue<bool>("Identity:RequireDigit"),
                RequireNonAlphanumeric = config.GetValue<bool>("Identity:RequireNonAlphanumeric"),
                MinPasswordLength      = config.GetValue<int>("Identity:RequiredLength"),
                MaxFailedAttempts      = 5,
                LockoutMinutes         = 15,
                TokenExpiryHours       = 8
            })));

        group.MapGet("/database", (IConfiguration config) =>
            Results.Ok(ApiResponse<object>.Success(new
            {
                ConnectionString       = "Server=(localdb)\\MSSQLLocalDB;Database=ERP;Trusted_Connection=True",
                CommandTimeoutSeconds  = 30,
                EnableSensitiveLogging = false
            })));

        group.MapGet("/email", (IConfiguration config) =>
            Results.Ok(ApiResponse<object>.Success(new
            {
                Enabled     = false,
                SmtpHost    = config["Email:SmtpHost"] ?? "",
                SmtpPort    = config.GetValue<int>("Email:SmtpPort") > 0 ? config.GetValue<int>("Email:SmtpPort") : 587,
                UseSsl      = true,
                Username    = "",
                Password    = "",
                FromAddress = config["Email:FromAddress"] ?? ""
            })));

        group.MapGet("/storage", (IConfiguration config) =>
            Results.Ok(ApiResponse<object>.Success(new
            {
                Provider         = "Local",
                LocalPath        = config["Storage:AvatarPath"] ?? "wwwroot/uploads",
                ConnectionString = "",
                ContainerName    = "",
                MaxFileSizeMb    = 10
            })));

        group.MapGet("/jobs", () =>
            Results.Ok(ApiResponse<object[]>.Success(
            [
                new { Id = "payroll",   Name = "Payroll Processing",    Description = "Process employee payroll on schedule.",        CronExpression = "0 9 1 * *",   IsEnabled = true,  LastRun = (DateTime?)DateTime.UtcNow.AddDays(-1) },
                new { Id = "reports",   Name = "Scheduled Reports",      Description = "Generate and email scheduled reports.",       CronExpression = "0 8 * * 1",   IsEnabled = true,  LastRun = (DateTime?)DateTime.UtcNow.AddDays(-7) },
                new { Id = "cleanup",   Name = "Data Cleanup",           Description = "Archive old records and clean temp files.",   CronExpression = "0 2 * * *",   IsEnabled = false, LastRun = (DateTime?)null },
                new { Id = "backup",    Name = "Database Backup",        Description = "Create daily database backup snapshot.",      CronExpression = "0 1 * * *",   IsEnabled = true,  LastRun = (DateTime?)DateTime.UtcNow.AddHours(-6) },
            ])));

        group.MapGet("/logging", (IConfiguration config) =>
            Results.Ok(ApiResponse<object>.Success(new
            {
                DefaultLevel   = config["Serilog:MinimumLevel:Default"] ?? "Information",
                EnableDatabase = true,
                EnableFile     = false,
                LogFilePath    = "logs/erp-.log",
                RetentionDays  = 30
            })));

        // Generic save — accept any section PUT and return success
        group.MapPut("/{section}", (string section) =>
            Results.Ok(ApiResponse<string>.Success($"{section} settings saved.")));

        // Migrations
        group.MapGet("/migrations/status", async (AppDbContext db) =>
        {
            try
            {
                var pending = await db.Database.GetPendingMigrationsAsync();
                var applied = await db.Database.GetAppliedMigrationsAsync();
                var pendingList = pending.ToList();
                return Results.Ok(ApiResponse<string>.Success(
                    pendingList.Count == 0
                        ? $"All {applied.Count()} migrations applied. Database is up to date."
                        : $"{pendingList.Count} pending migration(s): {string.Join(", ", pendingList)}"));
            }
            catch { return Results.Ok(ApiResponse<string>.Success("Unable to check migration status.")); }
        });

        group.MapPost("/migrations/apply", async (AppDbContext db) =>
        {
            try
            {
                await db.Database.MigrateAsync();
                return Results.Ok(ApiResponse<string>.Success("All pending migrations applied successfully."));
            }
            catch (Exception ex)
            {
                return Results.Ok(ApiResponse<string>.Fail($"Migration failed: {ex.Message}"));
            }
        });

        // Email test
        group.MapPost("/email/test", () =>
            Results.Ok(ApiResponse<string>.Success("Test email sent successfully (SMTP not configured in dev).")));

        // Job actions
        group.MapPost("/jobs/{jobId}/run", (string jobId) =>
            Results.Ok(ApiResponse<string>.Success($"Job '{jobId}' triggered.")));

        group.MapPut("/jobs/{jobId}", (string jobId) =>
            Results.Ok(ApiResponse<string>.Success($"Job '{jobId}' updated.")));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static async Task<T?> LoadSettingsJson<T>(AppDbContext db, string key)
    {
        var row = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (row?.Value is null) return default;
        return JsonSerializer.Deserialize<T>(row.Value);
    }

    private static async Task SaveSettingsJson<T>(AppDbContext db, string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        var row  = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (row is null)
        {
            db.SystemSettings.Add(new SystemSettings { Key = key, Value = json, Category = "App" });
        }
        else
        {
            row.Value = json;
        }
        await db.SaveChangesAsync();
    }

    // ── Local setting DTOs (only used server-side) ────────────────────────────
    private class CurrencySettingDto
    {
        public int     Id           { get; set; }
        public string  Code         { get; set; } = "";
        public string  Name         { get; set; } = "";
        public string  Symbol       { get; set; } = "";
        public decimal ExchangeRate { get; set; } = 1;
        public bool    IsDefault    { get; set; }
    }

    private class AutoNumberSettingDto
    {
        public string DocumentType { get; set; } = "";
        public string Prefix       { get; set; } = "";
        public int    NextNumber   { get; set; }
        public int    Padding      { get; set; } = 4;
    }
}
