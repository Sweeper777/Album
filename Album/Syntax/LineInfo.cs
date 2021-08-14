using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace Album.Syntax {
    public struct LineInfo {
        private LineType type;
        private string? stringValue;
        private int? intValue;

        public int LineNumber { get; }

        public LineType Type => type;

        private LineInfo(LineType type, string? stringValue, int? intValue, int lineNo) {
            LineNumber = lineNo;
            this.type = type;
            this.stringValue = stringValue;
            this.intValue = intValue;
        }

        public static LineInfo OfType(LineType type, int lineNo = 0) {
            if (type == LineType.Push ||
                type == LineType.OriginalSong ||
                type == LineType.Branch) {
                throw new ArgumentException(
                    "type must not be a type of line that takes arguments!",
                    nameof(type)
                    );
            }
            return new LineInfo(type, null, null, lineNo);
        }

        public static LineInfo Push(int number, int lineNo = 0)
            => new LineInfo(LineType.Push, null, number, lineNo);

        public static LineInfo OriginalSong(string name, int lineNo = 0)
            => new LineInfo(LineType.OriginalSong, name, null, lineNo);

        public static LineInfo Branch(string originalSong, int lineNo = 0)
            => new LineInfo(LineType.Branch, originalSong, null, lineNo);

        public static LineInfo UnconditionalBranch(string originalSong, int lineNo = 0)
            => new LineInfo(LineType.UnconditionalBranch, originalSong, null, lineNo);

        public bool IsPush([NotNullWhen(true)] out int? x) {
            if (type == LineType.Push) {
                x = intValue!;
            } else {
                x = null;
            }
            return x != null;
        }

        public bool IsOriginalSong([NotNullWhen(true)] out string? name) {
            if (type == LineType.OriginalSong) {
                name = stringValue!;
            } else {
                name = null;
            }
            return name != null;
        }

        public bool IsBranch([NotNullWhen(true)] out string? originalSong) {
            if (type == LineType.Branch) {
                originalSong = stringValue!;
            } else {
                originalSong = null;
            }
            return originalSong != null;
        }

        public bool IsUnconditionalBranch([NotNullWhen(true)] out string? originalSong) {
            if (type == LineType.UnconditionalBranch) {
                originalSong = stringValue!;
            } else {
                originalSong = null;
            }
            return originalSong != null;
        }

        public bool IsAnyBranch([NotNullWhen(true)] out string? originalSong) {
            if (type == LineType.Branch || type == LineType.UnconditionalBranch) {
                originalSong = stringValue!;
            } else {
                originalSong = null;
            }
            return originalSong != null;
        }

        public override string ToString()
        {
            var lineTypeName = Enum.GetName<LineType>(type) ?? "Unknown";
            if (stringValue is string paramStr) {
                return $"{lineTypeName} {paramStr}";
            } else if (intValue is int intParam) {
                return $"{lineTypeName} {intParam}";
            } else {
                return lineTypeName;
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is LineInfo info &&
                   type == info.type &&
                   stringValue == info.stringValue &&
                   intValue == info.intValue;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(type, stringValue, intValue);
        }

        public static readonly IEqualityComparer<LineInfo> OriginalSongEquality = new OriginalSongEqualityComparer();

        private class OriginalSongEqualityComparer : IEqualityComparer<LineInfo>
        {
            public bool Equals(LineInfo x, LineInfo y)
                => EqualityComparer<string>.Default.Equals(x.stringValue, y.stringValue);

            public int GetHashCode([DisallowNull] LineInfo obj)
                => EqualityComparer<string>.Default.GetHashCode(obj.stringValue ?? "");
        }
    }
}