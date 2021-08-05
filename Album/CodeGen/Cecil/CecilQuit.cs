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
                ILProcessor.Emit(OpCodes.Ldc_I4_0);
                ILProcessor.Emit(OpCodes.Call, methods.EnvironmentExit);
            }

            public override bool SupportsLineType(LineType type)
                => type == LineType.Quit;
        }
    }
}