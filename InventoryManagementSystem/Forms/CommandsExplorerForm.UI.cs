using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InventoryManagementSystem.Commands;
using InventoryManagementSystem.UI;

namespace InventoryManagementSystem.Forms
{
    public partial class CommandsExplorerForm
    {
        private void InitializeCustomUI()
        {
            Text = "Application Commands Explorer — InventoryHub";
            Size = new Size(1220, 760);
            MinimumSize = new Size(1040, 620);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = DesignSystem.AppBackground;
            Font = DesignSystem.FontBody;

            // 1. Top Header & Navigation Tabs
            BuildHeader();

            // 2. Bottom Status Strip
            Panel statusStrip = BuildStatusStrip();

            // 3. Central Content Views Container
            _contentContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesignSystem.AppBackground
            };

            // Build the 3 distinct views
            BuildCommandsTableView();
            BuildTerminalHelpView();
            BuildLiveTerminalView();

            _contentContainer.Controls.Add(_viewTableContainer);
            _contentContainer.Controls.Add(_viewHelpContainer);
            _contentContainer.Controls.Add(_viewTerminalContainer);

            // Adding order
            Controls.Add(_contentContainer);
            Controls.Add(statusStrip);
            Controls.Add(_headerPanel);

            KeyPreview = true;
            KeyDown += CommandsExplorerForm_KeyDown;

            // Activate initial view
            SwitchView(ExplorerView.CommandsTable);
        }

        // =================================================================
        // 1. HEADER & TOP NAVIGATION TABS
        // =================================================================
        private void BuildHeader()
        {
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = DesignSystem.CardBackground,
                Padding = new Padding(24, 14, 24, 0)
            };

            _headerPanel.Paint += (s, e) =>
            {
                using Pen p = new Pen(DesignSystem.BorderColor, 1);
                e.Graphics.DrawLine(p, 0, _headerPanel.Height - 1, _headerPanel.Width, _headerPanel.Height - 1);
            };

            // Top Row: Title + Search & Category
            Panel topRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.Transparent
            };

            Label lblTitle = new Label
            {
                Text = "⚡  Application Commands & Terminal Explorer",
                Font = DesignSystem.FontSectionHead,
                ForeColor = DesignSystem.PrimaryNavy,
                Location = new Point(0, 0),
                AutoSize = true
            };

            Label lblSubtitle = new Label
            {
                Text = "Browse all application commands, CLI terminal syntax, and database effects",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.SecondaryText,
                Location = new Point(0, 24),
                AutoSize = true
            };

            Panel titleBox = new Panel { Dock = DockStyle.Left, Width = 520, BackColor = Color.Transparent };
            titleBox.Controls.Add(lblTitle);
            titleBox.Controls.Add(lblSubtitle);

            // Search input container
            Panel searchContainer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 260,
                Height = 36,
                BackColor = DesignSystem.InputBackground,
                Padding = new Padding(8, 8, 8, 6)
            };

            searchContainer.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using Pen pen = new Pen(_txtSearch.Focused ? DesignSystem.PrimaryBlue : DesignSystem.BorderColor, 1.5f);
                Rectangle rect = new Rectangle(0, 0, searchContainer.Width - 1, searchContainer.Height - 1);
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
                PlaceholderText = "Search commands, syntax, or effects..."
            };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            _txtSearch.Enter += (s, e) => searchContainer.Invalidate();
            _txtSearch.Leave += (s, e) => searchContainer.Invalidate();
            searchContainer.Controls.Add(_txtSearch);

            Panel spacerSearch = new Panel { Dock = DockStyle.Right, Width = 12, BackColor = Color.Transparent };

            // Category filter dropdown
            _cmbCategory = new ComboBox
            {
                Dock = DockStyle.Right,
                Width = 170,
                Height = 36,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = DesignSystem.FontBody,
                BackColor = Color.White,
                ForeColor = DesignSystem.PrimaryText,
                FlatStyle = FlatStyle.Flat
            };

            _cmbCategory.Items.AddRange(new object[]
            {
                "All Categories",
                "Terminal CLI",
                "Products (CRUD)",
                "Navigation",
                "Data & Search",
                "Application & Help"
            });
            _cmbCategory.SelectedIndex = 0;
            _cmbCategory.SelectedIndexChanged += (s, e) => ApplyFilter();

            topRow.Controls.Add(titleBox);
            topRow.Controls.Add(searchContainer);
            topRow.Controls.Add(spacerSearch);
            topRow.Controls.Add(_cmbCategory);

            // Bottom Row: Navigation Tabs
            Panel tabsRow = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 4, 0, 0)
            };

            _btnTabTable = new ModernButton
            {
                Text = "📋  Commands Catalog (All Commands)",
                Type = ModernButton.ButtonType.SidebarNav,
                Dock = DockStyle.Left,
                Width = 270,
                Height = 38,
                IsActiveNav = true
            };
            _btnTabTable.Click += (s, e) => SwitchView(ExplorerView.CommandsTable);

            _btnTabHelp = new ModernButton
            {
                Text = "📖  Terminal Commands & Help Guide",
                Type = ModernButton.ButtonType.SidebarNav,
                Dock = DockStyle.Left,
                Width = 280,
                Height = 38,
                IsActiveNav = false
            };
            _btnTabHelp.Click += (s, e) => SwitchView(ExplorerView.TerminalHelpGuide);

            _btnTabTerminal = new ModernButton
            {
                Text = "💻  Live Command Terminal",
                Type = ModernButton.ButtonType.SidebarNav,
                Dock = DockStyle.Left,
                Width = 220,
                Height = 38,
                IsActiveNav = false
            };
            _btnTabTerminal.Click += (s, e) => SwitchView(ExplorerView.LiveTerminal);

            tabsRow.Controls.Add(_btnTabTerminal);
            tabsRow.Controls.Add(_btnTabHelp);
            tabsRow.Controls.Add(_btnTabTable);

            _headerPanel.Controls.Add(tabsRow);
            _headerPanel.Controls.Add(topRow);
        }

        // =================================================================
        // VIEW 1: COMMANDS TABLE WITH ADVANCED DETAILS CARD
        // =================================================================
        private void BuildCommandsTableView()
        {
            _viewTableContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesignSystem.AppBackground
            };

            // Selected Command Details Card (Bottom-docked)
            Panel detailsCard = BuildDetailsCard();

            // Center Grid Area (Fill)
            Panel gridContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 14, 20, 10),
                BackColor = DesignSystem.AppBackground
            };

            CardPanel tableCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };

            _dgvCommands = CreateConfiguredDataGridView();
            tableCard.Controls.Add(_dgvCommands);
            gridContainer.Controls.Add(tableCard);

            _viewTableContainer.Controls.Add(gridContainer);
            _viewTableContainer.Controls.Add(detailsCard);
        }

        private Panel BuildDetailsCard()
        {
            CardPanel detailsCard = new CardPanel
            {
                Dock = DockStyle.Bottom,
                Height = 175,
                Margin = new Padding(20, 0, 20, 10),
                Padding = new Padding(16, 12, 16, 12)
            };

            Panel wrapper = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 188,
                Padding = new Padding(20, 0, 20, 10),
                BackColor = DesignSystem.AppBackground
            };

            // Left Info Section (Command title + metadata chips)
            Panel leftInfo = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            _lblDetailName = new Label
            {
                Text = "Select a command to view details and execute",
                Font = DesignSystem.FontSectionHead,
                ForeColor = DesignSystem.PrimaryNavy,
                Location = new Point(0, 0),
                AutoSize = true
            };

            _lblDetailId = new Label
            {
                Text = "ID: -",
                Font = DesignSystem.FontBodyBold,
                ForeColor = DesignSystem.PrimaryBlue,
                Location = new Point(0, 26),
                AutoSize = true
            };

            _lblDetailCategory = new Label
            {
                Text = "Category: -",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.SecondaryText,
                Location = new Point(190, 27),
                AutoSize = true
            };

            _lblDetailShortcut = new Label
            {
                Text = "Shortcut / Syntax: -",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.StatusLowStockFg,
                Location = new Point(370, 27),
                AutoSize = true
            };

            _lblDetailHandler = new Label
            {
                Text = "Handler: -",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.PrimaryBlue,
                Location = new Point(560, 27),
                AutoSize = true
            };

            _lblDetailPurpose = new Label
            {
                Text = "Purpose: -",
                Font = DesignSystem.FontBody,
                ForeColor = DesignSystem.PrimaryText,
                Location = new Point(0, 52),
                Size = new Size(840, 32)
            };

            _lblDetailAction = new Label
            {
                Text = "📖 How to Use & Expected Action: -",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.SecondaryText,
                Location = new Point(0, 88),
                Size = new Size(840, 34)
            };

            _lblDetailAffected = new Label
            {
                Text = "⚡ What it Effects (DB & UI): -",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.PrimaryNavy,
                Location = new Point(0, 126),
                Size = new Size(840, 24)
            };

            leftInfo.Controls.Add(_lblDetailName);
            leftInfo.Controls.Add(_lblDetailId);
            leftInfo.Controls.Add(_lblDetailCategory);
            leftInfo.Controls.Add(_lblDetailShortcut);
            leftInfo.Controls.Add(_lblDetailHandler);
            leftInfo.Controls.Add(_lblDetailPurpose);
            leftInfo.Controls.Add(_lblDetailAction);
            leftInfo.Controls.Add(_lblDetailAffected);

            // Right Actions Section
            Panel rightActions = new Panel
            {
                Dock = DockStyle.Right,
                Width = 220,
                BackColor = Color.Transparent,
                Padding = new Padding(12, 4, 0, 0)
            };

            _btnExecuteSelected = new ModernButton
            {
                Text = "⚡  Execute Command",
                Type = ModernButton.ButtonType.Primary,
                Dock = DockStyle.Top,
                Height = 42,
                Enabled = false
            };
            _btnExecuteSelected.Click += (s, e) => ExecuteSelectedCommand();

            Panel btnSpacer1 = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.Transparent };

            _btnRunInTerminal = new ModernButton
            {
                Text = "💻  Try in Terminal",
                Type = ModernButton.ButtonType.Secondary,
                Dock = DockStyle.Top,
                Height = 36
            };
            _btnRunInTerminal.Click += (s, e) => RunSelectedInTerminal();

            Panel btnSpacer2 = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.Transparent };

            ModernButton btnClose = new ModernButton
            {
                Text = "Close (Esc)",
                Type = ModernButton.ButtonType.Secondary,
                Dock = DockStyle.Top,
                Height = 36
            };
            btnClose.Click += (s, e) => Close();

            rightActions.Controls.Add(btnClose);
            rightActions.Controls.Add(btnSpacer2);
            rightActions.Controls.Add(_btnRunInTerminal);
            rightActions.Controls.Add(btnSpacer1);
            rightActions.Controls.Add(_btnExecuteSelected);

            detailsCard.Controls.Add(leftInfo);
            detailsCard.Controls.Add(rightActions);
            wrapper.Controls.Add(detailsCard);

            return wrapper;
        }

        private DataGridView CreateConfiguredDataGridView()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = DesignSystem.CardBackground,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = DesignSystem.BorderColor,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                AutoGenerateColumns = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 42 },
                Font = DesignSystem.FontBody
            };

            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = DesignSystem.PrimaryNavy,
                ForeColor = Color.White,
                Font = DesignSystem.FontTableHeader,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 8, 0)
            };
            dgv.ColumnHeadersHeight = 44;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = DesignSystem.PrimaryText,
                SelectionBackColor = DesignSystem.LightBlue,
                SelectionForeColor = DesignSystem.PrimaryNavy,
                Font = DesignSystem.FontBody,
                Padding = new Padding(6, 0, 6, 0)
            };

            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = DesignSystem.RowAlternate,
                ForeColor = DesignSystem.PrimaryText,
                SelectionBackColor = DesignSystem.LightBlue,
                SelectionForeColor = DesignSystem.PrimaryNavy
            };

            // Columns
            var colId = new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "Command ID",
                DataPropertyName = "Id",
                Width = 150,
                ReadOnly = true
            };

            var colName = new DataGridViewTextBoxColumn
            {
                Name = "colName",
                HeaderText = "Command Name",
                DataPropertyName = "Name",
                Width = 175,
                ReadOnly = true
            };

            var colCategory = new DataGridViewTextBoxColumn
            {
                Name = "colCategory",
                HeaderText = "Category",
                DataPropertyName = "Category",
                Width = 130,
                ReadOnly = true
            };

            var colShortcut = new DataGridViewTextBoxColumn
            {
                Name = "colShortcut",
                HeaderText = "Shortcut / Syntax",
                DataPropertyName = "Shortcut",
                Width = 120,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = DesignSystem.FontBodyBold,
                    ForeColor = DesignSystem.PrimaryBlue
                }
            };

            var colHandler = new DataGridViewTextBoxColumn
            {
                Name = "colHandler",
                HeaderText = "Associated Handler",
                DataPropertyName = "HandlerName",
                Width = 190,
                ReadOnly = true
            };

            var colPurpose = new DataGridViewTextBoxColumn
            {
                Name = "colPurpose",
                HeaderText = "Purpose / Description",
                DataPropertyName = "Purpose",
                Width = 230,
                ReadOnly = true
            };

            var colExpectedAction = new DataGridViewTextBoxColumn
            {
                Name = "colExpectedAction",
                HeaderText = "Expected Action & Usage",
                DataPropertyName = "ExpectedAction",
                Width = 240,
                ReadOnly = true
            };

            var colAffected = new DataGridViewTextBoxColumn
            {
                Name = "colAffected",
                HeaderText = "What It Effects (DB & UI)",
                DataPropertyName = "AffectedComponents",
                Width = 220,
                ReadOnly = true
            };

            var colExecute = new DataGridViewButtonColumn
            {
                Name = "colExecute",
                HeaderText = "Action",
                Text = "⚡ Run",
                UseColumnTextForButtonValue = true,
                Width = 85,
                FlatStyle = FlatStyle.Flat
            };

            dgv.Columns.AddRange(new DataGridViewColumn[]
            {
                colId,
                colName,
                colCategory,
                colShortcut,
                colHandler,
                colPurpose,
                colExpectedAction,
                colAffected,
                colExecute
            });

            dgv.CellPainting += DgvCommands_CellPainting;
            dgv.SelectionChanged += (s, e) => UpdateSelectedCommandPreview();
            dgv.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == dgv.Columns["colExecute"].Index)
                {
                    ExecuteCommandAt(e.RowIndex);
                }
            };
            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    ExecuteCommandAt(e.RowIndex);
                }
            };

            return dgv;
        }

        private void DgvCommands_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Graphics == null) return;

            // Custom "⚡ Run" Button
            if (e.ColumnIndex == _dgvCommands.Columns["colExecute"].Index)
            {
                e.PaintBackground(e.ClipBounds, true);
                Rectangle rect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + 6, e.CellBounds.Width - 16, e.CellBounds.Height - 12);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using GraphicsPath path = DesignSystem.CreateRoundedRectanglePath(rect, 6);
                using SolidBrush bgBrush = new SolidBrush(ColorTranslator.FromHtml("#EFF6FF"));
                using Pen borderPen = new Pen(DesignSystem.PrimaryBlue, 1.2f);
                using SolidBrush textBrush = new SolidBrush(DesignSystem.PrimaryBlue);

                e.Graphics.FillPath(bgBrush, path);
                e.Graphics.DrawPath(borderPen, path);

                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString("⚡ Run", DesignSystem.FontBodyBold, textBrush, rect, sf);
                e.Handled = true;
                return;
            }

            // Command ID styling
            if (e.ColumnIndex == _dgvCommands.Columns["colId"].Index)
            {
                e.PaintBackground(e.ClipBounds, true);
                using SolidBrush textBrush = new SolidBrush(DesignSystem.PrimaryNavy);
                string val = e.Value?.ToString() ?? string.Empty;
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                Rectangle textRect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y, e.CellBounds.Width - 16, e.CellBounds.Height);
                e.Graphics.DrawString(val, DesignSystem.FontBodyBold, textBrush, textRect, sf);
                e.Handled = true;
                return;
            }

            // Handler column styling
            if (e.ColumnIndex == _dgvCommands.Columns["colHandler"].Index)
            {
                e.PaintBackground(e.ClipBounds, true);
                using SolidBrush textBrush = new SolidBrush(DesignSystem.PrimaryBlue);
                string val = e.Value?.ToString() ?? string.Empty;
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                Rectangle textRect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y, e.CellBounds.Width - 16, e.CellBounds.Height);
                e.Graphics.DrawString(val, DesignSystem.FontSecondary, textBrush, textRect, sf);
                e.Handled = true;
                return;
            }
        }

        private void UpdateSelectedCommandPreview()
        {
            if (_dgvCommands.CurrentRow != null && _dgvCommands.CurrentRow.DataBoundItem is AppCommand cmd)
            {
                _lblDetailName.Text = $"⚡ {cmd.Name}";
                _lblDetailId.Text = $"ID: {cmd.Id}";
                _lblDetailCategory.Text = $"Category: {cmd.Category}";
                _lblDetailShortcut.Text = $"Shortcut / Syntax: {cmd.Shortcut}";
                _lblDetailHandler.Text = $"Handler: {cmd.HandlerName}";
                _lblDetailPurpose.Text = $"Purpose: {cmd.Purpose}";
                _lblDetailAction.Text = $"📖 How to Use & Expected Action: {cmd.ExpectedAction}";
                _lblDetailAffected.Text = $"⚡ What it Effects (DB & UI): {cmd.AffectedComponents}";
                _btnExecuteSelected.Enabled = true;
                _btnExecuteSelected.Text = $"⚡  Execute '{cmd.Id}'";
                _btnRunInTerminal.Enabled = true;
            }
            else
            {
                _lblDetailName.Text = "Select a command to view details and execute";
                _lblDetailId.Text = "ID: -";
                _lblDetailCategory.Text = "Category: -";
                _lblDetailShortcut.Text = "Shortcut / Syntax: -";
                _lblDetailHandler.Text = "Handler: -";
                _lblDetailPurpose.Text = "Purpose: -";
                _lblDetailAction.Text = "📖 How to Use & Expected Action: -";
                _lblDetailAffected.Text = "⚡ What it Effects (DB & UI): -";
                _btnExecuteSelected.Enabled = false;
                _btnExecuteSelected.Text = "⚡  Execute Command";
                _btnRunInTerminal.Enabled = false;
            }
        }

        // =================================================================
        // BOTTOM STATUS STRIP
        // =================================================================
        private Panel BuildStatusStrip()
        {
            Panel statusStrip = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                BackColor = DesignSystem.SidebarBackground,
                Padding = new Padding(16, 6, 16, 6)
            };

            _lblStatusLeft = new Label
            {
                Text = "Total Commands: 0 | Displayed: 0",
                Font = DesignSystem.FontSecondary,
                ForeColor = Color.White,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            _lblStatusRight = new Label
            {
                Text = "Ready | Select any command and click '⚡ Run' or press Enter to execute",
                Font = DesignSystem.FontSecondary,
                ForeColor = DesignSystem.LightBlue,
                Dock = DockStyle.Right,
                AutoSize = true
            };

            statusStrip.Controls.Add(_lblStatusLeft);
            statusStrip.Controls.Add(_lblStatusRight);
            return statusStrip;
        }
    }
}

