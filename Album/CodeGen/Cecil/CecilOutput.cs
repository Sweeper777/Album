using Album.Syntax;
using Mono.Cecil.Cil;
using System;

namespace Album.CodeGen.Cecil
{
    internal partial class CecilCodeGenerationStrategies {
        public class Output : CecilCodeGenerationStrategy
        {
            public Output(MethodReferenceProvider methods, ILProcessor ilProcessor) 
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
                ILProcessor.Emit(OpCodes.Ldloc_0);
                if (line.Type == LineType.OutputChar) {
                    ILProcessor.Emit(OpCodes.Call, methods.ConsoleWriteChar);
                } else if (line.Type == LineType.OutputInt) {
                    ILProcessor.Emit(OpCodes.Call, methods.ConsoleWriteInt);
                } else {
                    throw new InvalidOperationException("Unsupported Line Type!");
                }
            }

            public override bool SupportsLineType(LineType type)
                => type == LineType.OutputChar || type == LineType.OutputInt;
        }
    }
}