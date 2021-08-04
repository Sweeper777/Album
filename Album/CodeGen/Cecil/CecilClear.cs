using Album.Syntax;
using Mono.Cecil.Cil;

namespace Album.CodeGen.Cecil
{
    internal partial class CecilCodeGenerationStrategies {
        public class Clear : CecilCodeGenerationStrategy
        {
            public Clear(MethodReferenceProvider methods, ILProcessor ilProcessor) 
                : base(methods, ilProcessor) {
            }

            public override void GenerateCodeForSong(LineInfo line)
            {
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListClear);
            }

            public override bool SupportsLineType(LineType type)
                => type == LineType.Clear;
        }
    }
}