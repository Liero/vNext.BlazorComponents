using Microsoft.AspNetCore.Components.Web;

namespace vNext.BlazorComponents.Grid
{
    public class HeaderMouseEventArgs<TRow>
    {
        public HeaderMouseEventArgs(Header<TRow> header, MouseEventArgs eventArgs)
        {
            Header = header;
            EventArgs = eventArgs;
        }
        public ColumnDef<TRow> ColumnDef => Header.ColumnDef;
        public Header<TRow> Header { get; }
        public MouseEventArgs EventArgs { get; }
    }
}
