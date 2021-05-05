using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using vNext.BlazorComponents.Demo.Data;
using vNext.BlazorComponents.Demo.Shared;
using vNext.BlazorComponents.Grid;

namespace BlazorComponents.Demo.Tests
{

    [TestClass]
    public class ExcelExporterTests
    {
        [TestMethod]
        [SuppressMessage("warning", "BL0005")]
        public void TestMethod1()
        {
            var data = new Product[]
            {
                new () { Id = 1, Name = "abc", Height = 1, Width = 1.2, Length = 0.1, Details = new ProductDetails() },
                new () { Id = 2, Name = "def", Height = 1, Width = 1.2, Length = 0.1, },
                new () { Id = 3, Name = "efg", Height = 1, Width = 1.1, Length = 0.111, },
            };
            var columnDefs = new ColumnDefEx<Product>[]
            {
                new () { ValueGetter = item => item.Name, Header = "2" },
                new () { ValueGetter = item => item.Height, Header = "Height" },
                new () { ValueGetter = item => item.Width, Header = "Width"  },
                new () { ValueGetter = item => item.NullableBoolean, Header = "NullableBoolean" },
                new () { ValueGetter = item => item.Details?.DateTimeProperty, Header = "DateTime" },
            };
           
            ExcelExporter excelExporter = new ExcelExporter()
            {
                MaxColumnWidth = 50,
            };
            excelExporter.Export(@"C:\temp\test.xlsx", data, columnDefs);
        }
    }

}
