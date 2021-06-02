using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Grid
{
    public partial class Row<TRow> : IDisposable
    {
        protected internal bool ShouldRenderFlag = true;
        private List<CellRef>? _cellRefs;

        [Parameter] public TRow? Data { get; set; }
        [Parameter] public int Index { get; set; }
        [CascadingParameter(Name = "Grid")] public SimpleGrid<TRow>? Grid { get; set; }

        public bool IsSelected => Grid!.SelectedItems?.Contains(Data!) == true;

        internal void Invalidate(bool invalidateCells = true)
        {
            ShouldRenderFlag = true;
            if (invalidateCells && _cellRefs != null)
            {
                _cellRefs.ForEach(c => c.Ref?.Invalidate());
            }
            //clear all precalulated assets here
        }

        public void Refresh(bool refreshCells = true)
        {
            Invalidate(refreshCells);
            StateHasChanged();
        }

        public Cell<TRow>? FindCell(ColumnDef<TRow> column)
        {
            return _cellRefs?.Find(c => c.Column == column)?.Ref;
        }

        protected override void OnInitialized() => Grid!.AddRow(this);

        protected override bool ShouldRender() => ShouldRenderFlag;

        protected virtual Task OnClick(MouseEventArgs mouseEvent) =>
            Grid!.OnRowClick.InvokeAsync(new RowMouseEventArgs<TRow>(this, mouseEvent));

        protected virtual Task OnContextMenu(MouseEventArgs mouseEvent) =>
            Grid!.OnRowContextMenu.InvokeAsync(new RowMouseEventArgs<TRow>(this, mouseEvent));

        private string ResolveCssClass()
        {
            string result = "sg-row ";
            if (IsSelected)
            {
                result += "sg-row-selected ";
            }
            if (Grid!.RowClassSelector != null)
            {
                result += Grid.RowClassSelector(this);
            }
            return result;
        }

        void IDisposable.Dispose() => Grid!.RemoveRow(this);

        class CellRef
        {
            public CellRef(ColumnDef<TRow> column) => Column = column;

            public ColumnDef<TRow> Column;
            public Cell<TRow>? Ref;
        }
    }


}
