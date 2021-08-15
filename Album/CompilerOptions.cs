using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Album {
    public class CompilerOptions {

        [Value(0, MetaName = "Input", Required = true)]
        public string? InputPath { get; set; }

        [Option('o', "output", Required = false, HelpText = "Path to output file.")]
        public string? OutputPath { get; set; }

        [Option('P', "parse-only", Required = false, 
        HelpText = "Generates the parser output, with optimisations applied, if any")]
        public bool ParseOnly { get; set; }

        [Option('O', "optimise", Required = false, 
        HelpText = "Smaller output code size, longer compile time")]
        public bool DoesOptimise { get; set; }

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
                } else {
                    OutputPath = "Program.exe";
                }
            }
        }
    }
}