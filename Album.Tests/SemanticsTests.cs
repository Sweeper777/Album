using NUnit.Framework;
using Album.Semantics;
using System.Linq;
using static Album.Syntax.LineInfo;
using static Album.Syntax.LineType;

namespace Album.Tests {
    [Timeout(1000)]
    public class SemanticsTests {
        [Test]
        public void NoErrorsWhenEveryOriginalSongIsUsed() {
            SemanticAnalyser analyser = new(new[] {
                OriginalSong("foo", 1),
                Push(1, 2),
                OfType(Dup, 3),
                OriginalSong("bar", 4),
                OfType(OutputChar, 5),
                Branch("foo", 6),
                Branch("bar", 7)
            });
            analyser.Analyse();
            CollectionAssert.IsEmpty(analyser.Outputs);
        }
    }
}