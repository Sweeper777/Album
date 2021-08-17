using Album.Syntax;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Album.Semantics {
    public class BasicBlock {
        public BasicBlock(int startIndex, int endIndexExclusive, ControlFlowGraph controlFlowGraph)
        {
            if (startIndex < 0 || startIndex > controlFlowGraph.SourceCode.Count ||
                endIndexExclusive < 0 || endIndexExclusive > controlFlowGraph.SourceCode.Count) {
                throw new IndexOutOfRangeException("Indices for the basic block are out of range!");
            }

            StartIndex = startIndex;
            EndIndexExclusive = endIndexExclusive;
            ControlFlowGraph = controlFlowGraph;
        }

        public int StartIndex { get; }

        public int EndIndexExclusive { get; }

        public ControlFlowGraph ControlFlowGraph { get; }

        public IEnumerable<LineInfo> Lines 
            => ControlFlowGraph.SourceCode.Skip(StartIndex).Take(EndIndexExclusive - StartIndex);

        public bool IsEmpty => StartIndex == EndIndexExclusive;

        public LineInfo? FirstLine
            => IsEmpty ? null : ControlFlowGraph.SourceCode[StartIndex];

        public LineInfo? LastLine
            => IsEmpty ? null : ControlFlowGraph.SourceCode[EndIndexExclusive - 1];
    }
}