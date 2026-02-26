-- ================================================================
-- VIEW ALL DATA — Inventory Management System
-- Open a NEW query tab, paste this, run each section with Ctrl+Enter
-- ================================================================

USE InventoryManagementDB;

-- ----------------------------------------------------------------
-- 1. Product Categories (with parent name)
-- ----------------------------------------------------------------
SELECT 
    c.CategoryId,
    c.CategoryName,
    c.Description,
    IFNULL(p.CategoryName, '-- Root --') AS ParentCategory
FROM ProductCategory c
LEFT JOIN ProductCategory p ON p.CategoryId = c.ParentCategoryId
ORDER BY c.ParentCategoryId, c.CategoryId;

-- ----------------------------------------------------------------
-- 2. Suppliers
-- ----------------------------------------------------------------
SELECT 
    SupplierId,
    SupplierName,
    Email,
    Phone,
    Website
FROM Supplier;

-- ----------------------------------------------------------------
-- 3. Products (with category name)
-- ----------------------------------------------------------------
SELECT 
    p.ProductId,
    p.SKU,
    p.ProductName,
    p.Description,
    c.CategoryName,
    p.UnitOfMeasure,
    p.Cost,
    p.ListPrice,
    IF(p.IsActive, 'Active', 'Inactive') AS Status
FROM Product p
LEFT JOIN ProductCategory c ON c.CategoryId = p.CategoryId
ORDER BY p.ProductId;

-- ----------------------------------------------------------------
-- 4. Product-Supplier Mapping
-- ----------------------------------------------------------------
SELECT 
    ps.SupplierProductId,
    p.ProductName,
    s.SupplierName,
    ps.SupplierSKU,
    ps.LeadTime AS LeadTimeDays
FROM ProductSupplier ps
JOIN Product  p ON p.ProductId  = ps.ProductId
JOIN Supplier s ON s.SupplierId = ps.SupplierId
ORDER BY ps.SupplierProductId;

-- ----------------------------------------------------------------
-- 5. Warehouses
-- ----------------------------------------------------------------
SELECT 
    WarehouseId,
    WarehouseName,
    Location,
    Capacity
FROM Warehouse;

-- ----------------------------------------------------------------
-- 6. Stock Levels (with product & warehouse names)
-- ----------------------------------------------------------------
SELECT 
    sl.StockLevelId,
    p.ProductName,
    w.WarehouseName,
    sl.QuantityOnHand,
    sl.ReorderLevel,
    sl.SafetyStock,
    sl.ReservedQuantity,
    (sl.QuantityOnHand - sl.ReservedQuantity) AS AvailableQty,
    IF(sl.QuantityOnHand <= sl.ReorderLevel, '⚠ LOW STOCK', 'OK') AS StockStatus
FROM StockLevel sl
JOIN Product   p ON p.ProductId   = sl.ProductId
JOIN Warehouse w ON w.WarehouseId = sl.WarehouseId
ORDER BY StockStatus DESC, p.ProductName;

-- ----------------------------------------------------------------
-- 7. Stock Transactions (with product & warehouse names)
-- ----------------------------------------------------------------
SELECT 
    st.TransactionId,
    st.TransactionDate,
    st.TransactionType,
    p.ProductName,
    w.WarehouseName,
    st.Quantity,
    st.Reference
FROM StockTransaction st
JOIN Product   p ON p.ProductId   = st.ProductId
JOIN Warehouse w ON w.WarehouseId = st.WarehouseId
ORDER BY st.TransactionDate DESC;

-- ----------------------------------------------------------------
-- 8. Purchase Orders (with supplier name)
-- ----------------------------------------------------------------
SELECT 
    po.PurchaseOrderId,
    s.SupplierName,
    po.OrderDate,
    po.Status
FROM PurchaseOrder po
JOIN Supplier s ON s.SupplierId = po.SupplierId
ORDER BY po.OrderDate DESC;

-- ----------------------------------------------------------------
-- 9. Purchase Order Items (with PO, product, supplier details)
-- ----------------------------------------------------------------
SELECT 
    poi.POItemId,
    poi.PurchaseOrderId,
    s.SupplierName,
    p.ProductName,
    poi.QuantityOrdered,
    poi.UnitPrice,
    ROUND(poi.QuantityOrdered * poi.UnitPrice, 2) AS LineTotal
FROM PurchaseOrderItem poi
JOIN PurchaseOrder po ON po.PurchaseOrderId = poi.PurchaseOrderId
JOIN Supplier      s  ON s.SupplierId       = po.SupplierId
JOIN Product       p  ON p.ProductId        = poi.ProductId
ORDER BY poi.PurchaseOrderId, poi.POItemId;

-- ----------------------------------------------------------------
-- 10. Batches (with product & warehouse names)
-- ----------------------------------------------------------------
SELECT 
    b.BatchId,
    b.BatchNumber,
    p.ProductName,
    w.WarehouseName,
    b.ManufacturingDate,
    b.ExpiryDate,
    b.Quantity,
    CASE
        WHEN b.ExpiryDate IS NULL THEN 'No Expiry'
        WHEN b.ExpiryDate < CURDATE() THEN '❌ Expired'
        WHEN DATEDIFF(b.ExpiryDate, CURDATE()) <= 30 THEN '⚠ Expiring Soon'
        ELSE 'OK'
    END AS ExpiryStatus
FROM Batch b
JOIN Product   p ON p.ProductId   = b.ProductId
JOIN Warehouse w ON w.WarehouseId = b.WarehouseId
ORDER BY b.ExpiryDate;

-- ----------------------------------------------------------------
-- BONUS: Full Inventory Summary
-- ----------------------------------------------------------------
SELECT 
    p.SKU,
    p.ProductName,
    w.WarehouseName,
    sl.QuantityOnHand,
    sl.ReservedQuantity,
    (sl.QuantityOnHand - sl.ReservedQuantity)   AS AvailableQty,
    ROUND(sl.QuantityOnHand * p.Cost, 2)         AS StockValue
FROM StockLevel sl
JOIN Product   p ON p.ProductId   = sl.ProductId
JOIN Warehouse w ON w.WarehouseId = sl.WarehouseId
ORDER BY StockValue DESC;
