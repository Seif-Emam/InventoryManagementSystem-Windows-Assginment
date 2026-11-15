using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InventoryManagementSystem.Commands;
using InventoryManagementSystem.UI;

namespace InventoryManagementSystem.Forms
{
    /// <summary>
    /// Data Transfer Object defining the documentation metadata for a CLI terminal command.
    /// </summary>
    public class TerminalCommandDoc
    {
        public string Icon { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Syntax { get; set; } = string.Empty;
        public string HowToUse { get; set; } = string.Empty;
        public string WhatItEffects { get; set; } = string.Empty;
        public string Example { get; set; } = string.Empty;
    }

    public partial class CommandsExplorerForm
    {
        // =================================================================
        // VIEW 2: TERMINAL CLI COMMANDS & HELP GUIDE
        // Detailed cards for every terminal command explaining how it's used & what it effects
        // =================================================================
        private void BuildTerminalHelpView()
        {
            _viewHelpContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesignSystem.AppBackground,
                AutoScroll = true,
                Padding = new Padding(24, 16, 24, 20),
                Visible = false
            };

            // Overview Banner
            CardPanel introCard = new CardPanel
            {
                Dock = DockStyle.Top,
                Height = 85,
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(0, 0, 0, 14)
            };

            Label lblIntroTitle = new Label
            {
                Text = "⚡  Direct SQLite Terminal Commands Reference (No Windows Opened)",
                Font = DesignSystem.FontSectionHead,
                ForeColor = DesignSystem.PrimaryNavy,
                Location = new Point(14, 10),
                AutoSize = true
            };

            Label lblIntroDesc = new Label
            {
                Text = "Execute CRUD commands directly via command line by passing parameters in the command string itself.\nEvery command updates SQLite, recalculates stock status, and refreshes the UI live without opening any dialogs.",
                Font = DesignSystem.FontBody,
                ForeColor = DesignSystem.SecondaryText,
                Location = new Point(16, 36),
                Size = new Size(1080, 40)
            };
            introCard.Controls.Add(lblIntroTitle);
            introCard.Controls.Add(lblIntroDesc);

            // Catalog of all 8 terminal commands
            var docCatalog = new List<TerminalCommandDoc>
            {
                new TerminalCommandDoc
                {
                    Icon = "➕",
                    Name = "add — Insert Product Directly",
                    Syntax = "add --name <name> --price <price> --stock <qty> [--min <min>]  OR  add \"<name>\" <price> <qty> [min]",
                    HowToUse = "• Named syntax:   add --name \"Razer Basilisk V3\" --price 69.99 --stock 25 --min 5\n" +
                               "• Short flags:    add -n \"Razer Mouse\" -p 69.99 -s 25 -m 5\n" +
                               "• Positional:     add \"Razer Basilisk V3\" 69.99 25 5\n" +
                               "• Validation:     Name required, price & stock must be >= 0. MinStock defaults to 5 if omitted.",
                    WhatItEffects = "• SQLite DB: Inserts a new row into the 'Products' table with an auto-incremented primary key.\n" +
                                   "• Stock Status: Dynamically evaluated as 'Available', 'Low Stock', or 'Out of Stock'.\n" +
                                   "• UI Live Refresh: Increments Total Products KPI card, recalculates Catalog Value, redraws GDI+ Stock Chart with new bar, and updates DataGridViews without opening any modal window.",
                    Example = "add --name \"Razer Basilisk V3\" --price 69.99 --stock 25 --min 5"
                },
                new TerminalCommandDoc
                {
                    Icon = "✏️",
                    Name = "edit — Update Product Directly (Partial Updates Supported)",
                    Syntax = "edit <id> [--name <name>] [--price <price>] [--stock <qty>] [--min <min>]  OR  edit <id> \"<name>\" <price> <qty> [min]",
                    HowToUse = "• Partial update: edit 3 --price 24.99 --stock 15   (only changes specified fields; name & min are untouched!)\n" +
                               "• Deplete stock:  edit 2 --stock 0   (depletes inventory; status recalculates to Out of Stock!)\n" +
                               "• Full update:    edit 3 \"SteelSeries Rival 3 Wired\" 24.99 15 5\n" +
                               "• Validation:     Target ID must exist in SQLite; values must be non-negative.",
                    WhatItEffects = "• SQLite DB: Executes UPDATE Products SET ... WHERE Id = @Id directly on the database.\n" +
                                   "• Stock Status: Recalculates dynamically (triggers Low Stock alert if stock <= minStock; Out of Stock if 0).\n" +
                                   "• UI Live Refresh: Updates table row, synchronizes Low Stock / Out of Stock KPI cards, and updates GDI+ chart bar.",
                    Example = "edit 3 --price 24.99 --stock 15"
                },
                new TerminalCommandDoc
                {
                    Icon = "🗑️",
                    Name = "delete — Remove Product Directly",
                    Syntax = "delete <id>   OR   delete --id <id>",
                    HowToUse = "• Usage:      delete 5   or   delete --id 5\n" +
                               "• Safe check: Verifies that product ID exists in SQLite before deletion.\n" +
                               "• No Dialogs: Deletes immediately without showing blocking confirmation popups.",
                    WhatItEffects = "• SQLite DB: Executes DELETE FROM Products WHERE Id = @Id.\n" +
                                   "• Catalog Metrics: Decrements Total Products counter and subtracts inventory valuation.\n" +
                                   "• UI Live Refresh: Removes item from table, removes corresponding bar from GDI+ chart, and refreshes catalog.",
                    Example = "delete 5"
                },
                new TerminalCommandDoc
                {
                    Icon = "📋",
                    Name = "list — Output Products Table in Terminal",
                    Syntax = "list [--low | --out | --available | --all]",
                    HowToUse = "• Show all:       list   or   list --all\n" +
                               "• Show low stock: list --low\n" +
                               "• Depleted items: list --out\n" +
                               "• In stock only:  list --available",
                    WhatItEffects = "• SQLite DB: Executes read-only query on Products table.\n" +
                                   "• Terminal Output: Formats an aligned ASCII table with ID, Name, Price, Stock, Min, and Status badges.\n" +
                                   "• UI Effect: Displays output in terminal without modifying active GUI table search/filters.",
                    Example = "list --low"
                },
                new TerminalCommandDoc
                {
                    Icon = "🔍",
                    Name = "search — Query Inventory by Keyword or ID",
                    Syntax = "search <keyword>   OR   find <keyword>",
                    HowToUse = "• Search by Name: search logitech   or   search monitor\n" +
                               "• Search by ID:   search 12\n" +
                               "• Output: Aligned table of matching products with live stock status badges.",
                    WhatItEffects = "• SQLite DB: Executes SELECT ... WHERE Name LIKE @query OR CAST(Id AS TEXT) LIKE @query.\n" +
                                   "• Terminal Output: Renders matching products list and count summary.\n" +
                                   "• Safe Query: Uses SQL parameterization against SQL injection.",
                    Example = "search logitech"
                },
                new TerminalCommandDoc
                {
                    Icon = "ℹ️",
                    Name = "info — Display Detailed Product Card",
                    Syntax = "info <id>   OR   get <id>   OR   show <id>",
                    HowToUse = "• Usage:      info 1   or   info 8\n" +
                               "• Output: Displays unit price, stock, threshold, inventory status, and total valuation.",
                    WhatItEffects = "• SQLite DB: Queries product by primary key Id.\n" +
                                   "• Valuation: Dynamically calculates total asset value = Price × Quantity.\n" +
                                   "• Read-only: Does not modify database data or UI.",
                    Example = "info 1"
                },
                new TerminalCommandDoc
                {
                    Icon = "❓",
                    Name = "help — Command Syntax & Usage Reference",
                    Syntax = "help [command]   OR   ? [command]",
                    HowToUse = "• Complete guide: help\n" +
                               "• Command guide:  help add   or   help edit   or   help delete\n" +
                               "• Displays full parameter list, options, and copyable examples.",
                    WhatItEffects = "• Terminal Output: Prints the full CLI user guide directly to the terminal screen.\n" +
                                   "• No Database interaction; purely instructional.",
                    Example = "help add"
                },
                new TerminalCommandDoc
                {
                    Icon = "🧹",
                    Name = "clear — Clear Terminal Screen Buffer",
                    Syntax = "clear   OR   cls",
                    HowToUse = "• Usage: clear   or   cls\n" +
                               "• Wipes the console display clean and reprints the welcome banner.\n" +
                               "• Note: Command history (Up/Down arrow keys) remains preserved in memory.",
                    WhatItEffects = "• Terminal UI: Resets RichTextBox text buffer.\n" +
                                   "• Does not touch SQLite database or application state.",
                    Example = "clear"
                }
            };

            Panel bottomSpacer = new Panel { Dock = DockStyle.Top, Height = 20, BackColor = Color.Transparent };

            // Adding order for Top docked elements: Add from bottom to top
            _viewHelpContainer.Controls.Add(bottomSpacer);
            for (int i = docCatalog.Count - 1; i >= 0; i--)
            {
                _viewHelpContainer.Controls.Add(CreateTerminalDocCard(docCatalog[i]));
            }
            _viewHelpContainer.Controls.Add(introCard);
        }

        private Panel CreateTerminalDocCard(TerminalCommandDoc doc)
        {
            CardPanel card = new CardPanel
            {
                Dock = DockStyle.Top,
                Height = 250,
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(0, 0, 0, 14)
            };

            Panel wrapper = new Panel
            {
                Dock = DockStyle.Top,
                Height = 264,
                Padding = new Padding(0, 0, 0, 14),
                BackColor = Color.Transparent
            };

            // Title line
            Label lblTitle = new Label
            {
                Text = $"{doc.Icon}  {doc.Name}",
                Font = DesignSystem.FontSectionHead,
                ForeColor = DesignSystem.PrimaryNavy,
                Location = new Point(14, 10),
                AutoSize = true
            };

            // Syntax Box (Dark badge container)
            Panel syntaxBox = new Panel
            {
                Location = new Point(16, 38),
                Size = new Size(880, 32),
                BackColor = Color.FromArgb(15, 23, 42), // #0F172A
                Padding = new Padding(10, 6, 10, 6)
            };

            syntaxBox.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(51, 65, 85), 1);
                e.Graphics.DrawRectangle(p, 0, 0, syntaxBox.Width - 1, syntaxBox.Height - 1);
            };

            Label lblSyntax = new Label
            {
                Text = $"Syntax:  {doc.Syntax}",
                Font = new Font("Consolas", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(74, 222, 128), // Emerald Green
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            syntaxBox.Controls.Add(lblSyntax);

            // How To Use Section
            Label lblHowHeader = new Label
            {
                Text = "📖 How to Use & Parameters:",
                Font = DesignSystem.FontBodyBold,
                ForeColor = DesignSystem.PrimaryText,
                Location = new Point(16, 76),
                AutoSize = true
            };

            Label lblHowDesc = new Label
            {
                Text = doc.HowToUse,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = DesignSystem.SecondaryText,
                Location = new Point(18, 96),
                Size = new Size(880, 64)
            };

            // What It Effects Section
            Label lblEffectHeader = new Label
            {
                Text = "⚡ What Each Command Effects:",
                Font = DesignSystem.FontBodyBold,
                ForeColor = DesignSystem.PrimaryNavy,
                Location = new Point(16, 162),
                AutoSize = true
            };

            Label lblEffectDesc = new Label
            {
                Text = doc.WhatItEffects,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(18, 182),
                Size = new Size(880, 52)
            };

            // Right Action: Try In Terminal Button
            ModernButton btnTry = new ModernButton
            {
                Text = "⚡  Try in Terminal",
                Type = ModernButton.ButtonType.Primary,
                Location = new Point(915, 36),
                Size = new Size(160, 36)
            };
            btnTry.Click += (s, e) =>
            {
                SwitchView(ExplorerView.LiveTerminal);
                _embeddedTerminal.ExecuteLine(doc.Example);
            };

            ModernButton btnCopy = new ModernButton
            {
                Text = "📋 Copy Example",
                Type = ModernButton.ButtonType.Secondary,
                Location = new Point(915, 80),
                Size = new Size(160, 32)
            };
            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(doc.Example);
                btnCopy.Text = "✔ Copied!";
                System.Windows.Forms.Timer t = new System.Windows.Forms.Timer { Interval = 1500 };
                t.Tick += (ts, te) =>
                {
                    btnCopy.Text = "📋 Copy Example";
                    t.Stop();
                    t.Dispose();
                };
                t.Start();
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(syntaxBox);
            card.Controls.Add(lblHowHeader);
            card.Controls.Add(lblHowDesc);
            card.Controls.Add(lblEffectHeader);
            card.Controls.Add(lblEffectDesc);
            card.Controls.Add(btnTry);
            card.Controls.Add(btnCopy);

            wrapper.Controls.Add(card);
            return wrapper;
        }

        // =================================================================
        // VIEW 3: EMBEDDED LIVE COMMAND TERMINAL
        // =================================================================
        private void BuildLiveTerminalView()
        {
            _viewTerminalContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(11, 17, 32),
                Visible = false
            };

            _embeddedTerminal = new TerminalPanel(new TerminalCommandProcessor(_mainForm.Repository, () => _mainForm.RefreshAllUI()))
            {
                Dock = DockStyle.Fill
            };

            _embeddedTerminal.CloseRequested += (s, e) => SwitchView(ExplorerView.CommandsTable);

            _viewTerminalContainer.Controls.Add(_embeddedTerminal);
        }

        private void RunSelectedInTerminal()
        {
            if (_dgvCommands.CurrentRow != null && _dgvCommands.CurrentRow.DataBoundItem is AppCommand cmd)
            {
                // Switch to Terminal Tab
                SwitchView(ExplorerView.LiveTerminal);

                // Run template command or help
                string line = cmd.Id switch
                {
                    "CMD_CLI_ADD" => "add --name \"Sample Product\" --price 49.99 --stock 20 --min 5",
                    "CMD_CLI_EDIT" => "edit 1 --price 89.99 --stock 25",
                    "CMD_CLI_DELETE" => "delete 5",
                    "CMD_CLI_LIST" => "list",
                    "CMD_CLI_SEARCH" => "search logitech",
                    "CMD_CLI_INFO" => "info 1",
                    "CMD_CLI_HELP" => "help",
                    "CMD_CLI_CLEAR" => "clear",
                    _ => "help"
                };

                _embeddedTerminal.ExecuteLine(line);
            }
        }
    }
}
