using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using vNext.BlazorComponents.Data;

namespace vNext.BlazorComponents.Demo.Data
{
    public class CustomFilterPredicates
    {
        public static Expression<Func<TItem, bool>> ContainsIgnoreCase<TItem>(string field, string value)
        {
            return FieldUtils.CreatePredicateLambda<TItem>(
                field, 
                propertyExpression => Expression.Call(
                    propertyExpression,
                    typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string), typeof(StringComparison) }),
                    Expression.Constant(value ?? ""),
                    Expression.Constant(StringComparison.CurrentCultureIgnoreCase))
            );
        }

        public static Expression<Func<TItem, bool>> StartsWithIgnoreCase<TItem>(string field, string value)
        {
            return FieldUtils.CreatePredicateLambda<TItem>(
                field,
                propertyExpression => Expression.Call(
                    propertyExpression,
                    typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string), typeof(StringComparison) }),
                    Expression.Constant(value ?? ""),
                    Expression.Constant(StringComparison.CurrentCultureIgnoreCase))
            );
        }

        public static Expression<Func<TItem, bool>> CreatePredicate<TItem>(IFilterDescriptor filterDescriptor)
        {
            Expression<Func<TItem, bool>> customPredicate = null;

            if (filterDescriptor is FilterDescriptor filter) 
            {
                customPredicate = filter.Operator switch
                {
                    "startswith" => StartsWithIgnoreCase<TItem>(filter.Field, (string)filter.Value),
                    "contains" => ContainsIgnoreCase<TItem>(filter.Field, (string)filter.Value),
                    _ => null
                };
            }
            return customPredicate ?? filterDescriptor.CreatePredicate<TItem>();
        }
    }
}
