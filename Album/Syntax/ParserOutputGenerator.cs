using Album.CodeGen;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace Album.Syntax {
    public class ParserOutputGenerator : CodeGenerator
    {

        [DisallowNull]
        public StringBuilder? Output { get; private set; }

        public override void GenerateCode(IEnumerable<LineInfo> lines)
        {
            Output = new();
            foreach (var line in lines) {
                Output.AppendLine(line.ToString());
            }
        }
    }
}