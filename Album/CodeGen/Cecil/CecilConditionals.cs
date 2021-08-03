using Album.Syntax;
using Mono.Cecil.Cil;
using System;

namespace Album.CodeGen.Cecil
{
    internal partial class CecilCodeGenerationStrategies {
        public class Conditionals : CecilCodeGenerationStrategy
        {

            public Conditionals(MethodReferenceProvider methods, ILProcessor ilProcessor) 
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
                ILProcessor.Emit(OpCodes.Ldloc_0);
                ILProcessor.Emit(OpCodes.Ldc_I4_0);
                switch (line.Type) {
                case LineType.TopZero:
                    ILProcessor.Emit(OpCodes.Ceq);
                    break;
                case LineType.TopPositive:
                    ILProcessor.Emit(OpCodes.Cgt);
                    break;
                case LineType.TopNegative:
                    ILProcessor.Emit(OpCodes.Clt);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported Line Type!");
                }
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListAddLast);
                ILProcessor.Emit(OpCodes.Pop);
            }

            public override bool SupportsLineType(LineType type)
                => type == LineType.TopNegative || type == LineType.TopPositive || type == LineType.TopZero;
        }
    }
}