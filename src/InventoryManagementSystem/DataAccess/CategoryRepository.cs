// DataAccess/CategoryRepository.cs
using MySql.Data.MySqlClient;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.DataAccess
{
    public class CategoryRepository
    {
        public List<ProductCategory> GetAll()
        {
            var list = new List<ProductCategory>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT c.CategoryId, c.CategoryName, c.Description,
                                  c.ParentCategoryId, p.CategoryName AS ParentCategoryName
                           FROM ProductCategory c
                           LEFT JOIN ProductCategory p ON p.CategoryId = c.ParentCategoryId
                           ORDER BY c.ParentCategoryId, c.CategoryName";
            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(Map(rdr));
            return list;
        }

        public ProductCategory? GetById(string id)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT c.CategoryId, c.CategoryName, c.Description,
                                  c.ParentCategoryId, p.CategoryName AS ParentCategoryName
                           FROM ProductCategory c
                           LEFT JOIN ProductCategory p ON p.CategoryId = c.ParentCategoryId
                           WHERE c.CategoryId = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var rdr = cmd.ExecuteReader();
            return rdr.Read() ? Map(rdr) : null;
        }

        public bool Insert(ProductCategory c)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"INSERT INTO ProductCategory (CategoryId,CategoryName,Description,ParentCategoryId)
                           VALUES (@id,@name,@desc,@parent)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id",     c.CategoryId);
            cmd.Parameters.AddWithValue("@name",   c.CategoryName);
            cmd.Parameters.AddWithValue("@desc",   c.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@parent", c.ParentCategoryId ?? (object)DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static ProductCategory Map(MySqlDataReader r) => new()
        {
            CategoryId         = r["CategoryId"].ToString()!,
            CategoryName       = r["CategoryName"].ToString()!,
            Description        = r["Description"] == DBNull.Value ? null : r["Description"].ToString(),
            ParentCategoryId   = r["ParentCategoryId"] == DBNull.Value ? null : r["ParentCategoryId"].ToString(),
            ParentCategoryName = r["ParentCategoryName"] == DBNull.Value ? null : r["ParentCategoryName"].ToString()
        };
    }
}
