using NUnit.Framework;
using Album.Semantics;
using static Album.Syntax.LineInfo;
using static Album.Syntax.LineType;
using System.Linq;
using System.Collections.Generic;

namespace Album.Tests {
    [Timeout(1000)]
    public class SemanticsTests {
        [Test]
        public void NoErrorsWhenEveryOriginalSongIsUsed() {
            SemanticAnalyser analyser = new();
            analyser.Analyse(new[] {
                OriginalSong("foo", 1),
                Push(1, 2),
                OfType(Dup, 3),
                OriginalSong("bar", 4),
                OfType(OutputChar, 5),
                Branch("foo", 6),
                Branch("bar", 7)
            });
            CollectionAssert.IsEmpty(analyser.Outputs);
        }

        [Test]
        public void FindsUnknownOriginalSongs() {
            SemanticAnalyser analyser = new();
            analyser.Analyse(new[] {
                Push(1, 2),
                OfType(Dup, 3),
                OfType(OutputChar, 5),
                Branch("foo", 6),
                Branch("bar", 7)
            });
            CollectionAssert.AreEquivalent(
                new[] { 
                    new CompilerOutput(CompilerMessage.UnknownOriginalSong, CompilerOutputType.Error, 6),
                    new CompilerOutput(CompilerMessage.UnknownOriginalSong, CompilerOutputType.Error, 7)
                },
                analyser.Outputs
            );
        }

        [Test]
        public void FindsDuplicateOriginalSongs() {
            SemanticAnalyser analyser = new();
            analyser.Analyse(new[] {
                OriginalSong("foo", 1),
                Push(1, 2),
                OfType(Dup, 3),
                OriginalSong("foo", 4),
                OfType(OutputChar, 5),
                Branch("foo", 7),
            });
            CollectionAssert.AreEquivalent(
                new[] { 
                    new CompilerOutput(CompilerMessage.DuplicateOriginalSong, CompilerOutputType.Error, 4)
                },
                analyser.Outputs
            );
        }

        [Test]
        public void FindsUnusedOriginalSongs() {
            SemanticAnalyser analyser = new();
            analyser.Analyse(new[] {
                OriginalSong("foo", 1),
                Push(1, 2),
                OfType(Dup, 3),
                OfType(OutputChar, 5),
            });
            CollectionAssert.AreEquivalent(
                new[] { 
                    new CompilerOutput(CompilerMessage.UnusedOriginalSong, CompilerOutputType.Warning, 1)
                },
                analyser.Outputs
            );
        }

        [Test]
        public void GeneratesSimpleCFG() {
            SemanticAnalyser analyser = new();
            analyser.Analyse(new[] {
                Push(1), Push(2), Push(3), OfType(OutputInt)
            });
            var cfg = analyser.CFG;
            Assert.NotNull(cfg);
            Assert.AreEqual(1, cfg!.BasicBlocks.Count);
            Assert.Multiple(() => {
                Assert.AreEqual(0, cfg!.BasicBlocks[0].StartIndex);
                Assert.AreEqual(4, cfg!.BasicBlocks[0].EndIndexExclusive);
                Assert.That(cfg.Successors, Is.All
                    .Matches<KeyValuePair<BasicBlock, HashSet<BasicBlock>>>(
                        x => !x.Value.Any()
                    )
                );
            });
        }

        [Test]
        public void RemovesBasicBlockWithAllComments() {
            SemanticAnalyser analyser = new();
            analyser.Analyse(Enumerable.Repeat(OfType(Comment), 10));
            var cfg = analyser.CFG;
            Assert.NotNull(cfg);
            Assert.Multiple(() => {
                Assert.AreEqual(0, cfg!.BasicBlocks.Count);
                Assert.That(cfg.Successors, Is.All
                    .Matches<KeyValuePair<BasicBlock, HashSet<BasicBlock>>>(
                        x => !x.Value.Any()
                    )
                );
            });
        }

        [Test]
        public void GeneratesCFGForConditionalBranch() {
            SemanticAnalyser analyser = new();
            analyser.Analyse(new[] {
                Push(1), Push(2), Branch("foo"), Push(3), Push(4), OriginalSong("foo"),
                Push(5), Push(6)
            });
            var cfg = analyser.CFG;
            Assert.NotNull(cfg);
            Assert.AreEqual(3, cfg!.BasicBlocks.Count);
            Assert.Multiple(() => {
                Assert.AreEqual(0, cfg.BasicBlocks[0].StartIndex);
                Assert.AreEqual(3, cfg.BasicBlocks[0].EndIndexExclusive);
                Assert.AreEqual(3, cfg.BasicBlocks[1].StartIndex);
                Assert.AreEqual(5, cfg.BasicBlocks[1].EndIndexExclusive);
                Assert.AreEqual(5, cfg.BasicBlocks[2].StartIndex);
                Assert.AreEqual(8, cfg.BasicBlocks[2].EndIndexExclusive);
                CollectionAssert.AreEquivalent(
                    new[] { cfg.BasicBlocks[1], cfg.BasicBlocks[2] },
                    cfg.Successors[cfg.BasicBlocks[0]]
                );
                CollectionAssert.AreEquivalent(
                    new[] { cfg.BasicBlocks[2] },
                    cfg.Successors[cfg.BasicBlocks[1]]
                );
                CollectionAssert.IsEmpty(cfg.Successors[cfg.BasicBlocks[2]]);
            });
        }

        [Test]
        public void GeneratesCFGForIfThenElse() {
            SemanticAnalyser analyser = new();
            analyser.Analyse(new[] {
                Push(1), Push(2), Branch("foo"), Push(3), Push(4), 
                UnconditionalBranch("bar"), OriginalSong("foo"),
                Push(5), Push(6), OriginalSong("bar")
            });
            var cfg = analyser.CFG;
            Assert.NotNull(cfg);
            Assert.AreEqual(4, cfg!.BasicBlocks.Count);
            Assert.Multiple(() => {
                Assert.AreEqual(0, cfg.BasicBlocks[0].StartIndex);
                Assert.AreEqual(3, cfg.BasicBlocks[0].EndIndexExclusive);
                Assert.AreEqual(3, cfg.BasicBlocks[1].StartIndex);
                Assert.AreEqual(6, cfg.BasicBlocks[1].EndIndexExclusive);
                Assert.AreEqual(6, cfg.BasicBlocks[2].StartIndex);
                Assert.AreEqual(9, cfg.BasicBlocks[2].EndIndexExclusive);
                Assert.AreEqual(9, cfg.BasicBlocks[3].StartIndex);
                Assert.AreEqual(10, cfg.BasicBlocks[3].EndIndexExclusive);
                CollectionAssert.AreEquivalent(
                    new[] { cfg.BasicBlocks[1], cfg.BasicBlocks[2] },
                    cfg.Successors[cfg.BasicBlocks[0]]
                );
                CollectionAssert.AreEquivalent(
                    new[] { cfg.BasicBlocks[3] },
                    cfg.Successors[cfg.BasicBlocks[1]]
                );
                CollectionAssert.AreEquivalent(
                    new[] { cfg.BasicBlocks[3] },
                    cfg.Successors[cfg.BasicBlocks[2]]
                );
                CollectionAssert.IsEmpty(cfg.Successors[cfg.BasicBlocks[3]]);
            });
        }

        [Test]
        public void GeneratesCFGForWhileLoop() {
            SemanticAnalyser analyser = new();
            analyser.Analyse(new[] {
                OriginalSong("foo"), Push(1), Push(2), Branch("bar"), Push(3),
                UnconditionalBranch("foo"), OriginalSong("bar")
            });
            var cfg = analyser.CFG;
            Assert.NotNull(cfg);
            Assert.AreEqual(3, cfg!.BasicBlocks.Count);
            Assert.Multiple(() => {
                Assert.AreEqual(0, cfg.BasicBlocks[0].StartIndex);
                Assert.AreEqual(4, cfg.BasicBlocks[0].EndIndexExclusive);
                Assert.AreEqual(4, cfg.BasicBlocks[1].StartIndex);
                Assert.AreEqual(6, cfg.BasicBlocks[1].EndIndexExclusive);
                Assert.AreEqual(6, cfg.BasicBlocks[2].StartIndex);
                Assert.AreEqual(7, cfg.BasicBlocks[2].EndIndexExclusive);
                CollectionAssert.AreEquivalent(
                    new[] { cfg.BasicBlocks[1], cfg.BasicBlocks[2] },
                    cfg.Successors[cfg.BasicBlocks[0]]
                );
                CollectionAssert.AreEquivalent(
                    new[] { cfg.BasicBlocks[0] },
                    cfg.Successors[cfg.BasicBlocks[1]]
                );
                CollectionAssert.IsEmpty(cfg.Successors[cfg.BasicBlocks[2]]);
            });
        }

    }
}