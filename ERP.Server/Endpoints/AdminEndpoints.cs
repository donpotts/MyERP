using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ERP.Server.Data;
using ERP.Shared.DTOs;
using ERP.Shared.Models;

namespace ERP.Server.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin").WithTags("Admin");

        // ── /api/admin/users ─────────────────────────────────────────────────
        group.MapGet("/users", async (int page, int pageSize, string? search, UserManager<ApplicationUser> userManager) =>
        {
            page     = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = userManager.Users.Where(u => u.IsActive);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => (u.FirstName + " " + u.LastName).Contains(search) || u.Email!.Contains(search));

            var total = await query.CountAsync();
            var paged = await query.OrderBy(u => u.FirstName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var items = new List<object>();
            foreach (var u in paged)
            {
                var roles = await userManager.GetRolesAsync(u);
                items.Add(new
                {
                    Id       = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}".Trim(),
                    Email    = u.Email ?? "",
                    Role     = roles.FirstOrDefault() ?? "",
                    IsActive = u.IsActive
                });
            }

            return Results.Ok(ApiResponse<PagedResult<object>>.Success(new PagedResult<object>
            {
                Items      = items,
                TotalCount = total,
                Page       = page,
                PageSize   = pageSize
            }));
        }).RequireAuthorization();

        // ── /api/admin/users/invite ──────────────────────────────────────────
        group.MapPost("/users/invite", async (UserInviteRequest req, UserManager<ApplicationUser> userManager) =>
        {
            var nameParts = req.FullName.Trim().Split(' ', 2);
            var user      = new ApplicationUser
            {
                UserName  = req.Email,
                Email     = req.Email,
                FirstName = nameParts[0],
                LastName  = nameParts.Length > 1 ? nameParts[1] : "",
                IsActive  = true
            };
            var result = await userManager.CreateAsync(user, "Temp1234!");
            if (!result.Succeeded)
                return Results.BadRequest(ApiResponse<string>.Fail(string.Join(", ", result.Errors.Select(e => e.Description))));

            await userManager.AddToRoleAsync(user, req.Role);
            return Results.Ok(ApiResponse<string>.Success("User invited successfully."));
        }).RequireAuthorization();

        group.MapPost("/load-demo", async (IServiceProvider sp) =>
        {
            await SeedData.SeedDemoDataAsync(sp);
            return Results.Ok(ApiResponse<string>.Success("Demo data loaded successfully."));
        }).RequireAuthorization();

        group.MapPost("/reset", async (IServiceProvider sp) =>
        {
            await SeedData.ResetAsync(sp);
            return Results.Ok(ApiResponse<string>.Success("Database reset to clean state."));
        }).RequireAuthorization();

        group.MapPost("/reset-demo", async (IServiceProvider sp) =>
        {
            await SeedData.ResetWithDemoAsync(sp);
            return Results.Ok(ApiResponse<string>.Success("Database reset and demo data loaded."));
        }).RequireAuthorization();
    }

    private record UserInviteRequest(string FullName, string Email, string Role);
}
