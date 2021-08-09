using System;

namespace Album {
    public class CompilerOutput {
        public CompilerMessage Message { get; set; }
        public CompilerOutputType Type { get; set; }
        public int LineNumber { get; set; }

        public CompilerOutput(CompilerMessage message, CompilerOutputType type, int line) {
            Message = message;
            Type = type;
            LineNumber = line;
        }

        public override string ToString()
        {
            return $"Line {LineNumber}: {Enum.GetName<CompilerMessage>(Message)}";
        }

        public override bool Equals(object? obj)
        {
            return obj is CompilerOutput output &&
                   Message == output.Message &&
                   Type == output.Type &&
                   LineNumber == output.LineNumber;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Message, Type, LineNumber);
        }
    }

    public enum CompilerOutputType {
        Error, Warning
    }
}