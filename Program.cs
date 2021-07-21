using System;
using System.IO;
using Newtonsoft.Json;
using Album.Syntax;

namespace Album
{
    class Program
    {
        static void Main(string[] args)
        {
            using var s = typeof(Program).Assembly.GetManifestResourceStream("Album.Resources.songManifest.json");
            using var reader = new StreamReader(s);
            var serializer = new JsonSerializer();
            using var jsonTextReader = new JsonTextReader(reader);
            var manifest = serializer.Deserialize<SongManifest>(jsonTextReader);
            System.Console.WriteLine(manifest.SongNames["voracity, by myth & roid"]);
        }
    }
}
