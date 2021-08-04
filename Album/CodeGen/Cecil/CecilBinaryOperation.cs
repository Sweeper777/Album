using Album.Syntax;
using Mono.Cecil.Cil;
using System.Linq;
using System;

namespace Album.CodeGen.Cecil
{
    internal partial class CecilCodeGenerationStrategies {
        public class BinaryOperation : CecilCodeGenerationStrategy
        {
            public BinaryOperation(MethodReferenceProvider methods, ILProcessor ilProcessor) 
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
                switch (line.Type) {
                    case LineType.Add:
                        ILProcessor.Emit(OpCodes.Add);
                        break;
                    case LineType.Sub:
                        ILProcessor.Emit(OpCodes.Sub);
                        break;
                    case LineType.And:
                        ILProcessor.Emit(OpCodes.And);
                        break;
                    case LineType.Or:
                        ILProcessor.Emit(OpCodes.Or);
                        break;
                    case LineType.Eor:
                        ILProcessor.Emit(OpCodes.Xor);
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported Line Type!");
                }
                ILProcessor.Emit(OpCodes.Stloc_0);
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListRemoveLast);
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Ldloc_0);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListAddLast);
                ILProcessor.Emit(OpCodes.Pop);
            }

            private static readonly LineType[] supportedLineTypes = new[] {
                LineType.Add, LineType.Sub, LineType.And, LineType.Or, LineType.Eor
            };

            public override bool SupportsLineType(LineType type)
                => supportedLineTypes.Contains(type);
        }
    }
}