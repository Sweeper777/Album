using Album.Syntax;
using Mono.Cecil.Cil;

namespace Album.CodeGen.Cecil
{
    internal partial class CecilCodeGenerationStrategies {
        public class Dup : CecilCodeGenerationStrategy
        {
            public Dup(MethodReferenceProvider methods, ILProcessor ilProcessor) 
                : base(methods, ilProcessor) {
            }

            public override void GenerateCodeForSong(LineInfo line)
            {
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListLast);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListNodeValue);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListAddLast);
                ILProcessor.Emit(OpCodes.Pop);
            }

            public override bool SupportsLineType(LineType type)
                => type == LineType.Dup;
        }
    }
}