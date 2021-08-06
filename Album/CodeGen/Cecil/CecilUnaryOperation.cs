using Album.Syntax;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace Album.CodeGen.Cecil
{
    internal partial class CecilCodeGenerationStrategies {
        public class UnaryOperation : CecilCodeGenerationStrategy
        {

            public UnaryOperation(MethodReferenceProvider methods, ILProcessor ilProcessor) 
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
                switch (line.Type) {
                case LineType.TopZero:
                    ILProcessor.Emit(OpCodes.Ldc_I4_0);
                    ILProcessor.Emit(OpCodes.Ceq);
                    break;
                case LineType.TopPositive:
                    ILProcessor.Emit(OpCodes.Ldc_I4_0);
                    ILProcessor.Emit(OpCodes.Cgt);
                    break;
                case LineType.TopNegative:
                    ILProcessor.Emit(OpCodes.Ldc_I4_0);
                    ILProcessor.Emit(OpCodes.Clt);
                    break;
                case LineType.Double:
                    ILProcessor.Emit(OpCodes.Ldc_I4_1);
                    ILProcessor.Emit(OpCodes.Shl);
                    break;
                case LineType.Halve:
                    ILProcessor.Emit(OpCodes.Ldc_I4_1);
                    ILProcessor.Emit(OpCodes.Shr);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported Line Type!");
                }
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListAddLast);
                ILProcessor.Emit(OpCodes.Pop);
            }

            private static readonly LineType[] supportedLineTypes = new[] {
                LineType.TopNegative, LineType.TopPositive, LineType.TopZero, LineType.Double, LineType.Halve
            };

            public override bool SupportsLineType(LineType type)
                => supportedLineTypes.Contains(type);
        }
    }
}