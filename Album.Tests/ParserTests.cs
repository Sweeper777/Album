using NUnit.Framework;
using Album.Syntax;
using System.Collections.Generic;

namespace Album.Tests {
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
            CollectionAssert.IsNotEmpty(parser.Outputs);
            Assert.AreEqual(CompilerMessage.NoPlaylistCreatorDeclFound, parser.Outputs[0].Message);
            CollectionAssert.IsSubsetOf(lines, new List<LineInfo> { LineInfo.OfType(LineType.Comment) });
        }
    }
}