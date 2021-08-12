using System.Linq;
using System.Collections.Generic;
using Album.Syntax;

namespace Album.Semantics {
    public interface IOptimisationContext {
        int CurrentLineCount { get; }

        IEnumerable<LineInfo> PreviousLines { get; }

    }

    public static class OptimisationContextExtensions {
        public static LineInfo PreviousLine(this IOptimisationContext context)
            => context.PreviousLines.First();
        
        public static (LineInfo, LineInfo) PreviousTwoLines(this IOptimisationContext context) {
            using var enumerator = context.PreviousLines.GetEnumerator();
            enumerator.MoveNext();
            LineInfo first = enumerator.Current;
            enumerator.MoveNext();
            LineInfo second = enumerator.Current;
            return (first, second);
        }
    }
}