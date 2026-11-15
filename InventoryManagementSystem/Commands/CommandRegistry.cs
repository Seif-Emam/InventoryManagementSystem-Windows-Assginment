using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using InventoryManagementSystem.Forms;

namespace InventoryManagementSystem.Commands
{
    public class CommandExecutedEventArgs : EventArgs
    {
        public AppCommand Command { get; }
        public ExecutionResult Result { get; }

        public CommandExecutedEventArgs(AppCommand command, ExecutionResult result)
        {
            Command = command;
            Result = result;
        }
    }

    /// <summary>
    /// Central registry and coordinator for application commands.
    /// Demonstrates Windows Programming Topic 4: Menus, Commands and Resources.
    /// Supports registration, querying, filtering, and execution tracking.
    /// </summary>
    public class CommandRegistry
    {
        private readonly List<AppCommand> _commands = new List<AppCommand>();

        public event EventHandler<CommandExecutedEventArgs>? CommandExecuted;

        public IReadOnlyList<AppCommand> Commands => _commands.AsReadOnly();

        public void Register(AppCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            if (_commands.Any(c => c.Id.Equals(command.Id, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Command with ID '{command.Id}' is already registered.");
            }

            _commands.Add(command);
        }

        public AppCommand? FindById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _commands.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public ExecutionResult ExecuteCommand(string id, MainForm target)
        {
            var command = FindById(id);
            if (command == null)
            {
                var failure = ExecutionResult.Failed($"Command '{id}' not found in registry.");
                return failure;
            }

            var result = command.Execute(target);
            CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(command, result));
            return result;
        }

        /// <summary>
        /// Creates a fully initialized CommandRegistry with all 15 functional application commands
        /// connected directly to MainForm's actual methods.
        /// </summary>
        public static CommandRegistry CreateDefault(MainForm mainForm)
        {
            var registry = new CommandRegistry();

            // 1. Add Product Command
            registry.Register(new AppCommand(
                id: "CMD_PROD_ADD",
                name: "Add New Product",
                category: "Products & CRUD",
                purpose: "Opens the Product Creation dialog to collect product details, validate data, and insert a new record into SQLite.",
                expectedAction: "Displays ProductForm in Add mode; on save, validates inputs, generates unique ID, inserts into database, and updates all views.",
                handlerName: "MainForm.HandleAddProduct()",
                shortcut: "Ctrl+N",
                keyShortcut: Keys.Control | Keys.N,
                affectedComponents: "SQLite Database, Products Table, DataGridViews, Stat Cards, GDI+ Stock Chart",
                action: f => f.HandleAddProduct()
            ));

            // 2. Edit Selected Product Command
            registry.Register(new AppCommand(
                id: "CMD_PROD_EDIT",
                name: "Edit Selected Product",
                category: "Products & CRUD",
                purpose: "Opens the Product Edit dialog for the currently selected item in the inventory table.",
                expectedAction: "Populates ProductForm with existing record values; validates modifications, preserves original ID, and updates SQLite and UI.",
                handlerName: "MainForm.HandleEditProduct()",
                shortcut: "Ctrl+E",
                keyShortcut: Keys.Control | Keys.E,
                affectedComponents: "SQLite Database, ProductForm, DataGridViews, Dashboard Cards, GDI+ Chart",
                action: f => f.HandleEditProduct()
            ));

            // 3. Delete Selected Product Command
            registry.Register(new AppCommand(
                id: "CMD_PROD_DELETE",
                name: "Delete Selected Product",
                category: "Products & CRUD",
                purpose: "Permanently deletes the selected inventory product after prompt confirmation.",
                expectedAction: "Displays custom confirmation modal (NotificationDialog) with product chip; upon 'Yes', removes record from SQLite and refreshes UI.",
                handlerName: "MainForm.HandleDeleteProduct()",
                shortcut: "Delete",
                keyShortcut: Keys.Delete,
                affectedComponents: "SQLite Database, DataGridViews, Stat Cards, GDI+ Chart, NotificationDialog",
                action: f => f.HandleDeleteProduct()
            ));

            // 4. Refresh Inventory Data Command
            registry.Register(new AppCommand(
                id: "CMD_DATA_REFRESH",
                name: "Refresh Inventory Data",
                category: "Data & Analytics",
                purpose: "Reloads all inventory data and dashboard metrics from the SQLite database.",
                expectedAction: "Queries SQLite repository, updates displayed product list, recalculates KPI metrics, and redraws the GDI+ chart.",
                handlerName: "MainForm.HandleRefreshData()",
                shortcut: "F5",
                keyShortcut: Keys.F5,
                affectedComponents: "ProductRepository, DataGridViews, StatCardPanels, StockChartPanel",
                action: f => f.HandleRefreshData()
            ));

            // 5. Focus Search Box Command
            registry.Register(new AppCommand(
                id: "CMD_SEARCH_FOCUS",
                name: "Focus Search Box",
                category: "Search & Filters",
                purpose: "Immediately transfers keyboard focus to the search input field for rapid item lookup.",
                expectedAction: "Sets focus to header TextBox, selects all existing text, and highlights input container with blue accent border.",
                handlerName: "MainForm.HandleFocusSearch()",
                shortcut: "Ctrl+F",
                keyShortcut: Keys.Control | Keys.F,
                affectedComponents: "Header TextBox (_txtSearch), Search Box Container",
                action: f => f.HandleFocusSearch()
            ));

            // 6. Clear Search Filter Command
            registry.Register(new AppCommand(
                id: "CMD_SEARCH_CLEAR",
                name: "Clear Search Filter",
                category: "Search & Filters",
                purpose: "Clears any active search query and restores the complete inventory list.",
                expectedAction: "Empties search TextBox, triggers live TextChanged filter, and restores all database products to the table.",
                handlerName: "MainForm.HandleClearSearch()",
                shortcut: "Ctrl+Shift+X",
                keyShortcut: Keys.Control | Keys.Shift | Keys.X,
                affectedComponents: "Header TextBox (_txtSearch), DataGridViews, Record Counters",
                action: f => f.HandleClearSearch()
            ));

            // 7. Navigate to Dashboard View Command
            registry.Register(new AppCommand(
                id: "CMD_NAV_DASHBOARD",
                name: "Switch to Dashboard View",
                category: "Navigation & Views",
                purpose: "Switches the active workspace to the interactive Dashboard Overview view.",
                expectedAction: "Highlights Dashboard sidebar tab, brings _dashboardView to front, and initializes overview cards and GDI+ chart.",
                handlerName: "MainForm.HandleNavDashboard()",
                shortcut: "Ctrl+1",
                keyShortcut: Keys.Control | Keys.D1,
                affectedComponents: "Sidebar Navigation, _viewsContainer, _dashboardView, StockChartPanel",
                action: f => f.HandleNavDashboard()
            ));

            // 8. Navigate to Products Catalog View Command
            registry.Register(new AppCommand(
                id: "CMD_NAV_CATALOG",
                name: "Switch to Products Catalog View",
                category: "Navigation & Views",
                purpose: "Switches the active workspace to the full-page Products Catalog table view.",
                expectedAction: "Highlights Products Catalog sidebar tab, brings _productsView to front, and displays full-screen DataGridView.",
                handlerName: "MainForm.HandleNavProducts()",
                shortcut: "Ctrl+2",
                keyShortcut: Keys.Control | Keys.D2,
                affectedComponents: "Sidebar Navigation, _viewsContainer, _productsView, _dgvProducts",
                action: f => f.HandleNavProducts()
            ));

            // 9. Quick Filter: Low Stock Items Command
            registry.Register(new AppCommand(
                id: "CMD_FILTER_LOW_STOCK",
                name: "Quick Filter: Low Stock Items",
                category: "Search & Filters",
                purpose: "Filters the displayed products to items that have reached or fallen below minimum stock threshold.",
                expectedAction: "Filters displayed products to condition (Quantity > 0 AND Quantity <= MinStock) and updates status counter.",
                handlerName: "MainForm.HandleFilterLowStock()",
                shortcut: "Ctrl+Shift+L",
                keyShortcut: Keys.Control | Keys.Shift | Keys.L,
                affectedComponents: "DataGridViews, Record Counter Labels, Search State",
                action: f => f.HandleFilterLowStock()
            ));

            // 10. Quick Filter: Out of Stock Items Command
            registry.Register(new AppCommand(
                id: "CMD_FILTER_OUT_OF_STOCK",
                name: "Quick Filter: Out of Stock Items",
                category: "Search & Filters",
                purpose: "Filters the displayed products to completely depleted items requiring urgent restocking.",
                expectedAction: "Filters displayed products to condition (Quantity == 0) and highlights items with red badges.",
                handlerName: "MainForm.HandleFilterOutOfStock()",
                shortcut: "Ctrl+Shift+O",
                keyShortcut: Keys.Control | Keys.Shift | Keys.O,
                affectedComponents: "DataGridViews, Record Counter Labels, Search State",
                action: f => f.HandleFilterOutOfStock()
            ));

            // 11. Reset Filters: Show All Products Command
            registry.Register(new AppCommand(
                id: "CMD_FILTER_ALL",
                name: "Reset Filters: Show All",
                category: "Search & Filters",
                purpose: "Removes all search and stock status filters, displaying every item in the SQLite database.",
                expectedAction: "Resets internal filter state and search box; displays all 25+ products in the catalog.",
                handlerName: "MainForm.HandleResetFilter()",
                shortcut: "Ctrl+Shift+A",
                keyShortcut: Keys.Control | Keys.Shift | Keys.A,
                affectedComponents: "DataGridViews, Record Counter Labels, Search Box",
                action: f => f.HandleResetFilter()
            ));

            // 12. Open User Guide & Shortcuts Command
            registry.Register(new AppCommand(
                id: "CMD_HELP_GUIDE",
                name: "Open User Guide & Shortcuts",
                category: "Help & System",
                purpose: "Displays the comprehensive in-app user guide and keyboard shortcuts reference modal.",
                expectedAction: "Opens HelpForm modal dialog centered over MainForm with CRUD walkthroughs and hotkey reference cards.",
                handlerName: "MainForm.HandleUserGuide()",
                shortcut: "F1",
                keyShortcut: Keys.F1,
                affectedComponents: "HelpForm Dialog, Application Window",
                action: f => f.HandleUserGuide()
            ));

            // 13. Open System Diagnostics & About Command
            registry.Register(new AppCommand(
                id: "CMD_SYS_ABOUT",
                name: "Open System Diagnostics & About",
                category: "Help & System",
                purpose: "Shows technical application information, .NET 8 runtime version, SQLite database location, and architecture details.",
                expectedAction: "Opens AboutForm modal dialog with real database path and environment metrics.",
                handlerName: "MainForm.HandleAbout()",
                shortcut: "Ctrl+I",
                keyShortcut: Keys.Control | Keys.I,
                affectedComponents: "AboutForm Dialog, Application Window",
                action: f => f.HandleAbout()
            ));

            // 14. Open Commands Explorer Command
            registry.Register(new AppCommand(
                id: "CMD_EXPLORER_OPEN",
                name: "Open Commands Explorer",
                category: "Help & System",
                purpose: "Launches the Application Commands Explorer to inspect, search, and execute all application commands.",
                expectedAction: "Opens CommandsExplorerForm displaying all registered commands, metadata, search filters, and execution triggers.",
                handlerName: "MainForm.HandleOpenCommandsExplorer()",
                shortcut: "Ctrl+Shift+C",
                keyShortcut: Keys.Control | Keys.Shift | Keys.C,
                affectedComponents: "CommandsExplorerForm Dialog, MenuStrip (Commands Menu)",
                action: f => f.HandleOpenCommandsExplorer()
            ));

            // 15. Open Interactive Command Terminal
            registry.Register(new AppCommand(
                id: "CMD_TERMINAL_OPEN",
                name: "Toggle Command Terminal",
                category: "Data & Analytics",
                purpose: "Toggles the interactive command-line terminal to insert, edit, or delete inventory directly without dialogs.",
                expectedAction: "Opens or hides the built-in Terminal drawer; focuses input for immediate CLI command entry.",
                handlerName: "MainForm.ToggleTerminal()",
                shortcut: "Ctrl+~",
                keyShortcut: Keys.Control | Keys.Oemtilde,
                affectedComponents: "TerminalPanel Drawer, SQLite Database, Live Views",
                action: f => f.ToggleTerminal()
            ));

            // 16. Terminal: Add Product Directly
            registry.Register(new AppCommand(
                id: "CMD_CLI_ADD",
                name: "Terminal: add (Direct Insert)",
                category: "Terminal CLI",
                purpose: "Directly inserts a new product into SQLite without opening any modal dialog windows.",
                expectedAction: "Usage: add --name <name> --price <price> --stock <qty> [--min <min>] OR add \"<name>\" <price> <qty> [min]. Effect: Inserts record into SQLite, dynamically calculates stock status, and live updates KPI cards, chart, and table.",
                handlerName: "TerminalCommandProcessor.ExecuteAdd()",
                shortcut: "add ...",
                keyShortcut: Keys.None,
                affectedComponents: "SQLite Products Table (INSERT), Stat Cards (+1), GDI+ Stock Chart (New Bar), DataGridViews",
                action: f => f.ToggleTerminal(true)
            ));

            // 17. Terminal: Edit Product Directly
            registry.Register(new AppCommand(
                id: "CMD_CLI_EDIT",
                name: "Terminal: edit (Direct Update)",
                category: "Terminal CLI",
                purpose: "Updates existing product fields directly from command line, supporting partial updates without popup windows.",
                expectedAction: "Usage: edit <id> [--name <name>] [--price <price>] [--stock <qty>] [--min <min>]. Effect: Updates specified columns in SQLite, dynamically recalculates stock status, updates KPI cards, and updates GDI+ chart.",
                handlerName: "TerminalCommandProcessor.ExecuteEdit()",
                shortcut: "edit ...",
                keyShortcut: Keys.None,
                affectedComponents: "SQLite Products Table (UPDATE), Stock Status Recalculation, KPI Alert Cards, GDI+ Chart",
                action: f => f.ToggleTerminal(true)
            ));

            // 18. Terminal: Delete Product Directly
            registry.Register(new AppCommand(
                id: "CMD_CLI_DELETE",
                name: "Terminal: delete (Direct Delete)",
                category: "Terminal CLI",
                purpose: "Permanently deletes a product from SQLite by ID directly from the command line with zero confirmation popups.",
                expectedAction: "Usage: delete <id> (e.g. delete 5). Effect: Removes row from SQLite, decrements Total Products card, removes bar from GDI+ chart, and refreshes catalog.",
                handlerName: "TerminalCommandProcessor.ExecuteDelete()",
                shortcut: "delete <id>",
                keyShortcut: Keys.None,
                affectedComponents: "SQLite Products Table (DELETE), Stat Cards (-1), GDI+ Stock Chart (Removes Bar), DataGridViews",
                action: f => f.ToggleTerminal(true)
            ));

            // 19. Terminal: List Products Table
            registry.Register(new AppCommand(
                id: "CMD_CLI_LIST",
                name: "Terminal: list (ASCII Table)",
                category: "Terminal CLI",
                purpose: "Outputs an aligned ASCII table of products directly in the terminal console.",
                expectedAction: "Usage: list [--low | --out | --available]. Effect: Queries SQLite and renders aligned table with ID, Name, Price, Stock, Min, and Status badges without altering GUI table filters.",
                handlerName: "TerminalCommandProcessor.ExecuteList()",
                shortcut: "list",
                keyShortcut: Keys.None,
                affectedComponents: "Terminal Output Display, SQLite Database",
                action: f => f.ToggleTerminal(true)
            ));

            // 20. Terminal: Search Products
            registry.Register(new AppCommand(
                id: "CMD_CLI_SEARCH",
                name: "Terminal: search (Instant Query)",
                category: "Terminal CLI",
                purpose: "Searches products by partial name or ID directly in terminal.",
                expectedAction: "Usage: search <keyword> (e.g. search logitech). Effect: Executes case-insensitive LIKE query on SQLite and renders matching rows in terminal.",
                handlerName: "TerminalCommandProcessor.ExecuteSearch()",
                shortcut: "search <kw>",
                keyShortcut: Keys.None,
                affectedComponents: "Terminal Output Display, SQLite Database",
                action: f => f.ToggleTerminal(true)
            ));

            // 21. Terminal: Product Valuation Card
            registry.Register(new AppCommand(
                id: "CMD_CLI_INFO",
                name: "Terminal: info (Product Valuation)",
                category: "Terminal CLI",
                purpose: "Displays comprehensive details and total calculated inventory valuation for a single product ID.",
                expectedAction: "Usage: info <id> (e.g. info 1). Effect: Fetches product from SQLite and prints inventory valuation card (Price × Stock).",
                handlerName: "TerminalCommandProcessor.ExecuteInfo()",
                shortcut: "info <id>",
                keyShortcut: Keys.None,
                affectedComponents: "Terminal Output Display, SQLite Database",
                action: f => f.ToggleTerminal(true)
            ));

            // 22. Terminal: Help Reference
            registry.Register(new AppCommand(
                id: "CMD_CLI_HELP",
                name: "Terminal: help (Syntax Reference)",
                category: "Terminal CLI",
                purpose: "Displays the complete syntax and examples reference for all terminal commands.",
                expectedAction: "Usage: help. Effect: Renders formatted CLI reference for add, edit, delete, list, search, info, and clear.",
                handlerName: "TerminalCommandProcessor.ExecuteHelp()",
                shortcut: "help",
                keyShortcut: Keys.None,
                affectedComponents: "Terminal Output Display",
                action: f => f.ToggleTerminal(true)
            ));

            // 23. Terminal: Clear Screen
            registry.Register(new AppCommand(
                id: "CMD_CLI_CLEAR",
                name: "Terminal: clear (Wipe Screen)",
                category: "Terminal CLI",
                purpose: "Wipes terminal output text and restores the clean welcome banner.",
                expectedAction: "Usage: clear or cls. Effect: Resets RichTextBox display buffer.",
                handlerName: "TerminalCommandProcessor.ExecuteClear()",
                shortcut: "clear",
                keyShortcut: Keys.None,
                affectedComponents: "Terminal Output Display",
                action: f => f.ToggleTerminal(true)
            ));

            // 24. Exit Application Command
            registry.Register(new AppCommand(
                id: "CMD_APP_EXIT",
                name: "Exit Application",
                category: "Help & System",
                purpose: "Safely terminates the Inventory Management System application.",
                expectedAction: "Flushes SQLite connection pools, saves any pending changes, and closes the main window.",
                handlerName: "MainForm.HandleExit()",
                shortcut: "Alt+F4",
                keyShortcut: Keys.Alt | Keys.F4,
                affectedComponents: "Application Process, SQLite Connections, Main Window",
                action: f => f.HandleExit()
            ));

            return registry;
        }
    }
}

