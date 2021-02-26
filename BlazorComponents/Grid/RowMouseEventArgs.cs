using Microsoft.AspNetCore.Components.Web;

namespace vNext.BlazorComponents.Grid
{
    public class RowMouseEventArgs<TRow>
    {
        public RowMouseEventArgs(TRow? data, MouseEventArgs eventArgs)
        {
            Data = data;
            EventArgs = eventArgs;
        }

        public TRow? Data { get; }
        public MouseEventArgs EventArgs { get; }
    }
}
