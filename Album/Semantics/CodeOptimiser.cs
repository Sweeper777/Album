using System.Linq;
using System.Collections.Generic;
using Album.Syntax;
using static Album.Semantics.OptimisationRules;

namespace Album.Semantics {
    public class CodeOptimiser : IOptimisationContext {
        private LinkedList<LineInfo> seenLines = new();

        public IList<OptimisationRule> Rules { get; set; } = new List<OptimisationRule> {
            RemoveComments,
            EvaluateBinaryOperators, 
            EvaluateUnaryOperators, 
            RemoveUselessBranches, 
            DetectUnconditionalBranches
        };

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
            var result = Rules.Select(x => x.Invoke(this, line)).FirstOrDefault(x => x != null);
            if (result == null) {
                seenLines.AddLast(line);
                return;
            }
            for (int i = 0 ; i < result.Value.NumberOfLinesPruned ; i++) {
                seenLines.RemoveLast();
            }
            foreach (var newLine in result.Value.ReplacementLines) {
                OptimiseLine(newLine);
            }
        }
    }

    public delegate OptimisationResult? OptimisationRule(IOptimisationContext context, LineInfo newLine);
}