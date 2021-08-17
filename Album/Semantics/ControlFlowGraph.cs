using System.Collections.Generic;
using System;
using Album.Syntax;

namespace Album.Semantics {
    public class ControlFlowGraph {
        private List<BasicBlock> basicBlocks = new();

        public IReadOnlyList<BasicBlock> BasicBlocks => basicBlocks;

        public IReadOnlyList<LineInfo> SourceCode { get; }

        public Dictionary<BasicBlock, IEnumerable<BasicBlock>> Successors = new();

        public ControlFlowGraph(IReadOnlyList<LineInfo> sourceCode)
        {
            SourceCode = sourceCode;
        }

    }
}