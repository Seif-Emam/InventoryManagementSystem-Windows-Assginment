using System;

namespace InventoryManagementSystem.Data
{
    /// <summary>
    /// Exception thrown when a SQLite database operation fails, tagged with the specific inventory feature.
    /// </summary>
    public class RepositoryException : Exception
    {
        public string Feature { get; }
        public string Operation { get; }

        public RepositoryException(string feature, string operation, string message, Exception? innerException = null)
            : base($"[{feature}] Database operation '{operation}' failed: {message}", innerException)
        {
            Feature = feature;
            Operation = operation;
        }
    }
}

