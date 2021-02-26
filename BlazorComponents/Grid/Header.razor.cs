using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Grid
{
    public partial class Header<TRow> : IDisposable
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Parameter] public ColumnDef<TRow> ColumnDef { get; set; }
        [CascadingParameter(Name = "Grid")] public SimpleGrid<TRow>? Grid { get; set; }


#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void Refresh()
        {
            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Grid?.Headers.Add(this);
        }

        private string ResolveCssClass()
        {
            string result = "sg-cell sg-header-cell ";
            if (ColumnDef.IsFrozen)
            {
                result += "sg-cell-frozen ";
            }
            if (ColumnDef.HeaderClass != null)
            {
                result += ColumnDef.HeaderClass;
            }
            if (ColumnDef.HeaderClassSelector != null)
            {
                result += " " + ColumnDef.HeaderClassSelector(ColumnDef);
            }
            return result;
        }

        void IDisposable.Dispose()
        {
            Grid?.Headers.Remove(this);
        }
    }
}
