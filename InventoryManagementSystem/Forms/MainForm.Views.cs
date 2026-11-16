using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.UI;

namespace InventoryManagementSystem.Forms
{
    public partial class MainForm
    {
        // =================================================================
        // VIEW 1: DASHBOARD VIEW (Cards + Interactive GDI+ Chart + Overview Table with Scrolling)
        // =================================================================
        private void BuildDashboardView()
        {
            _dashboardView = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesignSystem.AppBackground,
                AutoScroll = true
            };

            // Enable mouse-wheel scrolling anywhere on dashboard
            _dashboardView.MouseEnter += (s, e) =>
            {
                if (!_dashboardView.Focused)
                {
                    _dashboardView.Focus();
                }
            };

            // 1. Top Section: 3 Dynamic Dashboard Cards (Section 6)
            TableLayoutPanel cardsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 114,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 16)
            };
            cardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            cardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            cardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            _cardTotal = new StatCardPanel("Total Products", DesignSystem.PrimaryBlue, DesignSystem.LightBlue, DesignSystem.PrimaryBlue, "Catalog Size") { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 8, 0) };
            _cardLowStock = new StatCardPanel("Low Stock", DesignSystem.StatusLowStockBar, DesignSystem.StatusLowStockBg, DesignSystem.StatusLowStockFg, "Threshold Alert") { Dock = DockStyle.Fill, Margin = new Padding(8, 0, 8, 0) };
            _cardOutOfStock = new StatCardPanel("Out of Stock", DesignSystem.StatusOutOfStockBar, DesignSystem.StatusOutOfStockBg, DesignSystem.StatusOutOfStockFg, "Restock Needed") { Dock = DockStyle.Fill, Margin = new Padding(8, 0, 0, 0) };

            cardsLayout.Controls.Add(_cardTotal, 0, 0);
            cardsLayout.Controls.Add(_cardLowStock, 1, 0);
            cardsLayout.Controls.Add(_cardOutOfStock, 2, 0);

            Panel gapPanel = new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Color.Transparent };

            // 2. Middle Section: Interactive Custom GDI+ Chart (Topic 2: Graphics and Drawing)
            _chartPanel = new StockChartPanel
            {
                Dock = DockStyle.Top,
                Height = 310
            };

            // Feature: Chart Click-to-Edit interaction!
            _chartPanel.ProductEditRequested += (s, product) => OpenEditDialog(product);

            Panel gapPanel2 = new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Color.Transparent };

            // 3. Bottom Section: Quick DataGridView in Dashboard
            CardPanel tableCard = new CardPanel
            {
                Dock = DockStyle.Top,
                Height = 480,
                Padding = new Padding(16)
            };

            Panel tableHeaderPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 2, 0, 6)
            };

            Label lblTableTitle = new Label
            {
                Text = "Product Inventory Overview",
                Font = DesignSystem.FontSectionHead,
                ForeColor = DesignSystem.PrimaryText,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            _lblDashboardProductsCount = new Label
            {
                Text = "Loading products...",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.SecondaryText,
                Dock = DockStyle.Left,
                Padding = new Padding(16, 4, 0, 0),
                AutoSize = true
            };

            ModernButton btnDeleteQuick = new ModernButton
            {
                Text = "Delete",
                Type = ModernButton.ButtonType.Danger,
                Dock = DockStyle.Right,
                Width = 90
            };
            btnDeleteQuick.Click += (s, e) => HandleDeleteProduct();

            Panel dashActionSpacer = new Panel { Dock = DockStyle.Right, Width = 8, BackColor = Color.Transparent };

            ModernButton btnEditQuick = new ModernButton
            {
                Text = "Edit Selected",
                Type = ModernButton.ButtonType.Secondary,
                Dock = DockStyle.Right,
                Width = 120
            };
            btnEditQuick.Click += (s, e) => HandleEditProduct();

            tableHeaderPanel.Controls.Add(lblTableTitle);
            tableHeaderPanel.Controls.Add(_lblDashboardProductsCount);
            tableHeaderPanel.Controls.Add(btnEditQuick);
            tableHeaderPanel.Controls.Add(dashActionSpacer);
            tableHeaderPanel.Controls.Add(btnDeleteQuick);

            _dgvDashboardProducts = CreateConfiguredDataGridView();

            tableCard.Controls.Add(_dgvDashboardProducts);
            tableCard.Controls.Add(tableHeaderPanel);

            Panel bottomSpacer = new Panel { Dock = DockStyle.Top, Height = 24, BackColor = Color.Transparent };

            // Adding order for Top docked elements: bottom-most element added first
            _dashboardView.Controls.Add(bottomSpacer);
            _dashboardView.Controls.Add(tableCard);
            _dashboardView.Controls.Add(gapPanel2);
            _dashboardView.Controls.Add(_chartPanel);
            _dashboardView.Controls.Add(gapPanel);
            _dashboardView.Controls.Add(cardsLayout);

            // Recursively attach scroll hover to children so mouse wheel scrolls smoothly
            AttachScrollHover(_dashboardView);
        }

        private void AttachScrollHover(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is not DataGridView && child is not TextBox)
                {
                    child.MouseEnter += (s, e) =>
                    {
                        if (!_dashboardView.Focused)
                        {
                            _dashboardView.Focus();
                        }
                    };
                    if (child.HasChildren)
                    {
                        AttachScrollHover(child);
                    }
                }
            }
        }

        // =================================================================
        // VIEW 2: PRODUCTS VIEW (Dedicated Full-Height Management Table)
        // =================================================================
        private void BuildProductsView()
        {
            _productsView = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesignSystem.AppBackground,
                Visible = false
            };

            CardPanel fullTableCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16)
            };

            // Action Toolbar inside Products View
            Panel toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 2, 0, 6)
            };

            Label lblViewTitle = new Label
            {
                Text = "Product Catalog Management",
                Font = DesignSystem.FontSectionHead,
                ForeColor = DesignSystem.PrimaryText,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            _lblProductsCount = new Label
            {
                Text = "Loading products...",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.SecondaryText,
                Dock = DockStyle.Left,
                Padding = new Padding(16, 4, 0, 0),
                AutoSize = true
            };

            ModernButton btnDeleteProduct = new ModernButton
            {
                Text = "🗑 Delete",
                Type = ModernButton.ButtonType.Danger,
                Dock = DockStyle.Right,
                Width = 100
            };
            btnDeleteProduct.Click += (s, e) => HandleDeleteProduct();

            Panel spc1 = new Panel { Dock = DockStyle.Right, Width = 8, BackColor = Color.Transparent };

            ModernButton btnEditProduct = new ModernButton
            {
                Text = "✏ Edit Product",
                Type = ModernButton.ButtonType.Secondary,
                Dock = DockStyle.Right,
                Width = 125
            };
            btnEditProduct.Click += (s, e) => HandleEditProduct();

            Panel spc2 = new Panel { Dock = DockStyle.Right, Width = 8, BackColor = Color.Transparent };

            ModernButton btnAddProduct = new ModernButton
            {
                Text = "+ Add Product",
                Type = ModernButton.ButtonType.Primary,
                Dock = DockStyle.Right,
                Width = 135
            };
            btnAddProduct.Click += (s, e) => HandleAddProduct();

            toolbar.Controls.Add(lblViewTitle);
            toolbar.Controls.Add(_lblProductsCount);
            toolbar.Controls.Add(btnAddProduct);
            toolbar.Controls.Add(spc2);
            toolbar.Controls.Add(btnEditProduct);
            toolbar.Controls.Add(spc1);
            toolbar.Controls.Add(btnDeleteProduct);

            _dgvProducts = CreateConfiguredDataGridView();

            fullTableCard.Controls.Add(_dgvProducts);
            fullTableCard.Controls.Add(toolbar);

            _productsView.Controls.Add(fullTableCard);
        }

        // =================================================================
        // DATAGRIDVIEW SETUP & CUSTOM BADGE & INLINE EDIT BUTTON
        // =================================================================
        private DataGridView CreateConfiguredDataGridView()
        {
            DataGridView dgv = new DataGridView
            {
                Dock = DockStyle.Fill
            };

            DesignSystem.StyleDataGridView(dgv);

            // Columns definition
            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn
            {
                Name = "Id",
                DataPropertyName = "Id",
                HeaderText = "ID",
                Width = 65,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };

            DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn
            {
                Name = "Name",
                DataPropertyName = "Name",
                HeaderText = "Product Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 180,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            };

            DataGridViewTextBoxColumn colPrice = new DataGridViewTextBoxColumn
            {
                Name = "Price",
                DataPropertyName = "Price",
                HeaderText = "Price",
                Width = 110,
                DefaultCellStyle =
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "$#,##0.00"
                }
            };

            DataGridViewTextBoxColumn colQuantity = new DataGridViewTextBoxColumn
            {
                Name = "Quantity",
                DataPropertyName = "Quantity",
                HeaderText = "Quantity",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };

            DataGridViewTextBoxColumn colMinStock = new DataGridViewTextBoxColumn
            {
                Name = "MinStock",
                DataPropertyName = "MinStock",
                HeaderText = "Min Stock",
                Width = 105,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };

            DataGridViewTextBoxColumn colStockStatus = new DataGridViewTextBoxColumn
            {
                Name = "StockStatus",
                DataPropertyName = "StockStatus",
                HeaderText = "Stock Status",
                Width = 140,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };

            // Feature: Inline "✏ Edit" Button Column in each product row!
            DataGridViewButtonColumn colEditAction = new DataGridViewButtonColumn
            {
                Name = "EditAction",
                HeaderText = "Action",
                Text = "✏ Edit",
                UseColumnTextForButtonValue = true,
                Width = 90,
                FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };

            dgv.Columns.AddRange(new DataGridViewColumn[]
            {
                colId, colName, colPrice, colQuantity, colMinStock, colStockStatus, colEditAction
            });

            // Custom Cell Painting:
            // 1. Stock Status Badge (rounded pill)
            // 2. Action Column (modern rounded "✏ Edit" button)
            dgv.CellPainting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.Graphics != null)
                {
                    // Stock Status Column
                    if (e.ColumnIndex == colStockStatus.Index)
                    {
                        e.PaintBackground(e.ClipBounds, true);
                        string status = e.Value?.ToString() ?? "Available";
                        DesignSystem.PaintStockBadge(e.Graphics, e.CellBounds, status);
                        e.Handled = true;
                        return;
                    }

                    // Inline Edit Button Column
                    if (e.ColumnIndex == colEditAction.Index)
                    {
                        e.PaintBackground(e.ClipBounds, true);

                        Rectangle btnRect = new Rectangle(
                            e.CellBounds.X + 8,
                            e.CellBounds.Y + 8,
                            e.CellBounds.Width - 16,
                            e.CellBounds.Height - 16
                        );

                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                        // Modern SaaS outline pill button
                        using (GraphicsPath path = DesignSystem.CreateRoundedRectanglePath(btnRect, 6))
                        {
                            using (SolidBrush bgBrush = new SolidBrush(DesignSystem.LightBlue))
                            {
                                e.Graphics.FillPath(bgBrush, path);
                            }
                            using (Pen borderPen = new Pen(DesignSystem.PrimaryBlue, 1f))
                            {
                                e.Graphics.DrawPath(borderPen, path);
                            }
                        }

                        using (SolidBrush textBrush = new SolidBrush(DesignSystem.PrimaryBlue))
                        {
                            StringFormat sf = new StringFormat
                            {
                                Alignment = StringAlignment.Center,
                                LineAlignment = StringAlignment.Center
                            };
                            e.Graphics.DrawString("✏ Edit", DesignSystem.FontBadge, textBrush, btnRect, sf);
                        }

                        e.Handled = true;
                        return;
                    }
                }
            };

            // Handle clicking the inline Edit button
            dgv.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == colEditAction.Index)
                {
                    if (dgv.Rows[e.RowIndex].DataBoundItem is Product product)
                    {
                        OpenEditDialog(product);
                    }
                }
            };

            // Topic 3: Mouse selection & Double Click on row to edit
            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex != colEditAction.Index)
                {
                    HandleEditProduct();
                }
            };

            return dgv;
        }

        // =================================================================
        // VIEW SWITCHING
        // =================================================================
        private void ShowView(bool isDashboard)
        {
            _dashboardView.Visible = isDashboard;
            _productsView.Visible = !isDashboard;

            _btnNavDashboard.IsActiveNav = isDashboard;
            _btnNavProducts.IsActiveNav = !isDashboard;
            _btnNavGuide.IsActiveNav = false;
            _btnNavAbout.IsActiveNav = false;

            if (isDashboard)
            {
                _lblHeaderTitle.Text = "Dashboard Overview";
                _lblHeaderSubtitle.Text = "Persistent stock monitoring, interactive GDI+ analytics, and SQLite catalog";
            }
            else
            {
                _lblHeaderTitle.Text = "Products Catalog";
                _lblHeaderSubtitle.Text = "Comprehensive inventory catalog, search filtering, and CRUD management";
            }
        }

        // =================================================================
        // SYNCHRONIZATION & UI REFRESH WITH SQLITE
        // =================================================================
        public void RefreshAllUI()
        {
            ApplySearchFilter();
            RefreshStatistics();
            RefreshChart();
        }

        private void ApplySearchFilter()
        {
            try
            {
                string query = _txtSearch.Text.Trim();

                // Load directly from SQLite persistent repository
                _displayedProducts = _repository.Search(query);

                // Bind to DataGridViews
                BindTableData(_dgvDashboardProducts);
                BindTableData(_dgvProducts);

                // Get total database count for header counter
                var (total, _, _) = _repository.GetDashboardStatistics();
                string countText = $"Showing {_displayedProducts.Count} of {total} products";
                _lblDashboardProductsCount.Text = countText;
                _lblProductsCount.Text = countText;
            }
            catch (Exception ex)
            {
                NotificationDialog.ShowError(
                    this,
                    "[Inventory Search Error]",
                    $"Error querying products from SQLite database:\n\n{ex.Message}"
                );
            }
        }

        private void BindTableData(DataGridView dgv)
        {
            int? currentSelectedId = null;
            if (dgv.SelectedRows.Count > 0 && dgv.SelectedRows[0].DataBoundItem is Product current)
            {
                currentSelectedId = current.Id;
            }

            dgv.DataSource = null;
            dgv.DataSource = _displayedProducts;

            // Restore selection if item is still present
            if (currentSelectedId.HasValue && dgv.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.DataBoundItem is Product p && p.Id == currentSelectedId.Value)
                    {
                        row.Selected = true;
                        dgv.CurrentCell = row.Cells[0];
                        break;
                    }
                }
            }
        }

        private void RefreshStatistics()
        {
            try
            {
                var (total, lowStock, outOfStock) = _repository.GetDashboardStatistics();

                _cardTotal.Value = total.ToString();
                _cardLowStock.Value = lowStock.ToString();
                _cardOutOfStock.Value = outOfStock.ToString();
            }
            catch
            {
                int total = _displayedProducts.Count;
                int lowStock = _displayedProducts.Count(p => p.StockStatus == "Low Stock");
                int outOfStock = _displayedProducts.Count(p => p.StockStatus == "Out of Stock");

                _cardTotal.Value = total.ToString();
                _cardLowStock.Value = lowStock.ToString();
                _cardOutOfStock.Value = outOfStock.ToString();
            }
        }

        private void RefreshChart()
        {
            try
            {
                var allProducts = _repository.GetAll();
                _chartPanel.UpdateData(allProducts);
            }
            catch
            {
                _chartPanel.UpdateData(_displayedProducts);
            }
        }

        private Product? GetActiveSelectedProduct()
        {
            DataGridView activeDgv = _dashboardView.Visible ? _dgvDashboardProducts : _dgvProducts;
            if (activeDgv.SelectedRows.Count > 0 && activeDgv.SelectedRows[0].DataBoundItem is Product p)
            {
                return p;
            }

            DataGridView otherDgv = _dashboardView.Visible ? _dgvProducts : _dgvDashboardProducts;
            if (otherDgv.SelectedRows.Count > 0 && otherDgv.SelectedRows[0].DataBoundItem is Product pOther)
            {
                return pOther;
            }

            return null;
        }
    }
}

