using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Grid
{
    public partial class Header<TRow>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Parameter] public ColumnDef<TRow> ColumnDef { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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
    }
}
