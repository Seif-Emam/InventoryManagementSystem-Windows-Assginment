using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InventoryManagementSystem.UI;

namespace InventoryManagementSystem.Forms
{
    /// <summary>
    /// User Guide and Keyboard Shortcuts Help Dialog.
    /// Provides clear instructions on how to use the Inventory Management System:
    /// - Keyboard shortcuts (Ctrl+N, Ctrl+F, Delete, F1)
    /// - How to Add, Edit, Search, and Delete products
    /// - How to interact with the GDI+ chart (click bars to edit)
    /// - Modern SaaS styling matching the application design system
    /// </summary>
    public class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeCustomUI();
        }

        private void InitializeCustomUI()
        {
            Text = "User Guide & Keyboard Shortcuts — InventoryHub";
            Size = new Size(680, 720);
            MinimumSize = new Size(680, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = DesignSystem.AppBackground;

            // Header Banner
            Panel banner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = DesignSystem.PrimaryNavy,
                Padding = new Padding(24, 16, 24, 16)
            };

            Label title = new Label
            {
                Text = "📖  InventoryHub User Guide & Shortcuts",
                Font = DesignSystem.FontPageTitle,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(24, 16)
            };

            Label subtitle = new Label
            {
                Text = "Master keyboard shortcuts, catalog management, and interactive chart features",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.MutedText,
                AutoSize = true,
                Location = new Point(28, 50)
            };

            banner.Controls.Add(title);
            banner.Controls.Add(subtitle);

            // Scrollable Content Panel
            Panel scrollContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesignSystem.AppBackground,
                AutoScroll = true,
                Padding = new Padding(24, 16, 24, 16)
            };

            int cardWidth = 612;

            // Card 1: Keyboard Shortcuts
            CardPanel shortcutsCard = CreateSectionCard(
                "⌨️  Keyboard Shortcuts",
                "Speed up your inventory management workflow with native desktop shortcuts:",
                cardWidth,
                260
            );

            AddShortcutRow(shortcutsCard, "Ctrl + N", "Add New Product", "Opens the product creation dialog instantly", 16, 56);
            AddShortcutRow(shortcutsCard, "Ctrl + ~ / F12", "Toggle Terminal", "Opens the direct interactive CLI command terminal", 16, 92);
            AddShortcutRow(shortcutsCard, "Ctrl + Shift + C", "Commands Explorer", "Opens the application commands explorer catalog", 16, 128);
            AddShortcutRow(shortcutsCard, "Ctrl + F", "Focus Search Bar", "Jumps directly to search box and highlights text", 16, 164);
            AddShortcutRow(shortcutsCard, "F5", "Refresh Database", "Reloads records from SQLite and updates cards & chart", 16, 200);
            AddShortcutRow(shortcutsCard, "Delete", "Delete Selected", "Prompts to delete the highlighted product", 16, 236);

            // Card 2: Adding & Editing Products
            CardPanel crudCard = CreateSectionCard(
                "📦  Managing Products (Add, Edit & Delete)",
                "Step-by-step instructions for product catalog operations:",
                cardWidth,
                240
            );

            AddGuideBullet(crudCard, "➕  How to Add a Product:",
                "Click the '+ Add Product' button in the top header or table toolbar (or press Ctrl+N). Fill in the product name, price, quantity, and minimum stock threshold. Validation guarantees positive values and auto-assigns a unique ID in SQLite.", 16, 54);

            AddGuideBullet(crudCard, "✏️  How to Edit a Product (4 Convenient Ways):",
                "1. Click the inline '✏ Edit' button directly in any row of the table.\n2. Select a row and click 'Edit Selected'.\n3. Double-click any product row in the table.\n4. Click directly on any bar in the Stock Level Chart!", 16, 114);

            AddGuideBullet(crudCard, "🗑️  How to Delete a Product:",
                "Select a product from the table and click 'Delete' or press the Delete key. A confirmation prompt prevents accidental deletion before removing the record from SQLite.", 16, 184);

            // Card 3: Direct Command Terminal (CLI Mode)
            CardPanel terminalCard = CreateSectionCard(
                "💻  Direct Command Terminal (CLI Mode — No Dialogs)",
                "Insert, edit, and delete products directly from the command line without opening windows:",
                cardWidth,
                240
            );

            AddGuideBullet(terminalCard, "➕  Add via Terminal:",
                "add --name \"Product Name\" --price 49.99 --stock 20 --min 5\nOr: add \"Product Name\" 49.99 20 5\nInserts directly into SQLite and refreshes tables, cards, and charts in real-time.", 16, 54);

            AddGuideBullet(terminalCard, "✏️  Edit via Terminal (Supports Partial Updates):",
                "edit <id> --price 34.99 --stock 15   (only changes specified fields!)\nOr: edit <id> \"New Name\" 29.99 10 2", 16, 120);

            AddGuideBullet(terminalCard, "🗑️  Delete & List via Terminal:",
                "delete <id>   (e.g. delete 5)   |   list [--low|--out]   |   search <query>", 16, 186);

            // Card 4: Searching & Live Filtering
            CardPanel searchCard = CreateSectionCard(
                "🔍  Live Search & Filtering",
                "Instant search query powered by SQLite pattern matching:",
                cardWidth,
                130
            );

            AddGuideBullet(searchCard, "Live Querying:",
                "Type into the search box in the top-right header (Ctrl+F). The system searches both Product Name (case-insensitive partial match) and Product ID in real-time. Clearing the search box restores the full catalog.", 16, 54);

            // Card 5: Interactive GDI+ Chart
            CardPanel chartCard = CreateSectionCard(
                "📊  Interactive GDI+ Stock Chart",
                "Clickable and responsive stock distribution visualization:",
                cardWidth,
                150
            );

            AddGuideBullet(chartCard, "Hover & Inspect:",
                "Move your mouse over any bar to view the exact product name, stock quantity, threshold, and status badge.", 16, 54);

            AddGuideBullet(chartCard, "💡 Click to Edit:",
                "Click directly on any bar or marker in the chart to immediately open the Edit Product dialog for that item. Changes save to SQLite and refresh the chart in real time!", 16, 96);

            // Add cards in top-docked flow
            Panel spacerBottom = new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Color.Transparent };
            Panel spc4 = new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Color.Transparent };
            Panel spc3 = new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Color.Transparent };
            Panel spc2 = new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Color.Transparent };
            Panel spc1 = new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Color.Transparent };

            scrollContent.Controls.Add(spacerBottom);
            scrollContent.Controls.Add(chartCard);
            scrollContent.Controls.Add(spc4);
            scrollContent.Controls.Add(searchCard);
            scrollContent.Controls.Add(spc3);
            scrollContent.Controls.Add(terminalCard);
            scrollContent.Controls.Add(spc2);
            scrollContent.Controls.Add(crudCard);
            scrollContent.Controls.Add(spc1);
            scrollContent.Controls.Add(shortcutsCard);

            // Bottom Action Bar
            Panel bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = DesignSystem.CardBackground,
                Padding = new Padding(24, 12, 24, 12)
            };

            bottomBar.Paint += (s, e) =>
            {
                using Pen borderPen = new Pen(DesignSystem.BorderColor, 1);
                e.Graphics.DrawLine(borderPen, 0, 0, bottomBar.Width, 0);
            };

            ModernButton btnClose = new ModernButton
            {
                Text = "Got It, Close Guide",
                Type = ModernButton.ButtonType.Primary,
                Size = new Size(160, 38),
                Location = new Point(bottomBar.Width - 184, 13),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                DialogResult = DialogResult.OK
            };
            btnClose.Click += (s, e) => Close();

            bottomBar.Controls.Add(btnClose);

            Controls.Add(scrollContent);
            Controls.Add(bottomBar);
            Controls.Add(banner);

            AcceptButton = btnClose;
        }

        private CardPanel CreateSectionCard(string title, string subtitle, int width, int height)
        {
            CardPanel card = new CardPanel
            {
                Dock = DockStyle.Top,
                Height = height,
                BackColor = DesignSystem.CardBackground,
                Padding = new Padding(20)
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = DesignSystem.FontSectionHead,
                ForeColor = DesignSystem.PrimaryText,
                Location = new Point(16, 12),
                AutoSize = true
            };

            Label lblSubtitle = new Label
            {
                Text = subtitle,
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.SecondaryText,
                Location = new Point(18, 34),
                AutoSize = true
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblSubtitle);
            return card;
        }

        private void AddShortcutRow(CardPanel card, string shortcut, string action, string desc, int x, int y)
        {
            // Pill Badge for Shortcut key
            Panel badge = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(90, 26),
                BackColor = DesignSystem.LightBlue
            };

            badge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using Pen p = new Pen(DesignSystem.PrimaryBlue, 1);
                Rectangle r = new Rectangle(0, 0, badge.Width - 1, badge.Height - 1);
                using GraphicsPath path = DesignSystem.CreateRoundedRectanglePath(r, 6);
                e.Graphics.DrawPath(p, path);

                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                using SolidBrush b = new SolidBrush(DesignSystem.PrimaryBlue);
                e.Graphics.DrawString(shortcut, DesignSystem.FontBadge, b, r, sf);
            };

            Label lblAction = new Label
            {
                Text = action,
                Font = DesignSystem.FontTableHeader,
                ForeColor = DesignSystem.PrimaryText,
                Location = new Point(x + 104, y + 4),
                AutoSize = true
            };

            Label lblDesc = new Label
            {
                Text = "— " + desc,
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.SecondaryText,
                Location = new Point(x + 240, y + 5),
                AutoSize = true
            };

            card.Controls.Add(badge);
            card.Controls.Add(lblAction);
            card.Controls.Add(lblDesc);
        }

        private void AddGuideBullet(CardPanel card, string heading, string details, int x, int y)
        {
            Label lblHead = new Label
            {
                Text = heading,
                Font = DesignSystem.FontTableHeader,
                ForeColor = DesignSystem.PrimaryNavy,
                Location = new Point(x, y),
                AutoSize = true
            };

            Label lblBody = new Label
            {
                Text = details,
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.PrimaryText,
                Location = new Point(x + 4, y + 20),
                Size = new Size(570, details.Contains('\n') ? 56 : 38)
            };

            card.Controls.Add(lblHead);
            card.Controls.Add(lblBody);
        }
    }
}

