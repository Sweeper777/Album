using Album.Syntax;
using Mono.Cecil.Cil;

namespace Album.CodeGen.Cecil
{
    internal partial class CecilCodeGenerationStrategies {
        public class Push : CecilCodeGenerationStrategy
        {
            public Push(MethodReferenceProvider methods, ILProcessor ilProcessor) 
                : base(methods, ilProcessor) {
            }

            public override void GenerateCodeForSong(LineInfo line)
            {
                if (line.IsPush(out int? push)) {
                    ILProcessor.Emit(OpCodes.Dup);
                    ILProcessor.Emit(OpCodes.Ldc_I4, push.Value);
                    ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListAddLast);
                    ILProcessor.Emit(OpCodes.Pop);
                }
            }

            public override bool SupportsLineType(LineType type)
                => type == LineType.Push;
        }
    }
}