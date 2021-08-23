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

        public void GenerateEdges() {
            Predicate<BasicBlock> predicate = b => 
                (
                    b.FirstLine?.Type != LineType.OriginalSong &&
                    b.Lines.All(l => l.Type == LineType.Comment || l.Type == LineType.OriginalSong)
                ) || b.IsEmpty;
            basicBlocks.RemoveAll(predicate);
            foreach (var key in Successors.Keys) {
                if (predicate(key)) {
                    Successors.Remove(key);
                }
            }
            basicBlocks.Sort((b1, b2) => b1.StartIndex.CompareTo(b2.StartIndex));

            string? name = null;
            ILookup<string, BasicBlock> originalSongLeaders = 
                BasicBlocks
                    .Where(x => x.FirstLine?.IsOriginalSong(out name) == true)
                    .ToLookup(x => name!);
            int basicBlockCount = BasicBlocks.Count;
            for (int i = 0; i < basicBlockCount; i++) {
                BasicBlock block = BasicBlocks[i];
                if (block.IsEmpty) {
                    throw new Exception("There should be no empty basic blocks when calling BuildJumpEdges!");
                }
                if (block.LastLine.Value.IsUnconditionalBranch(out var originalSong) &&
                    originalSongLeaders[originalSong].FirstOrDefault() is BasicBlock unconditionalSuccessor) {
                    Successors[block].Add(unconditionalSuccessor);
                    continue;
                }
                if (block.LastLine.Value.Type == LineType.InfiniteLoop || block.LastLine.Value.Type == LineType.Quit) {
                    continue;
                }
                if (block.LastLine.Value.IsBranch(out originalSong) &&
                    originalSongLeaders[originalSong].FirstOrDefault() is BasicBlock conditionalSuccessor) {
                    Successors[block].Add(conditionalSuccessor);
                }
                if (i + 1 < BasicBlocks.Count) {
                    BasicBlock nextBlock = BasicBlocks[i + 1];
                    Successors[block].Add(nextBlock);
                } else {
                    BuildBasicBlock(SourceCode.Count).AddToCFG();
                    Successors[block].Add(BasicBlocks.Last());
                }
            }
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