using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using vNext.BlazorComponents.Data;
using vNext.BlazorComponents.Data.Expressions;

namespace BlazorComponents.Demo.Tests
{
    [TestClass]
    public class FieldUtilsTests
    {
        record Foo(string? Name);

        [TestMethod]
        public void FieldUtils_AddNullChecks_WorksWithCastedLambda()
        {
            Expression<Func<Foo, object?>> fieldExpression = foo => foo.Name!.Length;

            Expression<Func<Foo, object?>> expressionWithNullChecks = fieldExpression.AddNullChecks();
            
            Func<Foo, object?> valueGetter = expressionWithNullChecks.Compile();

            Assert.AreEqual(3, valueGetter(new Foo("abc")));
            Assert.AreEqual(null, valueGetter(new Foo(null)));

            Expression<Func<Foo, object?>> fieldExpression2 = foo => foo.Name!;
            Expression<Func<Foo, object?>> expressionWithNullChecks2 = fieldExpression2.AddNullChecks();
        }


        [TestMethod]
        public void FieldUtils_ValueGetter_CanHandleNullsInPropertyPath()
        {
            const string field = "Name.Length";

            var expr = FieldUtils.CreatePropertyLambda(typeof(Foo), field)
                .AddNullChecks()
                .CastFunc<Foo, object?>();

            Func<Foo, object?> valueGetter = expr.Compile();

            Assert.AreEqual(3, valueGetter(new Foo("abc")));
            Assert.AreEqual(null, valueGetter(new Foo(null)));
        }

        [TestMethod]
        public void FieldUtils_CanAssign()
        {
            Assert.IsFalse(FieldUtils.CreatePropertyLambda(typeof(Foo), "Name.Length").CanAssign(), "string.Length");
            Assert.IsTrue(FieldUtils.CreatePropertyLambda(typeof(Foo), "Name").CanAssign(), "Foo.Name (writable string property)");
        }

        [TestMethod]
        public void FieldUtils_Assign()
        {
            Action<Foo, object?> valueSetter = FieldUtils.CreatePropertyLambda(typeof(Foo), "Name")
                .CreateAssignLambda<Foo, object?>()
                .Compile();

            var foo = new Foo(null);
            valueSetter(foo, "NewValue");

            Assert.AreEqual("NewValue", foo.Name);
        }

        [TestMethod]
        public void FieldUtils_Predicate_CanHandleNullsInPropertyPath()
        {
            const string field = "Name.Length";
            Func<Foo, bool> predicate = FieldUtils.CreatePropertyLambda<Foo>(field)
                .AddNullChecks()
                .CreatePredicateLambda<Foo>("==", 3)
                .Compile();

            var foo1 = new Foo(null);
            var foo2 = new Foo("abc");
            var foo3 = new Foo("def");


            Assert.IsFalse(predicate(foo1));
            Assert.IsTrue(predicate(foo2));
            Assert.IsTrue(predicate(foo3));
        }

        [TestMethod]
        public void FindExpressionVisitor_Find()
        {
            Expression<Func<Foo, object?>> f = foo => (object?)foo.Name!.Length;
            var memberExpression = FindExpressionVisitor.Find(f, f => f is MemberExpression);
            Assert.IsInstanceOfType(memberExpression, typeof(MemberExpression));
            Assert.AreEqual("foo.Name.Length", memberExpression!.ToString());
        }


    }
}
