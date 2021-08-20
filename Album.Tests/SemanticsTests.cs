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

    }
}