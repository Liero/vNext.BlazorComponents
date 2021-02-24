using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace vNext.BlazorComponents
{
    /// <summary>
    /// threats empty string as default(TValue).
    /// E.g if TValue is a string or a class, it returns null;
    /// </summary>
    public class InputSelectNullable<TValue> : InputSelect<TValue>
    {
        protected override bool TryParseValueFromString(
            string? value,
            out TValue result,
            out string validationErrorMessage)
        {
            validationErrorMessage = "";
            if (string.IsNullOrEmpty(value))
            {
#pragma warning disable CS8601 // Possible null reference assignment.
                result = default;
#pragma warning restore CS8601 // Possible null reference assignment.
                return true;
            }
            return base.TryParseValueFromString(value, out result!, out validationErrorMessage!);
        }

        protected override string? FormatValueAsString(TValue? value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return base.FormatValueAsString(value);
        }
    }
}
