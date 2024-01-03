using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using vNext.BlazorComponents.Data.Expressions;

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

        public static bool CanAssign(this LambdaExpression propertyLambda)
        {
            return propertyLambda.Body is MemberExpression memberExpression
                && memberExpression.Member switch
                {
                    PropertyInfo property => property.CanWrite,
                    FieldInfo field => !field.IsInitOnly,
                    _ => false
                };
        }

        public static Expression<Action<TItem, TValue>> CreateAssignLambda<TItem, TValue>(this LambdaExpression propertyLambda)
         => (Expression<Action<TItem, TValue>>)CreateAssignLambda(propertyLambda, typeof(TValue));

        /// <summary>
        /// converts 
        /// <code>(TItem item) => item.Property1</code> 
        /// to 
        /// <code>(TItem item, TValue value) => { item.Property = (TProperty)value; }</code>
        /// </summary>
        /// <param name="valueType">default value: propertyLambda.ReturnType</param>
        public static LambdaExpression CreateAssignLambda(this LambdaExpression propertyLambda, Type? valueType = null)
        {
            if (valueType == null)
            {
                valueType = propertyLambda.ReturnType;
            }
            ParameterExpression valueParameter = Expression.Parameter(valueType, "value");
            ParameterExpression itemParameter = propertyLambda.Parameters.Single();
            Expression right = valueType.IsAssignableTo(propertyLambda.ReturnType)
                ? valueParameter
                : Expression.Convert(valueParameter, propertyLambda.ReturnType);


            return Expression.Lambda(
                Expression.GetActionType(itemParameter.Type, valueType),
                Expression.Block(Expression.Assign(
                    left: propertyLambda.Body,
                    right: right
                )),
                propertyLambda.Parameters[0],
                valueParameter); ;
        }

        public static Expression<Func<TSource, TValue>> CastFunc<TSource, TValue>(this LambdaExpression propertyLambda)
        {
            // adds cast to getter if needed. e.g. item.Property1 -> (TValue)item.Property1
            Expression body = propertyLambda.ReturnType.IsAssignableFrom(typeof(TValue))
                ? propertyLambda.Body 
                : Expression.Convert(propertyLambda.Body, typeof(TValue));

            //todo: cast parameter to TSource if needed
            return Expression.Lambda<Func<TSource, TValue>>(body, propertyLambda.Parameters);
        }

        public static MemberExpression? FindMemberExpression(this Expression expression) => 
            (MemberExpression?)FindExpressionVisitor.Find(expression, e => e is MemberExpression);

        public static Expression<Func<TItem, bool>> CreatePredicateLambda<TItem>(this LambdaExpression fieldExpression, string @operator, object? value)
            => (Expression<Func<TItem, bool>>)CreatePredicateLambda(fieldExpression, @operator, value);

        public static Expression<Func<TItem, bool>> Not<TItem>(this Expression<Func<TItem, bool>> predicate)
        {
            return Expression.Lambda<Func<TItem, bool>>(
                Expression.Not(predicate.Body),
                predicate.Parameters);
        }

        public static LambdaExpression CreatePredicateLambda(this LambdaExpression fieldExpression, string @operator, object? value)
        {       
            Expression propertyExp = fieldExpression.Body;
            string operatorBase = @operator;
            bool negate = false;
            const string negatePrefix = "not";
            if (operatorBase.StartsWith(negatePrefix) && operatorBase != "notequals")
            {
                operatorBase = operatorBase.Substring(negatePrefix.Length).Trim();
                negate = true;
            }
            Expression predicateBody = operatorBase switch
            {
                "==" or "equals" => Expression.Equal(propertyExp, Expression.Constant(value, fieldExpression.ReturnType)),
                "!=" or "<>" or "notequals" => Expression.NotEqual(propertyExp, Expression.Constant(value, fieldExpression.ReturnType)),
                "<" => Expression.LessThan(propertyExp, Expression.Constant(value, fieldExpression.ReturnType)),
                "<=" => Expression.LessThanOrEqual(propertyExp, Expression.Constant(value, fieldExpression.ReturnType)),
                ">" => Expression.GreaterThan(propertyExp, Expression.Constant(value, fieldExpression.ReturnType)),
                ">=" => Expression.GreaterThanOrEqual(propertyExp, Expression.Constant(value, fieldExpression.ReturnType)),
                "startswith" => Expression.Call(
                    propertyExp,
                    typeof(string).GetMethod(nameof(string.StartsWith), new Type[] { typeof(string) })!,
                    Expression.Constant(value ?? "")),
                "contains" => Expression.Call(
                    propertyExp,
                    typeof(string).GetMethod(nameof(string.Contains), new Type[] { typeof(string) })!,
                    Expression.Constant(value ?? "")),
                _ => throw new ArgumentOutOfRangeException($"Operator '{@operator}' not supported", nameof(@operator))
            };
            if (negate)
            {
                predicateBody = Expression.Not(predicateBody);
            }
            return Expression.Lambda(predicateBody, fieldExpression.Parameters);
        }

        public static Expression<Func<TItem, TValue?>> AddNullChecks<TItem, TValue>(this Expression<Func<TItem, TValue?>> lambda)
            => (Expression<Func<TItem, TValue?>>)AddNullChecks((Expression)lambda);

        /// <summary>Converts <code>item=>item.Property1.Property2</code> to <code>item => item?.Property1?.Property2</code></summary>
        public static LambdaExpression AddNullChecks(this LambdaExpression lambda)
         => (LambdaExpression)AddNullChecks((Expression)lambda);

        public static Expression AddNullChecks(this Expression expression)
        {
            var nullPropagationVisitor = new NullPropagationVisitor(true);
            return nullPropagationVisitor.Visit(expression);
        }


        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> query, LambdaExpression expression, bool desc = false, bool thenBy = false)
        {
            string methodName = thenBy ? nameof(Queryable.ThenBy) : nameof(Queryable.OrderBy);
            if (desc) methodName += "Descending";
            var method = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == methodName && m.GetParameters().Length == 2);
            method = method.MakeGenericMethod(typeof(TSource), expression.ReturnType);
            return (IOrderedQueryable<TSource>)method.Invoke(null, new object[] { query, expression })!;
        }
        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IOrderedQueryable<TSource> query, LambdaExpression expression, bool desc = false)
        {
            return OrderBy(query, expression, desc, true);
        }
    }


}
