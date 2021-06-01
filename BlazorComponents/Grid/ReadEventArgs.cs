#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace vNext.BlazorComponents.Grid
{
    public class ReadEventArgs<TRow>
    {

        public ReadEventArgs(int startIndex, int count, CancellationToken cancellationToken = default(CancellationToken))
        {
            StartIndex = startIndex;
            Count = count;
            CancellationToken = cancellationToken;
        }

        public int StartIndex { get; }
        public int Count { get; }
        public CancellationToken CancellationToken { get; }
        public int? Total { get; set; }
        public IEnumerable<TRow> Items { get; set; } = Enumerable.Empty<TRow>();
    }
}
