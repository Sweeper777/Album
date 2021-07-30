using System;
using System.Collections.Generic;
using Album.Syntax;

namespace Album.Semantics {
    public class SemanticAnalyser {
        public IList<CompilerOutput> Outputs { get; } = new List<CompilerOutput>();

        private IEnumerable<LineInfo> lines;

        private HashSet<LineInfo> originalSongLines = new();
        private HashSet<LineInfo> branchLines = new();

        public SemanticAnalyser(IEnumerable<LineInfo> lines) {
            this.lines = lines;
        }

    }
}