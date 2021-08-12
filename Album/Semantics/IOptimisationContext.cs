using System.Linq;
using System.Collections.Generic;
using Album.Syntax;

namespace Album.Semantics {
    public interface IOptimisationContext {
        int CurrentLineCount { get; }

        IEnumerable<LineInfo> PreviousLines { get; }

    }
}