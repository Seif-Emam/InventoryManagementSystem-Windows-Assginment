using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using InventoryManagementSystem.Commands;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Forms;

namespace InventoryManagementSystem
{
    internal static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer, int nNumberOfCharsToWrite, out int lpNumberOfCharsWritten, IntPtr lpReserved);

        private const int ATTACH_PARENT_PROCESS = -1;
        private const int STD_OUTPUT_HANDLE = -11;

        /// <summary>
        /// The main entry point for the Windows Forms application.
        /// Supports both full desktop GUI and direct command-line terminal execution.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            // If command-line arguments are passed, execute directly in terminal mode without opening GUI window
            if (args != null && args.Length > 0)
            {
                RunDirectTerminalMode(args);
                return;
            }

            // Initialize high DPI, modern visual styles, and default font
            ApplicationConfiguration.Initialize();

            try
            {
                // Initialize SQLite database schema and idempotent seeding
                DatabaseInitializer.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to initialize the local SQLite database:\n\n{ex.Message}\n\nThe application will now close.",
                    "Database Initialization Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            Application.Run(new MainForm());
        }

        /// <summary>
        /// Executes terminal CLI commands (add, edit, delete, list, search, help) directly from the console
        /// without opening any Windows Forms dialogs or windows.
        /// </summary>
        private static void RunDirectTerminalMode(string[] args)
        {
            bool attached = AttachConsole(ATTACH_PARENT_PROCESS);

            try
            {
                DatabaseInitializer.Initialize();
                var repo = new ProductRepository();
                var processor = new TerminalCommandProcessor(repo);

                string commandLine = string.Join(" ", args);
                var result = processor.Process(commandLine);

                string outputText = "\r\n" + result.Message + "\r\n";

                if (attached)
                {
                    IntPtr hOut = GetStdHandle(STD_OUTPUT_HANDLE);
                    WriteConsole(hOut, outputText, outputText.Length, out _, IntPtr.Zero);
                }

                Console.WriteLine(outputText);
            }
            catch (Exception ex)
            {
                string errText = $"\r\n[ERROR] Command failed: {ex.Message}\r\n";
                if (attached)
                {
                    IntPtr hOut = GetStdHandle(STD_OUTPUT_HANDLE);
                    WriteConsole(hOut, errText, errText.Length, out _, IntPtr.Zero);
                }
                Console.Error.WriteLine(errText);
            }
        }
    }
}
