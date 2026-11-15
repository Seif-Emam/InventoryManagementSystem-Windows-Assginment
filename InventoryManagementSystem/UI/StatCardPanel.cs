using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem.UI
{
    /// <summary>
    /// Modern SaaS Dashboard Stat Card.
    /// Displays key performance metrics with custom accent styling, distinct color indicators, and Segoe UI typography.
    /// </summary>
    public class StatCardPanel : Panel
    {
        private string _title = "METRIC";
        private string _value = "0";
        private string _badgeText = "Status";
        private Color _accentColor = DesignSystem.PrimaryBlue;
        private Color _badgeBgColor = DesignSystem.LightBlue;
        private Color _badgeFgColor = DesignSystem.PrimaryBlue;

        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        public string Value
        {
            get => _value;
            set { _value = value; Invalidate(); }
        }

        public string BadgeText
        {
            get => _badgeText;
            set { _badgeText = value; Invalidate(); }
        }

        public StatCardPanel(string title, Color accentColor, Color badgeBgColor, Color badgeFgColor, string badgeText)
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );

            _title = title;
            _accentColor = accentColor;
            _badgeBgColor = badgeBgColor;
            _badgeFgColor = badgeFgColor;
            _badgeText = badgeText;

            BackColor = DesignSystem.CardBackground;
            Padding = new Padding(20);
            Height = 110;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // 1. Draw Card Surface and Border
            DesignSystem.PaintCard(g, ClientRectangle, 8);

            // 2. Left Accent Indicator Strip
            Rectangle accentRect = new Rectangle(0, 0, 6, Height - 1);
            using (GraphicsPath accentPath = new GraphicsPath())
            {
                accentPath.AddArc(0, 0, 8, 8, 180, 90);
                accentPath.AddLine(4, 0, 4, Height - 1);
                accentPath.AddLine(4, Height - 1, 4, Height - 1);
                accentPath.AddArc(0, Height - 9, 8, 8, 90, 90);
                accentPath.CloseFigure();
                using (SolidBrush accentBrush = new SolidBrush(_accentColor))
                {
                    g.FillPath(accentBrush, accentPath);
                }
            }

            // 3. Draw Title (Uppercase secondary text)
            using (SolidBrush titleBrush = new SolidBrush(DesignSystem.SecondaryText))
            {
                g.DrawString(_title.ToUpperInvariant(), DesignSystem.FontSecondary, titleBrush, 22, 16);
            }

            // 4. Draw Metric Value (28px bold number)
            using (SolidBrush valBrush = new SolidBrush(DesignSystem.PrimaryText))
            {
                g.DrawString(_value, DesignSystem.FontStatNumber, valBrush, 20, 38);
            }

            // 5. Draw Badge / Visual Indicator Pill on the right
            if (!string.IsNullOrEmpty(_badgeText))
            {
                SizeF badgeSize = g.MeasureString(_badgeText, DesignSystem.FontBadge);
                int badgeW = (int)badgeSize.Width + 16;
                int badgeH = 24;
                int badgeX = Width - badgeW - 18;
                int badgeY = 16;

                Rectangle badgeRect = new Rectangle(badgeX, badgeY, badgeW, badgeH);
                using (GraphicsPath badgePath = DesignSystem.CreateRoundedRectanglePath(badgeRect, 12))
                {
                    using (SolidBrush bgBrush = new SolidBrush(_badgeBgColor))
                    {
                        g.FillPath(bgBrush, badgePath);
                    }
                }

                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                using (SolidBrush fgBrush = new SolidBrush(_badgeFgColor))
                {
                    g.DrawString(_badgeText, DesignSystem.FontBadge, fgBrush, badgeRect, sf);
                }
            }
        }
    }
}

