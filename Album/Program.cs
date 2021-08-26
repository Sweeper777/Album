using System;
using System.IO;
using System.Collections.Generic;
using Album.CodeGen.Cecil;
using Album.Syntax;
using CommandLine;
using CommandLine.Text;
using System.Globalization;

namespace Album
{
    class Program
    {
        static void Main(string[] args)
        {
            
            ParserResult<CompilerOptions> parserResult = new Parser(settings => {
                settings.AutoVersion = false;
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = null;
            }).ParseArguments<CompilerOptions>(args);
            parserResult
                .WithParsed(RunWithOptions)
                .WithNotParsed(x => {
                    HelpText text = HelpText.AutoBuild(parserResult, h => {
                        h.AutoVersion = false;
                        h.Copyright = "";
                        h.Heading = "";
                        h.AddEnumValuesToHelpText = true;
                        return HelpText.DefaultParsingErrorsHandler(parserResult, h);;
                    }, e => e);
                    Console.WriteLine(text);
                });
        }

        private static void RunWithOptions(CompilerOptions options) {
            options.SetDefaultOutputPathIfNeeded();
            if (!ValidateOptions(options)) {
                return;
            }

            try {
                string absolutePath = Path.GetFullPath(options.OutputPath);
                if (!(Path.GetDirectoryName(absolutePath) is string directory) ||
                    !(Path.ChangeExtension(absolutePath, "runtimeconfig.json") is string configPath)) {
                    Console.WriteLine($"Output file '{options.OutputPath}' is a directory!");
                    return;
                }
                
                CecilCodeGenerator codegen = new();
                ParserOutputGenerator parserOutputGen = new();
                SongManifest? inputManifest = null;
                if (options.SongManifestPath != null) {
                    if (SongManifest.FromFile(options.SongManifestPath) is SongManifest manifest) {
                        inputManifest = manifest;
                    } else {
                        Console.WriteLine("Invalid Song Manifest!");
                        return;
                    }
                }
                using (var sourceFile = File.OpenRead(options.InputPath!)) {
                    AlbumCompiler compiler = new(options.ParseOnly ? parserOutputGen : codegen);
                    compiler.WarningLevel = options.WarningLevel;
                    compiler.EnableOptimisation = options.DoesOptimise;
                    compiler.Compile(sourceFile);
                    if (inputManifest != null) {
                        compiler.SongManifest = inputManifest;
                    }
                    PrintErrorsAndWarnings(compiler.Outputs);
                }

                if (codegen.GeneratedAssembly != null && codegen.GeneratedModule != null) {
                    Directory.CreateDirectory(directory);
                    codegen.GeneratedAssembly.Name = new Mono.Cecil.AssemblyNameDefinition("Program", new(1, 0));
                    codegen.GeneratedModule.Name = "Program";
                    codegen.GeneratedAssembly.Write(options.OutputPath);
                    string runtimeConfigContents;
                    using (var stream = new StreamReader(typeof(Program).Assembly.GetManifestResourceStream("Album.runtimeconfig_template.json")!)) {
                        runtimeConfigContents = stream.ReadToEnd();
                    }
                    File.WriteAllText(configPath, runtimeConfigContents);
                    Environment.Exit(0);
                }

                if (parserOutputGen.Output != null) {
                    File.WriteAllText(options.OutputPath, parserOutputGen.Output.ToString());
                    Environment.Exit(0);
                }
                
            } catch (IOException ex) {
                Console.WriteLine($"IO Error: {ex.Message}");
                Environment.Exit(1);
            }
            Environment.Exit(1);
        }

        private static bool ValidateOptions(CompilerOptions options) {
            if (!File.Exists(options.InputPath)) {
                Console.WriteLine($"Input file '{options.InputPath}' not found!");
                return false;
            }
            if (options.SongManifestPath != null && !File.Exists(options.SongManifestPath)) {
                Console.WriteLine($"Song manifest file '{options.SongManifestPath}' not found!");
                return false;
            }
            if (Directory.Exists(options.OutputPath)) {
                Console.WriteLine($"Output file '{options.OutputPath}' is a directory!");
                return false;
            }
            if (!Enum.IsDefined<WarningLevel>(options.WarningLevel)) {
                Console.WriteLine($"Invalid warning level: '{options.WarningLevel}'!");
                return false;
            }
            return true;
        }

        private static void PrintErrorsAndWarnings(IEnumerable<CompilerOutput> outputs) {
            ICompilerMessagePrinter printer;
            try {
                printer = new LocalisedCompilerMessagePrinter(CultureInfo.CurrentCulture);
            } catch {
                printer = new CompilerMessagePrinter();
            }
            foreach (var output in outputs) {
                printer.Print(output);
            }
        }
    }
}
