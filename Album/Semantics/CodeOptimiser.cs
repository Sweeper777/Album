using System.Linq;
using System.Collections.Generic;
using Album.Syntax;

namespace Album.Semantics {
    public class CodeOptimiser : IOptimisationContext {
        private LinkedList<LineInfo> seenLines = new();

        public IEnumerable<OptimisationRule> Rules { get; set; } = Enumerable.Empty<OptimisationRule>();

        int IOptimisationContext.CurrentLineCount => seenLines.Count;

        IEnumerable<LineInfo> IOptimisationContext.PreviousLines {
            get {
                var lastNode = seenLines.Last;
                while (lastNode != null) {
                    yield return lastNode.Value;
                    lastNode = lastNode.Previous;
                }
            }
        }

        public IList<LineInfo> Optimise(IEnumerable<LineInfo> lines) {
            seenLines.Clear();
            foreach (var line in lines) {
                if (!(seenLines.Last?.Value is LineInfo lastLine)) {
                    continue;
                }
            }
            return null;
        }
    }

    public delegate OptimisationResult OptimisationRule(IOptimisationContext context);
}