using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Demo.Data
{
    public record DataEnvelope<T>(
        IEnumerable<T> Items,
        int? Total
        );

}
