using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Data
{
    public class SortingModel
    {
        public SortingModel(params SortDescriptor[] sortings)
        {
            Descriptors = new ObservableCollection<SortDescriptor>(sortings);
        }

        public ObservableCollection<SortDescriptor> Descriptors { get; }

        public void AddSorting(string field, bool multiple = false)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            SortDescriptor? existingDescriptor = Descriptors.FirstOrDefault(s => s.Field == field);
            SortDescriptor newDescriptor = new(
                Field: field,
                Descending: existingDescriptor?.Descending == false);

            if (!multiple)
            {
                Descriptors.Clear();
            }
            if (existingDescriptor == default || Descriptors.Count == 0)
            {
                Descriptors.Add(newDescriptor);
            }
            else
            {
                Descriptors[Descriptors.IndexOf(existingDescriptor)] = newDescriptor;
            }
        }
    }

    public record SortDescriptor(string Field, bool Descending = false);
}
