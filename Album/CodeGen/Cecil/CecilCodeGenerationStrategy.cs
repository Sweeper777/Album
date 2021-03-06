using Album.Syntax;
using Mono.Cecil.Cil;

namespace Album.CodeGen.Cecil
{
    internal abstract class CecilCodeGenerationStrategy : ICodeGenerationStrategy
    {
        protected MethodReferenceProvider methods;

        public ILProcessor ILProcessor { get; set; }

        public CecilCodeGenerationStrategy(MethodReferenceProvider methods, ILProcessor ilProcessor)
        {
            this.methods = methods;
            ILProcessor = ilProcessor;
        }

        public abstract void GenerateCodeForSong(LineInfo line);
        public abstract bool SupportsLineType(LineType type);
    }
}