using System.Collections.Generic;
using Album.Syntax;

namespace Album.Semantics {
    public struct OptimisationResult {
        public OptimisationResult(int numberOfLinesPruned, IEnumerable<LineInfo> replacementLines)
        {
            NumberOfLinesPruned = numberOfLinesPruned;
            ReplacementLines = replacementLines;
        }

        public int NumberOfLinesPruned { get; set; }

        public IEnumerable<LineInfo> ReplacementLines { get; set; }

        public static OptimisationResult None => new OptimisationResult();

        
    }
}