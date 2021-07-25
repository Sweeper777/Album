using Album.Syntax;
using System.Collections.Generic;

namespace Album.Tests {
    public class DummySongManifest : ISongManifest
    {
        private static SpecialSongInfo pushXSyntax = new(
            startWith: null, endWith: " pushes"
        );
        private static SpecialSongInfo branchSyntax = new(
            startWith: "branch to ", endWith: null
        );

        public SpecialSongInfo PushXSyntax { get; } = pushXSyntax;

        public SpecialSongInfo BranchSyntax { get; } = branchSyntax;

        private static readonly Dictionary<string, LineType> songNames = new() {
            { "input", LineType.Input },
            { "outputchar", LineType.OutputChar },
            { "outputint", LineType.OutputInt },
            { "double", LineType.Double },
            { "halve", LineType.Halve },
            { "clear", LineType.Clear },
            { "pop", LineType.Pop },
            { "dup", LineType.Dup },
            { "add", LineType.Add },
            { "and", LineType.And },
            { "sub", LineType.Sub },
            { "or", LineType.Or },
            { "eor", LineType.Eor },
            { "quit", LineType.Quit },
            { "cycle", LineType.Cycle },
            { "rcycle", LineType.RCycle },
            { "topzero", LineType.TopZero },
            { "toppositive", LineType.TopPositive },
            { "topnegative", LineType.TopNegative },
            { "infinite loop", LineType.InfiniteLoop }
        };

        private static readonly Dictionary<string, int> specialPushes = new() {
            { "1 push", 1 }, { "no pushes", 0 }
        };

        public LineType GetFixedLineType(string songName)
            => songNames.GetValueOrDefault(songName);

        public int? GetFixedPushAmount(string songName)
            => specialPushes.TryGetValue(songName, out int value) ? value : null;
    }
}