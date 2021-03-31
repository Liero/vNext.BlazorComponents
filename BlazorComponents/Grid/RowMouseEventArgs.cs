using Microsoft.AspNetCore.Components.Web;

namespace vNext.BlazorComponents.Grid
{
    public class RowMouseEventArgs<TRow>
    {
        public RowMouseEventArgs(Row<TRow>? row, MouseEventArgs eventArgs)
        {
            Row = row;
            EventArgs = eventArgs;
        }

        public TRow? Data => Row == null ? default(TRow) : Row.Data;
        public int? Index => Row?.Index;
        public Row<TRow>? Row { get; }
        public MouseEventArgs EventArgs { get; }
    }
}
