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
        }

        private static void RunWithOptions(CompilerOptions options) {
            if (!ValidateOptions(options)) {
                return;
            }

            
            string absolutePath = Path.GetFullPath(options.OutputPath ?? "Program.exe");
            if (!(Path.GetDirectoryName(absolutePath) is string directory) ||
                !(Path.ChangeExtension(absolutePath, "runtimeconfig.json") is string configPath)) {
                Console.WriteLine($"Output file '{options.OutputPath}' is a directory!");
                return;
            }
            
            CecilCodeGenerator codegen = new();
            using (var file = File.OpenRead(options.InputPath!)) {
                AlbumCompiler compiler = new(codegen);
                compiler.Compile(file);
            }

            try {
                if (codegen.GeneratedAssembly != null && codegen.GeneratedModule != null) {
                    Directory.CreateDirectory(directory);
                    codegen.GeneratedAssembly.Name = new Mono.Cecil.AssemblyNameDefinition("Program", new(1, 0));
                    codegen.GeneratedModule.Name = "Program";
                    codegen.GeneratedAssembly.Write(options.OutputPath ?? "Program.exe");
                    string runtimeConfigContents;
                    using (var stream = new StreamReader(typeof(Program).Assembly.GetManifestResourceStream("Album.runtimeconfig_template.json")!)) {
                        runtimeConfigContents = stream.ReadToEnd();
                    }
                    File.WriteAllText(configPath, runtimeConfigContents);
                }
            } catch (IOException ex) {
                Console.WriteLine($"IO Error: {ex.Message}");
            }
        }

        private static bool ValidateOptions(CompilerOptions options) {
            if (!File.Exists(options.InputPath)) {
                Console.WriteLine($"Input file '{options.InputPath}' not found!");
                return false;
            }
            if (Directory.Exists(options.OutputPath ?? "Program.exe") 
                || Directory.Exists(Path.ChangeExtension(options.OutputPath ?? "Program.exe", "runtimeconfig.json"))) {
                Console.WriteLine($"Output file '{options.OutputPath}' is a directory!");
                return false;
            }
            if (!Enum.IsDefined<WarningLevel>(options.WarningLevel)) {
                Console.WriteLine($"Invalid warning level: '{options.WarningLevel}'!");
                return false;
            }
            return true;
        }
    }
}
