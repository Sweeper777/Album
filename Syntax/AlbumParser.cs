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

        private void OutputError(CompilerMessage message) {
            Outputs.Add(new CompilerOutput(
                message,
                CompilerOutputType.Error,
                currentLine
            ));
        }

        public IList<LineInfo> Parse() {
            var list = new List<LineInfo>();
            var playlistCreatorName = FindPlaylistCreatorName();
            if (playlistCreatorName == null) {
                OutputError(CompilerMessage.NoPlaylistCreatorDeclFound);
                return list;
            }

            // TODO: continue parsing

            return list;
        }

        private string? ReadNextLine() {
            var line = reader.ReadLine()?.Cleaned().ToLowerInvariant();
            if (line != null) {
                currentLine++;
            }
            return line;
        }

        public string? FindPlaylistCreatorName() {
            string? line;
            while ((line = ReadNextLine()) != null) {
                var name = FindPlaylistCreatorNameFrom(line);
                if (name != null) {
                    return name;
                }
            }
            return null;
        }

        public string? FindPlaylistCreatorNameFrom(string line) {
            if (line.StartsWith(PLAYLIST_CREATOR_PREFIX)) {
                return line.Substring(PLAYLIST_CREATOR_PREFIX.Length).Cleaned();
            } else {
                return null;
            }
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