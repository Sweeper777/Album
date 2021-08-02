using Album.Syntax;
using Mono.Cecil.Cil;

namespace Album.CodeGen.Cecil
{
    internal partial class CecilCodeGenerationStrategies {
        public class Sub : CecilCodeGenerationStrategy
        {
            public Sub(MethodReferenceProvider methods, ILProcessor ilProcessor) 
                : base(methods, ilProcessor) {
            }

            public override void GenerateCodeForSong(LineInfo line)
            {
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListLast);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListNodeValue);
                ILProcessor.Emit(OpCodes.Stloc_0);
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListRemoveLast);
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListLast);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListNodeValue);
                ILProcessor.Emit(OpCodes.Ldloc_0);
                ILProcessor.Emit(OpCodes.Sub);
                ILProcessor.Emit(OpCodes.Stloc_0);
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListRemoveLast);
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Ldloc_0);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListAddLast);
                ILProcessor.Emit(OpCodes.Pop);
            }

            public override bool SupportsLineType(LineType type)
                => type == LineType.Sub;
        }
    }
}