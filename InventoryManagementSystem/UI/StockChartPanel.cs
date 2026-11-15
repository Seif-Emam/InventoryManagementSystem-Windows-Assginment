using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.UI
{
    /// <summary>
    /// Custom GDI+ Stock Chart Control with interactive Hover and Click-to-Edit functionality.
    /// Demonstrates Windows Programming Course Topics:
    /// - Topic 2 (Graphics & Drawing): Panel.Paint, Graphics, FillRectangle, scaling, anti-aliasing
    /// - Topic 3 (Mouse Input): MouseMove hit testing, Cursors.Hand, ToolTips, MouseClick event triggering
    /// </summary>
    public class StockChartPanel : Panel
    {
        private List<Product> _products = new List<Product>();
        private readonly List<(Rectangle Bounds, Product Product)> _hitTargets = new List<(Rectangle, Product)>();
        private Product? _hoveredProduct = null;
        private readonly ToolTip _toolTip = new ToolTip();

        /// <summary>
        /// Event fired when the user clicks on any product bar in the chart to edit it.
        /// </summary>
        public event EventHandler<Product>? ProductEditRequested;

        public StockChartPanel()
        {
            // Enable double buffering and resize redraw to eliminate flicker and support smooth resizing
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
            UpdateStyles();

            BackColor = DesignSystem.CardBackground;
            Padding = new Padding(20);

            // Configure modern tooltip
            _toolTip.AutoPopDelay = 5000;
            _toolTip.InitialDelay = 150;
            _toolTip.ReshowDelay = 100;
            _toolTip.ShowAlways = true;

            // Hook up Paint & Mouse events
            Paint += StockChartPanel_Paint;
            MouseMove += StockChartPanel_MouseMove;
            MouseLeave += StockChartPanel_MouseLeave;
            MouseClick += StockChartPanel_MouseClick;
        }

        /// <summary>
        /// Updates the product data and triggers a repaint via GDI+.
        /// </summary>
        public void UpdateData(IEnumerable<Product> products)
        {
            _products = products.ToList();
            _hoveredProduct = null;
            Invalidate(); // Requests Windows to trigger Panel.Paint
        }

        private void StockChartPanel_MouseMove(object? sender, MouseEventArgs e)
        {
            var hit = _hitTargets.FirstOrDefault(t => t.Bounds.Contains(e.Location));

            if (hit.Product != null)
            {
                Cursor = Cursors.Hand;

                if (_hoveredProduct != hit.Product)
                {
                    _hoveredProduct = hit.Product;
                    string tip = $"{hit.Product.Name}\n" +
                                 $"Price: ${hit.Product.Price:F2}\n" +
                                 $"Stock Quantity: {hit.Product.Quantity} units\n" +
                                 $"Min Stock: {hit.Product.MinStock}\n" +
                                 $"Status: {hit.Product.StockStatus}\n\n" +
                                 $"💡 Click bar to edit product";
                    _toolTip.SetToolTip(this, tip);
                    Invalidate();
                }
            }
            else
            {
                Cursor = Cursors.Default;
                if (_hoveredProduct != null)
                {
                    _hoveredProduct = null;
                    _toolTip.RemoveAll();
                    Invalidate();
                }
            }
        }

        private void StockChartPanel_MouseLeave(object? sender, EventArgs e)
        {
            Cursor = Cursors.Default;
            if (_hoveredProduct != null)
            {
                _hoveredProduct = null;
                _toolTip.RemoveAll();
                Invalidate();
            }
        }

        private void StockChartPanel_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var hit = _hitTargets.FirstOrDefault(t => t.Bounds.Contains(e.Location));
                if (hit.Product != null)
                {
                    // Raise interactive edit event
                    ProductEditRequested?.Invoke(this, hit.Product);
                }
            }
        }

        private void StockChartPanel_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Configure High Quality Rendering
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle clientRect = ClientRectangle;
            _hitTargets.Clear();

            // 1. Draw Card Background & Outer Border
            DesignSystem.PaintCard(g, clientRect, 8);

            // 2. Chart Header (Title & Legend & Interactive Hint)
            int headerHeight = DrawChartHeader(g, clientRect);

            // 3. Define Plotting Area Bounds
            int marginLeft = 60;
            int marginRight = 40;
            int marginTop = headerHeight + 20;
            int marginBottom = 70;

            int plotX = marginLeft;
            int plotY = marginTop;
            int plotWidth = clientRect.Width - marginLeft - marginRight;
            int plotHeight = clientRect.Height - marginTop - marginBottom;

            if (plotWidth <= 20 || plotHeight <= 20)
                return;

            Rectangle plotArea = new Rectangle(plotX, plotY, plotWidth, plotHeight);

            // Handle Empty Collection
            if (_products.Count == 0)
            {
                DrawEmptyState(g, plotArea);
                return;
            }

            // 4. Calculate Max Value & Y-Axis Scale
            int maxStock = _products.Max(p => p.Quantity);
            // Ensure minimum scale ceiling of at least 10 and round up to a multiple of 5
            int yMax = Math.Max(10, ((maxStock / 5) + 1) * 5);

            // 5. Draw Y-Axis Gridlines and Numerical Scale
            DrawYAxisAndGrid(g, plotArea, yMax);

            // 6. Draw Bars, Quantities, Product Labels & Record Hit Targets
            DrawStockBars(g, plotArea, yMax);
        }

        private int DrawChartHeader(Graphics g, Rectangle rect)
        {
            int top = 16;
            int left = 20;

            // Chart Title with Interactive badge
            using (SolidBrush titleBrush = new SolidBrush(DesignSystem.PrimaryText))
            {
                g.DrawString("Stock Level Distribution", DesignSystem.FontSectionHead, titleBrush, left, top);
            }

            // Interactive Click-to-Edit Hint
            using (SolidBrush hintBrush = new SolidBrush(DesignSystem.PrimaryBlue))
            {
                g.DrawString("• Click any bar to edit", DesignSystem.FontSecondary, hintBrush, left + 215, top + 2);
            }

            // Chart Subtitle
            using (SolidBrush subBrush = new SolidBrush(DesignSystem.SecondaryText))
            {
                g.DrawString("Live stock quantities rendered via GDI+ Graphics.FillRectangle", DesignSystem.FontSecondary, subBrush, left, top + 24);
            }

            // Draw Legend in Top Right
            int legendRight = rect.Width - 24;
            int legendY = top + 8;

            DrawLegendItem(g, "Out of Stock", DesignSystem.StatusOutOfStockBar, ref legendRight, legendY);
            DrawLegendItem(g, "Low Stock", DesignSystem.StatusLowStockBar, ref legendRight, legendY);
            DrawLegendItem(g, "Available", DesignSystem.StatusAvailableBar, ref legendRight, legendY);

            return top + 44;
        }

        private void DrawLegendItem(Graphics g, string label, Color color, ref int rightX, int y)
        {
            SizeF textSize = g.MeasureString(label, DesignSystem.FontSecondary);
            int itemWidth = (int)textSize.Width + 24;
            int startX = rightX - itemWidth;

            // Color swatch
            using (SolidBrush brush = new SolidBrush(color))
            {
                Rectangle swatchRect = new Rectangle(startX, y + 2, 12, 12);
                using GraphicsPath path = DesignSystem.CreateRoundedRectanglePath(swatchRect, 3);
                g.FillPath(brush, path);
            }

            // Legend text
            using (SolidBrush textBrush = new SolidBrush(DesignSystem.SecondaryText))
            {
                g.DrawString(label, DesignSystem.FontSecondary, textBrush, startX + 16, y);
            }

            rightX = startX - 16;
        }

        private void DrawEmptyState(Graphics g, Rectangle plotArea)
        {
            string emptyMsg = "No products found in inventory";
            string hintMsg = "Add products using the '+ Add Product' button to visualize stock levels.";

            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            Rectangle textRect = new Rectangle(plotArea.X, plotArea.Y + (plotArea.Height / 2) - 20, plotArea.Width, 24);
            Rectangle hintRect = new Rectangle(plotArea.X, textRect.Bottom + 4, plotArea.Width, 20);

            using (SolidBrush brush = new SolidBrush(DesignSystem.MutedText))
            {
                g.DrawString(emptyMsg, DesignSystem.FontBodyBold, brush, textRect, sf);
                g.DrawString(hintMsg, DesignSystem.FontSecondary, brush, hintRect, sf);
            }
        }

        private void DrawYAxisAndGrid(Graphics g, Rectangle plotArea, int yMax)
        {
            int gridSteps = 4;
            using Pen gridPen = new Pen(DesignSystem.BorderColor, 1) { DashStyle = DashStyle.Dash };
            using Pen axisPen = new Pen(DesignSystem.BorderColor, 1.5f);
            using SolidBrush labelBrush = new SolidBrush(DesignSystem.MutedText);

            StringFormat sfRight = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center
            };

            for (int i = 0; i <= gridSteps; i++)
            {
                float ratio = (float)i / gridSteps;
                int y = plotArea.Bottom - (int)(ratio * plotArea.Height);
                int val = (int)(ratio * yMax);

                if (i > 0)
                {
                    g.DrawLine(gridPen, plotArea.Left, y, plotArea.Right, y);
                }

                Rectangle labelRect = new Rectangle(plotArea.Left - 50, y - 10, 42, 20);
                g.DrawString(val.ToString(), DesignSystem.FontSecondary, labelBrush, labelRect, sfRight);
            }

            g.DrawLine(axisPen, plotArea.Left, plotArea.Bottom, plotArea.Right, plotArea.Bottom);
        }

        private void DrawStockBars(Graphics g, Rectangle plotArea, int yMax)
        {
            int count = _products.Count;
            if (count == 0) return;

            float slotWidth = (float)plotArea.Width / count;
            float maxBarWidth = 56f;
            float minBarWidth = Math.Min(18f, Math.Max(4f, slotWidth * 0.75f));
            float barWidth = Math.Clamp(slotWidth * 0.55f, minBarWidth, maxBarWidth);

            StringFormat sfCenter = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            StringFormat sfNear = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near,
                Trimming = StringTrimming.EllipsisCharacter
            };

            for (int i = 0; i < count; i++)
            {
                Product product = _products[i];
                bool isHovered = (_hoveredProduct != null && _hoveredProduct.Id == product.Id);

                float centerX = plotArea.Left + (i + 0.5f) * slotWidth;
                float barX = centerX - (barWidth / 2f);

                // Calculate Bar Height
                float heightFraction = (float)product.Quantity / yMax;
                float barHeight = heightFraction * plotArea.Height;
                float barY = plotArea.Bottom - barHeight;

                var (_, _, barColor) = DesignSystem.GetStockColors(product.StockStatus);

                // Define interactive hit area for mouse clicks and hover
                Rectangle hitArea = new Rectangle(
                    (int)(centerX - slotWidth / 2f + 1),
                    plotArea.Top,
                    (int)slotWidth - 2,
                    plotArea.Height + 60
                );
                _hitTargets.Add((hitArea, product));

                // Draw Bar using Graphics.FillRectangle
                if (product.Quantity == 0)
                {
                    // Zero quantity (Out of Stock): baseline marker
                    int zeroMarkerHeight = 5;
                    int zeroMarkerY = plotArea.Bottom - zeroMarkerHeight;
                    using (SolidBrush zeroBrush = new SolidBrush(barColor))
                    {
                        g.FillRectangle(zeroBrush, barX, zeroMarkerY, barWidth, zeroMarkerHeight);
                    }

                    Rectangle zeroValRect = new Rectangle((int)barX - 10, zeroMarkerY - 22, (int)barWidth + 20, 18);
                    using (SolidBrush textBrush = new SolidBrush(barColor))
                    {
                        g.DrawString("0", DesignSystem.FontBadge, textBrush, zeroValRect, sfCenter);
                    }
                }
                else
                {
                    // Standard Bar with FillPath / FillRectangle
                    Rectangle barRect = new Rectangle((int)barX, (int)barY, (int)barWidth, (int)barHeight);
                    using (GraphicsPath barPath = DesignSystem.CreateRoundedRectanglePath(barRect, 4))
                    {
                        using (SolidBrush barBrush = new SolidBrush(barColor))
                        {
                            g.FillPath(barBrush, barPath);
                        }

                        // If hovered: draw subtle highlight outline
                        if (isHovered)
                        {
                            using Pen glowPen = new Pen(DesignSystem.PrimaryBlue, 2.5f);
                            g.DrawPath(glowPen, barPath);
                        }
                    }

                    // Value label above bar
                    Rectangle valRect = new Rectangle((int)barX - 10, (int)barY - 22, (int)barWidth + 20, 18);
                    using (SolidBrush valBrush = new SolidBrush(isHovered ? DesignSystem.PrimaryBlue : DesignSystem.PrimaryText))
                    {
                        g.DrawString(product.Quantity.ToString(), isHovered ? DesignSystem.FontBadge : DesignSystem.FontSecondary, valBrush, valRect, sfCenter);
                    }
                }

                // Hovered Indicator Pill: "✏ Edit" above bar
                if (isHovered)
                {
                    int editBadgeW = 50;
                    int editBadgeH = 20;
                    int editBadgeY = product.Quantity == 0 ? plotArea.Bottom - 44 : Math.Max(plotArea.Top, (int)barY - 44);
                    Rectangle editRect = new Rectangle((int)centerX - (editBadgeW / 2), editBadgeY, editBadgeW, editBadgeH);

                    using GraphicsPath editBadgePath = DesignSystem.CreateRoundedRectanglePath(editRect, 4);
                    using SolidBrush editBadgeBg = new SolidBrush(DesignSystem.PrimaryBlue);
                    using SolidBrush editBadgeFg = new SolidBrush(Color.White);

                    g.FillPath(editBadgeBg, editBadgePath);
                    g.DrawString("✏ Edit", DesignSystem.FontSecondary, editBadgeFg, editRect, sfCenter);
                }

                // X-Axis Product Name label
                Rectangle nameRect = new Rectangle((int)(centerX - slotWidth / 2f + 4), plotArea.Bottom + 8, (int)slotWidth - 8, 36);
                using (SolidBrush nameBrush = new SolidBrush(isHovered ? DesignSystem.PrimaryBlue : DesignSystem.PrimaryText))
                {
                    g.DrawString(product.Name, DesignSystem.FontSecondary, nameBrush, nameRect, sfNear);
                }

                // Status Mini Indicator under name
                Rectangle statusRect = new Rectangle((int)(centerX - slotWidth / 2f + 4), plotArea.Bottom + 46, (int)slotWidth - 8, 16);
                using (SolidBrush statusBrush = new SolidBrush(barColor))
                {
                    g.DrawString(product.StockStatus, DesignSystem.FontSecondary, statusBrush, statusRect, sfCenter);
                }
            }
        }
    }
}
