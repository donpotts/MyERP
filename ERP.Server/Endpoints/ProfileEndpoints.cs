using Microsoft.AspNetCore.Identity;
using ERP.Shared.Models;
using ERP.Shared.DTOs;

namespace ERP.Server.Endpoints;

public static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile").WithTags("Profile").RequireAuthorization();

        group.MapGet("/", async (HttpContext ctx, UserManager<ApplicationUser> userManager) =>
        {
            var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Results.Unauthorized();
            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.NotFound();

            return Results.Ok(ApiResponse<UpdateProfileRequest>.Success(new UpdateProfileRequest
            {
                FirstName = user.FirstName, LastName = user.LastName, ShortBio = user.ShortBio,
                JobTitle = user.JobTitle, DateOfBirth = user.DateOfBirth, Phone = user.PhoneNumber,
                Address = user.Address, City = user.City, Country = user.Country, PostalCode = user.PostalCode
            }));
        });

        group.MapPut("/", async (UpdateProfileRequest req, HttpContext ctx, UserManager<ApplicationUser> userManager) =>
        {
            var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Results.Unauthorized();
            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.NotFound();

            user.FirstName = req.FirstName; user.LastName = req.LastName; user.ShortBio = req.ShortBio;
            user.JobTitle = req.JobTitle; user.DateOfBirth = req.DateOfBirth; user.PhoneNumber = req.Phone;
            user.Address = req.Address; user.City = req.City; user.Country = req.Country; user.PostalCode = req.PostalCode;
            await userManager.UpdateAsync(user);

            return Results.Ok(ApiResponse<string>.Success("Profile updated."));
        });

        group.MapPost("/change-password", async (ChangePasswordRequest req, HttpContext ctx, UserManager<ApplicationUser> userManager) =>
        {
            var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Results.Unauthorized();
            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.NotFound();
            var result = await userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);
            if (!result.Succeeded)
                return Results.Ok(ApiResponse<string>.Fail(string.Join("; ", result.Errors.Select(e => e.Description))));
            return Results.Ok(ApiResponse<string>.Success("Password changed."));
        });

        group.MapPost("/avatar", async (HttpRequest request, HttpContext ctx, UserManager<ApplicationUser> userManager, IWebHostEnvironment env) =>
        {
            var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return Results.Unauthorized();
            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.NotFound();

            var form = await request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
                return Results.BadRequest(ApiResponse<string>.Fail("No file uploaded"));

            var uploadsDir = Path.Combine(env.WebRootPath ?? "wwwroot", "avatars");
            Directory.CreateDirectory(uploadsDir);
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{userId}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var url = $"/avatars/{fileName}?v={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            user.AvatarUrl = url;
            await userManager.UpdateAsync(user);
            return Results.Ok(ApiResponse<string>.Success(url));
        }).DisableAntiforgery();

        group.MapGet("/sessions", (HttpContext ctx) =>
        {
            var session = new SessionInfo
            {
                SessionId = Guid.NewGuid().ToString(),
                IpAddress = ctx.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = ctx.Request.Headers.UserAgent.ToString(),
                LoginTime = DateTime.UtcNow.AddHours(-1),
                IsCurrent = true
            };
            return Results.Ok(ApiResponse<List<SessionInfo>>.Success([session]));
        });
    }
}
