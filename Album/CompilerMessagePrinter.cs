using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System;

namespace Album {
    public interface ICompilerMessagePrinter {
        void Print(CompilerOutput output);
    }
    public class LocalisedCompilerMessagePrinter : ICompilerMessagePrinter {
        private LocalisedCompilerMessages messages;

        public LocalisedCompilerMessagePrinter(CultureInfo culture) {
            

            using var stream = GetResourceStreamFromCulture(culture) ?? 
                                GetResourceStreamFromCulture(new CultureInfo("en"));
            if (stream == null) {
                throw new IOException("No suitable localisation file found!");
            }
            var reader = new StreamReader(stream);
            var serializer = new JsonSerializer();
            var jsonTextReader = new JsonTextReader(reader);
            messages = serializer.Deserialize<LocalisedCompilerMessages>(jsonTextReader) ??
                        throw new JsonSerializationException($"Invalid localisation file for {culture}");
        }

        private static Stream? GetResourceStreamFromCulture(CultureInfo culture) {
            if (culture.Name == "") return null;
            return typeof(LocalisedCompilerMessagePrinter).Assembly.GetManifestResourceStream($"Album.{culture.Name}.json")
                ?? GetResourceStreamFromCulture(culture.Parent);
        }

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