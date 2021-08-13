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
                OptimiseLine(line);
            }
            return seenLines.ToList();
        }

        private void OptimiseLine(LineInfo line) {
            var result = Rules.Select(x => x.Invoke(this)).FirstOrDefault(x => !x.IsNone);
            if (result.IsNone) {
                seenLines.AddLast(line);
                return;
            }
            for (int i = 0 ; i < result.NumberOfLinesPruned ; i++) {
                seenLines.RemoveLast();
            }
            foreach (var newLine in result.ReplacementLines) {
                OptimiseLine(newLine);
            }
        }
    }

    public delegate OptimisationResult OptimisationRule(IOptimisationContext context);
}