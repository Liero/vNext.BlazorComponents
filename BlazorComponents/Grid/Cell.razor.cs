using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace vNext.BlazorComponents.Grid
{
    public partial class Cell<TRow> : ComponentBase
    {
        private bool _shouldRender = true;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Parameter] public TRow Data { get; set; }
        [Parameter] public ColumnDef<TRow> ColumnDef { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        public void Refresh()
        {
            _shouldRender = true;
            StateHasChanged();
        }

        protected override bool ShouldRender() => _shouldRender;

        protected override void OnAfterRender(bool firstRender)
        {
            _shouldRender = false;
            base.OnAfterRender(firstRender);
        }

        private string ResolveCssClass()
        {
            string result = "sg-cell ";
            if (ColumnDef.IsFrozen)
            {
                result += "sg-cell-frozen ";
            }
            if (ColumnDef.CellClass != null)
            {
                result += ColumnDef.CellClass;
            }
            if (ColumnDef.CellClassSelector != null)
            {
                result += " " + ColumnDef.CellClassSelector(this);
            }
            return result;
        }
    }
}
