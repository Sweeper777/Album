using System;
using System.Collections.Generic;
using Album.Syntax;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Album.Semantics {
    public class SemanticAnalyser {
        public IList<CompilerOutput> Outputs { get; } = new List<CompilerOutput>();

        [DisallowNull]
        public HashSet<string>? UnusedOriginalSongs { get; private set; }
        
        [DisallowNull]
        public HashSet<string>? UsedOriginalSongs { get; private set; } 

        public ControlFlowGraph? CFG { get; private set; } 

        public void Analyse(IEnumerable<LineInfo> lines) {
            Outputs.Clear();
            HashSet<LineInfo> originalSongLines = new();
            HashSet<LineInfo> branchLines = new();
            UnusedOriginalSongs = new();
            UsedOriginalSongs = new();
            foreach (var line in lines) {
                if (line.IsOriginalSong(out var name)) {
                    if (!originalSongLines.Add(line)) {
                        Outputs.Add(new CompilerOutput(
                            CompilerMessage.DuplicateOriginalSong,
                            CompilerOutputType.Error,
                            line.LineNumber
                        ));
                    }
                    UsedOriginalSongs.Add(name);
                } else if (line.IsAnyBranch(out _)) {
                    branchLines.Add(line);
                }
            }

            HashSet<LineInfo> diff = new(originalSongLines, LineInfo.OriginalSongEquality);
            diff.SymmetricExceptWith(branchLines);
            foreach (var line in diff) {
                if (line.IsOriginalSong(out var name)) {
                    Outputs.Add(new CompilerOutput(
                        CompilerMessage.UnusedOriginalSong,
                        CompilerOutputType.Warning,
                        line.LineNumber
                    ));
                    UnusedOriginalSongs.Add(name);
                    UsedOriginalSongs.Remove(name);
                } else if (line.IsAnyBranch(out _)) {
                    Outputs.Add(new CompilerOutput(
                        CompilerMessage.UnknownOriginalSong,
                        CompilerOutputType.Error,
                        line.LineNumber
                    ));
                }
            }
            if (lines.Any()) {
                CFG = GenerateCFG(lines.ToList());
            } else {
                CFG = null;
            }
        }

        private ControlFlowGraph GenerateCFG(IReadOnlyList<LineInfo> lines) {
            if (UnusedOriginalSongs == null || UsedOriginalSongs == null) {
                throw new InvalidOperationException("GenerateCFG must be called after Analyse!");
            }

            ControlFlowGraph cfg = new(lines);
            BuildBasicBlocks(cfg, UsedOriginalSongs);
            return cfg;
        }

        private void BuildBasicBlocks(ControlFlowGraph cfg, HashSet<string> usedOriginalSongs) {
        }
    }
}