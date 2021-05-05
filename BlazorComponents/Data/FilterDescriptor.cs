using System;
using System.Linq.Expressions;

namespace vNext.BlazorComponents.Data
{
    public interface IFilterDescriptor
    {
        Expression<Func<TItem, bool>> CreatePredicate<TItem>();
    }

    public record FieldFilterDescriptor(string Field, string Operator, object? Value) : IFilterDescriptor
    {
        /// <summary>
        /// Creates Expression&lt;Func&lt;<typeparamref name="TItem"/>, bool>> predicate that can be passed to IQueryable&lt;<typeparamref name="TItem"/>>.Filter(..)
        /// </summary>
        /// <typeparam name="TItem">Source type of the IQueryable to be filteed. Typically the same as SimpleGrid<typeparamref name="TItem"/></typeparam>
        public Expression<Func<TItem, bool>> CreatePredicate<TItem>() 
            => FieldUtils.CreatePredicateLambda<TItem>(
                FieldUtils.CreatePropertyLambda<TItem>(Field), Operator, Value);        
    }
}
