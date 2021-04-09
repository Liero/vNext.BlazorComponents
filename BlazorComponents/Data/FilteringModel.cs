﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        public ObservableCollection<IFilterDescriptor> FilterDescriptors { get; } = new ObservableCollection<IFilterDescriptor>();

        public virtual bool CanConvert(string? valueAsString, out string errorMessage)
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

        public void AddFilter()
        {
            if (string.IsNullOrEmpty(SelectedField) || string.IsNullOrEmpty(SelectedOperator))
            {
                return;
            }
            if (!CanConvert(ValueAsString, out var _))
            {
                return;
            }
            FilterDescriptors.Add(new FilterDescriptor(SelectedField, SelectedOperator, GetValue()));
        }
    }

    public class FilteringModel<TItem> : FilterigModelBase
    {
        static readonly string[] StringOnlyOperators = new[] { "contains", "startswith" };

        public Type? SelectedFieldClrType { get; set; }
        
        public override string? SelectedField
        {
            get => base.SelectedField; 
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var propertyLambda = FieldUtils.CreatePropertyLambda(typeof(TItem), value);
                    SelectedFieldClrType = FieldUtils.GetMemberType(propertyLambda.Body);
                }
                base.SelectedField = value;
            }
        }

        public override IEnumerable<string> GetOperators(string field)
        {
            if (SelectedFieldClrType?.IsAssignableFrom(typeof(string)) != true)
            {
                return base.GetOperators(field).Except(StringOnlyOperators);
            }
            return base.GetOperators(field);
        }

        protected override object? GetValue(string valueAsString)
        {
            return Convert.ChangeType(valueAsString, SelectedFieldClrType ?? typeof(string));
        }
    }

    public class CanConvertValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var filteringModel = ((FilterigModelBase)validationContext.ObjectInstance);
            if (!filteringModel.CanConvert((string?)value, out string errorMessage))
            {
                return new ValidationResult(errorMessage, new[] { validationContext.MemberName! });
            }
            return ValidationResult.Success;
        } 
    }
}