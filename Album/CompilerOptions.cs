using System;
using CommandLine;

namespace Album {
    public class CompilerOptions {

        [Value(0, MetaName = "Input", Required = true)]
        public string? InputPath { get; set; }

        [Option('o', "output", Required = false, HelpText = "Path to output file.")]
        public string? OutputPath { get; set; }

        [Option('p', "parse-only", Required = false, 
        HelpText = "Generates the parser output, with optimisations applied, if any")]
        public bool ParseOnly { get; set; }

        [Option('O', "optimise", Required = false, 
        HelpText = "Smaller output code size, longer compile time")]
        public bool DoesOptimise { get; set; }

        [Option('w', "warn", Required = false, Default = WarningLevel.Warning,
        HelpText = "Warning level, 0: no warning, 1: normal warnings, 2: treat warnings as errors")]
        public WarningLevel WarningLevel { get; set; }
    }
}