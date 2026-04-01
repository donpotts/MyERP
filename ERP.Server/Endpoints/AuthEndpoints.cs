using Microsoft.AspNetCore.Identity;
using ERP.Shared.Models;
using ERP.Shared.DTOs;
using ERP.Server.Helpers;

namespace ERP.Server.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/login", async (LoginRequest req, UserManager<ApplicationUser> userManager, IConfiguration config) =>
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null || !user.IsActive)
                return Results.Ok(new AuthResponse { Message = "Invalid credentials" });

            if (!await userManager.CheckPasswordAsync(user, req.Password))
                return Results.Ok(new AuthResponse { Message = "Invalid credentials" });

            user.LastLogin = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            var roles = await userManager.GetRolesAsync(user);
            var token = JwtHelper.GenerateToken(user, roles, config);

            return Results.Ok(new AuthResponse
            {
                IsSuccess = true,
                Token = token,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AvatarUrl = user.AvatarUrl,
                    Role = roles.FirstOrDefault()
                }
            });
        });

        group.MapPost("/register", async (RegisterRequest req, UserManager<ApplicationUser> userManager, IConfiguration config) =>
        {
            if (await userManager.FindByEmailAsync(req.Email) is not null)
                return Results.Ok(new AuthResponse { Message = "Email already registered" });

            var user = new ApplicationUser
            {
                UserName = req.Email, Email = req.Email,
                FirstName = req.FirstName, LastName = req.LastName,
                EmailConfirmed = true, IsActive = true
            };

            var result = await userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded)
                return Results.Ok(new AuthResponse { Message = string.Join("; ", result.Errors.Select(e => e.Description)) });

            await userManager.AddToRoleAsync(user, "Employee");
            var roles = await userManager.GetRolesAsync(user);
            var token = JwtHelper.GenerateToken(user, roles, config);

            return Results.Ok(new AuthResponse
            {
                IsSuccess = true,
                Token = token,
                User = new UserInfo { Id = user.Id, Email = user.Email!, FirstName = user.FirstName, LastName = user.LastName, Role = "Employee" }
            });
        });
    }
}
