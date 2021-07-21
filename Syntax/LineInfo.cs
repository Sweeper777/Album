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
    }
}