using System;
using System.Collections.Generic;
using Album.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Album.CodeGen
{
    public abstract class CodeGenerator {
        [DisallowNull]
        protected IEnumerable<LineInfo>? lines;
        public abstract void GenerateCode(IEnumerable<LineInfo> lines);
    }
}