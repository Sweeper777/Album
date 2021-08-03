using System;
using System.IO;
using Album.CodeGen.Cecil;

namespace Album
{
    class Program
    {
        static void Main(string[] args)
        {
            using var file = File.OpenRead(args[0]);
            CecilCodeGenerator codegen = new();
            AlbumCompiler compiler = new(codegen);
            compiler.Compile(file);
            if (codegen.GeneratedAssembly != null && codegen.GeneratedModule != null) {
                codegen.GeneratedAssembly.Name = new Mono.Cecil.AssemblyNameDefinition("Program", new(1, 0));
                codegen.GeneratedModule.Name = "Program";
                codegen.GeneratedAssembly.Write("Program.exe");
            }
        }
    }
}
