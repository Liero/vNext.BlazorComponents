using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Data.Expressions
{
    public class FindExpressionVisitor : ExpressionVisitor
    {
        public static Expression? Find(Expression node, Func<Expression, bool> predicate)
        {
            var visitor = new FindExpressionVisitor(predicate);
            visitor.Visit(node);
            return visitor._result;
        }

        private readonly Func<Expression, bool> _predicate;
        private Expression? _result;

        private FindExpressionVisitor(Func<Expression, bool> predicate)
        {
            _predicate = predicate;
        }

        public override Expression? Visit(Expression? node)
        {
            if (node != null && _predicate(node))
            {
                _result = node;
                return _result;
            }
            else
            {
                return base.Visit(node);
            }
        }
    }
}
