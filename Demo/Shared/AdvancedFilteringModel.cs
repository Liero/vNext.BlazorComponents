using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vNext.BlazorComponents.Data;

namespace vNext.BlazorComponents.Demo.Shared
{
    public class AdvancedFilteringModel<TItem> : FilteringModel<TItem>
    {
        public FilterDescriptor CurrentDescriptor { get; private set; }

        public void StartEdit(FilterDescriptor filterDescriptor)
        {
            CurrentDescriptor = filterDescriptor;
            SelectedField = filterDescriptor?.Field;
            if (filterDescriptor?.Operator != null)
            {
                SelectedOperator = filterDescriptor?.Operator;
            }
            ValueAsString = filterDescriptor?.Value?.ToString();
        }

        public void EndEdit()
        {
            int index = Descriptors.IndexOf(CurrentDescriptor);
            var newFilter = CreateDescriptor();
            if (index < 0)
            {
                Descriptors.Add(newFilter);
            }
            else
            {
                Descriptors.RemoveAt(index);
                Descriptors.Insert(index, newFilter);
            }
            CurrentDescriptor = null;
            SelectedOperator = null;
            ValueAsString = string.Empty;
        }
    }
}
