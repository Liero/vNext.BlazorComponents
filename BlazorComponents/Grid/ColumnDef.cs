using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Grid
{
    public class ColumnDef<TRow> : ComponentBase
    {
        private string? _frozenLeft;

        [CascadingParameter(Name = "Grid")] internal SimpleGrid<TRow>? Grid { get; set; }

        [Parameter] public string? Header { get; set; }
        [Parameter] public string? HeaderClass { get; set; }
        [Parameter] public Func<ColumnDef<TRow>, string?>? HeaderClassSelector { get; set; }

        [Parameter] public RenderFragment<TRow>? ChildContent { get; set; }

        [Parameter] public bool FitToContent { get; set; }

        [Parameter] public string Width { get; set; } = "auto";

        [Parameter] public string? CellClass { get; set; }

        [Parameter] public Func<Cell<TRow>, string?>? CellClassSelector { get; set; }

        public string GridTemplateColumn => FitToContent ? "max-content" : Width;

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
                        var widths = Grid!.ColumnDefinitions
                            .TakeWhile(c => c != this)
                            .Select(c => c.Width);
                        _frozenLeft = $"left: calc({string.Join(" + ", widths)});";
                    }
                }
                return _frozenLeft;
            }
        }

        protected override void OnInitialized()
        {
            Grid?.AddColumnDefinition(this);
            Grid?.Refresh();
        }

        public void Dispose()
        {
            Grid?.ColumnDefinitions.Remove(this);
        }
    }
}
