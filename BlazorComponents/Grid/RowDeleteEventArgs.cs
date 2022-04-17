namespace vNext.BlazorComponents.Grid
{
    public class RowDeleteEventArgs<TRow>
    {
        public RowDeleteEventArgs(Row<TRow> row)
        {
            Row = row;
        }
        public TRow Data => Row.Data!;
        public Row<TRow> Row { get; }
    }
}
