using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using vNext.BlazorComponents.Data;
using vNext.BlazorComponents.Data.Expressions;

namespace BlazorComponents.Demo.Tests
{
    [TestClass]
    public class NullPropagationVisitorTests
    {
        record Foo(string? Name, Foo? Child = null);

        [TestMethod]
        public void NullPropagationVisitor_AddsNullChecks()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.

            Expression<Func<Foo, char>> expression = foo => foo.Child.Name.ToString()[0];

#pragma warning restore CS8602 // Dereference of a possibly null reference.

            //sut
            var visitor = new NullPropagationVisitor(true);

            //action
            var expressionWithNullChecks = (Expression<Func<Foo, char?>>)visitor.Visit(expression);
            var funcGetChildFirstChar = expressionWithNullChecks.Compile();

            Assert.IsNull(funcGetChildFirstChar(new Foo("parent")));
            Assert.IsNull(funcGetChildFirstChar(new Foo("parent", new Foo(null))));
            Assert.AreEqual('c', funcGetChildFirstChar(new Foo("parent", new Foo("child"))));
        }


    }
}
