using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Grid
{
    public partial class SimpleGrid<TRow> : ComponentBase, IDisposable
    {
        private bool _shouldRender = true;
        private string? _gridTemplateColumns;
        private ElementReference? _elementRef;
        private Virtualize<RowWrapper>? _virtualizeRef;
        private DotNetObjectReference<SimpleGrid<TRow>>? _dotNetRef;
        private IJSObjectReference? _jsApi;
        private HashSet<Row<TRow>> _rows = new();
        private ICollection<TRow>? _items;
        private ICollection<RowWrapper>? _wrappedItems;
        private ICollection<TRow> _selectedItems = Array.Empty<TRow>();
        private int? _providedTotalCount;
        #region parameters

        [Inject] protected IJSRuntime? JS { get; set; }

        [Parameter]
        public ICollection<TRow>? Items
        {
            get => _items; set
            {
                _items = value;
                _wrappedItems = _items?.Select((item, i) => new RowWrapper(i, item)).ToList();
                Invalidate();
            }
        }

        [Parameter] public Func<Row<TRow>, string?>? RowClassSelector { get; set; }

        [Parameter] public int OverscanCount { get; set; } = 3;

        [Parameter] public List<ColumnDef<TRow>> ColumnDefinitions { get; set; } = new List<ColumnDef<TRow>>();


        [Parameter] public int FrozenColumns { get; set; } = 0;

        [Parameter] public string? CssClass { get; set; }


        [Parameter] public RenderFragment ChildContent { get; set; } = default(RenderFragment)!;

        [Parameter] public EventCallback<ReadEventArgs<TRow>> OnRead { get; set; }
        [Parameter] public EventCallback<RowMouseEventArgs<TRow>> OnRowClick { get; set; }
        [Parameter] public EventCallback<RowMouseEventArgs<TRow>> OnRowContextMenu { get; set; }
        [Parameter] public EventCallback<HeaderMouseEventArgs<TRow>> OnHeaderClick { get; set; }


        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

        [Parameter]
        public ICollection<TRow> SelectedItems
        {
            get => _selectedItems;
            set
            {
                if (_selectedItems != value)
                {
                    var affectedItems = _selectedItems.Concat(value).Distinct();
                    _selectedItems = value;
                    foreach (var item in affectedItems)
                    {
                        FindRow(item)?.Refresh(false);
                    }
                }
            }
        }

        public ColumnDef<TRow>? DefaultColumn { get; internal set; }

        #endregion
        internal List<Header<TRow>> Headers { get; } = new List<Header<TRow>>();

        public string GridTemplateColumns =>
            _gridTemplateColumns ?? (_gridTemplateColumns = string.Join(' ', ColumnDefinitions.Select(c => c.GridTemplateWidth)));

        /// <summary>
        /// Returns <see cref="Row{TRow}"/> instances rendered by Virtualize component.
        /// </summary>
        /// <remarks>Not all items in <see cref="Items"/> have corresponding <see cref="Row{TRow}"/> due to virtualization</remarks>
        public IEnumerable<Row<TRow>> GetVisibleRows() => _rows.AsEnumerable();

        public void Refresh()
        {
            Invalidate();
            StateHasChanged();
        }
        public async Task ReloadAsync()
        {
            if (_virtualizeRef != null)
            {
                await _virtualizeRef.RefreshDataAsync();
            }
        }

        public async Task<Cell<TRow>?> GetCellFromPoint(double clientX, double clientY)
        {
            var result = await JS!.InvokeAsync<int[]>("vNext.SimpleGrid.getCellFromPoint", new { clientX, clientY });
            int colIndex = result[0];
            int rowIndex = result[1];
            if (colIndex < 0)
            {
                return null;
            }
            var colDef = ColumnDefinitions[colIndex];
            var row = _rows.FirstOrDefault(r => r.Index == rowIndex);
            return row?.FindCell(colDef);
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
                for (int i = 0; i < columnWidths.Length; i++)
                {
                    ColumnDefinitions[i].ActualWidth = columnWidths[i];
                }

                //frozen columns have style.left= calculated, which must be refreshed.  Dynamic css classIt would be nicer though
                for (int i = columnIndex + 1; i <= FrozenColumns; i++)
                {
                    var column = ColumnDefinitions[i];
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


        internal void Invalidate()
        {
            _gridTemplateColumns = null;
            _shouldRender = true;
            foreach (var row in _rows)
            {
                row.Refresh();
            }
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
                _shouldRender = false;
            }
        }

        protected override bool ShouldRender() => _shouldRender;

        public int? TotalCount { get => Items?.Count ?? _providedTotalCount; }

        protected virtual async ValueTask<ItemsProviderResult<RowWrapper>> ProvideItems(ItemsProviderRequest request)
        {
            var args = new ReadEventArgs<TRow>(request.StartIndex, request.Count);
            await OnRead.InvokeAsync(args);
            _providedTotalCount = args.Total;
            var refs = args.Items.Select((item, index) => new RowWrapper(request.StartIndex + index, item));
            return new ItemsProviderResult<RowWrapper>(refs, args.Total.GetValueOrDefault());
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
    }
}
