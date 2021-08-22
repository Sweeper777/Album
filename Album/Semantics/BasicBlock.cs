using Album.Syntax;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics.CodeAnalysis;

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

        [MemberNotNullWhen(false, nameof(FirstLine), nameof(LastLine))]
        public bool IsEmpty => StartIndex == EndIndexExclusive;

        public LineInfo? FirstLine
            => IsEmpty ? null : ControlFlowGraph.SourceCode[StartIndex];

        public LineInfo? LastLine
            => IsEmpty ? null : ControlFlowGraph.SourceCode[EndIndexExclusive - 1];

        public override string ToString()
            => $"Basic Block from index {StartIndex} ({FirstLine?.ToString() ?? "null"}) to {EndIndexExclusive} ({LastLine?.ToString() ?? "null"})";

        private class Comparer : IComparer<BasicBlock>
        {
            public int Compare(BasicBlock? x, BasicBlock? y)
                => Comparer<int?>.Default.Compare(x?.StartIndex, y?.StartIndex);
        }

        public static readonly IComparer<BasicBlock> StartIndexComparer = new Comparer();
    }
}