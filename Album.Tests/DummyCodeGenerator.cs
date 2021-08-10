using Album.Syntax;
using Album.CodeGen;

namespace Album.Tests {
    public class DummyCodeGenerator : CodeGenerator
    {

        public bool HasGenerated { get; set; }

        protected override void DidGenerateLines() {
            HasGenerated = true;
        }
        
        protected override ICodeGenerationStrategy? GetCodeGenerationStrategyForSong(LineType type)
            => null;
    }
}