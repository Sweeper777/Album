using Album.Syntax;
using Album.CodeGen;
using System.Collections.Generic;

namespace Album.Tests {
    public class DummyCodeGenerator : CodeGenerator
    {

        public bool HasGenerated { get; set; }

        public override void GenerateCode(IEnumerable<LineInfo> lines)
        {
            HasGenerated = true;
        }
    }
}