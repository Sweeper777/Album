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


        public class BasicBlockBuilder {
            private ControlFlowGraph controlFlowGraph;
            private int startIndex;
            private int endIndexExclusive;

            private bool added = false;

            public BasicBlockBuilder(ControlFlowGraph controlFlowGraph, int start)
            {
                if (start < 0 || start > controlFlowGraph.SourceCode.Count) {
                    throw new IndexOutOfRangeException("Start index of basic block is out of range!");
                }
                this.controlFlowGraph = controlFlowGraph;
                startIndex = start;
                endIndexExclusive = start;
            }

    }
}