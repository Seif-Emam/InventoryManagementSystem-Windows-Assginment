using System;
using System.Collections.Generic;
using System.Windows.Forms;
using InventoryManagementSystem.Commands;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.UI;

namespace InventoryManagementSystem.Forms
{
    /// <summary>
    /// MainForm: The core desktop interface of the Inventory Management System.
    /// Structured using clean partial classes for UI, Views, and Actions.
    /// Demonstrates all 5 Windows Programming Course Topics:
    /// - Topic 1: Events, Messages & Event Handling (Click, TextChanged, Confirmations, UI Refresh)
    /// - Topic 2: Graphics and Drawing (GDI+ Custom Stock Chart, Panel.Paint, FillRectangle)
    /// - Topic 3: Keyboard, Mouse & Input (Ctrl+N, Ctrl+F, Delete, F1, Chart Click, Row Action Button)
    /// - Topic 4: Menus, Commands & Resources (MenuStrip: File, Products, Commands, Help -> User Guide, About)
    /// - Topic 5: Forms (MainForm, ProductForm Dialog, CommandsExplorerForm, HelpForm, AboutForm, MessageBox Modals)
    /// </summary>
    public partial class MainForm : Form
    {
        // -----------------------------------------------------------------
        // Persistent SQLite Data Layer (Section 2 & 7)
        // -----------------------------------------------------------------
        private readonly ProductRepository _repository;
        public ProductRepository Repository => _repository;
        private List<Product> _displayedProducts = new List<Product>();

        // -----------------------------------------------------------------
        // Application Commands Management System (Topic 4)
        // -----------------------------------------------------------------
        public CommandRegistry Commands { get; private set; } = null!;

        // -----------------------------------------------------------------
        // UI Controls
        // -----------------------------------------------------------------
        private MenuStrip _menuStrip = null!;
        private Panel _sidebarPanel = null!;
        private Panel _headerPanel = null!;
        private Label _lblHeaderTitle = null!;
        private Label _lblHeaderSubtitle = null!;
        private TextBox _txtSearch = null!;
        private Panel _searchBoxContainer = null!;

        // Sidebar Navigation Buttons
        private ModernButton _btnNavDashboard = null!;
        private ModernButton _btnNavProducts = null!;
        private ModernButton _btnNavCommands = null!;
        private ModernButton _btnNavTerminal = null!;
        private ModernButton _btnNavGuide = null!;
        private ModernButton _btnNavAbout = null!;

        // Content Views & Terminal
        private Panel _viewsContainer = null!;
        private Panel _dashboardView = null!;
        private Panel _productsView = null!;
        private TerminalPanel _terminalPanel = null!;
        private bool _isTerminalVisible = false;

        // Dashboard Components
        private StatCardPanel _cardTotal = null!;
        private StatCardPanel _cardLowStock = null!;
        private StatCardPanel _cardOutOfStock = null!;
        private StockChartPanel _chartPanel = null!;

        // DataGridView Components
        private DataGridView _dgvProducts = null!;
        private DataGridView _dgvDashboardProducts = null!;
        private Label _lblProductsCount = null!;
        private Label _lblDashboardProductsCount = null!;

        public MainForm() : this(new ProductRepository())
        {
        }

        public MainForm(ProductRepository repository)
        {
            _repository = repository ?? new ProductRepository();
            Commands = CommandRegistry.CreateDefault(this);
            InitializeCustomUI();
            RefreshAllUI();
        }

        // =================================================================
        // TOPIC 3: KEYBOARD SHORTCUTS VIA ProcessCmdKey
        // =================================================================
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Detect if a text box is focused to avoid interfering with normal text editing
            bool isTextBoxFocused = ActiveControl is TextBox ||
                (ActiveControl is ContainerControl container && container.ActiveControl is TextBox);

            // Ctrl+Shift+C: Open Commands Explorer
            if (keyData == (Keys.Control | Keys.Shift | Keys.C))
            {
                HandleOpenCommandsExplorer();
                return true;
            }

            // Ctrl+~ or F12: Toggle Command Terminal (Direct CLI)
            if (keyData == (Keys.Control | Keys.Oemtilde) || keyData == Keys.F12)
            {
                ToggleTerminal();
                return true;
            }

            // F5: Refresh Data from SQLite
            if (keyData == Keys.F5)
            {
                HandleRefreshData();
                return true;
            }

            // Ctrl+1: Switch to Dashboard
            if (keyData == (Keys.Control | Keys.D1))
            {
                HandleNavDashboard();
                return true;
            }

            // Ctrl+2: Switch to Products Catalog
            if (keyData == (Keys.Control | Keys.D2))
            {
                HandleNavProducts();
                return true;
            }

            // Ctrl+E: Edit selected product
            if (keyData == (Keys.Control | Keys.E))
            {
                HandleEditProduct();
                return true;
            }

            // Ctrl+Shift+L: Quick filter Low Stock
            if (keyData == (Keys.Control | Keys.Shift | Keys.L))
            {
                HandleFilterLowStock();
                return true;
            }

            // Ctrl+Shift+O: Quick filter Out of Stock
            if (keyData == (Keys.Control | Keys.Shift | Keys.O))
            {
                HandleFilterOutOfStock();
                return true;
            }

            // Ctrl+Shift+X: Clear search filter
            if (keyData == (Keys.Control | Keys.Shift | Keys.X))
            {
                HandleClearSearch();
                return true;
            }

            // Ctrl+Shift+A: Reset filters (Show all)
            if (keyData == (Keys.Control | Keys.Shift | Keys.A))
            {
                HandleResetFilter();
                return true;
            }

            // Ctrl+N: Add New Product
            if (keyData == (Keys.Control | Keys.N))
            {
                HandleAddProduct();
                return true;
            }

            // Ctrl+F: Focus Search TextBox
            if (keyData == (Keys.Control | Keys.F))
            {
                FocusSearchBox();
                return true;
            }

            // F1: Open User Guide & Keyboard Shortcuts
            if (keyData == Keys.F1)
            {
                HandleUserGuide();
                return true;
            }

            // Delete: Delete selected product (only when not editing inside a TextBox!)
            if (keyData == Keys.Delete && !isTextBoxFocused)
            {
                HandleDeleteProduct();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
