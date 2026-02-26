// ConsoleUI/ConsoleHelper.cs
namespace InventoryManagementSystem.ConsoleUI
{
    public static class ConsoleHelper
    {
        public static void PrintHeader(string title)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            string line = new string('=', 65);
            Console.WriteLine(line);
            Console.WriteLine($"  {title.ToUpper().PadRight(61)}");
            Console.WriteLine(line);
            Console.ResetColor();
        }

        public static void PrintSuccess(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  ✔ {msg}");
            Console.ResetColor();
        }

        public static void PrintError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n  ✘ {msg}");
            Console.ResetColor();
        }

        public static void PrintWarning(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  ⚠ {msg}");
            Console.ResetColor();
        }

        public static void PrintInfo(string msg)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"  {msg}");
            Console.ResetColor();
        }

        public static void PrintSectionTitle(string title)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  --- {title} ---");
            Console.ResetColor();
        }

        public static string AskInput(string prompt)
        {
            Console.Write($"\n  {prompt}: ");
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        public static string AskRequired(string prompt)
        {
            string val;
            do
            {
                val = AskInput(prompt);
                if (string.IsNullOrWhiteSpace(val))
                    PrintError("This field is required.");
            } while (string.IsNullOrWhiteSpace(val));
            return val;
        }

        public static decimal AskDecimal(string prompt, decimal defaultVal = 0)
        {
            while (true)
            {
                string s = AskInput(prompt);
                if (string.IsNullOrWhiteSpace(s)) return defaultVal;
                if (decimal.TryParse(s, out decimal d)) return d;
                PrintError("Please enter a valid number.");
            }
        }

        public static int AskInt(string prompt, int defaultVal = 0)
        {
            while (true)
            {
                string s = AskInput(prompt);
                if (string.IsNullOrWhiteSpace(s)) return defaultVal;
                if (int.TryParse(s, out int i)) return i;
                PrintError("Please enter a valid integer.");
            }
        }

        public static void PressEnterToContinue()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("\n  Press Enter to continue...");
            Console.ResetColor();
            Console.ReadLine();
        }

        // Generic table printer
        public static void PrintTable(string[] headers, int[] widths, List<string[]> rows, ConsoleColor rowColor = ConsoleColor.White)
        {
            Console.WriteLine();
            // Header
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            PrintTableRow(headers, widths);
            Console.WriteLine("  " + new string('-', widths.Sum() + widths.Length * 3));
            Console.ResetColor();

            if (rows.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  (No records found)");
                Console.ResetColor();
                return;
            }

            foreach (var row in rows)
            {
                Console.ForegroundColor = rowColor;
                PrintTableRow(row, widths);
            }
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n  Total: {rows.Count} record(s)");
            Console.ResetColor();
        }

        private static void PrintTableRow(string[] cells, int[] widths)
        {
            Console.Write("  ");
            for (int i = 0; i < cells.Length; i++)
            {
                string cell = cells[i] ?? "";
                if (cell.Length > widths[i]) cell = cell[..(widths[i] - 1)] + "…";
                Console.Write(cell.PadRight(widths[i]) + " | ");
            }
            Console.WriteLine();
        }
    }
}
