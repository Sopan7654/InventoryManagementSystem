// Program.cs — Main entry point
using InventoryManagementSystem.ConsoleUI;
using InventoryManagementSystem.DataAccess;

namespace InventoryManagementSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Inventory Management System";

            // Initialize DB connection
            try
            {
                DatabaseHelper.Initialize();
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n  Failed to load configuration. Please check appsettings.json.");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }

            // Test connection
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  Connecting to database...");
            Console.ResetColor();

            if (!DatabaseHelper.TestConnection())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(@"
  =====================================================
   ERROR: Cannot connect to MySQL database!
  =====================================================
   Please check:
   1. MySQL server is running
   2. appsettings.json has correct host/port/user/password
   3. Database 'InventoryManagementDB' exists
  =====================================================");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  Connected successfully!\n");
            Console.ResetColor();
            System.Threading.Thread.Sleep(800);

            // Show Main Menu
            ShowMainMenu();
        }

        static void ShowMainMenu()
        {
            var productMenu  = new ProductMenu();
            var inventoryMenu= new InventoryMenu();
            var reportMenu   = new ReportMenu();
            var supplierMenu = new SupplierMenu();
            var warehouseMenu= new WarehouseMenu();
            var poMenu       = new PurchaseOrderMenu();

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(@"
  ╔══════════════════════════════════════════════════════════════╗
  ║          INVENTORY MANAGEMENT SYSTEM  v1.0                  ║
  ╚══════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.WriteLine("  1.  Product Management");
                Console.WriteLine("  2.  Inventory Operations  (Stock In/Out/Transfer/Hold)");
                Console.WriteLine("  3.  Purchase Orders");
                Console.WriteLine("  4.  Supplier Management");
                Console.WriteLine("  5.  Warehouse Management  (+ Batch Tracking)");
                Console.WriteLine("  6.  Reports & Alerts");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  0.  Exit");
                Console.ResetColor();

                Console.Write("\n  Select option: ");
                string choice = Console.ReadLine()?.Trim() ?? "";

                switch (choice)
                {
                    case "1": productMenu.Show();   break;
                    case "2": inventoryMenu.Show();  break;
                    case "3": poMenu.Show();          break;
                    case "4": supplierMenu.Show();    break;
                    case "5": warehouseMenu.Show();   break;
                    case "6": reportMenu.Show();      break;
                    case "0":
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n  Goodbye!\n");
                        Console.ResetColor();
                        return;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  Invalid option. Try again.");
                        Console.ResetColor();
                        System.Threading.Thread.Sleep(500);
                        break;
                }
            }
        }
    }
}
