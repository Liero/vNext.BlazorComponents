﻿using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Grid
{
    public class ColumnDef<TRow> : ComponentBase
    {
        private string? _frozenLeft;

        [CascadingParameter(Name = "Grid")] internal SimpleGrid<TRow>? Grid { get; set; }

        [Parameter] public bool IsDefault { get; set; }


        [Parameter] public string? Field { get; set; }

        [Parameter] public string? Header { get; set; }
        [Parameter] public RenderFragment<Header<TRow>>? HeaderTemplate { get; set; }
        [Parameter] public string? HeaderClass { get; set; }
        [Parameter] public Func<ColumnDef<TRow>, string?>? HeaderClassSelector { get; set; }

        [Parameter] public RenderFragment<TRow>? ChildContent { get; set; }

        [Parameter] public string? Width { get; set; }

        [Parameter] public bool IsVisible { get; set; } = true;
        [Parameter] public int? Order { get; set; }

        [Parameter] public string? CellClass { get; set; }

        [Parameter] public Func<Cell<TRow>, string?>? CellClassSelector { get; set; }

        public string GridTemplateWidth => ActualWidth.HasValue ? $"{ActualWidth}px" : Width ?? Grid?.DefaultColumn?.Width ?? "min-content";

        public bool IsFrozen { get; internal set; }
        public int Index { get; internal set; } = -1;

        /// <summary>
        /// return css style if columns is frozen;
        /// e.g. left: calc(100px + 200px);
        /// </summary>
        internal string? FrozenLeft
        {
            get
            {
                if (IsFrozen && _frozenLeft == null)
                {
                    if (Index == 0)
                    {
                        _frozenLeft = "left: 0;";
                    }
                    else
                    {
                        var widths = Grid!.VisibleColumns
                            .TakeWhile(c => c != this)
                            .Select(c => c.GridTemplateWidth);
                        _frozenLeft = $"left: calc({string.Join(" + ", widths)});";
                    }
                }
                return _frozenLeft;
            }
        }

        internal double? ActualWidth { get; set; }

        internal void Invalidate()
        {
            _frozenLeft = null;
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            var old = (Order, IsVisible);
            await base.SetParametersAsync(parameters);

            var updated = (Order, IsVisible);

            if (old != updated)
            {
                Grid?.InvalidateColumns();
            }
        }

        protected override void OnInitialized()
        {
            Order ??= Grid?.ColumnDefinitions.Count + 1;
            Grid?.AddColumnDefinition(this);
            Grid?.InvalidateColumns();
        }

        public void Dispose()
        {
            Grid?.ColumnDefinitions.Remove(this);
        }
    }
}
