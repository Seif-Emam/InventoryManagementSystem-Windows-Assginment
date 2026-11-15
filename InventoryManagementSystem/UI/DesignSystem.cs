using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem.UI
{
    /// <summary>
    /// Centralized UI/UX Design System implementing modern SaaS dashboard aesthetics.
    /// Strictly adheres to specified color palette, typography, and component styling.
    /// </summary>
    public static class DesignSystem
    {
        // -------------------------------------------------------------
        // Primary Colors
        // -------------------------------------------------------------
        public static readonly Color PrimaryNavy   = ColorTranslator.FromHtml("#172554");
        public static readonly Color PrimaryBlue   = ColorTranslator.FromHtml("#2563EB");
        public static readonly Color PrimaryHover  = ColorTranslator.FromHtml("#1D4ED8");
        public static readonly Color LightBlue     = ColorTranslator.FromHtml("#EFF6FF");

        // -------------------------------------------------------------
        // Background & Surfaces
        // -------------------------------------------------------------
        public static readonly Color AppBackground   = ColorTranslator.FromHtml("#F1F5F9");
        public static readonly Color CardBackground  = ColorTranslator.FromHtml("#FFFFFF");
        public static readonly Color SidebarBackground = ColorTranslator.FromHtml("#172554");
        public static readonly Color InputBackground = ColorTranslator.FromHtml("#FFFFFF");
        public static readonly Color RowAlternate    = ColorTranslator.FromHtml("#F8FAFC");
        public static readonly Color RowSelection    = ColorTranslator.FromHtml("#DBEAFE");

        // -------------------------------------------------------------
        // Typography Colors
        // -------------------------------------------------------------
        public static readonly Color PrimaryText   = ColorTranslator.FromHtml("#0F172A");
        public static readonly Color SecondaryText = ColorTranslator.FromHtml("#64748B");
        public static readonly Color MutedText     = ColorTranslator.FromHtml("#94A3B8");
        public static readonly Color BorderColor   = ColorTranslator.FromHtml("#E2E8F0");

        // -------------------------------------------------------------
        // Stock Status Colors
        // -------------------------------------------------------------
        // Available
        public static readonly Color StatusAvailableFg  = ColorTranslator.FromHtml("#166534");
        public static readonly Color StatusAvailableBg  = ColorTranslator.FromHtml("#DCFCE7");
        public static readonly Color StatusAvailableBar = ColorTranslator.FromHtml("#16A34A");

        // Low Stock
        public static readonly Color StatusLowStockFg   = ColorTranslator.FromHtml("#92400E");
        public static readonly Color StatusLowStockBg   = ColorTranslator.FromHtml("#FEF3C7");
        public static readonly Color StatusLowStockBar  = ColorTranslator.FromHtml("#D97706");

        // Out of Stock
        public static readonly Color StatusOutOfStockFg  = ColorTranslator.FromHtml("#991B1B");
        public static readonly Color StatusOutOfStockBg  = ColorTranslator.FromHtml("#FEE2E2");
        public static readonly Color StatusOutOfStockBar = ColorTranslator.FromHtml("#DC2626");

        // -------------------------------------------------------------
        // Feedback Colors
        // -------------------------------------------------------------
        public static readonly Color SuccessColor = ColorTranslator.FromHtml("#16A34A");
        public static readonly Color WarningColor = ColorTranslator.FromHtml("#F59E0B");
        public static readonly Color ErrorColor   = ColorTranslator.FromHtml("#DC2626");
        public static readonly Color InfoColor    = ColorTranslator.FromHtml("#2563EB");

        // -------------------------------------------------------------
        // Typography (Segoe UI)
        // -------------------------------------------------------------
        public static readonly Font FontAppTitle    = new Font("Segoe UI", 18f, FontStyle.Bold); // ~24px
        public static readonly Font FontPageTitle   = new Font("Segoe UI", 16.5f, FontStyle.Bold); // ~22px
        public static readonly Font FontSectionHead = new Font("Segoe UI", 13.5f, FontStyle.Bold); // ~18px
        public static readonly Font FontStatNumber  = new Font("Segoe UI", 21f, FontStyle.Bold); // ~28px
        public static readonly Font FontBody        = new Font("Segoe UI", 10.5f, FontStyle.Regular); // ~14px
        public static readonly Font FontBodyBold    = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        public static readonly Font FontTableHeader = new Font("Segoe UI", 9.75f, FontStyle.Bold); // ~13px
        public static readonly Font FontSecondary   = new Font("Segoe UI", 9f, FontStyle.Regular); // ~12px
        public static readonly Font FontBadge       = new Font("Segoe UI", 9f, FontStyle.Bold);

        /// <summary>
        /// Retrieves the exact visual colors for a given stock status string.
        /// </summary>
        public static (Color Foreground, Color Background, Color Bar) GetStockColors(string? status)
        {
            if (string.Equals(status, "Out of Stock", StringComparison.OrdinalIgnoreCase))
                return (StatusOutOfStockFg, StatusOutOfStockBg, StatusOutOfStockBar);
            if (string.Equals(status, "Low Stock", StringComparison.OrdinalIgnoreCase))
                return (StatusLowStockFg, StatusLowStockBg, StatusLowStockBar);
            return (StatusAvailableFg, StatusAvailableBg, StatusAvailableBar);
        }

        /// <summary>
        /// Configures a DataGridView with modern SaaS styling.
        /// </summary>
        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = CardBackground;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = BorderColor;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoGenerateColumns = false;
            dgv.Font = FontBody;
            dgv.RowTemplate.Height = 44;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ShowCellToolTips = true;

            // Column Headers
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersHeight = 44;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = PrimaryNavy;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = FontTableHeader;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 12, 0);

            // Default Cell Styles
            dgv.DefaultCellStyle.BackColor = CardBackground;
            dgv.DefaultCellStyle.ForeColor = PrimaryText;
            dgv.DefaultCellStyle.SelectionBackColor = RowSelection;
            dgv.DefaultCellStyle.SelectionForeColor = PrimaryText;
            dgv.DefaultCellStyle.Font = FontBody;
            dgv.DefaultCellStyle.Padding = new Padding(12, 0, 12, 0);

            // Alternating Row Style
            dgv.AlternatingRowsDefaultCellStyle.BackColor = RowAlternate;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = PrimaryText;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = RowSelection;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = PrimaryText;
            dgv.AlternatingRowsDefaultCellStyle.Font = FontBody;
            dgv.AlternatingRowsDefaultCellStyle.Padding = new Padding(12, 0, 12, 0);
        }

        /// <summary>
        /// Paints a rounded badge / pill for stock status inside a DataGridView cell.
        /// </summary>
        public static void PaintStockBadge(Graphics g, Rectangle cellBounds, string status)
        {
            var (fg, bg, _) = GetStockColors(status);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Measure string
            SizeF textSize = g.MeasureString(status, FontBadge);
            int badgeWidth = (int)textSize.Width + 16;
            int badgeHeight = 24;
            int x = cellBounds.X + (cellBounds.Width - badgeWidth) / 2;
            int y = cellBounds.Y + (cellBounds.Height - badgeHeight) / 2;

            Rectangle badgeRect = new Rectangle(x, y, badgeWidth, badgeHeight);

            using (GraphicsPath path = CreateRoundedRectanglePath(badgeRect, 12))
            {
                using (SolidBrush brushBg = new SolidBrush(bg))
                {
                    g.FillPath(brushBg, path);
                }
            }

            using (SolidBrush brushText = new SolidBrush(fg))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(status, FontBadge, brushText, badgeRect, sf);
            }
        }

        /// <summary>
        /// Creates a rounded rectangle path for smooth modern UI components.
        /// </summary>
        public static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            if (rect.Width <= diameter || rect.Height <= diameter)
            {
                path.AddRectangle(rect);
                return path;
            }

            Rectangle arc = new Rectangle(rect.X, rect.Y, diameter, diameter);

            // Top-left
            path.AddArc(arc, 180, 90);

            // Top-right
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom-right
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom-left
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Paints a card border and background on a panel.
        /// </summary>
        public static void PaintCard(Graphics g, Rectangle clientRect, int radius = 8)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(clientRect.X, clientRect.Y, clientRect.Width - 1, clientRect.Height - 1);

            using (GraphicsPath path = CreateRoundedRectanglePath(rect, radius))
            {
                using (SolidBrush brush = new SolidBrush(CardBackground))
                {
                    g.FillPath(brush, path);
                }

                using (Pen pen = new Pen(BorderColor, 1))
                {
                    g.DrawPath(pen, path);
                }
            }
        }
    }
}

