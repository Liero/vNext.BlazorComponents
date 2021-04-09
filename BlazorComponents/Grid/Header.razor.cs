using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Grid
{
    public partial class Header<TRow> : IDisposable
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Parameter] public ColumnDef<TRow> ColumnDef { get; set; }
        [CascadingParameter(Name = "Grid")] public SimpleGrid<TRow>? Grid { get; set; }


#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void Refresh()
        {
            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Grid?.Headers.Add(this);
        }

        protected virtual string ResolveCssClass()
        {
            var defaultColumn = Grid?.DefaultColumn;

            string result = "sg-cell sg-header-cell ";
            if (ColumnDef.IsFrozen)
            {
                result += "sg-cell-frozen ";
            }
      
            if (ColumnDef.HeaderClass != null)
            {
                result += ColumnDef.HeaderClass;
            }
            else if (defaultColumn?.HeaderClass != null)
            {
                result += defaultColumn.HeaderClass;
            }

            if (defaultColumn?.HeaderClassSelector != null)
            {
                result += " " + defaultColumn.HeaderClassSelector(ColumnDef);
            }
            else if (ColumnDef.HeaderClassSelector != null)
            {
                result += " " + ColumnDef.HeaderClassSelector(ColumnDef);
            }
            return result;
        }

        public string? Text => ColumnDef.Header ?? ColumnDef.Field;

        protected virtual async Task OnHeaderClick(MouseEventArgs evt)
        {
            if (Grid != null) 
            {
                await Grid.OnHeaderClick.InvokeAsync(new HeaderMouseEventArgs<TRow>(this, evt));
            }
        }


        private readonly static RenderFragment<Header<TRow>> DefaultTemplate = new RenderFragment<Header<TRow>>(args => builder =>
        {
            builder.AddContent(0, args.Text);
        });

        private RenderFragment<Header<TRow>> HeaderTemplate =>
            ColumnDef.HeaderTemplate ?? Grid?.DefaultColumn?.HeaderTemplate ?? DefaultTemplate;


        void IDisposable.Dispose()
        {
            Grid?.Headers.Remove(this);
        }
    }
}
