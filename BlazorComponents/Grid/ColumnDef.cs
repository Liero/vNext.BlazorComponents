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
        public ColumnDef()
        {
            CellClassSelector = _ => CellClass;
        }

        [CascadingParameter(Name = "Grid")] internal SimpleGrid<TRow>? Grid { get; set; }

        [Parameter] public string? Header { get; set; }

        [Parameter] public RenderFragment<TRow>? ChildContent { get; set; }

        [Parameter] public bool FitToContent { get; set; }

        [Parameter] public string Width { get; set; } = "auto";

        [Parameter] public string? CellClass { get; set; }

        [Parameter] public Func<TRow, string?> CellClassSelector { get; set; }

        public string GridTemplateColumn => FitToContent ? "max-content" : Width;

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
