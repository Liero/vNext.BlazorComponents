/* originated from https://github.com/leandromoh/NullPropagationVisitor  */

#nullable disable
using System;
using System.Linq.Expressions;
using static vNext.BlazorComponents.Data.Expressions.HelperMethods;

namespace vNext.BlazorComponents.Data.Expressions
{
    public class NullPropagationVisitor : ExpressionVisitor
    {
        private readonly bool _recursive;

        public NullPropagationVisitor(bool recursive)
        {
            _recursive = recursive;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Expression visitedBody = Visit(node.Body);
            
            if (visitedBody.Type.IsAssignableTo(node.ReturnType))
            {
                //creates lambda of the same type
                return Expression.Lambda(node.Type, visitedBody, node.Parameters);
            }
            //infer new return type based on body
            return Expression.Lambda(visitedBody, node.Parameters);
        }

        protected override Expression VisitUnary(UnaryExpression propertyAccess)
        {
            if (propertyAccess.NodeType == ExpressionType.Convert)
                return VisitConvert(propertyAccess);

            if (propertyAccess.Operand is MemberExpression member)
                return VisitMember(member);

            if (propertyAccess.Operand is MethodCallExpression method)
                return VisitMethodCall(method);

            if (propertyAccess.Operand is ConditionalExpression condition)
                return VisitConditional(condition);

            return base.VisitUnary(propertyAccess);
        }

        protected override Expression VisitConditional(ConditionalExpression cond)
        {
            return Expression.Condition(
                        test: cond.Test,
                        ifTrue: MakeNullable(Visit(cond.IfTrue)),
                        ifFalse: MakeNullable(Visit(cond.IfFalse)));
        }

        protected override Expression VisitMember(MemberExpression propertyAccess)
        {
            return Common(propertyAccess.Expression, caller =>
            {
                return MakeNullable(new ExpressionReplacerVisitor(propertyAccess.Expression,
                IsNullableStruct(propertyAccess.Expression) ? caller : RemoveNullable(caller)).Visit(propertyAccess));
            });
        }

        protected override Expression VisitMethodCall(MethodCallExpression propertyAccess)
        {
            if (propertyAccess.Object == null)
                return base.VisitMethodCall(propertyAccess);

            return Common(propertyAccess.Object, caller =>
            {
                return MakeNullable(new ExpressionReplacerVisitor(propertyAccess.Object,
                IsNullableStruct(propertyAccess.Object) ? caller : RemoveNullable(caller)).Visit(propertyAccess));
            });
        }

        protected virtual Expression VisitConvert(UnaryExpression propertyAccess)
        {
            if (propertyAccess.NodeType != ExpressionType.Convert)
                throw new InvalidOperationException("invalid call");

            return Common(propertyAccess.Operand, caller =>
            {
                return Expression.Convert(RemoveNullable(caller), propertyAccess.Type);
            });
        }

        private BlockExpression Common(Expression instance, Func<Expression, Expression> callback)
        {
            var safe = _recursive ? base.Visit(instance) : instance;

            if (!IsNullable(safe.Type))
                throw new InvalidOperationException($"Can not apply operand on type {safe.Type.Name}. Only nullable are allowed.");

            // assign expression in the left side of the operator '?.' to a variable to evaluate once
            var caller = Expression.Variable(safe.Type, "caller");
            var assign = Expression.Assign(caller, safe);

            var acess = MakeNullable(callback(caller));
            var ternary = Expression.Condition(
                test: Expression.Equal(caller, Expression.Constant(null)),
                ifTrue: Expression.Constant(null, acess.Type),
                ifFalse: acess);

            return Expression.Block(
                    type: acess.Type,
                    variables: new[]
                    {
                            caller,
                    },
                    expressions: new Expression[]
                    {
                            assign,
                            ternary,
                    });
        }
    }

    internal class ExpressionReplacerVisitor : ExpressionVisitor
    {
        private readonly Expression _oldEx;
        private readonly Expression _newEx;

        internal ExpressionReplacerVisitor(Expression oldEx, Expression newEx)
        {
            _oldEx = oldEx;
            _newEx = newEx;
        }

        public override Expression Visit(Expression node)
        {
            if (node == _oldEx)
                return _newEx;

            return base.Visit(node);
        }
    }

    static class HelperMethods
    {
        private static bool IsValueType(this Type type)
        {
#if !NETSTANDARD1_1
            return type.IsValueType;
#else
            return System.Reflection.IntrospectionExtensions.GetTypeInfo(type).IsValueType;
#endif
        }

        public static Expression MakeNullable(Expression ex)
        {
            if (IsNullable(ex))
                return ex;

            return Expression.Convert(ex, MakeNullable(ex.Type));
        }

        public static Type MakeNullable(Type type)
        {
            if (IsNullable(type))
                return type;

            return typeof(Nullable<>).MakeGenericType(type);
        }

        public static bool IsNullable(Expression ex)
        {
            return IsNullable(ex.Type);
        }

        public static bool IsNullable(Type type)
        {
            return !type.IsValueType() || (Nullable.GetUnderlyingType(type) != null);
        }

        public static bool IsNullableStruct(Expression ex)
        {
            return ex.Type.IsValueType() && (Nullable.GetUnderlyingType(ex.Type) != null);
        }

        public static bool IsReferenceType(Expression ex)
        {
            return !ex.Type.IsValueType();
        }

        public static Expression RemoveNullable(Expression ex)
        {
            if (IsNullableStruct(ex))
                return Expression.Convert(ex, ex.Type.GenericTypeArguments[0]);

            return ex;
        }
    }
}
