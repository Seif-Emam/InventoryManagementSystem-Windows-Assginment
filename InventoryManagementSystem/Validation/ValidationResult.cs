using System;
using System.Text;

namespace InventoryManagementSystem.Validation
{
    /// <summary>
    /// Represents the result of a domain or input validation operation across the application.
    /// Provides consistent, feature-related error formatting for both GUI forms and the terminal CLI.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string Feature { get; }
        public string? FieldName { get; }
        public string? InvalidValue { get; }
        public string ErrorMessage { get; }
        public string? Guidance { get; }

        public static ValidationResult Success(string feature = "General")
        {
            return new ValidationResult(true, feature, null, null, string.Empty, null);
        }

        public static ValidationResult Failure(
            string feature,
            string fieldName,
            string? invalidValue,
            string errorMessage,
            string? guidance = null)
        {
            return new ValidationResult(false, feature, fieldName, invalidValue, errorMessage, guidance);
        }

        private ValidationResult(
            bool isValid,
            string feature,
            string? fieldName,
            string? invalidValue,
            string errorMessage,
            string? guidance)
        {
            IsValid = isValid;
            Feature = feature;
            FieldName = fieldName;
            InvalidValue = invalidValue;
            ErrorMessage = errorMessage;
            Guidance = guidance;
        }

        /// <summary>
        /// Formats the validation failure into a clean, professional ASCII card for the Terminal CLI.
        /// </summary>
        public string ToTerminalErrorString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"✖ [{Feature.ToUpperInvariant()} ERROR]");
            if (!string.IsNullOrEmpty(FieldName))
            {
                sb.AppendLine($"  Field:    {FieldName}");
            }
            if (!string.IsNullOrEmpty(InvalidValue))
            {
                sb.AppendLine($"  Input:    '{InvalidValue}'");
            }
            sb.AppendLine($"  Reason:   {ErrorMessage}");
            if (!string.IsNullOrEmpty(Guidance))
            {
                sb.AppendLine($"  Guidance: {Guidance}");
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Formats a concise error message suitable for inline form labels.
        /// </summary>
        public string ToInlineMessage()
        {
            return $"⚠ [{Feature}] {ErrorMessage}";
        }

        /// <summary>
        /// Formats a comprehensive error message suitable for modal notification dialogs.
        /// </summary>
        public string ToDialogMessage()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Feature: {Feature}");
            if (!string.IsNullOrEmpty(FieldName))
            {
                sb.Append($"Field: {FieldName}");
                if (!string.IsNullOrEmpty(InvalidValue))
                {
                    sb.Append($" (Input: '{InvalidValue}')");
                }
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine($"Error: {ErrorMessage}");
            if (!string.IsNullOrEmpty(Guidance))
            {
                sb.AppendLine();
                sb.AppendLine($"How to fix: {Guidance}");
            }
            return sb.ToString().TrimEnd();
        }
    }
}

