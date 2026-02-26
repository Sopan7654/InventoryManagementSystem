// DataAccess/ProductRepository.cs
using MySql.Data.MySqlClient;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.DataAccess
{
    public class ProductRepository
    {
        public List<Product> GetAll()
        {
            var list = new List<Product>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT p.ProductId, p.SKU, p.ProductName, p.Description,
                                  p.CategoryId, c.CategoryName, p.UnitOfMeasure,
                                  p.Cost, p.ListPrice, p.IsActive
                           FROM Product p
                           LEFT JOIN ProductCategory c ON c.CategoryId = p.CategoryId
                           ORDER BY p.ProductName";
            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(Map(rdr));
            return list;
        }

        public Product? GetById(string productId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT p.ProductId, p.SKU, p.ProductName, p.Description,
                                  p.CategoryId, c.CategoryName, p.UnitOfMeasure,
                                  p.Cost, p.ListPrice, p.IsActive
                           FROM Product p
                           LEFT JOIN ProductCategory c ON c.CategoryId = p.CategoryId
                           WHERE p.ProductId = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", productId);
            using var rdr = cmd.ExecuteReader();
            return rdr.Read() ? Map(rdr) : null;
        }

        public Product? GetBySKU(string sku)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT p.ProductId, p.SKU, p.ProductName, p.Description,
                                  p.CategoryId, c.CategoryName, p.UnitOfMeasure,
                                  p.Cost, p.ListPrice, p.IsActive
                           FROM Product p
                           LEFT JOIN ProductCategory c ON c.CategoryId = p.CategoryId
                           WHERE p.SKU = @sku";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@sku", sku);
            using var rdr = cmd.ExecuteReader();
            return rdr.Read() ? Map(rdr) : null;
        }

        public bool Insert(Product p)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"INSERT INTO Product (ProductId,SKU,ProductName,Description,
                                               CategoryId,UnitOfMeasure,Cost,ListPrice,IsActive)
                           VALUES (@id,@sku,@name,@desc,@cat,@uom,@cost,@price,@active)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id",     p.ProductId);
            cmd.Parameters.AddWithValue("@sku",    p.SKU);
            cmd.Parameters.AddWithValue("@name",   p.ProductName);
            cmd.Parameters.AddWithValue("@desc",   p.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@cat",    p.CategoryId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@uom",    p.UnitOfMeasure);
            cmd.Parameters.AddWithValue("@cost",   p.Cost);
            cmd.Parameters.AddWithValue("@price",  p.ListPrice);
            cmd.Parameters.AddWithValue("@active", p.IsActive);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(Product p)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"UPDATE Product SET ProductName=@name, Description=@desc,
                                              CategoryId=@cat, UnitOfMeasure=@uom,
                                              Cost=@cost, ListPrice=@price, IsActive=@active
                           WHERE ProductId=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name",   p.ProductName);
            cmd.Parameters.AddWithValue("@desc",   p.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@cat",    p.CategoryId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@uom",    p.UnitOfMeasure);
            cmd.Parameters.AddWithValue("@cost",   p.Cost);
            cmd.Parameters.AddWithValue("@price",  p.ListPrice);
            cmd.Parameters.AddWithValue("@active", p.IsActive);
            cmd.Parameters.AddWithValue("@id",     p.ProductId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool SKUExists(string sku, string? excludeId = null)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = "SELECT COUNT(*) FROM Product WHERE SKU=@sku AND ProductId != @excl";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@sku",  sku);
            cmd.Parameters.AddWithValue("@excl", excludeId ?? string.Empty);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private static Product Map(MySqlDataReader r) => new()
        {
            ProductId     = r["ProductId"].ToString()!,
            SKU           = r["SKU"].ToString()!,
            ProductName   = r["ProductName"].ToString()!,
            Description   = r["Description"] == DBNull.Value ? null : r["Description"].ToString(),
            CategoryId    = r["CategoryId"] == DBNull.Value ? null : r["CategoryId"].ToString(),
            CategoryName  = r["CategoryName"] == DBNull.Value ? null : r["CategoryName"].ToString(),
            UnitOfMeasure = r["UnitOfMeasure"].ToString()!,
            Cost          = Convert.ToDecimal(r["Cost"]),
            ListPrice     = Convert.ToDecimal(r["ListPrice"]),
            IsActive      = Convert.ToBoolean(r["IsActive"])
        };
    }
}
