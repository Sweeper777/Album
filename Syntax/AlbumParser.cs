using System;
using System.IO;
using System.Collections.Generic;

namespace Album.Syntax {
    public class AlbumParser: IDisposable {
        private const string PLAYLIST_CREATOR_PREFIX = "playlist created by ";

        private bool disposedValue;
        private StreamReader reader;
        private int currentLine = 0;

        public ISongManifest SongManifest { get; set; }
        public IList<CompilerOutput> Outputs { get; set; } = new List<CompilerOutput>();

        public AlbumParser(ISongManifest manifest, Stream stream) {
            SongManifest = manifest;
            this.reader = new StreamReader(stream);
        }

        public AlbumParser(SongManifest manifest, string path) 
            : this(manifest, File.OpenRead(path)) {

        }


        private string? ReadNextLine() {
            var line = reader.ReadLine()?.Cleaned().ToLowerInvariant();
            if (line != null) {
                currentLine++;
            }
            return line;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    reader.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    static class StringCleaner {
        public static string Cleaned(this string s) => s.Trim().Trim(';', '.');
    }
}