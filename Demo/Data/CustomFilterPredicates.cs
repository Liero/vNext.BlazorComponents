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
        public static Expression<Func<TItem, bool>> ContainsIgnoreCase<TItem>(LambdaExpression fieldExpression, string value)
        {
            return (Expression<Func<TItem, bool>>)Expression.Lambda(
                Expression.Call(
                    fieldExpression.Body,
                    typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string), typeof(StringComparison) }),
                    Expression.Constant(value ?? ""),
                    Expression.Constant(StringComparison.CurrentCultureIgnoreCase)),
                fieldExpression.Parameters);
        }

        public static Expression<Func<TItem, bool>> StartsWithIgnoreCase<TItem>(LambdaExpression fieldExpression, string value)
        {
            return (Expression<Func<TItem, bool>>)Expression.Lambda(
               Expression.Call(
                    fieldExpression.Body,
                    typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string), typeof(StringComparison) }),
                    Expression.Constant(value ?? ""),
                    Expression.Constant(StringComparison.CurrentCultureIgnoreCase))
            );
        }

        /// <summary>
        /// adds custom operators startswith and contains.
        /// Optionally adds support for nulls in propertyPath
        /// </summary>
        public static Expression<Func<TItem, bool>> CreatePredicate<TItem>(IFilterDescriptor filterDescriptor, bool addNullChecks = true)
        {
            Expression<Func<TItem, bool>> customPredicate = null;

            if (filterDescriptor is FieldFilterDescriptor filter) 
            {
                LambdaExpression propertyLambda = FieldUtils.CreatePropertyLambda<TItem>(filter.Field);
                if (addNullChecks)
                {
                    propertyLambda = propertyLambda.AddNullChecks();
                }
                customPredicate = filter.Operator switch
                {
                    "startswith" => StartsWithIgnoreCase<TItem>(propertyLambda, (string)filter.Value),
                    "contains" => ContainsIgnoreCase<TItem>(propertyLambda, (string)filter.Value),
                    _ => propertyLambda.CreatePredicateLambda<TItem>(filter.Operator, filter.Value)
                };
            }
            return customPredicate ?? filterDescriptor.CreatePredicate<TItem>();
        }   
    }
}
