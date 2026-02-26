-- ================================================================
-- Inventory Management System — Simple Schema (matches diagram)
-- ================================================================

DROP DATABASE IF EXISTS InventoryManagementDB;
CREATE DATABASE InventoryManagementDB;
USE InventoryManagementDB;

-- ----------------------------------------------------------------
-- 1. ProductCategory
-- ----------------------------------------------------------------
CREATE TABLE ProductCategory (
    CategoryId       VARCHAR(10)  NOT NULL,
    CategoryName     VARCHAR(100) NOT NULL,
    Description      VARCHAR(255) NULL,
    ParentCategoryId VARCHAR(10)  NULL,
    PRIMARY KEY (CategoryId),
    FOREIGN KEY (ParentCategoryId) REFERENCES ProductCategory(CategoryId)
);

-- ----------------------------------------------------------------
-- 2. Supplier
-- ----------------------------------------------------------------
CREATE TABLE Supplier (
    SupplierId   VARCHAR(10)  NOT NULL,
    SupplierName VARCHAR(100) NOT NULL,
    Email        VARCHAR(100) NULL,
    Phone        VARCHAR(20)  NULL,
    Website      VARCHAR(150) NULL,
    PRIMARY KEY (SupplierId)
);

-- ----------------------------------------------------------------
-- 3. Product
-- ----------------------------------------------------------------
CREATE TABLE Product (
    ProductId     VARCHAR(10)    NOT NULL,
    SKU           VARCHAR(50)    NOT NULL,
    ProductName   VARCHAR(150)   NOT NULL,
    Description   VARCHAR(500)   NULL,
    CategoryId    VARCHAR(10)    NULL,
    UnitOfMeasure VARCHAR(20)    NOT NULL,
    Cost          DECIMAL(10,2)  NOT NULL DEFAULT 0.00,
    ListPrice     DECIMAL(10,2)  NOT NULL DEFAULT 0.00,
    IsActive      BOOLEAN        NOT NULL DEFAULT TRUE,
    PRIMARY KEY (ProductId),
    UNIQUE (SKU),
    FOREIGN KEY (CategoryId) REFERENCES ProductCategory(CategoryId)
);

-- ----------------------------------------------------------------
-- 4. ProductSupplier  (links Product <-> Supplier)
-- ----------------------------------------------------------------
CREATE TABLE ProductSupplier (
    SupplierProductId VARCHAR(10) NOT NULL,
    SupplierId        VARCHAR(10) NOT NULL,
    ProductId         VARCHAR(10) NOT NULL,
    SupplierSKU       VARCHAR(50) NULL,
    LeadTime          INT         NOT NULL DEFAULT 0,
    PRIMARY KEY (SupplierProductId),
    FOREIGN KEY (SupplierId) REFERENCES Supplier(SupplierId),
    FOREIGN KEY (ProductId)  REFERENCES Product(ProductId)
);

-- ----------------------------------------------------------------
-- 5. PurchaseOrder  (links to Supplier)
-- ----------------------------------------------------------------
CREATE TABLE PurchaseOrder (
    PurchaseOrderId VARCHAR(10) NOT NULL,
    SupplierId      VARCHAR(10) NOT NULL,
    OrderDate       DATE        NOT NULL,
    Status          VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    PRIMARY KEY (PurchaseOrderId),
    FOREIGN KEY (SupplierId) REFERENCES Supplier(SupplierId)
);

-- ----------------------------------------------------------------
-- 6. PurchaseOrderItem  (lines of a PurchaseOrder)
-- ----------------------------------------------------------------
CREATE TABLE PurchaseOrderItem (
    POItemId        VARCHAR(10)   NOT NULL,
    PurchaseOrderId VARCHAR(10)   NOT NULL,
    ProductId       VARCHAR(10)   NOT NULL,
    QuantityOrdered DECIMAL(10,2) NOT NULL,
    UnitPrice       DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    PRIMARY KEY (POItemId),
    FOREIGN KEY (PurchaseOrderId) REFERENCES PurchaseOrder(PurchaseOrderId),
    FOREIGN KEY (ProductId)       REFERENCES Product(ProductId)
);

-- ----------------------------------------------------------------
-- 7. Warehouse
-- ----------------------------------------------------------------
CREATE TABLE Warehouse (
    WarehouseId   VARCHAR(10)   NOT NULL,
    WarehouseName VARCHAR(100)  NOT NULL,
    Location      VARCHAR(200)  NULL,
    Capacity      DECIMAL(10,2) NULL,
    PRIMARY KEY (WarehouseId)
);

-- ----------------------------------------------------------------
-- 8. StockLevel  (stock per Product per Warehouse)
-- ----------------------------------------------------------------
CREATE TABLE StockLevel (
    StockLevelId     VARCHAR(10)   NOT NULL,
    ProductId        VARCHAR(10)   NOT NULL,
    WarehouseId      VARCHAR(10)   NOT NULL,
    QuantityOnHand   DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    ReorderLevel     DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    SafetyStock      DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    ReservedQuantity DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    PRIMARY KEY (StockLevelId),
    FOREIGN KEY (ProductId)   REFERENCES Product(ProductId),
    FOREIGN KEY (WarehouseId) REFERENCES Warehouse(WarehouseId)
);

-- ----------------------------------------------------------------
-- 9. StockTransaction  (every stock movement — audit trail)
-- ----------------------------------------------------------------
CREATE TABLE StockTransaction (
    TransactionId   VARCHAR(10)   NOT NULL,
    ProductId       VARCHAR(10)   NOT NULL,
    WarehouseId     VARCHAR(10)   NOT NULL,
    TransactionType VARCHAR(20)   NOT NULL,
    Quantity        DECIMAL(10,2) NOT NULL,
    TransactionDate TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Reference       VARCHAR(100)  NULL,
    PRIMARY KEY (TransactionId),
    FOREIGN KEY (ProductId)   REFERENCES Product(ProductId),
    FOREIGN KEY (WarehouseId) REFERENCES Warehouse(WarehouseId)
);

-- ----------------------------------------------------------------
-- 10. Batch  (batch/lot tracking with expiry)
-- ----------------------------------------------------------------
CREATE TABLE Batch (
    BatchId           VARCHAR(10)   NOT NULL,
    ProductId         VARCHAR(10)   NOT NULL,
    WarehouseId       VARCHAR(10)   NOT NULL,
    BatchNumber       VARCHAR(50)   NOT NULL,
    ManufacturingDate DATE          NULL,
    ExpiryDate        DATE          NULL,
    Quantity          DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    PRIMARY KEY (BatchId),
    FOREIGN KEY (ProductId)   REFERENCES Product(ProductId),
    FOREIGN KEY (WarehouseId) REFERENCES Warehouse(WarehouseId)
);


-- ================================================================
-- SAMPLE DATA
-- ================================================================

-- ProductCategory
INSERT INTO ProductCategory VALUES
('C1', 'Electronics',     'Electronic items',      NULL),
('C2', 'Clothing',        'Apparel items',          NULL),
('C3', 'Food',            'Food products',          NULL),
('C4', 'Office Supplies', 'Office stationery',      NULL),
('C5', 'Mobile Phones',   'Smartphones',            'C1'),
('C6', 'Laptops',         'Laptop computers',       'C1'),
('C7', 'Men Wear',        'Clothing for men',       'C2'),
('C8', 'Snacks',          'Packaged snack foods',   'C3');

-- Supplier
INSERT INTO Supplier VALUES
('S1', 'TechSource Ltd',   'sales@techsource.com',  '9876543210', 'techsource.com'),
('S2', 'FashionHub',       'info@fashionhub.com',   '9812345678', 'fashionhub.com'),
('S3', 'GreenFields Foods','supply@greenfields.com','9900001111', 'greenfields.com'),
('S4', 'OfficeWorld',      'info@officeworld.com',  '9811223344', 'officeworld.com');

-- Product
INSERT INTO Product VALUES
('P1',  'SKU-001', 'Samsung Galaxy A54', 'Smartphone 128GB',    'C5', 'PCS',  18000.00, 24999.00, TRUE),
('P2',  'SKU-002', 'iPhone 14',          'Apple iPhone 256GB',  'C5', 'PCS',  60000.00, 79900.00, TRUE),
('P3',  'SKU-003', 'HP Pavilion 15',     'Laptop i5 8GB 512GB', 'C6', 'PCS',  42000.00, 55999.00, TRUE),
('P4',  'SKU-004', 'Dell Inspiron 14',   'Laptop Ryzen 5 16GB', 'C6', 'PCS',  46000.00, 61999.00, TRUE),
('P5',  'SKU-005', 'Men Polo T-Shirt',   '100% cotton polo',    'C7', 'PCS',    350.00,   799.00, TRUE),
('P6',  'SKU-006', 'Potato Chips 100g',  'Salted chips pack',   'C8', 'PKT',     18.00,    40.00, TRUE),
('P7',  'SKU-007', 'A4 Paper Ream',      '80 GSM 500 sheets',   'C4', 'REAM',   180.00,   350.00, TRUE),
('P8',  'SKU-008', 'Ball Pen Blue',      'Pack of 10 pens',     'C4', 'PACK',    40.00,    90.00, TRUE);

-- ProductSupplier
INSERT INTO ProductSupplier VALUES
('PS1', 'S1', 'P1', 'TS-SGA54',  7),
('PS2', 'S1', 'P2', 'TS-IP14',  10),
('PS3', 'S1', 'P3', 'TS-HP15',   5),
('PS4', 'S1', 'P4', 'TS-DI14',   5),
('PS5', 'S2', 'P5', 'FH-POLO',   4),
('PS6', 'S3', 'P6', 'GF-CHIP',   2),
('PS7', 'S4', 'P7', 'OW-A4R',    3),
('PS8', 'S4', 'P8', 'OW-PEN',    3);

-- Warehouse
INSERT INTO Warehouse VALUES
('W1', 'Main Warehouse', 'Mumbai, Maharashtra',  50000.00),
('W2', 'North Hub',      'Delhi, NCR',           30000.00),
('W3', 'South Hub',      'Bangalore, Karnataka', 20000.00);

-- StockLevel
INSERT INTO StockLevel VALUES
('SL1',  'P1', 'W1', 150, 20, 10,  5),
('SL2',  'P1', 'W2',  40, 10,  5,  0),
('SL3',  'P2', 'W1',  60, 10,  5,  2),
('SL4',  'P3', 'W1',  80, 15,  5,  0),
('SL5',  'P4', 'W1',  55, 15,  5,  0),
('SL6',  'P5', 'W2', 200, 30, 15, 10),
('SL7',  'P6', 'W3', 800,100, 50,  0),
('SL8',  'P7', 'W1', 300, 50, 25,  0),
-- Low stock rows (triggers alert)
('SL9',  'P2', 'W2',   5, 10,  5,  0),
('SL10', 'P4', 'W3',   4, 10,  5,  0);

-- StockTransaction
INSERT INTO StockTransaction VALUES
('T1',  'P1', 'W1', 'PURCHASE',     200, '2026-01-05 09:00:00', 'PO1'),
('T2',  'P2', 'W1', 'PURCHASE',      70, '2026-01-05 09:30:00', 'PO1'),
('T3',  'P3', 'W1', 'PURCHASE',     100, '2026-01-06 10:00:00', 'PO2'),
('T4',  'P1', 'W1', 'SALE',          30, '2026-01-15 14:00:00', 'INV-001'),
('T5',  'P2', 'W1', 'SALE',           5, '2026-01-15 14:30:00', 'INV-001'),
('T6',  'P5', 'W2', 'SALE',          20, '2026-01-22 10:00:00', 'INV-002'),
('T7',  'P1', 'W1', 'TRANSFER_OUT',  20, '2026-01-25 09:00:00', 'TRF-001'),
('T8',  'P1', 'W2', 'TRANSFER_IN',   20, '2026-01-25 09:15:00', 'TRF-001'),
('T9',  'P6', 'W3', 'ADJUSTMENT',   -50, '2026-02-01 08:00:00', 'ADJ-001 Damaged'),
('T10', 'P3', 'W1', 'RETURN',         5, '2026-02-10 12:00:00', 'RET-001'),
('T11', 'P1', 'W1', 'HOLD',           5, '2026-02-20 15:00:00', 'QUO-001');

-- PurchaseOrder
INSERT INTO PurchaseOrder VALUES
('PO1', 'S1', '2026-01-03', 'RECEIVED'),
('PO2', 'S1', '2026-01-04', 'RECEIVED'),
('PO3', 'S3', '2026-02-10', 'PENDING'),
('PO4', 'S2', '2026-02-18', 'APPROVED');

-- PurchaseOrderItem
INSERT INTO PurchaseOrderItem VALUES
('I1', 'PO1', 'P1', 200, 18000.00),
('I2', 'PO1', 'P2',  70, 60000.00),
('I3', 'PO2', 'P3', 100, 42000.00),
('I4', 'PO2', 'P4',  60, 46000.00),
('I5', 'PO3', 'P6',2000,    18.00),
('I6', 'PO4', 'P5', 300,   350.00);

-- Batch
INSERT INTO Batch VALUES
('B1', 'P1', 'W1', 'SMSG-2025-A',  '2025-09-01', NULL,         150),
('B2', 'P2', 'W1', 'APPLE-2025-B', '2025-10-01', NULL,          60),
('B3', 'P6', 'W3', 'GF-CHIP-JAN',  '2026-01-10', '2026-04-10',1000),
('B4', 'P6', 'W3', 'GF-CHIP-FEB',  '2026-02-05', '2026-03-15', 800),
('B5', 'P7', 'W1', 'OW-A4-2026',   '2026-01-15', NULL,         300);


-- ================================================================
-- VERIFY ROW COUNTS
-- ================================================================
SELECT 'ProductCategory'  AS TableName, COUNT(*) AS RowCount FROM ProductCategory
UNION ALL SELECT 'Supplier',            COUNT(*) FROM Supplier
UNION ALL SELECT 'Product',             COUNT(*) FROM Product
UNION ALL SELECT 'ProductSupplier',     COUNT(*) FROM ProductSupplier
UNION ALL SELECT 'Warehouse',           COUNT(*) FROM Warehouse
UNION ALL SELECT 'StockLevel',          COUNT(*) FROM StockLevel
UNION ALL SELECT 'StockTransaction',    COUNT(*) FROM StockTransaction
UNION ALL SELECT 'PurchaseOrder',       COUNT(*) FROM PurchaseOrder
UNION ALL SELECT 'PurchaseOrderItem',   COUNT(*) FROM PurchaseOrderItem
UNION ALL SELECT 'Batch',               COUNT(*) FROM Batch;
