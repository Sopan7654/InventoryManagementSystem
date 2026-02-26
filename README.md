# 📦 Inventory Management System

A **Console-based Stock/Inventory Management System** built with:

- **C# (.NET 8)** — Programming language
- **ADO.NET** — Database connectivity (raw SQL queries)
- **MySQL** — Database server
- **xUnit** — Unit testing framework

> Designed for managing products, stock levels, warehouses, purchase orders, suppliers, and batches — all through a structured console menu.

---

## 📋 Table of Contents

1. [Project Structure](#project-structure)
2. [Prerequisites — What to Install](#prerequisites)
3. [Step-by-Step Setup Commands](#setup-commands)
4. [Database Setup](#database-setup)
5. [Configure Database Password](#configure-database-password)
6. [Build & Run](#build-and-run)
7. [Run Unit Tests](#run-unit-tests)
8. [Application Menu Guide](#application-menu-guide)
9. [Project Layers Explained](#project-layers-explained)
10. [Models (Data Blueprints)](#models)
11. [DataAccess (Repositories)](#dataaccess-repositories)
12. [Services (Business Logic)](#services)
13. [ConsoleUI (Menus)](#consoleui-menus)
14. [Database Schema](#database-schema)
15. [All Commands Quick Reference](#all-commands-quick-reference)

---

## 📁 Project Structure

```
E:\InventoryManagementSystem\
│
├── README.md                                    ← This file
├── InventoryManagementSystem.slnx               ← Solution file (groups projects)
│
├── 📁 database/
│   └── schema.sql                               ← MySQL tables + seed data script
│
├── 📁 src/
│   └── 📁 InventoryManagementSystem/            ← Main C# application
│       ├── InventoryManagementSystem.csproj     ← Project config (packages, .NET version)
│       ├── Program.cs                           ← Entry point (main menu start)
│       ├── appsettings.json                     ← DB connection string (password here)
│       │
│       ├── 📁 Models/                           ← Data blueprints (8 classes)
│       │   ├── Product.cs
│       │   ├── ProductCategory.cs
│       │   ├── Supplier.cs
│       │   ├── Warehouse.cs
│       │   ├── StockLevel.cs
│       │   ├── StockTransaction.cs
│       │   ├── PurchaseOrder.cs
│       │   └── Batch.cs
│       │
│       ├── 📁 DataAccess/                       ← Database queries (8 repositories)
│       │   ├── DatabaseHelper.cs                ← Opens MySQL connection
│       │   ├── ProductRepository.cs
│       │   ├── CategoryRepository.cs
│       │   ├── SupplierRepository.cs
│       │   ├── WarehouseRepository.cs
│       │   ├── StockLevelRepository.cs
│       │   ├── StockTransactionRepository.cs
│       │   ├── PurchaseOrderRepository.cs
│       │   └── BatchRepository.cs
│       │
│       ├── 📁 Services/                         ← Business rules (2 services)
│       │   ├── InventoryService.cs              ← Stock In/Out/Transfer/Hold/Adjust
│       │   └── ReportService.cs                 ← Reports, alerts, analytics
│       │
│       └── 📁 ConsoleUI/                        ← Screen menus (6 menu files)
│           ├── ConsoleHelper.cs                 ← Table printer, colored output
│           ├── ProductMenu.cs
│           ├── InventoryMenu.cs
│           ├── PurchaseOrderMenu.cs
│           ├── SupplierMenu.cs
│           ├── WarehouseMenu.cs
│           └── ReportMenu.cs
│
└── 📁 tests/
    └── 📁 InventoryManagementSystem.Tests/      ← Unit tests
        ├── InventoryManagementSystem.Tests.csproj
        └── InventoryServiceTests.cs             ← 19 test cases
```

---

## ✅ Prerequisites

Install these on your PC **before** starting:

| Software                     | Purpose                            | Download                                               |
| ---------------------------- | ---------------------------------- | ------------------------------------------------------ |
| **.NET 8 SDK**               | Runs C# programs                   | https://dotnet.microsoft.com/en-us/download/dotnet/8.0 |
| **MySQL Server + Workbench** | Database server + visual tool      | https://dev.mysql.com/downloads/installer/             |
| **Git**                      | Version control                    | https://git-scm.com/download/win                       |
| **Visual Studio / VS Code**  | Code editor (optional but helpful) | https://code.visualstudio.com/                         |

### Verify Installations (open PowerShell and run these):

```powershell
dotnet --version      # Should output: 8.0.xxx
mysql --version       # Should output: mysql Ver 8.x
git --version         # Should output: git version 2.x.x
```

---

## 🔨 Setup Commands

> These are the commands used to **create** the project from scratch.
> If you are cloning an existing repo, skip to [Database Setup](#database-setup).

### Step 1 — Go to project folder

```powershell
cd E:\InventoryManagementSystem
```

### Step 2 — Create the solution file

```powershell
dotnet new sln -n InventoryManagementSystem
```

> Creates `InventoryManagementSystem.slnx` — the container that holds all projects together.

### Step 3 — Create the main console application

```powershell
dotnet new console -n InventoryManagementSystem -o src/InventoryManagementSystem
```

> - `dotnet new console` → creates a Console Application
> - `-n InventoryManagementSystem` → names the project
> - `-o src/InventoryManagementSystem` → puts files into the `src/` folder (creates it automatically)

### Step 4 — Create the test project

```powershell
dotnet new xunit -n InventoryManagementSystem.Tests -o tests/InventoryManagementSystem.Tests
```

> Creates the unit test project using the xUnit framework inside `tests/` folder.

### Step 5 — Register both projects in the solution

```powershell
dotnet sln add src/InventoryManagementSystem/InventoryManagementSystem.csproj
dotnet sln add tests/InventoryManagementSystem.Tests/InventoryManagementSystem.Tests.csproj
```

> Without this, the solution doesn't know these projects exist.

### Step 6 — Link the test project to the main project

```powershell
dotnet add tests/InventoryManagementSystem.Tests/InventoryManagementSystem.Tests.csproj ^
    reference src/InventoryManagementSystem/InventoryManagementSystem.csproj
```

> Lets the test project use code from the main project (Models, Services, etc).

### Step 7 — Install the MySQL NuGet package

```powershell
dotnet add src/InventoryManagementSystem/InventoryManagementSystem.csproj package MySql.Data
```

> Downloads the `MySql.Data` library — this is what allows C# to communicate with MySQL.

### Step 8 — Create sub-folders

```powershell
mkdir src\InventoryManagementSystem\Models
mkdir src\InventoryManagementSystem\DataAccess
mkdir src\InventoryManagementSystem\Services
mkdir src\InventoryManagementSystem\ConsoleUI
mkdir database
```

> Creates the folder structure for organising code by layer.

---

## 🗄️ Database Setup

### Step 1 — Open MySQL Workbench

1. Launch **MySQL Workbench**
2. Click your local connection (e.g. `Local instance 3306`)
3. Enter your **root password** → Click **OK**

### Step 2 — Run the Schema Script

1. Click **File → Open SQL Script**
2. Select: `E:\InventoryManagementSystem\database\schema.sql`
3. Press **Ctrl + Shift + Enter** to run the whole script

> This creates the database `InventoryManagementDB` and all 10 tables with sample data.

### Step 3 — Verify (run in Workbench)

```sql
USE InventoryManagementDB;
SHOW TABLES;

-- Check row counts
SELECT 'ProductCategory' AS TableName, COUNT(*) AS RowCount FROM ProductCategory
UNION ALL SELECT 'Supplier',          COUNT(*) FROM Supplier
UNION ALL SELECT 'Product',           COUNT(*) FROM Product
UNION ALL SELECT 'Warehouse',         COUNT(*) FROM Warehouse
UNION ALL SELECT 'StockLevel',        COUNT(*) FROM StockLevel
UNION ALL SELECT 'StockTransaction',  COUNT(*) FROM StockTransaction
UNION ALL SELECT 'PurchaseOrder',     COUNT(*) FROM PurchaseOrder
UNION ALL SELECT 'PurchaseOrderItem', COUNT(*) FROM PurchaseOrderItem
UNION ALL SELECT 'Batch',             COUNT(*) FROM Batch;
```

**Expected result:**

| TableName         | RowCount |
| ----------------- | -------- |
| ProductCategory   | 14       |
| Supplier          | 5        |
| Product           | 12       |
| Warehouse         | 3        |
| StockLevel        | 16       |
| StockTransaction  | 15       |
| PurchaseOrder     | 6        |
| PurchaseOrderItem | 12       |
| Batch             | 8        |

---

## 🔑 Configure Database Password

Open: `src\InventoryManagementSystem\appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=InventoryManagementDB;Uid=root;Pwd=YOUR_PASSWORD_HERE;"
  }
}
```

Replace `YOUR_PASSWORD_HERE` with your MySQL root password and save.

---

## ▶️ Build and Run

### Restore packages (first time only)

```powershell
cd E:\InventoryManagementSystem
dotnet restore
```

> Downloads all NuGet packages listed in `.csproj` files.

### Build (compile the code)

```powershell
dotnet build InventoryManagementSystem.slnx
```

> Compiles C# code into executable files. Look for `Build succeeded`.

### Run the application

```powershell
dotnet run --project src\InventoryManagementSystem\InventoryManagementSystem.csproj
```

> Builds AND runs the application in one command. Shows the main menu.

---

## 🧪 Run Unit Tests

```powershell
dotnet test tests\InventoryManagementSystem.Tests\InventoryManagementSystem.Tests.csproj
```

**Expected output:**

```
Test summary: total: 19, failed: 0, succeeded: 19, skipped: 0
```

### What is tested:

| Test Class               | Count | What It Checks                                         |
| ------------------------ | ----- | ------------------------------------------------------ |
| `StockLevelModelTests`   | 5     | AvailableQuantity, IsLowStock computed properties      |
| `BatchModelTests`        | 5     | ExpiryStatus: No Expiry / Expired / Expiring Soon / OK |
| `ProductModelTests`      | 2     | Default IsActive=true, Default UOM="PCS"               |
| `PurchaseOrderItemTests` | 2     | LineTotal = Qty × UnitPrice                            |
| `TransactionTypeTests`   | 5     | String constants: PURCHASE, SALE, TRANSFER_IN, HOLD    |

---

## 🖥️ Application Menu Guide

```
MAIN MENU
├── 1. Product Management
│     ├── View all products (table view)
│     ├── Search product by SKU
│     ├── Add new product
│     ├── Edit product details
│     └── Toggle Active / Inactive status
│
├── 2. Inventory Operations
│     ├── View all stock levels
│     ├── Stock In   → Receive goods (e.g. from supplier)
│     ├── Stock Out  → Issue / Sell goods
│     ├── Transfer   → Move stock between warehouses
│     ├── Hold       → Reserve stock for a quotation/order
│     ├── Adjustment → Fix stock (damage, recount, write-off)
│     └── History    → View all past transactions
│
├── 3. Purchase Orders
│     ├── View all purchase orders
│     ├── Create new PO (automatically adds line items)
│     └── Receive PO  → Marks as RECEIVED and updates stock
│
├── 4. Supplier Management
│     ├── View all suppliers
│     ├── Add new supplier
│     └── Edit supplier details
│
├── 5. Warehouse Management
│     ├── View all warehouses
│     ├── Add new warehouse
│     ├── View all batches / lots
│     └── Add new batch (with expiry date)
│
└── 6. Reports & Alerts
      ├── Low Stock Alert        → Products at/below reorder level
      ├── Expiring Batches       → Batches expiring within 30 days
      ├── Inventory Valuation    → Total value = Qty × Cost per product
      ├── ABC Analysis           → A/B/C classification by stock value
      ├── Transaction History    → Last 20 stock movements
      └── Warehouse Summary      → Products and value per warehouse
```

---

## 🏗️ Project Layers Explained

This project follows a **Layered Architecture** pattern:

```
ConsoleUI  →  Services  →  DataAccess  →  MySQL Database
   ↑               ↑             ↑
(User sees)   (Business     (SQL queries)
              rules here)
```

| Layer          | Folder        | Role                                      |
| -------------- | ------------- | ----------------------------------------- |
| **Models**     | `Models/`     | Define the shape of data (like a form)    |
| **DataAccess** | `DataAccess/` | Read and write to the database            |
| **Services**   | `Services/`   | Apply business rules (validations, logic) |
| **ConsoleUI**  | `ConsoleUI/`  | What the user sees and types              |

---

## 📌 Models

Models are C# classes that represent a row of data from the database.

### `Product.cs`

```csharp
public class Product
{
    public string ProductId     { get; set; }          // Primary key: "P1"
    public string SKU           { get; set; }          // Unique code: "ELEC-001"
    public string ProductName   { get; set; }          // "Laptop"
    public string? Description  { get; set; }          // Optional text
    public string? CategoryId   { get; set; }          // Links to ProductCategory
    public string UnitOfMeasure { get; set; } = "PCS"; // Default: Pieces
    public decimal Cost         { get; set; }          // Buying price
    public decimal ListPrice    { get; set; }          // Selling price
    public bool IsActive        { get; set; } = true;  // Active by default
}
```

### `StockLevel.cs` (with computed properties)

```csharp
public class StockLevel
{
    public string  ProductId        { get; set; }
    public string  WarehouseId      { get; set; }
    public decimal QuantityOnHand   { get; set; }   // Physical units in warehouse
    public decimal ReservedQuantity { get; set; }   // Units held for pending orders
    public decimal ReorderLevel     { get; set; }   // Alert threshold

    // Calculated automatically — no database column needed:
    public decimal AvailableQuantity => QuantityOnHand - ReservedQuantity;
    public bool    IsLowStock        => QuantityOnHand <= ReorderLevel;
}
```

### `Batch.cs`

```csharp
public class Batch
{
    public string    BatchNumber       { get; set; }
    public string    ProductId         { get; set; }
    public string    WarehouseId       { get; set; }
    public decimal   Quantity          { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate        { get; set; }

    // Calculates expiry status automatically:
    public string ExpiryStatus =>
        ExpiryDate == null                            ? "No Expiry"      :
        ExpiryDate < DateTime.Today                   ? "EXPIRED"        :
        (ExpiryDate.Value - DateTime.Today).Days <= 30 ? "EXPIRING SOON" :
                                                         "OK";
}
```

### `StockTransaction.cs` + `TransactionTypes`

```csharp
public class StockTransaction
{
    public string   TransactionType { get; set; }   // e.g. "PURCHASE"
    public decimal  Quantity        { get; set; }
    public DateTime TransactionDate { get; set; }
    public string?  Reference       { get; set; }   // e.g. PO number
}

// All valid transaction type values:
public static class TransactionTypes
{
    public const string Purchase    = "PURCHASE";
    public const string Sale        = "SALE";
    public const string Adjustment  = "ADJUSTMENT";
    public const string TransferIn  = "TRANSFER_IN";
    public const string TransferOut = "TRANSFER_OUT";
    public const string Hold        = "HOLD";
    public const string HoldRelease = "HOLD_RELEASE";
    public const string WriteOff    = "WRITE_OFF";
}
```

### `PurchaseOrder.cs` + `PurchaseOrderItem.cs`

```csharp
public class PurchaseOrder
{
    public string   PurchaseOrderId { get; set; }
    public string   SupplierId      { get; set; }
    public DateTime OrderDate       { get; set; }
    public string   Status          { get; set; } = "PENDING"; // PENDING → RECEIVED
}

public class PurchaseOrderItem
{
    public string  PurchaseOrderId  { get; set; }
    public string  ProductId        { get; set; }
    public decimal QuantityOrdered  { get; set; }
    public decimal UnitPrice        { get; set; }
    public decimal LineTotal        => QuantityOrdered * UnitPrice;  // Auto-calculated
}
```

---

## 🔌 DataAccess (Repositories)

### `DatabaseHelper.cs` — Opens MySQL Connections

```csharp
// Every repository calls this to get a connection:
public static MySqlConnection GetConnection()
{
    string connStr = config["ConnectionStrings:DefaultConnection"];
    return new MySqlConnection(connStr);  // Connection is NOT yet open
}

// Test if database is reachable:
public static bool TestConnection()
{
    using var conn = GetConnection();
    conn.Open();           // Throws if MySQL is unreachable
    return conn.State == ConnectionState.Open;
}
```

### How ADO.NET Works (The Pattern Used Everywhere)

```csharp
// 1. Get a connection object
using var conn = DatabaseHelper.GetConnection();
conn.Open();   // Actually connect to MySQL

// 2. Write SQL (use @param to avoid SQL injection)
string sql = "SELECT * FROM Product WHERE SKU = @sku";

// 3. Create command
using var cmd = new MySqlCommand(sql, conn);

// 4. Bind parameters safely
cmd.Parameters.AddWithValue("@sku", "ELEC-001");

// 5a. If reading data:
using var rdr = cmd.ExecuteReader();
while (rdr.Read())
{
    string name = rdr["ProductName"].ToString()!;
}

// 5b. If inserting/updating/deleting:
int rowsAffected = cmd.ExecuteNonQuery();
```

### Repository Methods Summary

| Repository                   | Key Methods                                                                                       |
| ---------------------------- | ------------------------------------------------------------------------------------------------- |
| `ProductRepository`          | `GetAll()`, `GetById()`, `GetBySKU()`, `Insert()`, `Update()`, `SKUExists()`                      |
| `CategoryRepository`         | `GetAll()`, `GetById()`, `Insert()`                                                               |
| `SupplierRepository`         | `GetAll()`, `GetById()`, `Insert()`, `Update()`                                                   |
| `WarehouseRepository`        | `GetAll()`, `GetById()`, `Insert()`                                                               |
| `StockLevelRepository`       | `GetAll()`, `GetByProductAndWarehouse()`, `GetLowStock()`, `UpdateQuantity()`, `UpdateReserved()` |
| `StockTransactionRepository` | `Insert()`, `GetAll(limit)`, `GetByProduct()`                                                     |
| `PurchaseOrderRepository`    | `GetAll()`, `InsertPO()`, `InsertItem()`, `GetItemsByPO()`, `UpdateStatus()`                      |
| `BatchRepository`            | `GetAll()`, `GetExpiringSoon(days)`, `Insert()`                                                   |

---

## ⚙️ Services

### `InventoryService.cs` — ACID-Compliant Stock Operations

Every operation uses a **MySQL Transaction** to ensure data stays consistent:

- If step 1 succeeds but step 2 fails → everything is rolled back (undone)
- Either ALL steps succeed (commit), or NONE of them apply (rollback)

```csharp
// Stock In — Receive goods
public (bool Success, string Message) StockIn(productId, warehouseId, qty, reference)
{
    if (qty <= 0) return (false, "Quantity must be greater than zero.");

    using var txn = conn.BeginTransaction();  // Start all-or-nothing block
    try
    {
        // 1. Add to StockLevel
        UPDATE StockLevel SET QuantityOnHand = QuantityOnHand + qty

        // 2. Write audit trail
        INSERT INTO StockTransaction (type='PURCHASE', qty=qty)

        txn.Commit();                         // Save both changes
        return (true, "+50 units added.");
    }
    catch (Exception ex)
    {
        txn.Rollback();                       // Undo both if anything failed
        return (false, ex.Message);
    }
}
```

| Method         | Business Rule Applied                               |
| -------------- | --------------------------------------------------- |
| `StockIn()`    | qty must be > 0                                     |
| `StockOut()`   | AvailableQty must be >= requested qty               |
| `Transfer()`   | Source ≠ Destination; Source must have enough stock |
| `HoldStock()`  | AvailableQty must be >= hold qty                    |
| `Adjustment()` | Can be positive (add) or negative (subtract)        |

### `ReportService.cs` — Analytics

```csharp
// ABC Analysis Logic:
// Sort products by total stock value (Qty × Cost), highest first
// Running total as % of grand total:
//   0–70%   → Class A  (most valuable, tight control)
//   70–90%  → Class B  (moderate control)
//   90–100% → Class C  (low value, minimal control)
```

| Report            | SQL Used                                           |
| ----------------- | -------------------------------------------------- |
| Low Stock         | `WHERE QuantityOnHand <= ReorderLevel`             |
| Expiring Batches  | `WHERE DATEDIFF(ExpiryDate, CURDATE()) <= 30`      |
| Inventory Value   | `QuantityOnHand * Cost AS TotalValue`              |
| Warehouse Summary | `GROUP BY WarehouseId, SUM(QuantityOnHand * Cost)` |

---

## 🖥️ ConsoleUI Menus

### `ConsoleHelper.cs` — Reusable Output Methods

```csharp
// Colored header
ConsoleHelper.PrintHeader("Product Management");
// ════════════════════════════════════════════
//   PRODUCT MANAGEMENT
// ════════════════════════════════════════════

// Success (green)
ConsoleHelper.PrintSuccess("Product added!");       // ✔ Product added!

// Error (red)
ConsoleHelper.PrintError("SKU already exists.");    // ✘ SKU already exists.

// Warning (yellow)
ConsoleHelper.PrintWarning("3 items low on stock"); // ⚠ 3 items low on stock

// Ask input (with validation)
string name = ConsoleHelper.AskRequired("Product Name");  // Won't accept blank
decimal qty = ConsoleHelper.AskDecimal("Quantity");        // Validates it's a number

// Print a table
ConsoleHelper.PrintTable(
    headers: new[] { "ID", "Product",  "Price" },
    widths:  new[] {  6,    20,         10     },
    rows:    myProductList.Select(p => new[] { p.ProductId, p.ProductName, p.ListPrice.ToString() }).ToList()
);
```

---

## 🗄️ Database Schema

### Entity Relationship (How Tables Link Together)

```
ProductCategory ──┐
                  ├──► Product ──────────────────────► StockLevel ◄─── Warehouse
Supplier ─────────┘       │                                │
    │                     │                                │
    │             ProductSupplier                   StockTransaction
    │
    └──► PurchaseOrder ──► PurchaseOrderItem ──► Product
                                                     │
                                               Batch (with expiry)
```

### Table Descriptions

| Table               | Primary Key       | Foreign Keys                | Purpose                       |
| ------------------- | ----------------- | --------------------------- | ----------------------------- |
| `ProductCategory`   | CategoryId        | ParentCategoryId (self-ref) | Hierarchical categories       |
| `Supplier`          | SupplierId        | —                           | Supplier master               |
| `Product`           | ProductId         | CategoryId                  | Product catalog               |
| `ProductSupplier`   | SupplierProductId | ProductId, SupplierId       | Many-to-many link             |
| `Warehouse`         | WarehouseId       | —                           | Storage locations             |
| `StockLevel`        | StockLevelId      | ProductId, WarehouseId      | Qty per product per warehouse |
| `StockTransaction`  | TransactionId     | ProductId, WarehouseId      | Full audit trail              |
| `PurchaseOrder`     | PurchaseOrderId   | SupplierId                  | Inbound orders                |
| `PurchaseOrderItem` | POItemId          | PurchaseOrderId, ProductId  | Order line items              |
| `Batch`             | BatchId           | ProductId, WarehouseId      | Lot tracking with expiry      |

---

## 📋 All Commands Quick Reference

### Installation / Setup

```powershell
# Create solution
dotnet new sln -n InventoryManagementSystem

# Create main project
dotnet new console -n InventoryManagementSystem -o src/InventoryManagementSystem

# Create test project
dotnet new xunit -n InventoryManagementSystem.Tests -o tests/InventoryManagementSystem.Tests

# Add projects to solution
dotnet sln add src/InventoryManagementSystem/InventoryManagementSystem.csproj
dotnet sln add tests/InventoryManagementSystem.Tests/InventoryManagementSystem.Tests.csproj

# Link test → main project
dotnet add tests/InventoryManagementSystem.Tests/InventoryManagementSystem.Tests.csproj reference src/InventoryManagementSystem/InventoryManagementSystem.csproj

# Install MySQL package
dotnet add src/InventoryManagementSystem/InventoryManagementSystem.csproj package MySql.Data

# Create folder structure
mkdir src\InventoryManagementSystem\Models
mkdir src\InventoryManagementSystem\DataAccess
mkdir src\InventoryManagementSystem\Services
mkdir src\InventoryManagementSystem\ConsoleUI
mkdir database
```

### Daily Usage

```powershell
# Navigate to project
cd E:\InventoryManagementSystem

# Restore packages (first time or after pulling new code)
dotnet restore

# Build (check for errors)
dotnet build InventoryManagementSystem.slnx

# Run the application
dotnet run --project src\InventoryManagementSystem\InventoryManagementSystem.csproj

# Run unit tests
dotnet test tests\InventoryManagementSystem.Tests\InventoryManagementSystem.Tests.csproj

# Run tests with detailed output
dotnet test tests\InventoryManagementSystem.Tests\InventoryManagementSystem.Tests.csproj -v normal
```

### Git Commands

```powershell
# Check status of your changes
git status

# Create a new feature branch
git checkout -b feature/my-feature-name

# Stage all changes
git add .

# Save changes with a message
git commit -m "Add product search by category"

# Push to remote
git push origin feature/my-feature-name

# Switch back to main branch
git checkout main

# See commit history
git log --oneline -10
```

---

## ❌ Common Errors & Fixes

| Error                                  | Cause                                  | Fix                                                     |
| -------------------------------------- | -------------------------------------- | ------------------------------------------------------- |
| `Cannot connect to MySQL`              | Wrong password or MySQL not running    | Check `appsettings.json` password; start MySQL service  |
| `dotnet is not recognized`             | .NET not installed                     | Reinstall .NET 8 SDK                                    |
| `MSB1009: Project file does not exist` | Wrong folder                           | Run `cd E:\InventoryManagementSystem` first             |
| `Error Code: 1064` in MySQL            | SQL syntax error                       | Check the query for typos                               |
| `Build failed`                         | Code has errors                        | Read error message — it shows file name and line number |
| `No project was found`                 | Running dotnet command in wrong folder | Go to project root folder first                         |

---

## 👨‍💻 Tech Stack Summary

| Technology | Version  | Role                                   |
| ---------- | -------- | -------------------------------------- |
| C#         | .NET 8   | Programming language                   |
| ADO.NET    | Built-in | Database communication (raw SQL)       |
| MySQL      | 8.x      | Database server                        |
| MySql.Data | 9.6.0    | NuGet package — MySQL connector for C# |
| xUnit      | 2.5.3    | Unit testing framework                 |
| Git        | Any      | Version control                        |
