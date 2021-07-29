using System;
using System.Diagnostics.CodeAnalysis;

namespace Album.Syntax {
    public struct LineInfo {
        private LineType type;
        private string? stringValue;
        private int? intValue;

        private LineInfo(LineType type, string? stringValue, int? intValue) {
            this.type = type;
            this.stringValue = stringValue;
            this.intValue = intValue;
        }

        public static LineInfo OfType(LineType type) {
            if (type == LineType.Push ||
                type == LineType.OriginalSong ||
                type == LineType.Branch) {
                throw new ArgumentException(
                    "type must not be a type of line that takes arguments!",
                    nameof(type)
                    );
            }
            return new LineInfo(type, null, null);
        }

        public static LineInfo Push(int number)
            => new LineInfo(LineType.Push, null, number);

        public static LineInfo OriginalSong(string name)
            => new LineInfo(LineType.OriginalSong, name, null);

        public static LineInfo Branch(string originalSong)
            => new LineInfo(LineType.Branch, originalSong, null);

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
    }
}