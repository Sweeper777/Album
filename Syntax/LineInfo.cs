using System;

namespace Album.Syntax {
    public struct LineInfo {
        private LineType type;
        private string stringValue;
        private int? intValue;

        private LineInfo(LineType type, string stringValue, int? intValue) {
            this.type = type;
            this.stringValue = stringValue;
            this.intValue = intValue;
        }

        public static LineInfo OfType(LineType type) {
            if (type == LineType.Push ||
                type == LineType.Label ||
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

        public static LineInfo Label(string name)
            => new LineInfo(LineType.Label, name, null);

        public static LineInfo Branch(string label)
            => new LineInfo(LineType.Branch, label, null);
    }
}