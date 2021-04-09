using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using vNext.BlazorComponents.Data;

namespace vNext.BlazorComponents.Demo.Shared
{
    public class CreatingPredicateEventArgs<TItem>
    {
        public CreatingPredicateEventArgs(IFilterDescriptor filterDescriptor)
        {
            FilterDescriptor = filterDescriptor;
        }

        public IFilterDescriptor FilterDescriptor { get; }

        public Expression<Func<TItem, bool>> Predicate { get; set; }
    }
}
