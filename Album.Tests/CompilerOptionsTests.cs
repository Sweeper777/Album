using NUnit.Framework;
using static Album.CompilerMessage;
using static Album.CompilerOutputType;

namespace Album.Tests {
    [Timeout(1000)]
    public class CompilerOptionsTests {
        [Test]
        public void WarningLevelWorks() {
            DummyCodeGenerator mockCodeGen = new();
            AlbumCompiler compiler = new(mockCodeGen);
            compiler.SongManifest = new DummySongManifest();

            string source = @"
            playlist created by Sweeper
            1 push
            useless song, by Sweeper
            2 pushes
            3 pushes
            another useless song, by Sweeper
            4 pushes
            ";

            compiler.WarningLevel = WarningLevel.Warning;
            compiler.Compile(source);
            Assert.IsTrue(mockCodeGen.HasGenerated);
            mockCodeGen.HasGenerated = false;
            CollectionAssert.AreEquivalent(new[] {
                new CompilerOutput(UnusedOriginalSong, Warning, 4),
                new CompilerOutput(UnusedOriginalSong, Warning, 7)
            }, compiler.Outputs);

        }
    }
}