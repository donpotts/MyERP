using Microsoft.EntityFrameworkCore;
using ERP.Server.Data;
using ERP.Shared.DTOs;
using ERP.Shared.Models;

namespace ERP.Server.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard", async (AppDbContext db) =>
        {
            var totalCustomers = await db.Customers.CountAsync();
            var totalOrders = await db.SalesOrders.CountAsync();
            var pendingOrders = await db.SalesOrders.CountAsync(o => o.Status == SalesOrderStatus.Confirmed || o.Status == SalesOrderStatus.Processing);
            var activeProjects = await db.Projects.CountAsync(p => p.Status == ProjectStatus.Active);
            var openLeads = await db.Leads.CountAsync(l => l.Status != LeadStatus.Won && l.Status != LeadStatus.Lost);

            var totalRevenue = await db.SalesInvoices.SumAsync(i => i.PaidAmount);
            var totalAR = await db.SalesInvoices.SumAsync(i => i.TotalAmount - i.PaidAmount);
            var totalAP = await db.APInvoices.SumAsync(i => i.TotalAmount - i.PaidAmount);
            var cashBalance = await db.BankAccounts.SumAsync(b => b.Balance);
            var inventoryValue = await db.StockLevels
                .Join(db.Products, sl => sl.ProductId, p => p.Id, (sl, p) => sl.QuantityOnHand * p.CostPrice)
                .SumAsync();

            var totalExpenses = await db.APInvoices.SumAsync(i => i.PaidAmount);
            var grossProfit = totalRevenue - totalExpenses;
            var profitMargin = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0;

            var lowStockItems = await db.StockLevels
                .Join(db.Products, sl => sl.ProductId, p => p.Id, (sl, p) => new { sl, p })
                .CountAsync(x => x.sl.QuantityOnHand <= x.p.ReorderPoint);

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
                    Month = d.ToString("MMM"),
                    Revenue = revenueByMonth.FirstOrDefault(r => r.Year == d.Year && r.Month == d.Month)?.Total ?? 0,
                    Expenses = expensesByMonth.FirstOrDefault(e => e.Year == d.Year && e.Month == d.Month)?.Total ?? 0
                })
                .ToList();

            var recentOrders = await db.SalesOrders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new RecentOrderDto
                {
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.Customer != null ? o.Customer.Name : "Unknown",
                    Amount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    OrderDate = o.OrderDate
                })
                .ToListAsync();

            return Results.Ok(ApiResponse<ErpDashboardData>.Success(new ErpDashboardData
            {
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                GrossProfit = grossProfit,
                NetProfit = grossProfit,
                ProfitMargin = profitMargin,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                TotalCustomers = totalCustomers,
                NewCustomers = await db.Customers.CountAsync(c => c.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
                InventoryValue = inventoryValue,
                LowStockItems = lowStockItems,
                CashBalance = cashBalance,
                AccountsReceivable = totalAR,
                AccountsPayable = totalAP,
                ActiveProjects = activeProjects,
                OpenLeads = openLeads,
                RevenueChart = revenueChart,
                RecentOrders = recentOrders
            }));
        }).WithTags("Dashboard").RequireAuthorization();

        app.MapGet("/api/admin/status", async (AppDbContext db) =>
        {
            var customerCount = await db.Customers.CountAsync();
            var productCount = await db.Products.CountAsync();
            return Results.Ok(ApiResponse<SetupStatusDto>.Success(new SetupStatusDto
            {
                IsNew = customerCount == 0,
                HasDemoData = customerCount > 0,
                CustomerCount = customerCount,
                ProductCount = productCount
            }));
        });
    }
}
