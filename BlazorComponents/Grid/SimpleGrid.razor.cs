using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Grid
{
    public partial class SimpleGrid<TRow> : ComponentBase, IDisposable
    {
        public static float DefaultRowHeight { get; set; } = 32;
        public static int DefaultOverscanCount { get; set; } = 10;
        protected internal bool ShouldRenderFlag = true;

        private string? _gridTemplateColumns;
        private ElementReference? _elementRef;
        private Virtualize<RowWrapper>? _virtualizeRef;
        private DotNetObjectReference<SimpleGrid<TRow>>? _dotNetRef;
        private IJSObjectReference? _jsApi;
        private HashSet<Row<TRow>> _rows = new();
        private ICollection<TRow>? _items;
        private ICollection<RowWrapper>? _wrappedItems;
        private ICollection<TRow> _selectedItems = Array.Empty<TRow>();

        #region parameters

        [Inject] protected IJSRuntime JS { get; set; } = default!;

        [Parameter] public string? @class { get; set;}

        [Parameter]
        public ICollection<TRow>? Items
        {
            get => _items; set
            {
                if (_items != value)
                {
                    _items = value;
                    Invalidate(false);
                }
            }
        }

        [Parameter] public Func<Row<TRow>, string?>? RowClassSelector { get; set; }

        [Parameter] public float RowHeight { get; set; } = DefaultRowHeight;
        [Parameter] public int OverscanCount { get; set; } = DefaultOverscanCount;

        [Parameter] public List<ColumnDef<TRow>> ColumnDefinitions { get; set; } = new List<ColumnDef<TRow>>();


        [Parameter] public int FrozenColumns { get; set; } = 0;

        /// <summary>
        /// additional CSS class of the .simple-grid element. 
        /// In order to set css class to the container element use class="your-class"
        /// </summary>
        [Parameter] public string? CssClass { get; set; }


        [Parameter] public RenderFragment ChildContent { get; set; } = default(RenderFragment)!;

        [Parameter] public EventCallback<ReadEventArgs<TRow>> OnRead { get; set; }
        [Parameter] public EventCallback<RowMouseEventArgs<TRow>> OnRowClick { get; set; }
        [Parameter] public EventCallback<RowMouseEventArgs<TRow>> OnRowContextMenu { get; set; }
        [Parameter] public EventCallback<RowKeyboardEventArgs<TRow>> OnRowKeyPress { get; set; }
        [Parameter] public EventCallback<HeaderMouseEventArgs<TRow>> OnHeaderClick { get; set; }
        [Parameter] public EventCallback<RowDeleteEventArgs<TRow>> OnRowDelete { get; set; }


        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

        [Parameter]
        public ICollection<TRow> SelectedItems
        {
            get => _selectedItems;
            set
            {
                if (_selectedItems != value)
                {
                    var oldSelectedRows = GetVisibleRows().Where(r => r.IsSelected);
                    var newRows = value.Select(data => FindRow(data));
                    var affectedRows = oldSelectedRows.Concat(newRows).Distinct().ToArray();
                    
                    _selectedItems = value;
                    foreach (var row in affectedRows)
                    {
                        row?.Refresh(false);
                    }
                }
            }
        }

        public ColumnDef<TRow>? DefaultColumn { get; internal set; }

        #endregion
        internal List<Header<TRow>> Headers { get; } = new List<Header<TRow>>();

        public IEnumerable<ColumnDef<TRow>> VisibleColumns => ColumnDefinitions.Where(c => c.IsVisible);

        public string GridTemplateColumns =>
            _gridTemplateColumns ?? (_gridTemplateColumns = string.Join(' ', VisibleColumns.Select(c => c.GridTemplateWidth)));

        /// <summary>
        /// Returns <see cref="Row{TRow}"/> instances rendered by Virtualize component.
        /// </summary>
        /// <remarks>Not all items in <see cref="Items"/> have corresponding <see cref="Row{TRow}"/> due to virtualization</remarks>
        public IEnumerable<Row<TRow>> GetVisibleRows() => _rows.AsEnumerable();

        /// <summary>
        /// Grid will rerender next time StateHasChanged is called
        /// </summary>
        public void Invalidate(bool invalidateCells)
        {
            _gridTemplateColumns = null;
            ShouldRenderFlag = true;

            _wrappedItems = _items?.Select((item, i) => new RowWrapper(i, item)).ToList();

            foreach (var row in _rows)
            {
                row.Invalidate(invalidateCells);
            }
        }


        /// <param name="refreshCells">when false, cells won't be rerendered. E.g when rows collection is changed</param>
        public void Refresh(bool refreshCells = true)
        {
            Invalidate(refreshCells);
            StateHasChanged();
        }

        /// <summary>
        /// triggers OnRead event.
        /// </summary>
        public async Task ReloadAsync()
        {
            if (_virtualizeRef != null)
            {
                await _virtualizeRef.RefreshDataAsync();
            }
            Refresh(true);
        }

        public async Task<Cell<TRow>?> GetCellFromPoint(double clientX, double clientY)
        {
            var result = await JS.InvokeAsync<int[]?>("vNext.SimpleGrid.getCellFromPoint", new { clientX, clientY });
            if (result == null)
            {
                return null;
            }
            int colIndex = result[0];
            int rowIndex = result[1];
            if (colIndex < 0)
            {
                return null;
            }
            var colDef = VisibleColumns.ElementAt(colIndex);
            var row = _rows.FirstOrDefault(r => r.Index == rowIndex);
            return row?.FindCell(colDef);
        }

        public async Task ScrollToIndex(int index, bool smooth = true)
        {
            if (_jsApi == null) throw new InvalidOperationException("JS Api not initalized");
            await _jsApi.InvokeVoidAsync("scrollToIndex", index, smooth ? "smooth" : "auto");
        }

        public IEnumerable<Header<TRow>> GetHeaders() => Headers;

        public Row<TRow>? FindRow(TRow row) => _rows.FirstOrDefault(r => Object.Equals(r.Data, row));

        internal void AddColumnDefinition(ColumnDef<TRow> columnDef)
        {
            if (columnDef.IsDefault)
            {
                DefaultColumn = columnDef;
            }
            else
            {
                ColumnDefinitions.Add(columnDef);
            }
            Refresh();
        }
        internal void AddRow(Row<TRow> row) => _rows.Add(row);
        internal void RemoveRow(Row<TRow> row) => _rows.Remove(row);

        [JSInvokable]
        public async Task OnResizeInterop(int columnIndex, double[] columnWidths)
        {
            await InvokeAsync(() =>
            {
                var visibleColumns = VisibleColumns.ToArray();
                for (int i = 0; i < columnWidths.Length; i++)
                {
                    visibleColumns[i].ActualWidth = columnWidths[i];
                }

                //frozen columns have style.left= calculated, which must be refreshed.  Dynamic css classIt would be nicer though
                for (int i = columnIndex + 1; i <= FrozenColumns; i++)
                {
                    var column = visibleColumns[i];
                    column.Invalidate();
                    Headers.Find(h => h.ColumnDef == column)?.Refresh();
                    foreach (var row in _rows)
                    {
                        row.FindCell(column)?.Refresh();
                        row.Refresh(false);
                    }
                }

                _gridTemplateColumns = null;
                StateHasChanged();
            });
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            _dotNetRef = DotNetObjectReference.Create(this);
            if (firstRender)
            {
                _jsApi = await JS!.InvokeAsync<IJSObjectReference>("vNext.SimpleGrid.init", _elementRef, _dotNetRef);
            }
            if (_rows.Count > 0)
            {
                ShouldRenderFlag = false;
            }
        }

        protected override bool ShouldRender() => ShouldRenderFlag;

        protected virtual async ValueTask<ItemsProviderResult<RowWrapper>> ProvideItems(ItemsProviderRequest request)
        {
            var args = new ReadEventArgs<TRow>(request.StartIndex, request.Count, request.CancellationToken);
            await OnRead.InvokeAsync(args);
            var refs = args.Items.Select((item, index) => new RowWrapper(request.StartIndex + index, item));
            return new ItemsProviderResult<RowWrapper>(refs, args.Total.GetValueOrDefault());
        }

        protected virtual async Task CustomEventOnChange(ChangeEventArgs args)
        {
            if (args.Value is not string str) return;
            var evt = JsonSerializer.Deserialize<CustomEvent>(str, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (evt == null) return;

            var row = _rows?.FirstOrDefault(e => e.Index == evt.RowIndex);
            if (row == null) return;

            if (evt.Name == "Delete")
            {
                await OnRowDelete.InvokeAsync(new RowDeleteEventArgs<TRow>(row));
            }
        }

        void IDisposable.Dispose()
        {
            _dotNetRef?.Dispose();
            _jsApi?.DisposeAsync().GetAwaiter();
        }

        /// <summary>
        /// just to keep index of the row
        /// </summary>
        protected record RowWrapper(int Index, TRow Row);

        private class CustomEvent
        {
            public string Name { get; set; } = default!;            
            public int? RowIndex { get; set; }
            public int? ColIndex { get; set; }
        }
    }

 
}
