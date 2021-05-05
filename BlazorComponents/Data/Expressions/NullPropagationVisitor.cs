using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
            var body = Visit(node.Body);
            var expectedReturnType = MakeNullableType(node.ReturnType);
            if (body.Type != expectedReturnType)
            {
                body = Expression.Convert(body, expectedReturnType);
            }
            return Expression.Lambda(body, node.Parameters);
        }

        protected override Expression VisitUnary(UnaryExpression propertyAccess)
        {
            if (propertyAccess.Operand is MemberExpression mem)
                return VisitMember(mem);

            if (propertyAccess.Operand is MethodCallExpression met)
                return VisitMethodCall(met);

            if (propertyAccess.Operand is ConditionalExpression cond)
                return Expression.Condition(
                        test: cond.Test,
                        ifTrue: MakeNullable(Visit(cond.IfTrue)),
                        ifFalse: MakeNullable(Visit(cond.IfFalse)));

            return base.VisitUnary(propertyAccess);
        }

        protected override Expression VisitMember(MemberExpression propertyAccess)
        {
            if (propertyAccess.Expression == null)
            {
                throw new ArgumentException($"The parameter member {propertyAccess}.{propertyAccess.Expression} cannot be null", nameof(propertyAccess));
            }
            return Common(propertyAccess.Expression, propertyAccess);
        }

        protected override Expression VisitMethodCall(MethodCallExpression propertyAccess)
        {
            if (propertyAccess.Object == null)
                return base.VisitMethodCall(propertyAccess);

            return Common(propertyAccess.Object, propertyAccess);
        }

        private Expression Common(Expression instance, Expression propertyAccess)
        {
            
            var safe = _recursive ? base.Visit(instance) : instance;
            var caller = Expression.Variable(safe.Type, "caller");
            var assign = Expression.Assign(caller, safe);
            var acess = MakeNullable(new ExpressionReplacer(instance,
                IsNullableStruct(instance) ? caller : RemoveNullable(caller)).Visit(propertyAccess)!);
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

        public static Type MakeNullableType(Type type)
        {
            return type.IsValueType && Nullable.GetUnderlyingType(type) == null
                ? typeof(Nullable<>).MakeGenericType(type)
                : type;
        }

        public static Expression MakeNullable(Expression ex)
        {
            if (IsNullable(ex))
                return ex;

            return Expression.Convert(ex, typeof(Nullable<>).MakeGenericType(ex.Type));
        }

        private static bool IsNullable(Expression ex)
        {
            return !ex.Type.IsValueType || Nullable.GetUnderlyingType(ex.Type) != null;
        }

        private static bool IsNullableStruct(Expression ex)
        {
            return ex.Type.IsValueType && Nullable.GetUnderlyingType(ex.Type) != null;
        }

        private static Expression RemoveNullable(Expression ex)
        {
            if (IsNullableStruct(ex))
                return Expression.Convert(ex, ex.Type.GenericTypeArguments[0]);

            return ex;
        }

        private class ExpressionReplacer : ExpressionVisitor
        {
            private readonly Expression _oldEx;
            private readonly Expression _newEx;

            internal ExpressionReplacer(Expression oldEx, Expression newEx)
            {
                _oldEx = oldEx;
                _newEx = newEx;
            }

            public override Expression? Visit(Expression? node)
            {
                if (node == _oldEx)
                    return _newEx;

                return base.Visit(node);
            }
        }
    }
}
