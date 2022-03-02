using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Album {
    public class CompilerOptions {

        [Value(0, MetaName = "Input", Required = true)]
        public string? InputPath { get; set; }

        [Option('o', "output", Required = false, HelpText = "Path to output file.")]
        public string? OutputPath { get; set; }

        [Option('P', "parse-only", Required = false, SetName = "RunOptions",
        HelpText = "Generates the parser output, with optimisations applied, if any")]
        public bool ParseOnly { get; set; }

        [Option('R', "run", Required = false, SetName = "RunOptions",
        HelpText = "Runs the program immediately, without generating any files.")]
        public bool RunsImmediately { get; set; }

        [Option('O', "optimise", Required = false, 
        HelpText = "Smaller output code size, longer compile time")]
        public bool DoesOptimise { get; set; }

        [Option("llvm", Required = false, 
        HelpText = "Outputs LLVM IR", SetName = "RunOptions")]
        public bool UsesLlvm { get; set; }

        [Option('w', "warn", Required = false, Default = WarningLevel.Warning,
        HelpText = "What warnings should be output as")]
        public WarningLevel WarningLevel { get; set; }

        [Option('s', "manifest", Required = false, HelpText = "Path to custom song manifest file")]
        public string? SongManifestPath { get; set; }

        [MemberNotNull(nameof(OutputPath))]
        public void SetDefaultOutputPathIfNeeded() {
            if (OutputPath == null) {
                if (ParseOnly) {
                    OutputPath = "ParserOutput.txt";
                } else if (UsesLlvm) {
                    OutputPath = "Output.ll";
                } else  {
                    OutputPath = "Program.exe";
                }
            }
        }
    }
}