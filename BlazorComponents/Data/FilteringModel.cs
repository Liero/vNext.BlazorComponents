using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Data
{
    public abstract class FilterigModelBase
    {
        private string? _selectedField;

        [Required]
        public virtual string? SelectedField
        {
            get => _selectedField;
            set
            {
                _selectedField = value;
                if (value != null)
                {
                    AvailableOperators = GetOperators(value).ToArray();
                }
            }
        }

        [CanConvertValidation]
        public string ValueAsString { get; set; } = string.Empty;

        [Required]
        public string? SelectedOperator { get; set; }
        public string[] AvailableOperators { get; set; } = Array.Empty<string>();
        public ObservableCollection<IFilterDescriptor> Descriptors { get; } = new ObservableCollection<IFilterDescriptor>();


        public virtual bool CanConvert(string valueAsString, out string errorMessage)
        {
            try
            {
                GetValue(valueAsString);
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is FormatException)
            {
                errorMessage = ex.Message;
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }
        public object? GetValue() => GetValue(ValueAsString);

        protected abstract object? GetValue(string valueAsString);
    

        public virtual IEnumerable<string> GetOperators(string field)
        {
            yield return "==";
            yield return "!=";
            yield return "contains";
            yield return "startswith";
            yield return "<";
            yield return "<=";
            yield return ">";
            yield return ">=";
        }

        public virtual void AddFilter()
        {
            if (string.IsNullOrEmpty(SelectedField) || string.IsNullOrEmpty(SelectedOperator))
            {
                return;
            }
            if (!CanConvert(ValueAsString, out var _))
            {
                return;
            }
            Descriptors.Add(CreateDescriptor());
        }

        public virtual IFilterDescriptor CreateDescriptor()
        {
            return new FieldFilterDescriptor(SelectedField!, SelectedOperator!, GetValue());
        }
    }

    public class FilteringModel<TItem> : FilterigModelBase
    {
        static readonly string[] StringOnlyOperators = new[] { "contains", "startswith" };
        static readonly string[] CompareOperators = new[] { "<", ">", "<=", ">=" };

        public Type? SelectedFieldClrType { get; set; }
        
        public override string? SelectedField
        {
            get => base.SelectedField; 
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var propertyLambda = GetPropertyLambda(value);
                    SelectedFieldClrType = propertyLambda.ReturnType;
                }
                base.SelectedField = value;
                if (SelectedOperator == null || !AvailableOperators.Contains(SelectedOperator))
                {
                    if (value != null)
                    {
                        SelectedOperator = GetDefaultOperator(value);
                    }
                }
            }
        }

        protected virtual LambdaExpression GetPropertyLambda(string field)
        {
            return FieldUtils.CreatePropertyLambda(typeof(TItem), field);
        }

        public override IEnumerable<string> GetOperators(string field)
        {
            var allOperators = base.GetOperators(field);
            if (SelectedFieldClrType?.IsAssignableFrom(typeof(string)) == true)
            {
                return allOperators.Except(CompareOperators);
            }
            else
            {
                return allOperators.Except(StringOnlyOperators);
            }
        }

        public virtual string GetDefaultOperator(string field)
        {
            return SelectedFieldClrType == typeof(string) ? "startswith" : "==";
        }

        protected override object? GetValue(string valueAsString)
        {
            if (SelectedFieldClrType == null)
            {
                return null;
            }
            Type? fromNullable = Nullable.GetUnderlyingType(SelectedFieldClrType);
            if (fromNullable != null && string.IsNullOrEmpty(valueAsString))
            {
                return null;
            }
            return Convert.ChangeType(valueAsString, fromNullable ?? SelectedFieldClrType);
        }
    }

    public class CanConvertValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var filteringModel = ((FilterigModelBase)validationContext.ObjectInstance);
            if (!filteringModel.CanConvert((string)value!, out string errorMessage))
            {
                return new ValidationResult(errorMessage, new[] { validationContext.MemberName! });
            }
            return ValidationResult.Success;
        } 
    }
}
