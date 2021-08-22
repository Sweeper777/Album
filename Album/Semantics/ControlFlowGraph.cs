using System.Collections.Generic;
using System;
using Album.Syntax;
using System.Linq;

namespace Album.Semantics {
    public class ControlFlowGraph {
        private List<BasicBlock> basicBlocks = new();

        public IReadOnlyList<BasicBlock> BasicBlocks => basicBlocks;

        public IReadOnlyList<LineInfo> SourceCode { get; }

        public Dictionary<BasicBlock, HashSet<BasicBlock>> Successors = new();

        public ControlFlowGraph(IEnumerable<LineInfo> sourceCode)
        {
            SourceCode = sourceCode.ToList();
        }

        public void SortBasicBlocks() {
            basicBlocks.RemoveAll(b => 
                (
                    b.FirstLine?.Type != LineType.OriginalSong &&
                    b.Lines.All(l => l.Type == LineType.Comment || l.Type == LineType.OriginalSong)
                ) || b.IsEmpty
            );
            basicBlocks.Sort((b1, b2) => b1.StartIndex.CompareTo(b2.StartIndex));
        }

        public BasicBlockBuilder BuildBasicBlock(int startIndex) => new BasicBlockBuilder(this, startIndex);

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

            public BasicBlockBuilder AddLine() {
                endIndexExclusive++;
                if (endIndexExclusive > controlFlowGraph.SourceCode.Count) {
                    throw new IndexOutOfRangeException("There are no more lines in the source code to add to the basic block!");
                }
                return this;
            }

            public void AddToCFG() {
                if (added) {
                    throw new InvalidOperationException("This basic block has already been added to the CFG!");
                }
                BasicBlock block = new BasicBlock(startIndex, endIndexExclusive, controlFlowGraph);
                controlFlowGraph.basicBlocks.Add(block);
                controlFlowGraph.Successors.Add(block, new HashSet<BasicBlock>());
                added = true;
            }
        }
    }
}