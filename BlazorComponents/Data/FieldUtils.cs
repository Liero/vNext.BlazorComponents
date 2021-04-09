using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Data
{
    public static class FieldUtils
    {
        public static Expression ParsePropertyExpression(Expression parameter, string propertyPath)
        {
            var props = propertyPath.Split('.');
            var type = parameter.Type;

            var expr = parameter;
            foreach (var prop in props)
            {
                var pi = type.GetProperty(prop) ?? throw new ArgumentException("Property Not found", nameof(propertyPath));
                expr = Expression.PropertyOrField(expr, pi.Name);
                type = pi.PropertyType;
            }
            return expr;
        }

        public static LambdaExpression CreatePropertyLambda<TSource>(string propertyPath) => CreatePropertyLambda(typeof(TSource), propertyPath);
        public static LambdaExpression CreatePropertyLambda(Type sourceType, string propertyPath)
        {
            var paramExp = Expression.Parameter(sourceType, "item");
            var propertyExp = FieldUtils.ParsePropertyExpression(paramExp, propertyPath);
            return Expression.Lambda(propertyExp, paramExp);
        }

        public static LambdaExpression CreateAssignLambda<TSource>(string property) => CreateAssignLambda(typeof(TSource), property);
        public static LambdaExpression CreateAssignLambda(Type sourceType, string propertyPath)
        {
            var itemParam = Expression.Parameter(sourceType, "item");
            var propertyExp = FieldUtils.ParsePropertyExpression(itemParam, propertyPath);
            var valueParam = Expression.Parameter(propertyExp.GetMemberType(), "value");
            var assignExpr = Expression.Assign(propertyExp, valueParam);
            return Expression.Lambda(assignExpr, itemParam, valueParam);
        }

        public static Expression<Func<TItem, bool>> CreatePredicateLambda<TItem>(string propertyPath, string @operator, object? value)
            => (Expression<Func<TItem, bool>>)CreatePredicateLambda(typeof(TItem), propertyPath, @operator, value);
        public static LambdaExpression CreatePredicateLambda(Type sourceType, string propertyPath, string @operator, object? value)
        {
            return CreatePredicateLambda(sourceType, propertyPath, propertyExp =>
                @operator switch
                {
                    "==" or "equals" => Expression.Equal(propertyExp, Expression.Constant(value)),
                    "!=" or "<>" or "notequals" => Expression.NotEqual(propertyExp, Expression.Constant(value)),
                    "<" => Expression.LessThan(propertyExp, Expression.Constant(value)),
                    "<=" => Expression.LessThanOrEqual(propertyExp, Expression.Constant(value)),
                    ">" => Expression.GreaterThan(propertyExp, Expression.Constant(value)),
                    ">=" => Expression.GreaterThanOrEqual(propertyExp, Expression.Constant(value)),
                    "startswith" => Expression.Call(
                        propertyExp,
                        typeof(string).GetMethod(nameof(string.StartsWith), new Type[] { typeof(string) })!,
                        Expression.Constant(value ?? "")),
                    "contains" => Expression.Call(
                        propertyExp,
                        typeof(string).GetMethod(nameof(string.Contains), new Type[] { typeof(string) })!,
                        Expression.Constant(value ?? "")),
                    _ => throw new ArgumentOutOfRangeException($"Operator '{@operator}' not supported", nameof(@operator))
                });
        }

        public static Expression<Func<TItem, bool>> CreatePredicateLambda<TItem>(string propertyPath, Func<Expression, Expression> predicateBody)
            => (Expression<Func<TItem, bool>>)CreatePredicateLambda(typeof(TItem), propertyPath, predicateBody);
        public static LambdaExpression CreatePredicateLambda(Type sourceType, string propertyPath, Func<Expression, Expression> predicateBody)
        {
            var itemParam = Expression.Parameter(sourceType, "item");
            var propertyExp = FieldUtils.ParsePropertyExpression(itemParam, propertyPath);
            var predicateBodyInstance = predicateBody(propertyExp);
            return Expression.Lambda(predicateBodyInstance, itemParam);
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> query, LambdaExpression expression, bool desc = false, bool thenBy = false)
        {
            string methodName = thenBy ? nameof(Queryable.ThenBy) : nameof(Queryable.OrderBy);
            if (desc) methodName += "Descending";
            var method = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == methodName && m.GetParameters().Length == 2);
            method = method.MakeGenericMethod(typeof(TSource), expression.Body.GetMemberType());
            return (IOrderedQueryable<TSource>)method.Invoke(null, new object[] { query, expression })!;
        }
        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IOrderedQueryable<TSource> query, LambdaExpression expression, bool desc = false)
        {
            return OrderBy(query, expression, desc, true);
        }

        public static Type GetMemberType(this Expression expression)
        {
            return expression switch
            {
                MemberExpression m when m.Member is PropertyInfo p => p.PropertyType,
                MemberExpression m when m.Member is FieldInfo p => p.FieldType,
                _ => throw new NotImplementedException(expression.ToString())
            };
        }

        public static void SetPropertyValue<T, TValue>(LambdaExpression lambda, T target, TValue value)
        {
            lambda.Compile().DynamicInvoke(target, value);
        }
    }


}
