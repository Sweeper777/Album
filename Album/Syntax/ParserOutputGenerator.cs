using Album.CodeGen;
using System.Text;
using System.Diagnostics.CodeAnalysis;


namespace Album.Syntax {
    public class ParserOutputGenerator : CodeGenerator, ICodeGenerationStrategy
    {

        [DisallowNull]
        public StringBuilder? Output { get; private set; }

        protected override void WillGenerateLines() {
            Output = new();
        }
        
        protected override ICodeGenerationStrategy? GetCodeGenerationStrategyForSong(LineType type)
            => this;

        public void GenerateCodeForSong(LineInfo line)
        {
            Output?.AppendLine(line.ToString());
        }

        public bool SupportsLineType(LineType type)
            => true;
    }
}