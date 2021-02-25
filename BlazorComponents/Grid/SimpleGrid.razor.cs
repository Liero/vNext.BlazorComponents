#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
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
        private DotNetObjectReference<SimpleGrid<TRow>>? _dotNetRef;
        private HashSet<Row<TRow>> _rows = new();
        private ICollection<TRow>? _items;
        #region parameters

        [Inject] protected IJSRuntime? JS { get; set; }

        [Parameter]
        public ICollection<TRow>? Items
        {
            get => _items; set
            {
                _items = value;
                Invalidate();
            }
        }

        [Parameter] public EventCallback<ReadEventArgs<TRow>> OnRead { get; set; }
        [Parameter] public Func<Row<TRow>, string?>? RowClassSelector { get; set; }

        [Parameter] public int OverscanCount { get; set; } = 3;

        [Parameter] public List<ColumnDef<TRow>> ColumnDefinitions { get; set; } = new List<ColumnDef<TRow>>();

        [Parameter] public int FrozenColumns { get; set; } = 0;

        [Parameter] public string? CssClass { get; set; }


        [Parameter] public RenderFragment ChildContent { get; set; } = default(RenderFragment)!;

        [Parameter] public EventCallback<TRow> RowClicked { get; set; }


        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }


        #endregion

        public string GridTemplateColumns =>
            _gridTemplateColumns ?? (_gridTemplateColumns = string.Join(' ', ColumnDefinitions.Select(c => c.GridTemplateColumn)));

        public void Refresh()
        {
            Invalidate();
            StateHasChanged();
        }

        public Row<TRow>? FindRow(TRow row) => _rows.FirstOrDefault(r => Object.Equals(r.Data, row));

        internal void AddColumnDefinition(ColumnDef<TRow> columnDef)
        {
            ColumnDefinitions.Add(columnDef);
            Refresh();
        }
        internal void AddRow(Row<TRow> row) => _rows.Add(row);
        internal void RemoveRow(Row<TRow> row) => _rows.Remove(row);

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
                await JS!.InvokeAsync<IJSObjectReference>("vNext.initGrid", _elementRef, _dotNetRef);
            }
            if (_rows.Count > 0)
            {
                _shouldRender = false;
            }
        }

        protected override bool ShouldRender() => _shouldRender;

        protected virtual async ValueTask<ItemsProviderResult<TRow>> ProvideItems(ItemsProviderRequest request)
        {
            var args = new ReadEventArgs<TRow>(request.StartIndex, request.Count);
            await OnRead.InvokeAsync(args);
            return new ItemsProviderResult<TRow>(args.Items, args.Total.GetValueOrDefault());
        }

        void IDisposable.Dispose()
        {
            _dotNetRef?.Dispose();
        }
    }

    public class ReadEventArgs<TRow>
    {

        public ReadEventArgs(int startIndex, int count)
        {
            StartIndex = startIndex;
            Count = count;
        }

        public int StartIndex { get; }
        public int Count { get; }
        public int? Total { get; set; }
        public IEnumerable<TRow> Items { get; set; } = Enumerable.Empty<TRow>();
    }
}
