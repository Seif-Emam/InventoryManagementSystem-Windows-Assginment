using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InventoryManagementSystem.Commands;
using InventoryManagementSystem.UI;

namespace InventoryManagementSystem.Forms
{
    public enum ExplorerView
    {
        CommandsTable,
        TerminalHelpGuide,
        LiveTerminal
    }

    /// <summary>
    /// CommandsExplorerForm: Dedicated administrative viewer, interactive help guide, and live executor
    /// for all application commands and direct terminal CLI commands.
    /// Structured using clean partial classes for UI, Table View, and Terminal Help Guide.
    /// Demonstrates Windows Programming Course Topics:
    /// - Topic 4: Menus, Commands and Resources (Command Pattern UI, Metadata Explorer, Terminal Help & Execution)
    /// - Topic 5: Forms (Multi-view tabs, Dialog lifecycle, Parent-Child synchronization)
    /// - Topic 2: Graphics and Drawing (GDI+ custom button painting, category chips, status styling)
    /// - Topic 1 & 3: Events & User Input (Search TextChanged, Row clicks, Key navigation)
    /// </summary>
    public partial class CommandsExplorerForm : Form
    {
        private readonly MainForm _mainForm;
        private readonly CommandRegistry _registry;

        private List<AppCommand> _allCommands = new List<AppCommand>();
        private List<AppCommand> _filteredCommands = new List<AppCommand>();
        private ExplorerView _activeView = ExplorerView.CommandsTable;

        // Header & Navigation Controls
        private Panel _headerPanel = null!;
        private TextBox _txtSearch = null!;
        private ComboBox _cmbCategory = null!;
        private ModernButton _btnTabTable = null!;
        private ModernButton _btnTabHelp = null!;
        private ModernButton _btnTabTerminal = null!;

        // Content Containers
        private Panel _contentContainer = null!;
        private Panel _viewTableContainer = null!;
        private Panel _viewHelpContainer = null!;
        private Panel _viewTerminalContainer = null!;

        // Table View Controls
        private DataGridView _dgvCommands = null!;
        private Label _lblDetailName = null!;
        private Label _lblDetailId = null!;
        private Label _lblDetailCategory = null!;
        private Label _lblDetailShortcut = null!;
        private Label _lblDetailHandler = null!;
        private Label _lblDetailPurpose = null!;
        private Label _lblDetailAction = null!;
        private Label _lblDetailAffected = null!;
        private ModernButton _btnExecuteSelected = null!;
        private ModernButton _btnRunInTerminal = null!;

        // Embedded Terminal
        private TerminalPanel _embeddedTerminal = null!;

        // Status Strip
        private Label _lblStatusLeft = null!;
        private Label _lblStatusRight = null!;

        public CommandsExplorerForm(MainForm mainForm, CommandRegistry registry)
        {
            _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));

            InitializeCustomUI();
            LoadCommands();
        }

        // =================================================================
        // VIEW SWITCHING (Tabs: Commands Catalog, Help Guide, Live Terminal)
        // =================================================================
        public void SwitchView(ExplorerView targetView)
        {
            _activeView = targetView;

            // Toggle views
            _viewTableContainer.Visible = (targetView == ExplorerView.CommandsTable);
            _viewHelpContainer.Visible = (targetView == ExplorerView.TerminalHelpGuide);
            _viewTerminalContainer.Visible = (targetView == ExplorerView.LiveTerminal);

            // Update tab button active styles
            _btnTabTable.IsActiveNav = (targetView == ExplorerView.CommandsTable);
            _btnTabHelp.IsActiveNav = (targetView == ExplorerView.TerminalHelpGuide);
            _btnTabTerminal.IsActiveNav = (targetView == ExplorerView.LiveTerminal);

            // Update status text
            switch (targetView)
            {
                case ExplorerView.CommandsTable:
                    _lblStatusRight.Text = "Ready | Select any command to view usage & effects, or click '⚡ Run'";
                    _lblStatusRight.ForeColor = DesignSystem.LightBlue;
                    break;
                case ExplorerView.TerminalHelpGuide:
                    _lblStatusRight.Text = "Terminal Help Guide | Review syntax, parameter rules, and database effects";
                    _lblStatusRight.ForeColor = Color.FromArgb(147, 197, 253);
                    break;
                case ExplorerView.LiveTerminal:
                    _lblStatusRight.Text = "Interactive CLI | Type 'add', 'edit', 'delete', 'list' and press Enter";
                    _lblStatusRight.ForeColor = DesignSystem.SuccessColor;
                    _embeddedTerminal.FocusInput();
                    break;
            }
        }

        private void LoadCommands()
        {
            _allCommands = _registry.Commands.ToList();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string query = _txtSearch.Text.Trim();
            string category = _cmbCategory.SelectedItem?.ToString() ?? "All Categories";

            var filtered = _allCommands.AsEnumerable();

            if (!string.IsNullOrEmpty(query))
            {
                filtered = filtered.Where(c =>
                    c.Id.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    c.HandlerName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    c.Purpose.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    c.ExpectedAction.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    c.AffectedComponents.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    c.Shortcut.Contains(query, StringComparison.OrdinalIgnoreCase)
                );
            }

            if (!category.Equals("All Categories", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            _filteredCommands = filtered.ToList();

            _dgvCommands.DataSource = null;
            _dgvCommands.DataSource = _filteredCommands;

            _lblStatusLeft.Text = $"Total Commands: {_allCommands.Count} | Displayed: {_filteredCommands.Count}";

            UpdateSelectedCommandPreview();
        }

        private void ExecuteCommandAt(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < _filteredCommands.Count)
            {
                var command = _filteredCommands[rowIndex];
                ExecuteCommand(command);
            }
        }

        private void ExecuteSelectedCommand()
        {
            if (_dgvCommands.CurrentRow != null && _dgvCommands.CurrentRow.DataBoundItem is AppCommand cmd)
            {
                ExecuteCommand(cmd);
            }
        }

        private void ExecuteCommand(AppCommand command)
        {
            if (command == null) return;

            if (command.Id == "CMD_EXPLORER_OPEN")
            {
                _lblStatusRight.Text = "ℹ Commands Explorer is already open and active!";
                _lblStatusRight.ForeColor = DesignSystem.PrimaryBlue;
                return;
            }

            if (command.Id == "CMD_APP_EXIT")
            {
                Close();
                command.Execute(_mainForm);
                return;
            }

            // For CLI commands, execute directly via embedded terminal!
            if (command.Category == "Terminal CLI" || command.Id == "CMD_TERMINAL_OPEN")
            {
                RunSelectedInTerminal();
                return;
            }

            var result = command.Execute(_mainForm);

            if (result.Success)
            {
                _lblStatusRight.Text = $"✔ Executed '{command.Id}' successfully at {result.Timestamp:HH:mm:ss} (Run count: {command.ExecutionCount})";
                _lblStatusRight.ForeColor = DesignSystem.SuccessColor;
            }
            else
            {
                _lblStatusRight.Text = $"✖ Execution failed: {result.Message}";
                _lblStatusRight.ForeColor = DesignSystem.ErrorColor;
            }
        }

        private void CommandsExplorerForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (_activeView != ExplorerView.CommandsTable)
                {
                    SwitchView(ExplorerView.CommandsTable);
                    e.Handled = true;
                }
                else
                {
                    Close();
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Enter && _dgvCommands.Focused && !_txtSearch.Focused)
            {
                ExecuteSelectedCommand();
                e.Handled = true;
            }
        }
    }
}
