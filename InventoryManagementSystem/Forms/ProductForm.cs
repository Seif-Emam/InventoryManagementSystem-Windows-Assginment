using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.UI;

namespace InventoryManagementSystem.Forms
{
    /// <summary>
    /// Modal dialog for adding and editing products.
    /// Demonstrates Windows Programming Course Topics:
    /// - Forms (Topic 5): Modal dialog, DialogResult, ownership, FormStartPosition.CenterParent
    /// - Events & Messages (Topic 1): Validation, Button clicks, custom modal notifications
    /// - Modern UI/UX SaaS design system
    /// </summary>
    public class ProductForm : Form
    {
        public Product CurrentProduct { get; private set; }
        public bool IsEditMode { get; }

        private TextBox _txtName = null!;
        private TextBox _txtPrice = null!;
        private TextBox _txtQuantity = null!;
        private TextBox _txtMinStock = null!;
        private Label _lblValidationMessage = null!;
        private ModernButton _btnSave = null!;
        private ModernButton _btnCancel = null!;

        /// <summary>
        /// Constructor for Add New Product mode.
        /// </summary>
        public ProductForm()
        {
            IsEditMode = false;
            CurrentProduct = new Product();
            InitializeCustomUI();
        }

        /// <summary>
        /// Constructor for Edit Existing Product mode.
        /// </summary>
        public ProductForm(Product existingProduct)
        {
            IsEditMode = true;
            // Clone so modifications are only applied when saved
            CurrentProduct = existingProduct.Clone();
            InitializeCustomUI();
            PopulateFields();
        }

        private void InitializeCustomUI()
        {
            Text = IsEditMode ? "Edit Product — InventoryHub" : "Add New Product — InventoryHub";
            Size = new Size(520, 620);
            MinimumSize = new Size(520, 620);
            MaximumSize = new Size(520, 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = DesignSystem.AppBackground;
            Font = DesignSystem.FontBody;

            // Header Panel
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = DesignSystem.PrimaryNavy,
                Padding = new Padding(24, 16, 24, 16)
            };

            Label lblHeaderTitle = new Label
            {
                Text = IsEditMode ? "Edit Product" : "Add New Product",
                Font = DesignSystem.FontPageTitle,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(24, 14)
            };

            Label lblHeaderSubtitle = new Label
            {
                Text = IsEditMode
                    ? $"Updating ID #{CurrentProduct.Id} in inventory records"
                    : "Enter product details to register into the inventory system",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.MutedText,
                AutoSize = true,
                Location = new Point(25, 48)
            };

            headerPanel.Controls.Add(lblHeaderTitle);
            headerPanel.Controls.Add(lblHeaderSubtitle);

            // Main Card Container Panel
            CardPanel formCard = new CardPanel
            {
                Location = new Point(24, 100),
                Size = new Size(456, 380),
                BackColor = DesignSystem.CardBackground
            };

            int topOffset = 18;
            int labelWidth = 416;
            int inputHeight = 36;
            int vSpacing = 14;

            // 1. Product Name
            Label lblName = CreateFieldLabel("Product Name *", 20, topOffset);
            topOffset += 24;
            var (nameContainer, txtName) = CreateInputBox(20, topOffset, labelWidth, inputHeight);
            _txtName = txtName;
            formCard.Controls.Add(lblName);
            formCard.Controls.Add(nameContainer);
            topOffset += inputHeight + vSpacing;

            // 2. Price
            Label lblPrice = CreateFieldLabel("Price ($) *", 20, topOffset);
            topOffset += 24;
            var (priceContainer, txtPrice) = CreateInputBox(20, topOffset, labelWidth, inputHeight);
            _txtPrice = txtPrice;
            formCard.Controls.Add(lblPrice);
            formCard.Controls.Add(priceContainer);
            topOffset += inputHeight + vSpacing;

            // 3. Quantity
            Label lblQuantity = CreateFieldLabel("Initial Stock Quantity *", 20, topOffset);
            topOffset += 24;
            var (qtyContainer, txtQuantity) = CreateInputBox(20, topOffset, labelWidth, inputHeight);
            _txtQuantity = txtQuantity;
            formCard.Controls.Add(lblQuantity);
            formCard.Controls.Add(qtyContainer);
            topOffset += inputHeight + vSpacing;

            // 4. Minimum Stock Level
            Label lblMinStock = CreateFieldLabel("Minimum Stock Threshold *", 20, topOffset);
            topOffset += 24;
            var (minStockContainer, txtMinStock) = CreateInputBox(20, topOffset, labelWidth, inputHeight);
            _txtMinStock = txtMinStock;
            formCard.Controls.Add(lblMinStock);
            formCard.Controls.Add(minStockContainer);
            topOffset += inputHeight + 10;

            // Inline Validation Label
            _lblValidationMessage = new Label
            {
                Text = string.Empty,
                ForeColor = DesignSystem.ErrorColor,
                Font = DesignSystem.FontSecondary,
                Location = new Point(20, topOffset),
                Size = new Size(labelWidth, 24),
                Visible = false
            };
            formCard.Controls.Add(_lblValidationMessage);

            // Bottom Action Bar Panel
            Panel bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72,
                BackColor = DesignSystem.CardBackground,
                Padding = new Padding(24, 16, 24, 16)
            };

            bottomBar.Paint += (s, e) =>
            {
                using Pen borderPen = new Pen(DesignSystem.BorderColor, 1);
                e.Graphics.DrawLine(borderPen, 0, 0, bottomBar.Width, 0);
            };

            _btnCancel = new ModernButton
            {
                Text = "Cancel",
                Type = ModernButton.ButtonType.Secondary,
                Size = new Size(110, 40),
                Location = new Point(236, 16),
                DialogResult = DialogResult.Cancel
            };
            _btnCancel.Click += (s, e) => { Close(); };

            _btnSave = new ModernButton
            {
                Text = IsEditMode ? "Save Changes" : "Add Product",
                Type = ModernButton.ButtonType.Primary,
                Size = new Size(130, 40),
                Location = new Point(354, 16)
            };
            _btnSave.Click += OnSaveClicked;

            bottomBar.Controls.Add(_btnCancel);
            bottomBar.Controls.Add(_btnSave);

            // Form-level controls
            Controls.Add(formCard);
            Controls.Add(bottomBar);
            Controls.Add(headerPanel);

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;
        }

        private Label CreateFieldLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = DesignSystem.FontTableHeader,
                ForeColor = DesignSystem.PrimaryText,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private (Panel Container, TextBox Input) CreateInputBox(int x, int y, int width, int height)
        {
            Panel container = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = DesignSystem.InputBackground,
                Padding = new Padding(8, 8, 8, 8)
            };

            TextBox textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = DesignSystem.FontBody,
                ForeColor = DesignSystem.PrimaryText,
                BackColor = DesignSystem.InputBackground,
                Dock = DockStyle.Fill
            };

            container.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using Pen pen = new Pen(textBox.Focused ? DesignSystem.PrimaryBlue : DesignSystem.BorderColor, 1.5f);
                Rectangle rect = new Rectangle(0, 0, container.Width - 1, container.Height - 1);
                using GraphicsPath path = DesignSystem.CreateRoundedRectanglePath(rect, 6);
                e.Graphics.DrawPath(pen, path);
            };

            textBox.Enter += (s, e) => container.Invalidate();
            textBox.Leave += (s, e) => container.Invalidate();

            container.Controls.Add(textBox);
            return (container, textBox);
        }

        private void PopulateFields()
        {
            _txtName.Text = CurrentProduct.Name;
            _txtPrice.Text = CurrentProduct.Price.ToString("F2", CultureInfo.InvariantCulture);
            _txtQuantity.Text = CurrentProduct.Quantity.ToString();
            _txtMinStock.Text = CurrentProduct.MinStock.ToString();
        }

        /// <summary>
        /// Handles Save Click event: Validates inputs according to business rules.
        /// Demonstrates Windows Programming Course Topic 1 (Events & Messages).
        /// </summary>
        private void OnSaveClicked(object? sender, EventArgs e)
        {
            string name = _txtName.Text.Trim();
            string priceText = _txtPrice.Text.Trim();
            string qtyText = _txtQuantity.Text.Trim();
            string minStockText = _txtMinStock.Text.Trim();

            // 1. Validation: Product name cannot be empty
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowValidationError("Product name cannot be empty.", _txtName);
                return;
            }

            // 2. Validation: Price must be valid numeric value >= 0
            if (!decimal.TryParse(priceText, NumberStyles.Number | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal price) &&
                !decimal.TryParse(priceText, out price))
            {
                ShowValidationError("Price must be a valid numeric value.", _txtPrice);
                return;
            }

            if (price < 0)
            {
                ShowValidationError("Price cannot be negative.", _txtPrice);
                return;
            }

            // 3. Validation: Quantity must be valid integer >= 0
            if (!int.TryParse(qtyText, out int quantity))
            {
                ShowValidationError("Quantity must be a valid integer.", _txtQuantity);
                return;
            }

            if (quantity < 0)
            {
                ShowValidationError("Quantity cannot be negative.", _txtQuantity);
                return;
            }

            // 4. Validation: MinStock must be valid integer >= 0
            if (!int.TryParse(minStockText, out int minStock))
            {
                ShowValidationError("Minimum Stock must be a valid integer.", _txtMinStock);
                return;
            }

            if (minStock < 0)
            {
                ShowValidationError("Minimum Stock cannot be negative.", _txtMinStock);
                return;
            }

            // Validation passed
            _lblValidationMessage.Visible = false;

            CurrentProduct.Name = name;
            CurrentProduct.Price = price;
            CurrentProduct.Quantity = quantity;
            CurrentProduct.MinStock = minStock;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ShowValidationError(string message, TextBox targetBox)
        {
            _lblValidationMessage.Text = "⚠ " + message;
            _lblValidationMessage.Visible = true;
            targetBox.Focus();
            targetBox.SelectAll();

            // Modern pro warning dialog
            NotificationDialog.ShowWarning(
                this,
                "Validation Error",
                message
            );
        }
    }
}
