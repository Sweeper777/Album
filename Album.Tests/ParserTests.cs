using NUnit.Framework;
using Album.Syntax;
using System.Collections.Generic;

namespace Album.Tests {
    [Timeout(1000)]
    public class ParserTests {

        [TestCase("playlist created by Sweeper")]
        [TestCase("PLAYLIST CREATED BY SWEEPER")]
        [TestCase("Foo\nBar\nBaz\nBoz\nplaylist created by Sweeper")]
        [TestCase("; ; ; playlist created by        Sweeper . . . . ")]
        public void CanFindPlaylistCreatorDecl(string code) {
            string codeWithLabelAdded = code + "\noriginal song, by Sweeper";
            using var parser = new AlbumParser(new DummySongManifest(), codeWithLabelAdded);
            var lines = parser.Parse();
            Assert.Multiple(() => {
                CollectionAssert.IsEmpty(parser.Outputs);
                CollectionAssert.Contains(lines, LineInfo.OriginalSong("original song"));
            });
        }

        [TestCase("playlist       created     by    Sweeper")]
        [TestCase("Foo\nBar\nBaz\n")]
        [TestCase("This is a playlist created by Sweeper")]
        public void CannotFindPlaylistCreatorDecl(string code) {
            string codeWithLabelAdded = code + "\noriginal song, by Sweeper";
            using var parser = new AlbumParser(new DummySongManifest(), codeWithLabelAdded);
            var lines = parser.Parse();
            Assert.Multiple(() => {
                Assert.That(parser.Outputs, Has.One.Matches<CompilerOutput>(x => x.Message == CompilerMessage.NoPlaylistCreatorDeclFound));
                CollectionAssert.IsSubsetOf(lines, new[] { LineInfo.OfType(LineType.Comment) });
            });
        }

        [TestCase("input", LineType.Input)]
        [TestCase("; ; ; ; input . . . ", LineType.Input)]
        [TestCase("This will get input", LineType.Comment)]
        [TestCase("INPUT", LineType.Input)]
        [TestCase("  . . . ; ; ;   ", LineType.Comment)]
        [TestCase("50 non pushes", LineType.Comment)]
        [TestCase("An Original Song, by Sweepers", LineType.Comment)]
        [TestCase(" pushes", LineType.Comment)]
        [TestCase("...    , by Sweeper", LineType.Comment)]
        [TestCase("branch to ...", LineType.Comment)]
        [TestCase("branch     to somewhere", LineType.Comment)]
        public void CanParseLine(string line, LineType expectedType) {
            string codeWithPlaylistCreatorAdded = "playlist created by Sweeper\n" + line;
            using var parser = new AlbumParser(new DummySongManifest(), codeWithPlaylistCreatorAdded);
            var lines = parser.Parse();
            Assert.Multiple(() => {
                CollectionAssert.IsEmpty(parser.Outputs);
                CollectionAssert.AreEqual(lines, new[] { LineInfo.OfType(expectedType) });
            });
        }

        [TestCase("    An Original: Song     , by Sweeper", CompilerMessage.InvalidOriginalSongName)]
        [TestCase("100 pushes", CompilerMessage.PushXTooLarge)]
        [TestCase("1 pushes", CompilerMessage.Push1)]
        [TestCase("-1 pushes", CompilerMessage.PushXTooSmall)]
        public void CannotParseLinesWithErrors(string line, CompilerMessage expectedMessage) {
            string codeWithPlaylistCreatorAdded = "playlist created by Sweeper\n" + line;
            using var parser = new AlbumParser(new DummySongManifest(), codeWithPlaylistCreatorAdded);
            var lines = parser.Parse();
            Assert.Multiple(() => {
                Assert.That(parser.Outputs, Has.One.Matches<CompilerOutput>(x => x.Message == expectedMessage));
                CollectionAssert.AreEqual(lines, new[] { LineInfo.OfType(LineType.Comment) });
            });
        }

    }
}