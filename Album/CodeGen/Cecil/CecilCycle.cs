using Album.Syntax;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System;

namespace Album.CodeGen.Cecil
{
    internal partial class CecilCodeGenerationStrategies {
        public class Cycle : CecilCodeGenerationStrategy
        {
            public Cycle(MethodReferenceProvider methods, ILProcessor ilProcessor) 
                : base(methods, ilProcessor) {
            }

            public override void GenerateCodeForSong(LineInfo line)
            {
                MethodReference? nodeToRemove, addMethod, removeMethod;
                if (line.Type == LineType.Cycle) {
                    nodeToRemove = methods.LinkedListFirst;
                    addMethod = methods.LinkedListAddLast;
                    removeMethod = methods.LinkedListRemoveFirst;
                } else if (line.Type == LineType.RCycle) {
                    nodeToRemove = methods.LinkedListLast;
                    addMethod = methods.LinkedListAddFirst;
                    removeMethod = methods.LinkedListRemoveLast;
                } else {
                    throw new InvalidOperationException("Unsupported Line Type!");
                }
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Callvirt, nodeToRemove);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListNodeValue);
                ILProcessor.Emit(OpCodes.Stloc_0);
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Callvirt, removeMethod);
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Ldloc_0);
                ILProcessor.Emit(OpCodes.Callvirt, addMethod);
                ILProcessor.Emit(OpCodes.Pop);
            }

            public override bool SupportsLineType(LineType type)
                => type == LineType.Cycle || type == LineType.RCycle;
        }
    }
}