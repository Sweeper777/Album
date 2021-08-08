using System;
using System.IO;
using Album.CodeGen.Cecil;
using CommandLine;
using CommandLine.Text;

namespace Album
{
    class Program
    {
        static void Main(string[] args)
        {
            ParserResult<CompilerOptions> parserResult = 
                Parser.Default.ParseArguments<CompilerOptions>(args);
            parserResult
                .WithParsed(RunWithOptions);
            CecilCodeGenerator codegen = new();
            }
        }
    }
}
