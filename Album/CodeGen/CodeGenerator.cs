using System;
using System.Collections.Generic;
using Album.Syntax;

namespace Album.CodeGen
{
    public abstract class CodeGenerator {
        protected IEnumerable<LineInfo> lines;

        public CodeGenerator(IEnumerable<LineInfo> lines) {
            this.lines = lines;
        }

        protected abstract ICodeGenerationStrategy? GetCodeGenerationStrategyForSong(LineType type);

        public void GenerateCode() {
            foreach (var line in lines) {
                var strategy = GetCodeGenerationStrategyForSong(line.Type);
                if (strategy is null) {
                    continue;
                }
                if (strategy.SupportsLineType(line.Type)) {
                    strategy.GenerateCodeForSong(line);
                } else {
                    throw new InvalidOperationException($"Code Generation Strategy {strategy} Does Not Support Line Type {line.Type}!");
                }
            }
        }
    }
}