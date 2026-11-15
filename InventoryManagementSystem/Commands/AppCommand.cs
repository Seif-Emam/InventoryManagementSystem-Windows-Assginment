using System;
using System.Windows.Forms;
using InventoryManagementSystem.Forms;

namespace InventoryManagementSystem.Commands
{
    /// <summary>
    /// Encapsulates the result of executing an application command.
    /// </summary>
    public class ExecutionResult
    {
        public bool Success { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }
        public Exception? Error { get; }

        public ExecutionResult(bool success, string message, Exception? error = null)
        {
            Success = success;
            Message = message;
            Timestamp = DateTime.Now;
            Error = error;
        }

        public static ExecutionResult Succeeded(string message = "Command executed successfully.") =>
            new ExecutionResult(true, message);

        public static ExecutionResult Failed(string message, Exception? error = null) =>
            new ExecutionResult(false, message, error);
    }

    /// <summary>
    /// Represents an actionable command in the Inventory Management System.
    /// Demonstrates Windows Programming Topic 4: Menus, Commands and Resources.
    /// Stores comprehensive metadata, execution logic, associated handlers, and affected components.
    /// </summary>
    public class AppCommand
    {
        public string Id { get; }
        public string Name { get; }
        public string Category { get; }
        public string Purpose { get; }
        public string ExpectedAction { get; }
        public string HandlerName { get; }
        public string Shortcut { get; }
        public Keys KeyShortcut { get; }
        public string AffectedComponents { get; }
        public Action<MainForm> Action { get; }
        public Func<MainForm, bool>? CanExecutePredicate { get; }

        // Live telemetry
        public DateTime? LastExecutedAt { get; private set; }
        public int ExecutionCount { get; private set; }

        public AppCommand(
            string id,
            string name,
            string category,
            string purpose,
            string expectedAction,
            string handlerName,
            string shortcut,
            Keys keyShortcut,
            string affectedComponents,
            Action<MainForm> action,
            Func<MainForm, bool>? canExecutePredicate = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Category = category ?? "General";
            Purpose = purpose ?? string.Empty;
            ExpectedAction = expectedAction ?? string.Empty;
            HandlerName = handlerName ?? string.Empty;
            Shortcut = shortcut ?? "None";
            KeyShortcut = keyShortcut;
            AffectedComponents = affectedComponents ?? string.Empty;
            Action = action ?? throw new ArgumentNullException(nameof(action));
            CanExecutePredicate = canExecutePredicate;
        }

        /// <summary>
        /// Checks whether this command can be executed in the current context.
        /// </summary>
        public bool CanExecute(MainForm context)
        {
            if (context == null || context.IsDisposed)
            {
                return false;
            }

            return CanExecutePredicate == null || CanExecutePredicate(context);
        }

        /// <summary>
        /// Executes the command against the target MainForm instance.
        /// Catches and records any execution errors gracefully without crashing the application.
        /// </summary>
        public ExecutionResult Execute(MainForm context)
        {
            if (context == null)
            {
                return ExecutionResult.Failed($"Cannot execute command '{Id}': MainForm context is null.");
            }

            if (context.IsDisposed)
            {
                return ExecutionResult.Failed($"Cannot execute command '{Id}': MainForm has been disposed.");
            }

            if (!CanExecute(context))
            {
                return ExecutionResult.Failed($"Command '{Name}' ({Id}) cannot be executed in the current state.");
            }

            try
            {
                Action(context);
                LastExecutedAt = DateTime.Now;
                ExecutionCount++;
                return ExecutionResult.Succeeded($"Command '{Name}' ({Id}) executed successfully at {LastExecutedAt:HH:mm:ss}.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Failed($"Command '{Name}' ({Id}) failed: {ex.Message}", ex);
            }
        }

        public override string ToString() => $"[{Id}] {Name} ({HandlerName})";
    }
}

