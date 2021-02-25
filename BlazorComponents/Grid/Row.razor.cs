using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vNext.BlazorComponents.Grid;

namespace vNext.BlazorComponents.Grid
{
    public partial class Row<TRow> : IDisposable
    {
        private bool _shouldRender = true;
        private List<CellRef>? cellRefs;

        [Parameter] public TRow? Data { get; set; }

        [CascadingParameter(Name = "Grid")] public SimpleGrid<TRow>? Grid { get; set; }

        public void Refresh(bool refreshCells = true)
        {
            _shouldRender = true;
            if (refreshCells && cellRefs != null)
            {
                cellRefs.ForEach(c => c.Ref?.Refresh());
            }
        }


        protected override void OnInitialized() => Grid!.AddRow(this);

        protected override bool ShouldRender() => _shouldRender;

        protected override void OnAfterRender(bool firstRender)
        {
            _shouldRender = false;
            base.OnAfterRender(firstRender);
        }

        private string ResolveCssClass()
        {
            string result = "sg-row ";
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
