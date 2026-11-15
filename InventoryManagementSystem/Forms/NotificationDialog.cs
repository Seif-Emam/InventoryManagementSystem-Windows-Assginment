using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.UI;

namespace InventoryManagementSystem.Forms
{
    public enum NotificationType
    {
        Success,
        Danger,
        Warning,
        Info
    }

    /// <summary>
    /// Professional modern SaaS modal dialog for confirmations (Delete Yes/No) and event notifications (Add, Edit, Delete).
    /// Replaces default MessageBox dialogs with branded, high-contrast, rounded card modals.
    /// Demonstrates Windows Programming Course Topics:
    /// - Topic 5: Forms (Modal dialog, DialogResult, FormStartPosition.CenterParent, Owner relationship)
    /// - Topic 2: Graphics and Drawing (GDI+ icon badges, rounded containers, custom borders)
    /// - Topic 1: Events and Message handling
    /// </summary>
    public class NotificationDialog : Form
    {
        public bool Confirmed { get; private set; } = false;

        private readonly NotificationType _type;
        private readonly string _dialogTitle;
        private readonly string _message;
        private readonly Product? _product;
        private readonly bool _isConfirmation;
        private readonly string _confirmText;
        private readonly string _cancelText;

        public NotificationDialog(
            NotificationType type,
            string title,
            string message,
            bool isConfirmation = false,
            string confirmText = "OK",
            string cancelText = "Cancel",
            Product? product = null)
        {
            _type = type;
            _dialogTitle = title;
            _message = message;
            _isConfirmation = isConfirmation;
            _confirmText = confirmText;
            _cancelText = cancelText;
            _product = product;

            InitializeCustomUI();
        }

        private void InitializeCustomUI()
        {
            Text = _dialogTitle;
            Size = new Size(520, _product != null ? 360 : 310);
            MinimumSize = Size;
            MaximumSize = Size;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = DesignSystem.CardBackground;
            Font = DesignSystem.FontBody;

            // 1. Icon Badge configuration
            var (iconText, iconBgColor, iconFgColor, accentColor) = GetBadgeStyle(_type);

            Panel iconBadge = new Panel
            {
                Location = new Point(28, 24),
                Size = new Size(48, 48),
                BackColor = iconBgColor
            };

            iconBadge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, iconBadge.Width - 1, iconBadge.Height - 1);
                using GraphicsPath path = DesignSystem.CreateRoundedRectanglePath(r, 12);
                using SolidBrush bg = new SolidBrush(iconBgColor);
                using Pen border = new Pen(iconFgColor, 1.2f);
                using SolidBrush textBrush = new SolidBrush(iconFgColor);

                e.Graphics.FillPath(bg, path);
                e.Graphics.DrawPath(border, path);

                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(iconText, new Font("Segoe UI", 16f, FontStyle.Bold), textBrush, r, sf);
            };

            // 2. Title and Message
            Label lblTitle = new Label
            {
                Text = _dialogTitle,
                Font = DesignSystem.FontSectionHead,
                ForeColor = DesignSystem.PrimaryText,
                Location = new Point(90, 22),
                AutoSize = true
            };

            Label lblMessage = new Label
            {
                Text = _message,
                Font = DesignSystem.FontBody,
                ForeColor = DesignSystem.SecondaryText,
                Location = new Point(90, 52),
                Size = new Size(390, 60)
            };

            Controls.Add(iconBadge);
            Controls.Add(lblTitle);
            Controls.Add(lblMessage);

            int currentY = 120;

            // 3. Optional Product Details Chip Card
            if (_product != null)
            {
                Panel productChip = new Panel
                {
                    Location = new Point(28, currentY),
                    Size = new Size(448, 54),
                    BackColor = DesignSystem.RowAlternate
                };

                productChip.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    Rectangle r = new Rectangle(0, 0, productChip.Width - 1, productChip.Height - 1);
                    using GraphicsPath p = DesignSystem.CreateRoundedRectanglePath(r, 8);
                    using SolidBrush b = new SolidBrush(DesignSystem.RowAlternate);
                    using Pen pen = new Pen(DesignSystem.BorderColor, 1);
                    e.Graphics.FillPath(b, p);
                    e.Graphics.DrawPath(pen, p);
                };

                Label lblProdName = new Label
                {
                    Text = $"📦 {_product.Name}",
                    Font = DesignSystem.FontBodyBold,
                    ForeColor = DesignSystem.PrimaryNavy,
                    Location = new Point(14, 10),
                    AutoSize = true
                };

                Label lblProdMeta = new Label
                {
                    Text = $"ID: #{_product.Id}   •   Price: ${_product.Price:F2}   •   Stock: {_product.Quantity} units ({_product.StockStatus})",
                    Font = DesignSystem.FontSecondary,
                    ForeColor = DesignSystem.SecondaryText,
                    Location = new Point(16, 30),
                    AutoSize = true
                };

                productChip.Controls.Add(lblProdName);
                productChip.Controls.Add(lblProdMeta);
                Controls.Add(productChip);

                currentY += 66;
            }

            // 4. Bottom Action Bar
            Panel bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 68,
                BackColor = DesignSystem.AppBackground,
                Padding = new Padding(24, 14, 24, 14)
            };

            bottomBar.Paint += (s, e) =>
            {
                using Pen borderPen = new Pen(DesignSystem.BorderColor, 1);
                e.Graphics.DrawLine(borderPen, 0, 0, bottomBar.Width, 0);
            };

            if (_isConfirmation)
            {
                // Two Buttons: "Cancel" / "No" and "Yes, Delete"
                ModernButton btnCancel = new ModernButton
                {
                    Text = _cancelText,
                    Type = ModernButton.ButtonType.Secondary,
                    Size = new Size(130, 40),
                    Location = new Point(bottomBar.Width - 280, 14),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    DialogResult = DialogResult.Cancel
                };
                btnCancel.Click += (s, e) =>
                {
                    Confirmed = false;
                    DialogResult = DialogResult.Cancel;
                    Close();
                };

                ModernButton btnConfirm = new ModernButton
                {
                    Text = _confirmText,
                    Type = _type == NotificationType.Danger ? ModernButton.ButtonType.Danger : ModernButton.ButtonType.Primary,
                    Size = new Size(130, 40),
                    Location = new Point(bottomBar.Width - 142, 14),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    DialogResult = DialogResult.OK
                };
                btnConfirm.Click += (s, e) =>
                {
                    Confirmed = true;
                    DialogResult = DialogResult.OK;
                    Close();
                };

                bottomBar.Controls.Add(btnCancel);
                bottomBar.Controls.Add(btnConfirm);
                CancelButton = btnCancel;
                AcceptButton = btnConfirm;
            }
            else
            {
                // Single "OK / Continue" Button
                ModernButton btnOk = new ModernButton
                {
                    Text = _confirmText,
                    Type = _type == NotificationType.Danger ? ModernButton.ButtonType.Danger : ModernButton.ButtonType.Primary,
                    Size = new Size(120, 40),
                    Location = new Point(bottomBar.Width - 144, 14),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    DialogResult = DialogResult.OK
                };
                btnOk.Click += (s, e) =>
                {
                    DialogResult = DialogResult.OK;
                    Close();
                };

                bottomBar.Controls.Add(btnOk);
                AcceptButton = btnOk;
            }

            Controls.Add(bottomBar);
        }

        private static (string IconText, Color BgColor, Color FgColor, Color AccentColor) GetBadgeStyle(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => ("✓", DesignSystem.StatusAvailableBg, DesignSystem.StatusAvailableFg, DesignSystem.SuccessColor),
                NotificationType.Danger  => ("🗑", DesignSystem.StatusOutOfStockBg, DesignSystem.StatusOutOfStockFg, DesignSystem.ErrorColor),
                NotificationType.Warning => ("⚠", DesignSystem.StatusLowStockBg, DesignSystem.StatusLowStockFg, DesignSystem.WarningColor),
                NotificationType.Info    => ("ℹ", DesignSystem.LightBlue, DesignSystem.PrimaryBlue, DesignSystem.PrimaryBlue),
                _                        => ("ℹ", DesignSystem.LightBlue, DesignSystem.PrimaryBlue, DesignSystem.PrimaryBlue)
            };
        }

        // =================================================================
        // STATIC HELPER METHODS FOR MODAL INVOCATION
        // =================================================================

        /// <summary>
        /// Shows a modern confirmation dialog (Yes / No) with danger badge and optional product card.
        /// </summary>
        public static bool ShowConfirmation(
            IWin32Window owner,
            string title,
            string message,
            string confirmText = "Yes, Delete",
            string cancelText = "No, Cancel",
            Product? product = null,
            NotificationType type = NotificationType.Danger)
        {
            using var dialog = new NotificationDialog(
                type,
                title,
                message,
                isConfirmation: true,
                confirmText: confirmText,
                cancelText: cancelText,
                product: product
            );

            var result = dialog.ShowDialog(owner);
            return result == DialogResult.OK && dialog.Confirmed;
        }

        /// <summary>
        /// Shows a modern success notification dialog after Add, Edit, or Delete actions.
        /// </summary>
        public static void ShowSuccess(
            IWin32Window owner,
            string title,
            string message,
            Product? product = null,
            string okButtonText = "Continue")
        {
            using var dialog = new NotificationDialog(
                NotificationType.Success,
                title,
                message,
                isConfirmation: false,
                confirmText: okButtonText,
                product: product
            );

            dialog.ShowDialog(owner);
        }

        /// <summary>
        /// Shows a modern warning modal dialog.
        /// </summary>
        public static void ShowWarning(
            IWin32Window owner,
            string title,
            string message)
        {
            using var dialog = new NotificationDialog(
                NotificationType.Warning,
                title,
                message,
                isConfirmation: false,
                confirmText: "Understood"
            );

            dialog.ShowDialog(owner);
        }

        /// <summary>
        /// Shows a modern error modal dialog.
        /// </summary>
        public static void ShowError(
            IWin32Window owner,
            string title,
            string message)
        {
            using var dialog = new NotificationDialog(
                NotificationType.Danger,
                title,
                message,
                isConfirmation: false,
                confirmText: "Dismiss"
            );

            dialog.ShowDialog(owner);
        }
    }
}

