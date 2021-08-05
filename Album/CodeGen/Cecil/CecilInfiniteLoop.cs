using Album.Syntax;
using Mono.Cecil.Cil;

namespace Album.CodeGen.Cecil
{
    internal partial class CecilCodeGenerationStrategies {
        public class InfiniteLoop : CecilCodeGenerationStrategy
        {
            public InfiniteLoop(MethodReferenceProvider methods, ILProcessor ilProcessor) 
                : base(methods, ilProcessor) {
            }

            public override void GenerateCodeForSong(LineInfo line)
            {
                Instruction loopStart = ILProcessor.Create(OpCodes.Nop);
                ILProcessor.Append(loopStart);
                ILProcessor.Emit(OpCodes.Br, loopStart);
            }

            public override bool SupportsLineType(LineType type)
                => type == LineType.InfiniteLoop;
        }
    }
}