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
            var manifest = SongManifest.FromFile("/Users/mulangsu/Documents/Album/Resources/songManifest.json");
            System.Console.WriteLine(manifest?.SpecialSongs.Branch.StartWith);
        }
    }
}
