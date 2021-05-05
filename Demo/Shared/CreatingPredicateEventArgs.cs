using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using vNext.BlazorComponents.Data;
using vNext.BlazorComponents.Grid;

namespace vNext.BlazorComponents.Demo.Shared
{
    public class CreatingPredicateEventArgs<TItem>
    {
        public CreatingPredicateEventArgs(IFilterDescriptor filterDescriptor, ColumnDef<TItem>? column)
        {
            FilterDescriptor = filterDescriptor;
        }

        public ColumnDef<TItem>? Column { get; }
        public IFilterDescriptor FilterDescriptor { get; }

        public Expression<Func<TItem, bool>> Predicate { get; set; }
    }
}
