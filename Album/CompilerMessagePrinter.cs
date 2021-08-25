using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System;

namespace Album {
    public interface ICompilerMessagePrinter {
        void Print(CompilerOutput output);
    }

    public class LocalisedCompilerMessages {
        public LocalisedCompilerMessages(
            string format, 
            Dictionary<CompilerOutputType, string> localisedTypeNames, 
            Dictionary<CompilerMessage, string> localisedMessages
            ) {
            Format = format;
            LocalisedTypeNames = localisedTypeNames;
            LocalisedMessages = localisedMessages;
        }

        [JsonProperty("Format")]
        public string Format { get; }

        [JsonProperty("TypeNames")]
        public Dictionary<CompilerOutputType, string> LocalisedTypeNames { get; }

        [JsonProperty("Messages")]
        public Dictionary<CompilerMessage, string> LocalisedMessages { get; }
    }

    public class CompilerMessagePrinter : ICompilerMessagePrinter
    {
        public void Print(CompilerOutput output)
        {
            Console.WriteLine($"{output.Type} at line {output.LineNumber}: {output.Message}");
        }
    }
}