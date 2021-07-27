using NUnit.Framework;
using Album.Syntax;
using System.Linq;
using static Album.Syntax.LineInfo;

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
                CollectionAssert.Contains(lines, OriginalSong("original song"));
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
                CollectionAssert.IsSubsetOf(lines, new[] { OfType(LineType.Comment) });
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
                CollectionAssert.AreEqual(lines, new[] { OfType(expectedType) });
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
                CollectionAssert.AreEqual(lines, new[] { OfType(LineType.Comment) });
            });
        }

        [TestCase("branch to Somewhere", "somewhere")]
        [TestCase("branch to      Somewhere.     ", "somewhere")]
        [TestCase("branch to , by Sweeper", ", by sweeper")]
        public void CanParseBranches(string line, string expectedDestination) {
            string codeWithPlaylistCreatorAdded = "playlist created by Sweeper\n" + line;
            using var parser = new AlbumParser(new DummySongManifest(), codeWithPlaylistCreatorAdded);
            var lines = parser.Parse();
            Assert.Multiple(() => {
                CollectionAssert.IsEmpty(parser.Outputs);
                CollectionAssert.AreEqual(lines, new[] { Branch(expectedDestination) });
            });
        }

        [TestCase("50 pushes", 50)]
        [TestCase("            50       pushes", 50)]
        [TestCase("1 push", 1)]
        [TestCase("no pushes", 0)]
        public void CanParsePush(string line, int expectedPush) {
            string codeWithPlaylistCreatorAdded = "playlist created by Sweeper\n" + line;
            using var parser = new AlbumParser(new DummySongManifest(), codeWithPlaylistCreatorAdded);
            var lines = parser.Parse();
            Assert.Multiple(() => {
                CollectionAssert.IsEmpty(parser.Outputs);
                CollectionAssert.AreEqual(lines, new[] { Push(expectedPush) });
            });
        }

        [TestCase("an Original Song, by , by Sweeper", "an original song")]
        [TestCase("       an Original Song         , by , by Sweeper", "an original song")]
        public void CanParseOriginalSongs(string line, string expectedName) {
            string codeWithPlaylistCreatorAdded = "playlist created by , by Sweeper\n" + line;
            using var parser = new AlbumParser(new DummySongManifest(), codeWithPlaylistCreatorAdded);
            var lines = parser.Parse();
            Assert.Multiple(() => {
                CollectionAssert.IsEmpty(parser.Outputs);
                CollectionAssert.AreEqual(lines, new[] { OriginalSong(expectedName) });
            });
        }

        [Test]
        public void CanParseExampleProgram() {
            using var sourceCodeStream = typeof(ParserTests).Assembly.GetManifestResourceStream("Album.Tests.reverse_string.album");
            Assert.NotNull(sourceCodeStream);
            using var manifestStream = typeof(AlbumParser).Assembly.GetManifestResourceStream("Album.songManifest.json");
            Assert.NotNull(manifestStream);
            var manifest = SongManifest.FromStream(manifestStream!);
            using var parser = new AlbumParser(manifest!, sourceCodeStream!);
            var lines = parser.Parse().Where(x => !x.Equals(OfType(LineType.Comment)));
            Assert.Multiple(() => {
                CollectionAssert.IsEmpty(parser.Outputs);
                CollectionAssert.AreEqual(lines, new[] {
                    OriginalSong("to the start of the playlist"),
                    Push(0),
                    Push(10),
                    OriginalSong("to where we get input"),
                    OfType(LineType.Input),
                    OfType(LineType.Dup),
                    Push(32),
                    OfType(LineType.Sub),
                    Branch("to where we get input"),
                    OfType(LineType.Pop),
                    OriginalSong("to where we start printing the reversed string"),
                    OfType(LineType.Dup),
                    OfType(LineType.TopZero),
                    Branch("to where we stop printing the reversed string"),
                    OfType(LineType.Dup),
                    OfType(LineType.OutputChar),
                    Branch("to where we start printing the reversed string"),
                    OriginalSong("to where we stop printing the reversed string"),
                    OfType(LineType.Pop),
                    Push(1),
                    Branch("to the start of the playlist")
                 });
            });
        }
    }
}