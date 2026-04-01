using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using ERP.Server.Data;
using ERP.Server.Endpoints;
using ERP.Shared.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ──────────────────────────────────────────────────────────────────
var identityConfig = builder.Configuration.GetSection("Identity");
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit           = identityConfig.GetValue<bool>("RequireDigit");
    options.Password.RequiredLength         = identityConfig.GetValue<int>("RequiredLength");
    options.Password.RequireNonAlphanumeric = identityConfig.GetValue<bool>("RequireNonAlphanumeric");
    options.Password.RequireUppercase       = identityConfig.GetValue<bool>("RequireUppercase");
    options.Password.RequireLowercase       = identityConfig.GetValue<bool>("RequireLowercase");
    options.Password.RequiredUniqueChars    = identityConfig.GetValue<int>("RequiredUniqueChars");
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── JWT Authentication ────────────────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
    options.AddPolicy("AllowClient", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddOpenApi();

// ── App ───────────────────────────────────────────────────────────────────────
var app = builder.Build();

using (var scope = app.Services.CreateScope())
    await SeedData.InitializeAsync(scope.ServiceProvider);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ERP API")
               .WithPreferredScheme("Bearer")
               .WithHttpBearerAuthentication(bearer => bearer.Token = "");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowClient");
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// ── Endpoints ─────────────────────────────────────────────────────────────────
app.MapAdminEndpoints();
app.MapAuthEndpoints();
app.MapProfileEndpoints();
app.MapDashboardEndpoints();
app.MapReportsEndpoints();
app.MapFinanceEndpoints();
app.MapInventoryEndpoints();
app.MapProcurementEndpoints();
app.MapSalesEndpoints();
app.MapProjectsEndpoints();
app.MapCRMEndpoints();
app.MapSettingsEndpoints();

app.MapFallbackToFile("index.html");
app.Run();
