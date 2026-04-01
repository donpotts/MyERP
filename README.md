# ERP — Enterprise Resource Planning System

> A full-featured, self-hosted ERP built with .NET 10, Blazor WebAssembly, and ASP.NET Core Minimal APIs.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-WASM-7B42BC?style=flat-square&logo=blazor)
![EF Core](https://img.shields.io/badge/EF_Core-10.0-blue?style=flat-square)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-3-06B6D4?style=flat-square&logo=tailwindcss)
![SQL Server](https://img.shields.io/badge/SQL_Server-LocalDB-CC2927?style=flat-square&logo=microsoftsqlserver)

---

## Table of Contents

- [Overview](#overview)
- [Modules](#modules)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Default Credentials](#default-credentials)
- [API Reference](#api-reference)
- [Configuration](#configuration)
- [Shared Components](#shared-components)

---

## Overview

This ERP system covers the full business lifecycle — from lead capture to invoicing, from purchase orders to inventory management, and from project tracking to financial reporting. Everything is self-hosted: the ASP.NET Core server serves both the REST API and the compiled Blazor WebAssembly client as static files.

**Highlights:**

- Role-based access control with 6 built-in roles
- Dark mode with preference persisted to localStorage
- Company branding (logo, name, tagline, favicon) configurable live from the UI
- Server-side search, sort, and pagination on all list views
- Soft deletes throughout — data is never hard-deleted
- Interactive API docs at `/scalar` (development only)

---

## Modules

| Module | Features |
|--------|----------|
| **Dashboard** | KPIs, revenue/expense chart, recent orders, low-stock alerts — all from live DB data |
| **Sales** | Customers, Quotations, Sales Orders, Sales Invoices — full CRUD with modals |
| **Inventory** | Products, Categories, Warehouses (with total value), Stock Levels, Stock Movements |
| **Procurement** | Vendors, Purchase Orders, Goods Receipts |
| **Finance** | Chart of Accounts, Journal Entries, AP Invoices, AR Invoices, Bank Accounts & Transactions |
| **Projects** | Projects, Tasks (hierarchical), Milestones, Time Tracking |
| **CRM** | Leads, Contacts, Opportunities (full CRUD), Pipeline (Kanban view) |
| **Reports** | Financial, Sales, Inventory, and Project reports with live chart data |
| **Profile** | Personal info, password change, avatar upload, active session view |
| **App Settings** | Company branding with live preview, currencies, auto-numbering, user management |
| **System Settings** | Identity policy, database, email, storage, and logging configuration |
| **System Logs** | Database logs and file logs with search, filter, and pagination |

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Blazor WebAssembly (.NET 10) |
| Backend | ASP.NET Core 10 Minimal APIs |
| Database | SQL Server (LocalDB by default) |
| ORM | Entity Framework Core 10 |
| Auth | ASP.NET Core Identity + JWT Bearer (24 h tokens) |
| CSS | Tailwind CSS 3 + PostCSS |
| Icons | Material Icons (Outlined) |
| Logging | Serilog (file + console) |
| API Docs | Scalar |

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server or SQL Server LocalDB
- Node.js (for Tailwind CSS compilation)

### 1. Clone & restore

```bash
git clone <repo-url>
cd ERP
dotnet restore
```

### 2. Build Tailwind CSS

```bash
cd ERP.Client
npm install
npm run build:css
cd ..
```

### 3. Configure the database

The default connection string in `ERP.Server/appsettings.json` targets SQL Server LocalDB — no setup required if you have the .NET SDK:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ERP;Trusted_Connection=True;"
}
```

To use a full SQL Server instance, replace the value with your own connection string.

### 4. Apply migrations

```bash
dotnet ef database update --project ERP.Server
```

Seed data runs automatically on first startup and creates:

- Default roles: Super Admin, Admin, Finance Manager, Sales Manager, Inventory Manager, Employee
- Demo admin: `admin@root.com` / `admin`
- Sample data across all modules

### 5. Run

```bash
dotnet run --project ERP.Server
```

The server hosts both the API and the Blazor client. Open **https://localhost:5033**.

---

## Default Credentials

| Email | Password | Role |
|-------|----------|------|
| `admin@root.com` | `admin` | Super Admin |
| `finance@root.com` | `fin1234` | Finance Manager |
| `sales@root.com` | `sales123` | Sales Manager |
| `inventory@root.com` | `inv1234` | Inventory Manager |
| `emp@root.com` | `emp123` | Employee |

> **Change all passwords before any public deployment.**

---

## API Reference

All endpoints require a JWT Bearer token obtained from `POST /api/auth/login`. Interactive docs are at `/scalar` in development.

```http
Authorization: Bearer <token>
```

| Prefix | Description |
|--------|-------------|
| `/api/auth` | Login, register |
| `/api/dashboard` | Dashboard KPIs |
| `/api/customers` | Customer CRUD |
| `/api/quotes` | Quotation CRUD |
| `/api/sales-orders` | Sales order CRUD |
| `/api/sales-invoices` | Sales invoice CRUD |
| `/api/products` | Product CRUD |
| `/api/product-categories` | Category CRUD |
| `/api/warehouses` | Warehouse CRUD |
| `/api/stock-levels` | Stock level queries |
| `/api/stock-movements` | Stock movement history |
| `/api/vendors` | Vendor CRUD |
| `/api/purchase-orders` | Purchase order CRUD |
| `/api/goods-receipts` | Goods receipt CRUD |
| `/api/accounts` | Chart of accounts CRUD |
| `/api/journal-entries` | Journal entry CRUD |
| `/api/ap-invoices` | Accounts payable CRUD |
| `/api/ar-invoices` | Accounts receivable CRUD |
| `/api/bank-accounts` | Bank account management |
| `/api/projects` | Project CRUD |
| `/api/project-tasks` | Task CRUD |
| `/api/time-entries` | Time entry CRUD |
| `/api/leads` | CRM lead CRUD |
| `/api/contacts` | CRM contact CRUD |
| `/api/opportunities` | CRM opportunity CRUD |
| `/api/reports/*` | Financial, sales, inventory, project reports |
| `/api/profile` | User profile management |
| `/api/admin/users` | User administration |
| `/api/settings/*` | System & app settings, branding |
| `/api/system-logs/*` | Database and file log viewers |

All responses follow the standard envelope:

```json
{
  "isSuccess": true,
  "message": null,
  "data": { ... }
}
```

Paginated endpoints accept `page`, `pageSize`, `search`, `sortBy`, and `sortDescending` query parameters and return:

```json
{
  "items": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

---

## Configuration

Key settings in `ERP.Server/appsettings.json`:

```json
{
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "ERP.Server",
    "Audience": "ERP.Client",
    "ExpireMinutes": 1440
  },
  "DefaultAdmin": {
    "Email": "admin@root.com",
    "Password": "admin"
  },
  "Storage": {
    "AvatarPath": "wwwroot/avatars",
    "MaxFileSize": 5242880,
    "AllowedExtensions": ".jpg,.jpeg,.png,.gif"
  },
  "Email": {
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "FromAddress": "noreply@erp.com",
    "EnableSsl": true
  }
}
```

---

## Shared Components

Reusable Razor components in `ERP.Client/Shared/`:

| Component | Key Parameters |
|-----------|---------------|
| `Modal` | `IsVisible`, `Title`, `OnClose`, `ChildContent`, `Footer`, `WidthClass` |
| `Pagination` | `Page`, `PageSize`, `TotalCount`, `PageChanged`, `PageSizeChanged` |
| `PageHeader` | `Title`, `Subtitle`, `Breadcrumb`, `Actions` (RenderFragment slots) |
| `TabContainer` | `Tabs` (Dictionary), `ActiveTab`, `OnTabChanged` |
| `SettingRow` | `Label`, `Description`, `ChildContent` |
| `ThemePanel` | Dark/light mode toggle panel |
