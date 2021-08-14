using System;
using System.Collections.Generic;
using Album.Syntax;

namespace Album.Semantics {
    public class SemanticAnalyser {
        public IList<CompilerOutput> Outputs { get; } = new List<CompilerOutput>();

        public void Analyse(IEnumerable<LineInfo> lines) {
            Outputs.Clear();
            HashSet<LineInfo> originalSongLines = new();
            HashSet<LineInfo> branchLines = new();
            foreach (var line in lines) {
                if (line.IsOriginalSong(out _)) {
                    if (!originalSongLines.Add(line)) {
                        Outputs.Add(new CompilerOutput(
                            CompilerMessage.DuplicateOriginalSong,
                            CompilerOutputType.Error,
                            line.LineNumber
                        ));
                    }
                } else if (line.IsAnyBranch(out _)) {
                    branchLines.Add(line);
                }
            }

            HashSet<LineInfo> diff = new(originalSongLines, LineInfo.OriginalSongEquality);
            diff.SymmetricExceptWith(branchLines);
            foreach (var line in diff) {
                if (line.IsOriginalSong(out _)) {
                    Outputs.Add(new CompilerOutput(
                        CompilerMessage.UnusedOriginalSong,
                        CompilerOutputType.Warning,
                        line.LineNumber
                    ));
                } else if (line.IsAnyBranch(out _)) {
                    Outputs.Add(new CompilerOutput(
                        CompilerMessage.UnknownOriginalSong,
                        CompilerOutputType.Error,
                        line.LineNumber
                    ));
                }
            }
        }
    }
}