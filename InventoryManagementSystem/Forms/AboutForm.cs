using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.UI;

namespace InventoryManagementSystem.Forms
{
    /// <summary>
    /// Modern SaaS About dialog showcasing the academic Windows Programming project details.
    /// Demonstrates Topic 5 (Forms) and Topic 4 (Help -> About Menu Command).
    /// </summary>
    public class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeCustomUI();
        }

        private void InitializeCustomUI()
        {
            Text = "About InventoryHub";
            Size = new Size(540, 540);
            MinimumSize = new Size(540, 540);
            MaximumSize = new Size(540, 540);
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
                Height = 110,
                BackColor = DesignSystem.PrimaryNavy,
                Padding = new Padding(24, 20, 24, 20)
            };

            Label title = new Label
            {
                Text = "📦 InventoryHub Desktop",
                Font = DesignSystem.FontAppTitle,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(24, 16)
            };

            Label subtitle = new Label
            {
                Text = "Windows Programming Assignment • .NET 8 / WinForms / GDI+ / SQLite",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.MutedText,
                AutoSize = true,
                Location = new Point(28, 54)
            };

            banner.Controls.Add(title);
            banner.Controls.Add(subtitle);

            // Card Panel with Details
            CardPanel contentCard = new CardPanel
            {
                Location = new Point(24, 126),
                Size = new Size(476, 310),
                BackColor = DesignSystem.CardBackground
            };

            Label lblCourseHeading = new Label
            {
                Text = "Demonstrated Windows Programming Core Topics:",
                Font = DesignSystem.FontTableHeader,
                ForeColor = DesignSystem.PrimaryText,
                Location = new Point(16, 14),
                AutoSize = true
            };

            string topicText =
                "✓ Topic 1: Events, Messages & Event Handling (Click, TextChanged, Confirmations)\n" +
                "✓ Topic 2: Graphics & Drawing (Panel.Paint, Graphics, FillRectangle, GDI+ Chart)\n" +
                "✓ Topic 3: Keyboard, Mouse & Input (Ctrl+N, Ctrl+F, Delete, DGV Mouse Selection)\n" +
                "✓ Topic 4: Menus, Commands & Resources (MenuStrip: File, Products, Help)\n" +
                "✓ Topic 5: Forms (MainForm, ProductForm Dialog, MessageBox Modals)";

            Label lblTopics = new Label
            {
                Text = topicText,
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.SecondaryText,
                Location = new Point(16, 42),
                Size = new Size(440, 96)
            };

            Label lblArchitectureHeading = new Label
            {
                Text = "Technology & Data Architecture:",
                Font = DesignSystem.FontTableHeader,
                ForeColor = DesignSystem.PrimaryText,
                Location = new Point(16, 144),
                AutoSize = true
            };

            string archText =
                "• Database: SQLite in %LOCALAPPDATA%/InventoryManagementSystem/\n" +
                "• Idempotent Seeding: 25 realistic computer & office products seeded once\n" +
                "• Dynamic Stock Rules: Available, Low Stock, and Out of Stock\n" +
                "• Visualization: Real-time custom GDI+ stock distribution bar chart\n" +
                "• Modern SaaS UI/UX: Segoe UI typography, custom palettes, styled DataGridView";

            Label lblArch = new Label
            {
                Text = archText,
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.SecondaryText,
                Location = new Point(16, 170),
                Size = new Size(440, 110)
            };

            contentCard.Controls.Add(lblCourseHeading);
            contentCard.Controls.Add(lblTopics);
            contentCard.Controls.Add(lblArchitectureHeading);
            contentCard.Controls.Add(lblArch);

            // Close button
            ModernButton btnClose = new ModernButton
            {
                Text = "Close",
                Type = ModernButton.ButtonType.Primary,
                Size = new Size(110, 38),
                Location = new Point(390, 448),
                DialogResult = DialogResult.OK
            };
            btnClose.Click += (s, e) => Close();

            Controls.Add(btnClose);
            Controls.Add(contentCard);
            Controls.Add(banner);

            AcceptButton = btnClose;
        }
    }
}
