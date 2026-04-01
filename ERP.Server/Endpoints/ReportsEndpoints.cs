using Microsoft.EntityFrameworkCore;
using ERP.Server.Data;
using ERP.Shared.DTOs;
using ERP.Shared.Models;

namespace ERP.Server.Endpoints;

public static class ReportsEndpoints
{
    public static void MapReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports").WithTags("Reports").RequireAuthorization();

        group.MapGet("/financial", async (AppDbContext db) =>
        {
            var totalRevenue  = await db.SalesInvoices.SumAsync(i => i.PaidAmount);
            var totalExpenses = await db.APInvoices.SumAsync(i => i.PaidAmount);
            var totalAR       = await db.SalesInvoices.SumAsync(i => i.TotalAmount - i.PaidAmount);
            var totalAP       = await db.APInvoices.SumAsync(i => i.TotalAmount - i.PaidAmount);
            var cashBalance   = await db.BankAccounts.SumAsync(b => b.Balance);
            var grossProfit   = totalRevenue - totalExpenses;
            var profitMargin  = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0;

            var chartStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-11);
            var revenueByMonth = await db.SalesInvoices
                .Where(i => i.InvoiceDate >= chartStart)
                .GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(i => i.TotalAmount) })
                .ToListAsync();
            var expensesByMonth = await db.APInvoices
                .Where(i => i.InvoiceDate >= chartStart)
                .GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(i => i.TotalAmount) })
                .ToListAsync();
            var revenueChart = Enumerable.Range(0, 12)
                .Select(i => chartStart.AddMonths(i))
                .Select(d => new RevenueChartPoint
                {
                    Month    = d.ToString("MMM"),
                    Revenue  = revenueByMonth.FirstOrDefault(r => r.Year == d.Year && r.Month == d.Month)?.Total ?? 0,
                    Expenses = expensesByMonth.FirstOrDefault(e => e.Year == d.Year && e.Month == d.Month)?.Total ?? 0
                })
                .ToList();

            return Results.Ok(ApiResponse<ErpDashboardData>.Success(new ErpDashboardData
            {
                TotalRevenue      = totalRevenue,
                TotalExpenses     = totalExpenses,
                GrossProfit       = grossProfit,
                NetProfit         = grossProfit,
                ProfitMargin      = profitMargin,
                AccountsReceivable= totalAR,
                AccountsPayable   = totalAP,
                CashBalance       = cashBalance,
                RevenueChart      = revenueChart
            }));
        });

        group.MapGet("/sales", async (AppDbContext db) =>
        {
            var totalRevenue = await db.SalesInvoices.SumAsync(i => i.PaidAmount);
            var totalOrders  = await db.SalesOrders.CountAsync();
            var newCustomers = await db.Customers.CountAsync(c => c.CreatedAt >= DateTime.UtcNow.AddDays(-30));

            var topProducts = await db.SalesOrderLines
                .GroupBy(l => l.ProductId)
                .Select(g => new { ProductId = g.Key, QuantitySold = g.Sum(l => l.Quantity), Revenue = g.Sum(l => l.LineTotal) })
                .OrderByDescending(g => g.Revenue)
                .Take(10)
                .Join(db.Products, g => g.ProductId, p => p.Id, (g, p) => new TopProductDto
                {
                    ProductName  = p.Name,
                    QuantitySold = (int)g.QuantitySold,
                    Revenue      = g.Revenue
                })
                .ToListAsync();

            return Results.Ok(ApiResponse<ErpDashboardData>.Success(new ErpDashboardData
            {
                TotalRevenue  = totalRevenue,
                TotalOrders   = totalOrders,
                NewCustomers  = newCustomers,
                TopProducts   = topProducts
            }));
        });

        group.MapGet("/inventory", async (AppDbContext db) =>
        {
            var inventoryValue = await db.StockLevels
                .Join(db.Products, sl => sl.ProductId, p => p.Id, (sl, p) => sl.QuantityOnHand * p.CostPrice)
                .SumAsync();

            var lowStockItems = await db.StockLevels
                .Join(db.Products, sl => sl.ProductId, p => p.Id, (sl, p) => new { sl, p })
                .CountAsync(x => x.sl.QuantityOnHand <= x.p.ReorderPoint);

            var totalProducts = await db.Products.CountAsync(p => p.Status == ProductStatus.Active);

            return Results.Ok(ApiResponse<InventoryReportDto>.Success(new InventoryReportDto
            {
                InventoryValue = inventoryValue,
                LowStockItems  = lowStockItems,
                TotalProducts  = totalProducts
            }));
        });

        group.MapGet("/projects", async (AppDbContext db) =>
        {
            var activeProjects  = await db.Projects.CountAsync(p => p.Status == ProjectStatus.Active);
            var totalHours      = await db.TimeEntries.SumAsync(t => t.Hours);
            var billableRevenue = await db.TimeEntries
                .Where(t => t.IsBillable)
                .SumAsync(t => t.Hours * t.HourlyRate);

            return Results.Ok(ApiResponse<ProjectReportDto>.Success(new ProjectReportDto
            {
                ActiveProjects  = activeProjects,
                TotalHours      = totalHours,
                BillableRevenue = billableRevenue
            }));
        });
    }
}
