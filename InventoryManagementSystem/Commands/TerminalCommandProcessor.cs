using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Commands
{
    /// <summary>
    /// Represents the formatted result of a terminal command execution.
    /// </summary>
    public class TerminalOutput
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Color OutputColor { get; set; } = Color.White;
        public Product? AffectedProduct { get; set; }

        public static TerminalOutput Ok(string message, Product? product = null) =>
            new TerminalOutput { Success = true, Message = message, OutputColor = Color.FromArgb(74, 222, 128), AffectedProduct = product }; // Green-400

        public static TerminalOutput Error(string message) =>
            new TerminalOutput { Success = false, Message = message, OutputColor = Color.FromArgb(248, 113, 113) }; // Red-400

        public static TerminalOutput Info(string message) =>
            new TerminalOutput { Success = true, Message = message, OutputColor = Color.FromArgb(147, 197, 253) }; // Blue-300

        public static TerminalOutput Warning(string message) =>
            new TerminalOutput { Success = false, Message = message, OutputColor = Color.FromArgb(251, 191, 36) }; // Amber-400
    }

    /// <summary>
    /// TerminalCommandProcessor: Executes command-line style commands directly against the SQLite database
    /// without opening any dialogs or modal windows.
    /// Supports:
    /// - add <name> <price> <stock> <min_stock> OR add --name "..." --price ... --stock ... --min ...
    /// - edit <id> [--name ...] [--price ...] [--stock ...] [--min ...]
    /// - delete <id> OR delete --id <id>
    /// - list [--low|--out|--available]
    /// - search <query>
    /// - info <id>
    /// - help
    /// </summary>
    public class TerminalCommandProcessor
    {
        private readonly ProductRepository _repository;
        private readonly Action? _onDataModified;

        public TerminalCommandProcessor(ProductRepository repository, Action? onDataModified = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _onDataModified = onDataModified;
        }

        /// <summary>
        /// Parses and executes a single terminal input line.
        /// </summary>
        public TerminalOutput Process(string inputLine)
        {
            if (string.IsNullOrWhiteSpace(inputLine))
            {
                return TerminalOutput.Info("Type 'help' to see available commands.");
            }

            var tokens = Tokenize(inputLine);
            if (tokens.Count == 0)
            {
                return TerminalOutput.Info("Type 'help' to see available commands.");
            }

            string verb = tokens[0].ToLowerInvariant();

            try
            {
                return verb switch
                {
                    "add" => ExecuteAdd(tokens),
                    "edit" or "update" => ExecuteEdit(tokens),
                    "delete" or "del" or "remove" or "rm" => ExecuteDelete(tokens),
                    "list" or "ls" or "all" => ExecuteList(tokens),
                    "search" or "find" => ExecuteSearch(tokens),
                    "info" or "get" or "show" => ExecuteInfo(tokens),
                    "help" or "?" => ExecuteHelp(tokens),
                    "clear" or "cls" => TerminalOutput.Ok("__CLEAR__"),
                    _ => TerminalOutput.Warning($"Unknown command '{verb}'. Type 'help' to see supported commands.")
                };
            }
            catch (Exception ex)
            {
                return TerminalOutput.Error($"Unexpected execution error: {ex.Message}");
            }
        }

        // =================================================================
        // 1. ADD PRODUCT COMMAND
        // Syntax: add --name "Product Name" --price 49.99 --stock 20 --min 5
        // Or:     add "Product Name" 49.99 20 5
        // =================================================================
        private TerminalOutput ExecuteAdd(List<string> tokens)
        {
            string? name = null;
            decimal? price = null;
            int? stock = null;
            int? minStock = null;

            // Check if user passed flags or positional args
            var namedArgs = ParseFlags(tokens.Skip(1).ToList());

            if (namedArgs.Count > 0)
            {
                if (namedArgs.TryGetValue("name", out var n) || namedArgs.TryGetValue("n", out n))
                    name = n;
                if (namedArgs.TryGetValue("price", out var p) || namedArgs.TryGetValue("p", out p))
                {
                    if (decimal.TryParse(p, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedPrice))
                        price = parsedPrice;
                    else
                        return TerminalOutput.Error($"Invalid price value '{p}'. Must be a valid numeric value.");
                }
                if (namedArgs.TryGetValue("stock", out var s) || namedArgs.TryGetValue("s", out s) ||
                    namedArgs.TryGetValue("qty", out s) || namedArgs.TryGetValue("quantity", out s) || namedArgs.TryGetValue("q", out s))
                {
                    if (int.TryParse(s, out var parsedStock))
                        stock = parsedStock;
                    else
                        return TerminalOutput.Error($"Invalid stock quantity '{s}'. Must be a whole integer.");
                }
                if (namedArgs.TryGetValue("min", out var m) || namedArgs.TryGetValue("m", out m) ||
                    namedArgs.TryGetValue("minstock", out m) || namedArgs.TryGetValue("min-stock", out m))
                {
                    if (int.TryParse(m, out var parsedMin))
                        minStock = parsedMin;
                    else
                        return TerminalOutput.Error($"Invalid minimum stock '{m}'. Must be a whole integer.");
                }
            }

            // Fallback to positional arguments ONLY IF no flags were provided
            if (namedArgs.Count == 0)
            {
                var positionals = tokens.Skip(1).Where(t => !t.StartsWith("-") && !t.Contains("=")).ToList();

                if (string.IsNullOrEmpty(name) && positionals.Count >= 1)
                    name = positionals[0];

                if (!price.HasValue && positionals.Count >= 2)
                {
                    if (decimal.TryParse(positionals[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var p))
                        price = p;
                    else
                        return TerminalOutput.Error($"Invalid price value '{positionals[1]}'. Must be a valid numeric value.");
                }

                if (!stock.HasValue && positionals.Count >= 3)
                {
                    if (int.TryParse(positionals[2], out var s))
                        stock = s;
                    else
                        return TerminalOutput.Error($"Invalid stock value '{positionals[2]}'. Must be a whole integer.");
                }

                if (!minStock.HasValue && positionals.Count >= 4)
                {
                    if (int.TryParse(positionals[3], out var m))
                        minStock = m;
                    else
                        return TerminalOutput.Error($"Invalid minimum stock '{positionals[3]}'. Must be a whole integer.");
                }
            }

            // Defaults / Validation
            if (string.IsNullOrWhiteSpace(name))
            {
                return TerminalOutput.Error("Product name is required.\nUsage: add --name <name> --price <price> --stock <qty> [--min <min_stock>]\n   Or: add \"Product Name\" 49.99 20 5");
            }

            if (!price.HasValue)
            {
                return TerminalOutput.Error("Product price is required.\nUsage: add --name <name> --price <price> --stock <qty> [--min <min_stock>]");
            }

            if (price.Value < 0)
            {
                return TerminalOutput.Error("Product price cannot be negative.");
            }

            if (!stock.HasValue)
            {
                return TerminalOutput.Error("Product stock quantity is required.\nUsage: add --name <name> --price <price> --stock <qty> [--min <min_stock>]");
            }

            if (stock.Value < 0)
            {
                return TerminalOutput.Error("Product stock quantity cannot be negative.");
            }

            minStock ??= 5; // Default minimum stock if omitted

            if (minStock.Value < 0)
            {
                return TerminalOutput.Error("Minimum stock threshold cannot be negative.");
            }

            // Insert into SQLite database directly
            var product = new Product(0, name.Trim(), price.Value, stock.Value, minStock.Value);
            _repository.Add(product);
            _onDataModified?.Invoke();

            var sb = new StringBuilder();
            sb.AppendLine($"✔ [SUCCESS] Product #{product.Id} added to SQLite successfully!");
            sb.AppendLine($"  Name:     {product.Name}");
            sb.AppendLine($"  Price:    ${product.Price:F2}");
            sb.AppendLine($"  Quantity: {product.Quantity} units");
            sb.AppendLine($"  MinStock: {product.MinStock} units");
            sb.Append($"  Status:   {product.StockStatus}");

            return TerminalOutput.Ok(sb.ToString(), product);
        }

        // =================================================================
        // 2. EDIT PRODUCT COMMAND
        // Syntax: edit <id> [--name "New Name"] [--price 29.99] [--stock 10] [--min 3]
        // Or:     edit <id> "New Name" 29.99 10 3
        // Supports partial updates (e.g. edit 3 --price 24.99 only updates price!)
        // =================================================================
        private TerminalOutput ExecuteEdit(List<string> tokens)
        {
            if (tokens.Count < 2)
            {
                return TerminalOutput.Error("Product ID is required for editing.\nUsage: edit <id> [--name <name>] [--price <price>] [--stock <qty>] [--min <min_stock>]\n   Or: edit <id> \"New Name\" 39.99 15 5");
            }

            int? targetId = null;
            string? newName = null;
            decimal? newPrice = null;
            int? newStock = null;
            int? newMin = null;

            // Check if first arg after 'edit' is the ID
            if (int.TryParse(tokens[1], out var parsedId))
            {
                targetId = parsedId;
            }

            // Parse flags
            var remainingTokens = targetId.HasValue ? tokens.Skip(2).ToList() : tokens.Skip(1).ToList();
            var namedArgs = ParseFlags(remainingTokens);

            if (!targetId.HasValue)
            {
                if (namedArgs.TryGetValue("id", out var idStr) || namedArgs.TryGetValue("i", out idStr))
                {
                    if (int.TryParse(idStr, out var i))
                        targetId = i;
                }
            }

            if (!targetId.HasValue)
            {
                return TerminalOutput.Error($"Invalid or missing product ID '{tokens[1]}'. Must be a numeric product ID.");
            }

            // Fetch existing record
            var product = _repository.GetById(targetId.Value);
            if (product == null)
            {
                return TerminalOutput.Error($"Product with ID #{targetId.Value} does not exist in the database.");
            }

            // Read named updates
            if (namedArgs.TryGetValue("name", out var n) || namedArgs.TryGetValue("n", out n))
                newName = n;
            if (namedArgs.TryGetValue("price", out var p) || namedArgs.TryGetValue("p", out p))
            {
                if (decimal.TryParse(p, NumberStyles.Any, CultureInfo.InvariantCulture, out var pr))
                    newPrice = pr;
                else
                    return TerminalOutput.Error($"Invalid price value '{p}'. Must be a valid numeric value.");
            }
            if (namedArgs.TryGetValue("stock", out var s) || namedArgs.TryGetValue("s", out s) ||
                namedArgs.TryGetValue("qty", out s) || namedArgs.TryGetValue("quantity", out s) || namedArgs.TryGetValue("q", out s))
            {
                if (int.TryParse(s, out var st))
                    newStock = st;
                else
                    return TerminalOutput.Error($"Invalid stock quantity '{s}'. Must be a whole integer.");
            }
            if (namedArgs.TryGetValue("min", out var m) || namedArgs.TryGetValue("m", out m) ||
                namedArgs.TryGetValue("minstock", out m) || namedArgs.TryGetValue("min-stock", out m))
            {
                if (int.TryParse(m, out var mn))
                    newMin = mn;
                else
                    return TerminalOutput.Error($"Invalid minimum stock '{m}'. Must be a whole integer.");
            }

            // Read positional updates ONLY IF no flags were used
            if (namedArgs.Count == 0)
            {
                var positionals = remainingTokens.Where(t => !t.StartsWith("-") && !t.Contains("=")).ToList();
                if (string.IsNullOrEmpty(newName) && positionals.Count >= 1)
                    newName = positionals[0];
                if (!newPrice.HasValue && positionals.Count >= 2)
                {
                    if (decimal.TryParse(positionals[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var pr))
                        newPrice = pr;
                    else
                        return TerminalOutput.Error($"Invalid price value '{positionals[1]}'. Must be a valid numeric value.");
                }
                if (!newStock.HasValue && positionals.Count >= 3)
                {
                    if (int.TryParse(positionals[2], out var st))
                        newStock = st;
                    else
                        return TerminalOutput.Error($"Invalid stock value '{positionals[2]}'. Must be a whole integer.");
                }
                if (!newMin.HasValue && positionals.Count >= 4)
                {
                    if (int.TryParse(positionals[3], out var mn))
                        newMin = mn;
                    else
                        return TerminalOutput.Error($"Invalid minimum stock '{positionals[3]}'. Must be a whole integer.");
                }
            }

            // Ensure at least one field is being modified
            if (string.IsNullOrWhiteSpace(newName) && !newPrice.HasValue && !newStock.HasValue && !newMin.HasValue)
            {
                return TerminalOutput.Warning($"No changes specified for Product #{targetId.Value} ('{product.Name}').\nProvide at least one property to update:\n  edit {targetId.Value} --price <new_price>\n  edit {targetId.Value} --stock <new_stock>");
            }

            // Apply updates
            if (!string.IsNullOrWhiteSpace(newName))
                product.Name = newName.Trim();

            if (newPrice.HasValue)
            {
                if (newPrice.Value < 0) return TerminalOutput.Error("Price cannot be negative.");
                product.Price = newPrice.Value;
            }

            if (newStock.HasValue)
            {
                if (newStock.Value < 0) return TerminalOutput.Error("Stock quantity cannot be negative.");
                product.Quantity = newStock.Value;
            }

            if (newMin.HasValue)
            {
                if (newMin.Value < 0) return TerminalOutput.Error("Minimum stock cannot be negative.");
                product.MinStock = newMin.Value;
            }

            // Save to SQLite
            _repository.Update(product);
            _onDataModified?.Invoke();

            var sb = new StringBuilder();
            sb.AppendLine($"✔ [SUCCESS] Product #{product.Id} updated successfully!");
            sb.AppendLine($"  Name:     {product.Name}");
            sb.AppendLine($"  Price:    ${product.Price:F2}");
            sb.AppendLine($"  Quantity: {product.Quantity} units");
            sb.AppendLine($"  MinStock: {product.MinStock} units");
            sb.Append($"  Status:   {product.StockStatus}");

            return TerminalOutput.Ok(sb.ToString(), product);
        }

        // =================================================================
        // 3. DELETE PRODUCT COMMAND
        // Syntax: delete <id> OR delete --id <id>
        // =================================================================
        private TerminalOutput ExecuteDelete(List<string> tokens)
        {
            if (tokens.Count < 2)
            {
                return TerminalOutput.Error("Product ID is required for deletion.\nUsage: delete <id>   (e.g. delete 5)");
            }

            int? targetId = null;
            if (int.TryParse(tokens[1], out var id))
            {
                targetId = id;
            }
            else
            {
                var flags = ParseFlags(tokens.Skip(1).ToList());
                if (flags.TryGetValue("id", out var idStr) || flags.TryGetValue("i", out idStr))
                {
                    if (int.TryParse(idStr, out var i))
                        targetId = i;
                }
            }

            if (!targetId.HasValue)
            {
                return TerminalOutput.Error($"Invalid product ID '{tokens[1]}'. Must be a numeric ID.");
            }

            var existing = _repository.GetById(targetId.Value);
            if (existing == null)
            {
                return TerminalOutput.Error($"Product with ID #{targetId.Value} does not exist in the database.");
            }

            bool deleted = _repository.Delete(targetId.Value);
            if (!deleted)
            {
                return TerminalOutput.Error($"Failed to delete product #{targetId.Value} from database.");
            }

            _onDataModified?.Invoke();

            return TerminalOutput.Ok($"✔ [SUCCESS] Product '{existing.Name}' (ID: #{existing.Id}) deleted permanently from SQLite database.");
        }

        // =================================================================
        // 4. LIST PRODUCTS COMMAND
        // Syntax: list [--low | --out | --available]
        // =================================================================
        private TerminalOutput ExecuteList(List<string> tokens)
        {
            var products = _repository.GetAll();
            string filter = tokens.Count > 1 ? tokens[1].ToLowerInvariant() : "";

            if (filter.Contains("low"))
            {
                products = products.Where(p => p.StockStatus == "Low Stock").ToList();
            }
            else if (filter.Contains("out"))
            {
                products = products.Where(p => p.StockStatus == "Out of Stock").ToList();
            }
            else if (filter.Contains("avail"))
            {
                products = products.Where(p => p.StockStatus == "Available").ToList();
            }

            if (products.Count == 0)
            {
                return TerminalOutput.Info("No products found matching the criteria.");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"{"ID",-5} | {"PRODUCT NAME",-36} | {"PRICE",-10} | {"STOCK",-7} | {"MIN",-5} | {"STATUS",-12}");
            sb.AppendLine(new string('-', 88));

            foreach (var p in products)
            {
                string statusEmoji = p.StockStatus switch
                {
                    "Available" => "🟢 Available",
                    "Low Stock" => "🟡 Low Stock",
                    _ => "🔴 Out of Stock"
                };
                sb.AppendLine($"{p.Id,-5} | {Truncate(p.Name, 36),-36} | {p.Price.ToString("C", CultureInfo.CurrentCulture),-10} | {p.Quantity,-7} | {p.MinStock,-5} | {statusEmoji,-12}");
            }

            sb.Append(new string('-', 88));
            sb.Append($"\nTotal products: {products.Count}");

            return TerminalOutput.Info(sb.ToString());
        }

        // =================================================================
        // 5. SEARCH COMMAND
        // Syntax: search <query>
        // =================================================================
        private TerminalOutput ExecuteSearch(List<string> tokens)
        {
            if (tokens.Count < 2)
            {
                return TerminalOutput.Error("Search query is required.\nUsage: search <keyword or ID>   (e.g. search logitech)");
            }

            string query = string.Join(" ", tokens.Skip(1));
            var results = _repository.Search(query);

            if (results.Count == 0)
            {
                return TerminalOutput.Info($"No products found matching query '{query}'.");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Results for '{query}' ({results.Count} matches):");
            sb.AppendLine($"{"ID",-5} | {"PRODUCT NAME",-36} | {"PRICE",-10} | {"STOCK",-7} | {"MIN",-5} | {"STATUS",-12}");
            sb.AppendLine(new string('-', 88));

            foreach (var p in results)
            {
                string statusEmoji = p.StockStatus switch
                {
                    "Available" => "🟢 Available",
                    "Low Stock" => "🟡 Low Stock",
                    _ => "🔴 Out of Stock"
                };
                sb.AppendLine($"{p.Id,-5} | {Truncate(p.Name, 36),-36} | {p.Price.ToString("C", CultureInfo.CurrentCulture),-10} | {p.Quantity,-7} | {p.MinStock,-5} | {statusEmoji,-12}");
            }

            return TerminalOutput.Info(sb.ToString().TrimEnd());
        }

        // =================================================================
        // 6. INFO / GET COMMAND
        // Syntax: info <id>
        // =================================================================
        private TerminalOutput ExecuteInfo(List<string> tokens)
        {
            if (tokens.Count < 2 || !int.TryParse(tokens[1], out var id))
            {
                return TerminalOutput.Error("Product ID is required.\nUsage: info <id>   (e.g. info 3)");
            }

            var p = _repository.GetById(id);
            if (p == null)
            {
                return TerminalOutput.Error($"Product with ID #{id} was not found.");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"📦 Product Details [#{p.Id}]");
            sb.AppendLine($"  Name:             {p.Name}");
            sb.AppendLine($"  Price:            ${p.Price:F2}");
            sb.AppendLine($"  Stock Quantity:   {p.Quantity} units");
            sb.AppendLine($"  Minimum Stock:    {p.MinStock} units");
            sb.AppendLine($"  Stock Status:     {p.StockStatus}");
            sb.Append($"  Inventory Value:  ${p.Price * p.Quantity:N2}");

            return TerminalOutput.Info(sb.ToString());
        }

        // =================================================================
        // 7. HELP COMMAND
        // =================================================================
        private TerminalOutput ExecuteHelp(List<string> tokens)
        {
            var sb = new StringBuilder();
            sb.AppendLine("⚡ InventoryHub Direct Terminal Commands Reference:");
            sb.AppendLine(new string('=', 78));
            sb.AppendLine("➕ add <name> <price> <stock> [min_stock]");
            sb.AppendLine("   Flags: add --name \"Name\" --price 49.99 --stock 20 --min 5");
            sb.AppendLine("   Ex:    add \"Razer DeathAdder Mouse\" 39.99 15 4");
            sb.AppendLine();
            sb.AppendLine("✏️ edit <id> [--name <name>] [--price <price>] [--stock <qty>] [--min <min>]");
            sb.AppendLine("   Ex:    edit 3 --price 29.99 --stock 10   (partial updates supported!)");
            sb.AppendLine("   Ex:    edit 3 \"Updated Name\" 29.99 10 2");
            sb.AppendLine();
            sb.AppendLine("🗑️ delete <id>");
            sb.AppendLine("   Ex:    delete 5                          (permanently deletes record)");
            sb.AppendLine();
            sb.AppendLine("📋 list [--low | --out | --available]");
            sb.AppendLine("   Ex:    list --low                        (shows low-stock alerts)");
            sb.AppendLine();
            sb.AppendLine("🔍 search <query>");
            sb.AppendLine("   Ex:    search logitech                   (searches name or ID)");
            sb.AppendLine();
            sb.AppendLine("ℹ️ info <id>");
            sb.AppendLine("   Ex:    info 1                            (displays detailed card)");
            sb.AppendLine();
            sb.AppendLine("🧹 clear");
            sb.AppendLine("   Ex:    clear                             (clears terminal display)");
            sb.AppendLine(new string('=', 78));

            return TerminalOutput.Info(sb.ToString().TrimEnd());
        }

        // =================================================================
        // HELPERS: TOKENIZATION & FLAG PARSING
        // =================================================================
        public static List<string> Tokenize(string commandLine)
        {
            var tokens = new List<string>();
            if (string.IsNullOrWhiteSpace(commandLine)) return tokens;

            bool inQuotes = false;
            char quoteChar = '\0';
            var current = new StringBuilder();

            for (int i = 0; i < commandLine.Length; i++)
            {
                char c = commandLine[i];
                if (!inQuotes && (c == '"' || c == '\''))
                {
                    inQuotes = true;
                    quoteChar = c;
                }
                else if (inQuotes && c == quoteChar)
                {
                    inQuotes = false;
                    quoteChar = '\0';
                }
                else if (!inQuotes && char.IsWhiteSpace(c))
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                tokens.Add(current.ToString());
            }

            return tokens;
        }

        private static Dictionary<string, string> ParseFlags(List<string> tokens)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i];

                if (token.StartsWith("--") && token.Length > 2)
                {
                    string key = token.Substring(2);
                    if (key.Contains('='))
                    {
                        var parts = key.Split(new[] { '=' }, 2);
                        dict[parts[0]] = parts[1];
                    }
                    else if (i + 1 < tokens.Count && !tokens[i + 1].StartsWith("-"))
                    {
                        dict[key] = tokens[++i];
                    }
                    else
                    {
                        dict[key] = "true";
                    }
                }
                else if (token.StartsWith("-") && token.Length > 1 && !char.IsDigit(token[1]))
                {
                    string key = token.Substring(1);
                    if (key.Contains('='))
                    {
                        var parts = key.Split(new[] { '=' }, 2);
                        dict[parts[0]] = parts[1];
                    }
                    else if (i + 1 < tokens.Count && !tokens[i + 1].StartsWith("-"))
                    {
                        dict[key] = tokens[++i];
                    }
                    else
                    {
                        dict[key] = "true";
                    }
                }
                else if (token.Contains('='))
                {
                    var parts = token.Split(new[] { '=' }, 2);
                    dict[parts[0]] = parts[1];
                }
            }

            return dict;
        }

        private static string Truncate(string text, int maxLen)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length <= maxLen ? text : text.Substring(0, maxLen - 3) + "...";
        }
    }
}
