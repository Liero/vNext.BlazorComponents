using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using vNext.BlazorComponents.Data;
using vNext.BlazorComponents.Grid;

namespace vNext.BlazorComponents.Demo.Shared
{
    public class ColumnDefEx<TRow> : ColumnDef<TRow>
    {
        [Parameter]
        public Func<TRow, object> ValueGetter { get; set; }

        [Parameter]
        public LambdaExpression SortBy { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (Field != null)
            {
                var propertyLambda = FieldUtils.CreatePropertyLambda<TRow>(Field);
                if (ValueGetter == null)
                {
                    var valueGetter = propertyLambda.Compile();
                    ValueGetter = rowData => valueGetter.DynamicInvoke(rowData);
                }
                if (SortBy == null)
                {
                    SortBy = propertyLambda;
                }
                if (ChildContent == null)
                {
                    ChildContent = item => builder =>
                    {
                        builder.AddContent(0, ValueGetter(item));
                    };
                }
            }
        }
    }
}
