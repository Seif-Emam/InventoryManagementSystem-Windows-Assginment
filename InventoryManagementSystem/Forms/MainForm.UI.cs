using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InventoryManagementSystem.Commands;
using InventoryManagementSystem.UI;

namespace InventoryManagementSystem.Forms
{
    public partial class MainForm
    {
        // =================================================================
        // 1. UI INITIALIZATION & MODERN SAAS SHELL LAYOUT
        // =================================================================
        private void InitializeCustomUI()
        {
            Text = "InventoryHub — Inventory Management System (SQLite)";
            Size = new Size(1320, 860);
            MinimumSize = new Size(1080, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = DesignSystem.AppBackground;
            Font = DesignSystem.FontBody;

            // 1. Top MenuStrip (Topic 4: Menus, Commands and Resources)
            BuildMenuStrip();

            // 2. Left Sidebar Navigation Panel
            BuildSidebar();

            // 3. Main Content Container
            Panel mainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesignSystem.AppBackground
            };

            // 4. Top Header Panel
            BuildHeader();

            // 5. Views Container (Dashboard View & Products View)
            _viewsContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesignSystem.AppBackground,
                Padding = new Padding(24, 16, 24, 20)
            };

            BuildDashboardView();
            BuildProductsView();

            _viewsContainer.Controls.Add(_dashboardView);
            _viewsContainer.Controls.Add(_productsView);

            // 6. Direct Command Terminal Panel (Collapsible bottom drawer)
            _terminalPanel = new TerminalPanel(new TerminalCommandProcessor(_repository, () => RefreshAllUI()))
            {
                Visible = false
            };
            _terminalPanel.CloseRequested += (s, e) => ToggleTerminal(false);

            // WinForms Docking Layout: Add Fill first, then Bottom, then Top, and SendToBack on Top
            // This guarantees the Top Header reserves its 84px height and does NOT overlap _viewsContainer
            mainContent.Controls.Add(_viewsContainer);
            mainContent.Controls.Add(_terminalPanel);
            mainContent.Controls.Add(_headerPanel);
            _headerPanel.SendToBack();

            // Add top-level controls
            Controls.Add(mainContent);
            Controls.Add(_sidebarPanel);
            Controls.Add(_menuStrip);
            MainMenuStrip = _menuStrip;

            // Default active view: Dashboard
            ShowView(isDashboard: true);
        }

        // =================================================================
        // TOPIC 4: MENUS, COMMANDS AND RESOURCES
        // =================================================================
        private void BuildMenuStrip()
        {
            _menuStrip = new MenuStrip
            {
                BackColor = DesignSystem.PrimaryNavy,
                ForeColor = Color.White,
                Font = DesignSystem.FontSecondary,
                Padding = new Padding(8, 4, 8, 4),
                Renderer = new ToolStripProfessionalRenderer(new SaasMenuColorTable())
            };

            // --- File Menu ---
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("&File") { ForeColor = Color.White };
            ToolStripMenuItem exitItem = new ToolStripMenuItem("E&xit", null, (s, e) => HandleExit(), Keys.Alt | Keys.F4);
            fileMenu.DropDownItems.Add(exitItem);

            // --- Products Menu ---
            ToolStripMenuItem productsMenu = new ToolStripMenuItem("&Products") { ForeColor = Color.White };
            ToolStripMenuItem addItem = new ToolStripMenuItem("&Add Product...", null, (s, e) => HandleAddProduct(), Keys.Control | Keys.N);
            ToolStripMenuItem editItem = new ToolStripMenuItem("&Edit Product...", null, (s, e) => HandleEditProduct(), Keys.Control | Keys.E);
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("&Delete Product", null, (s, e) => HandleDeleteProduct(), Keys.Delete);
            productsMenu.DropDownItems.AddRange(new ToolStripItem[] { addItem, editItem, deleteItem });

            // --- Commands Menu (Requirement 3: Dedicated Commands Menu) ---
            ToolStripMenuItem commandsMenu = new ToolStripMenuItem("&Commands") { ForeColor = Color.White };
            ToolStripMenuItem explorerItem = new ToolStripMenuItem("⚡ &Commands Explorer...", null, (s, e) => HandleOpenCommandsExplorer(), Keys.Control | Keys.Shift | Keys.C);
            ToolStripMenuItem terminalItem = new ToolStripMenuItem("💻 &Command Terminal (Direct CLI)...", null, (s, e) => ToggleTerminal(), Keys.Control | Keys.Oemtilde);
            ToolStripSeparator sepCmd1 = new ToolStripSeparator();
            ToolStripMenuItem refreshItem = new ToolStripMenuItem("🔄 &Refresh Inventory Data", null, (s, e) => HandleRefreshData(), Keys.F5);
            ToolStripMenuItem focusSearchItem = new ToolStripMenuItem("🔍 &Focus Search Box", null, (s, e) => HandleFocusSearch(), Keys.Control | Keys.F);
            ToolStripMenuItem clearSearchItem = new ToolStripMenuItem("✖ &Clear Search Filter", null, (s, e) => HandleClearSearch(), Keys.Control | Keys.Shift | Keys.X);
            ToolStripSeparator sepCmd2 = new ToolStripSeparator();
            ToolStripMenuItem navDashItem = new ToolStripMenuItem("📊 Switch to &Dashboard", null, (s, e) => HandleNavDashboard(), Keys.Control | Keys.D1);
            ToolStripMenuItem navProdItem = new ToolStripMenuItem("📋 Switch to &Products Catalog", null, (s, e) => HandleNavProducts(), Keys.Control | Keys.D2);
            ToolStripSeparator sepCmd3 = new ToolStripSeparator();
            ToolStripMenuItem filterLowItem = new ToolStripMenuItem("⚠ Quick Filter: &Low Stock Items", null, (s, e) => HandleFilterLowStock(), Keys.Control | Keys.Shift | Keys.L);
            ToolStripMenuItem filterOutItem = new ToolStripMenuItem("🚨 Quick Filter: &Out of Stock Items", null, (s, e) => HandleFilterOutOfStock(), Keys.Control | Keys.Shift | Keys.O);
            ToolStripMenuItem filterResetItem = new ToolStripMenuItem("📦 Quick Filter: &Show All Products", null, (s, e) => HandleResetFilter(), Keys.Control | Keys.Shift | Keys.A);

            commandsMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                explorerItem,
                terminalItem,
                sepCmd1,
                refreshItem,
                focusSearchItem,
                clearSearchItem,
                sepCmd2,
                navDashItem,
                navProdItem,
                sepCmd3,
                filterLowItem,
                filterOutItem,
                filterResetItem
            });

            // --- Help Menu ---
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("&Help") { ForeColor = Color.White };
            ToolStripMenuItem guideItem = new ToolStripMenuItem("📖 &User Guide & Shortcuts...", null, (s, e) => HandleUserGuide(), Keys.F1);
            ToolStripSeparator sepHelp = new ToolStripSeparator();
            ToolStripMenuItem aboutItem = new ToolStripMenuItem("&About System...", null, (s, e) => HandleAbout());
            helpMenu.DropDownItems.AddRange(new ToolStripItem[] { guideItem, sepHelp, aboutItem });

            _menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, productsMenu, commandsMenu, helpMenu });
        }

        // =================================================================
        // SIDEBAR NAVIGATION PANEL (Topic 3: Mouse & Selection)
        // =================================================================
        private void BuildSidebar()
        {
            _sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = DesignSystem.SidebarBackground,
                Padding = new Padding(12, 16, 12, 16)
            };

            // Brand Header inside Sidebar
            Panel brandPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.Transparent
            };

            Label lblBrandIcon = new Label
            {
                Text = "📦",
                Font = new Font("Segoe UI Emoji", 20f, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(8, 6),
                Size = new Size(38, 38)
            };

            Label lblBrandName = new Label
            {
                Text = "InventoryHub",
                Font = DesignSystem.FontAppTitle,
                ForeColor = Color.White,
                Location = new Point(48, 8),
                AutoSize = true
            };

            Label lblBrandTag = new Label
            {
                Text = "SQLite Desktop Edition",
                Font = DesignSystem.FontBadge,
                ForeColor = DesignSystem.MutedText,
                Location = new Point(50, 36),
                AutoSize = true
            };

            brandPanel.Controls.Add(lblBrandIcon);
            brandPanel.Controls.Add(lblBrandName);
            brandPanel.Controls.Add(lblBrandTag);

            // Navigation Buttons
            _btnNavDashboard = new ModernButton
            {
                Text = "📊   Dashboard",
                Type = ModernButton.ButtonType.SidebarNav,
                Dock = DockStyle.Top,
                Height = 44,
                IsActiveNav = true
            };
            _btnNavDashboard.Click += (s, e) => ShowView(isDashboard: true);

            _btnNavProducts = new ModernButton
            {
                Text = "📋   Products Catalog",
                Type = ModernButton.ButtonType.SidebarNav,
                Dock = DockStyle.Top,
                Height = 44,
                IsActiveNav = false
            };
            _btnNavProducts.Click += (s, e) => ShowView(isDashboard: false);

            _btnNavCommands = new ModernButton
            {
                Text = "⚡   Commands Explorer",
                Type = ModernButton.ButtonType.SidebarNav,
                Dock = DockStyle.Top,
                Height = 44,
                IsActiveNav = false
            };
            _btnNavCommands.Click += (s, e) => HandleOpenCommandsExplorer();

            _btnNavTerminal = new ModernButton
            {
                Text = "💻   Command Terminal",
                Type = ModernButton.ButtonType.SidebarNav,
                Dock = DockStyle.Top,
                Height = 44,
                IsActiveNav = false
            };
            _btnNavTerminal.Click += (s, e) => ToggleTerminal();

            _btnNavGuide = new ModernButton
            {
                Text = "📖   User Guide (F1)",
                Type = ModernButton.ButtonType.SidebarNav,
                Dock = DockStyle.Top,
                Height = 44,
                IsActiveNav = false
            };
            _btnNavGuide.Click += (s, e) => HandleUserGuide();

            _btnNavAbout = new ModernButton
            {
                Text = "ℹ️   About System",
                Type = ModernButton.ButtonType.SidebarNav,
                Dock = DockStyle.Top,
                Height = 44,
                IsActiveNav = false
            };
            _btnNavAbout.Click += (s, e) => HandleAbout();

            Panel spacer1 = new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Color.Transparent };
            Panel spacer2 = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = Color.Transparent };
            Panel spacerTerm = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = Color.Transparent };
            Panel spacerCmd = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = Color.Transparent };
            Panel spacer3 = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = Color.Transparent };

            // Bottom System Info in Sidebar
            Panel footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.Transparent
            };

            Label lblEnv = new Label
            {
                Text = ".NET 8 WinForms\nSQLite Persistent Storage",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.MutedText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft
            };
            footerPanel.Controls.Add(lblEnv);

            _sidebarPanel.Controls.Add(footerPanel);
            _sidebarPanel.Controls.Add(_btnNavAbout);
            _sidebarPanel.Controls.Add(spacer3);
            _sidebarPanel.Controls.Add(_btnNavGuide);
            _sidebarPanel.Controls.Add(spacer2);
            _sidebarPanel.Controls.Add(_btnNavTerminal);
            _sidebarPanel.Controls.Add(spacerTerm);
            _sidebarPanel.Controls.Add(_btnNavCommands);
            _sidebarPanel.Controls.Add(spacerCmd);
            _sidebarPanel.Controls.Add(_btnNavProducts);
            _sidebarPanel.Controls.Add(spacer1);
            _sidebarPanel.Controls.Add(_btnNavDashboard);
            _sidebarPanel.Controls.Add(brandPanel);
        }

        // =================================================================
        // TOP HEADER & LIVE SEARCH (Topic 1: Events & TextChanged)
        // =================================================================
        private void BuildHeader()
        {
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = DesignSystem.CardBackground,
                Padding = new Padding(24, 18, 24, 18)
            };

            _headerPanel.Paint += (s, e) =>
            {
                using Pen borderPen = new Pen(DesignSystem.BorderColor, 1);
                e.Graphics.DrawLine(borderPen, 0, _headerPanel.Height - 1, _headerPanel.Width, _headerPanel.Height - 1);
            };

            // Right Actions Container: Add Button + Search Box
            ModernButton btnAddHeader = new ModernButton
            {
                Text = "+ Add Product (Ctrl+N)",
                Type = ModernButton.ButtonType.Primary,
                Dock = DockStyle.Right,
                Width = 190
            };
            btnAddHeader.Click += (s, e) => HandleAddProduct();

            Panel headerSpacer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 16,
                BackColor = Color.Transparent
            };

            // Search Box with Modern Outline Container
            _searchBoxContainer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 280,
                BackColor = DesignSystem.InputBackground,
                Padding = new Padding(10, 10, 10, 8)
            };

            _searchBoxContainer.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using Pen pen = new Pen(_txtSearch.Focused ? DesignSystem.PrimaryBlue : DesignSystem.BorderColor, 1.5f);
                Rectangle rect = new Rectangle(0, 0, _searchBoxContainer.Width - 1, _searchBoxContainer.Height - 1);
                using GraphicsPath path = DesignSystem.CreateRoundedRectanglePath(rect, 6);
                e.Graphics.DrawPath(pen, path);
            };

            _txtSearch = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = DesignSystem.FontBody,
                ForeColor = DesignSystem.PrimaryText,
                BackColor = DesignSystem.InputBackground,
                Dock = DockStyle.Fill,
                PlaceholderText = "Search by Name or ID... (Ctrl+F)"
            };

            // Topic 1: TextChanged Live Search Event connected to SQLite
            _txtSearch.TextChanged += (s, e) => ApplySearchFilter();
            _txtSearch.Enter += (s, e) => _searchBoxContainer.Invalidate();
            _txtSearch.Leave += (s, e) => _searchBoxContainer.Invalidate();

            _searchBoxContainer.Controls.Add(_txtSearch);

            // Left Titles Container: Dock = Fill so text expands without overlap
            Panel titleContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            _lblHeaderTitle = new Label
            {
                Text = "Dashboard Overview",
                Font = DesignSystem.FontPageTitle,
                ForeColor = DesignSystem.PrimaryText,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            _lblHeaderSubtitle = new Label
            {
                Text = "Persistent stock monitoring, interactive GDI+ analytics, and SQLite catalog",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.SecondaryText,
                AutoSize = true,
                Location = new Point(2, 30)
            };

            titleContainer.Controls.Add(_lblHeaderTitle);
            titleContainer.Controls.Add(_lblHeaderSubtitle);

            // Adding order: Add Fill titleContainer first, then Right-docked search and buttons
            _headerPanel.Controls.Add(titleContainer);
            _headerPanel.Controls.Add(_searchBoxContainer);
            _headerPanel.Controls.Add(headerSpacer);
            _headerPanel.Controls.Add(btnAddHeader);
        }

        public void FocusSearchBox()
        {
            _txtSearch.Focus();
            _txtSearch.SelectAll();
        }
    }
}

