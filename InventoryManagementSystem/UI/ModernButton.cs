using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventoryManagementSystem.UI
{
    /// <summary>
    /// Custom button control styled with modern SaaS appearance, smooth hover states,
    /// rounded corners, and anti-aliased GDI+ rendering.
    /// </summary>
    public class ModernButton : Button
    {
        public enum ButtonType
        {
            Primary,
            Secondary,
            Danger,
            SidebarNav
        }

        private ButtonType _buttonType = ButtonType.Primary;
        private bool _isHovered = false;
        private bool _isActiveNav = false;

        public ButtonType Type
        {
            get => _buttonType;
            set
            {
                _buttonType = value;
                Invalidate();
            }
        }

        public bool IsActiveNav
        {
            get => _isActiveNav;
            set
            {
                _isActiveNav = value;
                Invalidate();
            }
        }

        public ModernButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = DesignSystem.FontBodyBold;
            Cursor = Cursors.Hand;
            Height = 40;
            Padding = new Padding(16, 0, 16, 0);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            Color bgColor;
            Color textColor;
            Color borderColor = Color.Transparent;

            switch (_buttonType)
            {
                case ButtonType.Primary:
                    bgColor = _isHovered ? DesignSystem.PrimaryHover : DesignSystem.PrimaryBlue;
                    textColor = Color.White;
                    break;

                case ButtonType.Secondary:
                    bgColor = _isHovered ? DesignSystem.LightBlue : DesignSystem.CardBackground;
                    textColor = DesignSystem.PrimaryText;
                    borderColor = DesignSystem.BorderColor;
                    break;

                case ButtonType.Danger:
                    bgColor = _isHovered ? ColorTranslator.FromHtml("#B91C1C") : DesignSystem.ErrorColor;
                    textColor = Color.White;
                    break;

                case ButtonType.SidebarNav:
                    if (_isActiveNav)
                    {
                        bgColor = DesignSystem.PrimaryBlue;
                        textColor = Color.White;
                    }
                    else
                    {
                        bgColor = _isHovered ? ColorTranslator.FromHtml("#1E293B") : DesignSystem.SidebarBackground;
                        textColor = _isHovered ? Color.White : DesignSystem.MutedText;
                    }
                    break;

                default:
                    bgColor = DesignSystem.PrimaryBlue;
                    textColor = Color.White;
                    break;
            }

            int radius = _buttonType == ButtonType.SidebarNav ? 6 : 8;
            using (GraphicsPath path = DesignSystem.CreateRoundedRectanglePath(rect, radius))
            {
                using (SolidBrush brush = new SolidBrush(bgColor))
                {
                    g.FillPath(brush, path);
                }

                if (borderColor != Color.Transparent)
                {
                    using (Pen pen = new Pen(borderColor, 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            // Draw Text
            StringFormat sf = new StringFormat
            {
                Alignment = _buttonType == ButtonType.SidebarNav ? StringAlignment.Near : StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            Rectangle textRect = _buttonType == ButtonType.SidebarNav
                ? new Rectangle(20, 0, Width - 24, Height)
                : new Rectangle(0, 0, Width, Height);

            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                g.DrawString(Text, Font, textBrush, textRect, sf);
            }
        }
    }
}

