using System;
using System.Collections.Generic;
using Album.Syntax;

namespace Album.Semantics {
    public class CodeOptimiser {
        private LinkedList<LineInfo> seenLines = new();

        public OptimisationOptions Options { get; set; }

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

}