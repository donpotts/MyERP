using System.ComponentModel.DataAnnotations;

namespace ERP.Shared.DTOs;

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required, MinLength(4)]
    public string Password { get; set; } = string.Empty;
    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class AuthResponse
{
    public bool IsSuccess { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Message { get; set; }
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? AvatarUrl { get; set; }
    public string? Role { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required, MinLength(4)]
    public string NewPassword { get; set; } = string.Empty;
    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ShortBio { get; set; }
    public string? JobTitle { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
}

public class SessionInfo
{
    public string SessionId { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LoginTime { get; set; }
    public bool IsCurrent { get; set; }
}
