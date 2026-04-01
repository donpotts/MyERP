using System.ComponentModel.DataAnnotations;

namespace ERP.Shared.Models;

public class SystemLog : BaseEntity
{
    [MaxLength(20)]
    public string Level { get; set; } = "Information";
    [Required]
    public string Message { get; set; } = string.Empty;
    [MaxLength(200)]
    public string? Source { get; set; }
    public string? Exception { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    [MaxLength(200)]
    public string? UserId { get; set; }
    [MaxLength(100)]
    public string? UserName { get; set; }
}

public class SystemSettings : BaseEntity
{
    [Required, MaxLength(100)]
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    [MaxLength(50)]
    public string? Category { get; set; }
}

public class CompanyBranding : BaseEntity
{
    [Required, MaxLength(200)]
    public string CompanyName { get; set; } = "ERP";
    [MaxLength(200)]
    public string CompanyTagline { get; set; } = "Enterprise Resource Planning";
    [MaxLength(10)]
    public string LogoText { get; set; } = "ERP";
    [MaxLength(500)]
    public string? LogoUrl { get; set; }
    [MaxLength(500)]
    public string? FaviconUrl { get; set; }
    [MaxLength(200)]
    public string AppTitle { get; set; } = "Enterprise Resource Planning";
}
