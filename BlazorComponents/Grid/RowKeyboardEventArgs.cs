using Microsoft.AspNetCore.Components.Web;

namespace vNext.BlazorComponents.Grid
{
    public class RowKeyboardEventArgs<TRow>
    {
        public RowKeyboardEventArgs(Row<TRow> row, KeyboardEventArgs eventArgs)
        {
            Row = row;
            EventArgs = eventArgs;
        }

        public TRow Data => Row.Data!;
        public int? Index => Row?.Index;
        public Row<TRow> Row { get; }
        public KeyboardEventArgs EventArgs { get; }
    }
}
