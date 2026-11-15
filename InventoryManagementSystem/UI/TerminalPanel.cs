using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using InventoryManagementSystem.Commands;

namespace InventoryManagementSystem.UI
{
    /// <summary>
    /// TerminalPanel: An in-app interactive command terminal drawer.
    /// Allows users to execute CRUD commands (add, edit, delete, list, search) directly via command line
    /// with zero dialogs or popup windows.
    /// Demonstrates Windows Programming Topic 1, 3 & 4:
    /// - Topic 1: Events and Message Handling (KeyDown, Command execution, Text rendering)
    /// - Topic 3: Keyboard & User Input (Command history cycling via Up/Down arrows, Enter to execute)
    /// - Topic 4: Menus & Commands (Direct CLI execution)
    /// </summary>
    public class TerminalPanel : Panel
    {
        private readonly TerminalCommandProcessor _processor;
        private readonly List<string> _history = new List<string>();
        private int _historyIndex = -1;

        // UI Controls
        private Panel _headerBar = null!;
        private RichTextBox _rtbOutput = null!;
        private Panel _inputBar = null!;
        private TextBox _txtInput = null!;
        private ModernButton _btnRun = null!;

        public event EventHandler? CloseRequested;

        public TerminalPanel(TerminalCommandProcessor processor)
        {
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            InitializeUI();
            PrintWelcomeBanner();
        }

        private void InitializeUI()
        {
            Dock = DockStyle.Bottom;
            Height = 270;
            BackColor = Color.FromArgb(11, 17, 32); // #0B1120 Slate-950
            Padding = new Padding(0);

            // 1. Header Bar
            _headerBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(15, 23, 42), // #0F172A Slate-900
                Padding = new Padding(12, 0, 12, 0)
            };

            _headerBar.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(30, 41, 59), 1);
                e.Graphics.DrawLine(p, 0, _headerBar.Height - 1, _headerBar.Width, _headerBar.Height - 1);
            };

            Label lblTitle = new Label
            {
                Text = "💻  InventoryHub Command Terminal  •  Direct SQLite CLI",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 245, 249),
                Dock = DockStyle.Left,
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 0)
            };

            Label lblHint = new Label
            {
                Text = "No dialogs • Press Enter to run • Up/Down for command history",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Dock = DockStyle.Left,
                AutoSize = true,
                Padding = new Padding(16, 9, 0, 0)
            };

            ModernButton btnClose = new ModernButton
            {
                Text = "✖ Close (Esc)",
                Type = ModernButton.ButtonType.Secondary,
                Dock = DockStyle.Right,
                Width = 100,
                Height = 26
            };
            btnClose.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);

            Panel sep1 = new Panel { Dock = DockStyle.Right, Width = 8 };

            ModernButton btnClear = new ModernButton
            {
                Text = "🧹 Clear",
                Type = ModernButton.ButtonType.Secondary,
                Dock = DockStyle.Right,
                Width = 76,
                Height = 26
            };
            btnClear.Click += (s, e) => _rtbOutput.Clear();

            Panel sep2 = new Panel { Dock = DockStyle.Right, Width = 8 };

            ModernButton btnHelp = new ModernButton
            {
                Text = "❓ Help",
                Type = ModernButton.ButtonType.Secondary,
                Dock = DockStyle.Right,
                Width = 72,
                Height = 26
            };
            btnHelp.Click += (s, e) => ExecuteLine("help");

            _headerBar.Controls.Add(lblTitle);
            _headerBar.Controls.Add(lblHint);
            _headerBar.Controls.Add(btnClose);
            _headerBar.Controls.Add(sep1);
            _headerBar.Controls.Add(btnClear);
            _headerBar.Controls.Add(sep2);
            _headerBar.Controls.Add(btnHelp);

            // 2. Bottom Input Line
            _inputBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(2, 6, 23), // #020617
                Padding = new Padding(12, 6, 12, 6)
            };

            _inputBar.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(30, 41, 59), 1);
                e.Graphics.DrawLine(p, 0, 0, _inputBar.Width, 0);
            };

            Label lblPrompt = new Label
            {
                Text = "inventoryhub> ",
                Font = new Font("Consolas", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(74, 222, 128), // Emerald Green
                Dock = DockStyle.Left,
                AutoSize = true,
                Padding = new Padding(0, 4, 0, 0)
            };

            _btnRun = new ModernButton
            {
                Text = "⚡ Run",
                Type = ModernButton.ButtonType.Primary,
                Dock = DockStyle.Right,
                Width = 75,
                Height = 28
            };
            _btnRun.Click += (s, e) => SubmitCurrentInput();

            _txtInput = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(2, 6, 23),
                ForeColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 10.5f, FontStyle.Regular),
                PlaceholderText = "Type command (e.g. add --name \"Gaming Mouse\" --price 49.99 --stock 20 --min 5)"
            };

            _txtInput.KeyDown += TxtInput_KeyDown;

            _inputBar.Controls.Add(_txtInput);
            _inputBar.Controls.Add(lblPrompt);
            _inputBar.Controls.Add(_btnRun);

            // 3. Center Output RichTextBox
            _rtbOutput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(11, 17, 32),
                ForeColor = Color.FromArgb(226, 232, 240),
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 9.75f, FontStyle.Regular),
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            // Layout
            Controls.Add(_rtbOutput);
            Controls.Add(_inputBar);
            Controls.Add(_headerBar);
        }

        private void PrintWelcomeBanner()
        {
            AppendText("========================================================================================\n", Color.FromArgb(71, 85, 105));
            AppendText("  ⚡ INVENTORY MANAGEMENT DIRECT COMMAND TERMINAL\n", Color.FromArgb(56, 189, 248));
            AppendText("  Direct SQLite execution without opening dialog windows. Type 'help' for examples.\n", Color.FromArgb(148, 163, 184));
            AppendText("  Examples:  add --name \"Mouse\" --price 29.99 --stock 15 --min 5\n", Color.FromArgb(100, 116, 139));
            AppendText("             edit 3 --price 24.99 --stock 12   |   delete 5   |   list\n", Color.FromArgb(100, 116, 139));
            AppendText("========================================================================================\n\n", Color.FromArgb(71, 85, 105));
        }

        private void TxtInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                SubmitCurrentInput();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Up)
            {
                e.Handled = true;
                if (_history.Count > 0)
                {
                    if (_historyIndex == -1) _historyIndex = _history.Count - 1;
                    else if (_historyIndex > 0) _historyIndex--;

                    _txtInput.Text = _history[_historyIndex];
                    _txtInput.SelectionStart = _txtInput.Text.Length;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                e.Handled = true;
                if (_history.Count > 0 && _historyIndex != -1)
                {
                    if (_historyIndex < _history.Count - 1)
                    {
                        _historyIndex++;
                        _txtInput.Text = _history[_historyIndex];
                    }
                    else
                    {
                        _historyIndex = -1;
                        _txtInput.Text = string.Empty;
                    }
                    _txtInput.SelectionStart = _txtInput.Text.Length;
                }
            }
        }

        private void SubmitCurrentInput()
        {
            string line = _txtInput.Text.Trim();
            if (string.IsNullOrEmpty(line)) return;

            // Save to history
            _history.Add(line);
            _historyIndex = -1;

            _txtInput.Clear();
            ExecuteLine(line);
        }

        public void ExecuteLine(string line)
        {
            AppendText($"inventoryhub> {line}\n", Color.FromArgb(147, 197, 253)); // Soft blue

            var result = _processor.Process(line);

            if (result.Message == "__CLEAR__")
            {
                _rtbOutput.Clear();
                PrintWelcomeBanner();
                return;
            }

            AppendText($"{result.Message}\n\n", result.OutputColor);
            _rtbOutput.SelectionStart = _rtbOutput.Text.Length;
            _rtbOutput.ScrollToCaret();
        }

        private void AppendText(string text, Color color)
        {
            _rtbOutput.SelectionStart = _rtbOutput.Text.Length;
            _rtbOutput.SelectionLength = 0;
            _rtbOutput.SelectionColor = color;
            _rtbOutput.AppendText(text);
            _rtbOutput.SelectionColor = _rtbOutput.ForeColor;
        }

        public void FocusInput()
        {
            _txtInput.Focus();
            _txtInput.SelectAll();
        }
    }
}

