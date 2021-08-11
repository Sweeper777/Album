using System;
using System.Collections.Generic;
using Album.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Album.CodeGen
{
    public abstract class CodeGenerator {
        [DisallowNull]
        protected IEnumerable<LineInfo>? lines;
        protected abstract ICodeGenerationStrategy? GetCodeGenerationStrategyForSong(LineType type);
        protected virtual void WillGenerateLines() { }
        protected virtual void DidGenerateLines() { }

        public void GenerateCode(IEnumerable<LineInfo> lines) {
            this.lines = lines;
            WillGenerateLines();
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
            DidGenerateLines();
        }

        public virtual bool AllowsSemanticErrors => false;
    }
}