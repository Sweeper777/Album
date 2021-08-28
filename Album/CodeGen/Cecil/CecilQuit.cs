using Album.Syntax;
using Mono.Cecil.Cil;

namespace Album.CodeGen.Cecil
{
    internal partial class CecilCodeGenerationStrategies {
        public class Quit : CecilCodeGenerationStrategy
        {
            public Quit(MethodReferenceProvider methods, ILProcessor ilProcessor) 
                : base(methods, ilProcessor) {
            }

            public override void GenerateCodeForSong(LineInfo line)
            {
                ILProcessor.Emit(OpCodes.Pop);
                ILProcessor.Emit(OpCodes.Leave, methods.LastInstruction);
            }

            public override bool SupportsLineType(LineType type)
                => type == LineType.Quit;
        }
    }
}