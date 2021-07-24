using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Album.Syntax {
    public class AlbumParser: IDisposable {
        private const string PLAYLIST_CREATOR_PREFIX = "playlist created by ";

        private bool disposedValue;
        private TextReader reader;
        private int currentLine = 0;

        public ISongManifest SongManifest { get; set; }
        public IList<CompilerOutput> Outputs { get; set; } = new List<CompilerOutput>();

        public AlbumParser(ISongManifest manifest, Stream stream) {
            SongManifest = manifest;
            this.reader = new StreamReader(stream);
        }

        public AlbumParser(SongManifest manifest, string text) {
            SongManifest = manifest;
            this.reader = new StringReader(text);
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

        private string? FindPlaylistCreatorName() {
            while (ReadNextLine() is string line) {
                var name = FindPlaylistCreatorNameFrom(line);
                if (!string.IsNullOrEmpty(name)) {
                    return name;
                }
            }
            return null;
        }

        private string? FindPlaylistCreatorNameFrom(string line) {
            if (line.StartsWith(PLAYLIST_CREATOR_PREFIX)) {
                return line.Substring(PLAYLIST_CREATOR_PREFIX.Length).TrimStart();
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