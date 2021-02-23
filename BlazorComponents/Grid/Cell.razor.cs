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

        internal void Refresh()
        {
            _shouldRender = true;
        }

        protected override bool ShouldRender() => _shouldRender;

        protected override void OnAfterRender(bool firstRender)
        {
            _shouldRender = false;
            base.OnAfterRender(firstRender);
        }
    }
}
