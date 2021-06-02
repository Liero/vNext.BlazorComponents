using Microsoft.AspNetCore.Components;

namespace vNext.BlazorComponents.Grid
{
    public partial class Cell<TRow> : ComponentBase
    {
        protected internal bool ShouldRenderFlag { get; set; } = true;

        [CascadingParameter] public Row<TRow> Row { get; set; } = default!;
        [Parameter] public ColumnDef<TRow> ColumnDef { get; set; } = default!;

        public TRow Data => Row.Data!;

        internal void Invalidate()
        {
            ShouldRenderFlag = true;
            //clean all precalculated assets here
        }

        public void Refresh()
        {
            Invalidate();
            StateHasChanged();
        }

        protected override bool ShouldRender() => ShouldRenderFlag;

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
