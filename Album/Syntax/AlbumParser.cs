using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Album.Syntax {
    public class AlbumParser: IDisposable {
        private const string PLAYLIST_CREATOR_PREFIX = "playlist created by ";
        private const string ORIGINAL_SONG_SEPARATOR = ", by ";

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

            while (ReadNextLine() is string line) {
                list.Add(ParseLine(line, playlistCreatorName));
            }

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

        private LineInfo ParseLine(string line, string playlistCreator) {
            var fixedLineType = SongManifest.GetFixedLineType(line);
            if (fixedLineType != LineType.Comment) {
                return LineInfo.OfType(fixedLineType);
            }
            if (SongManifest.GetFixedPushAmount(line) is int pushAmount) {
                return LineInfo.Push(pushAmount);
            }

            if (TryParseFromSpecialSongInfo(SongManifest.PushXSyntax, line, out var pushAmountString) &&
                int.TryParse(pushAmountString, out pushAmount)) {
                if (pushAmount < 0) {
                    OutputError(CompilerMessage.PushXTooSmall);
                    return LineInfo.OfType(LineType.Comment);
                }
                if (pushAmount > 99) {
                    OutputError(CompilerMessage.PushXTooLarge);
                    return LineInfo.OfType(LineType.Comment);
                }
                if (pushAmount == 1) {
                    OutputError(CompilerMessage.Push1);
                    return LineInfo.OfType(LineType.Comment);
                }
                return LineInfo.Push(pushAmount);
            }

            if (TryParseFromSpecialSongInfo(SongManifest.BranchSyntax, line, out var originalSong) &&
                originalSong != "") {
                return LineInfo.Branch(originalSong);
            }

            if (TryParseOriginalSong(line, playlistCreator, out originalSong) && 
                originalSong != "") {
                if (originalSong.Any(":;.".Contains)) {
                    OutputError(CompilerMessage.InvalidOriginalSongName);
                    return LineInfo.OfType(LineType.Comment);
                } else {
                    return LineInfo.OriginalSong(originalSong);
                }
            }

            return LineInfo.OfType(LineType.Comment);
        }

        private bool TryParseFromSpecialSongInfo(SpecialSongInfo info, string line, 
                                                [NotNullWhen(true)] out string? result) {
            result = null;
            if (info.StartWith is string prefix && line.StartsWith(prefix)) {
                result = line.Substring(prefix.Length);
            }
            if (info.EndWith is string suffix && line.EndsWith(suffix)) {
                result = result ?? line;
                result = result.Substring(0, result.Length - 1 - suffix.Length);
            }
            result = result?.Trim();
            return result != null;
        }

        private bool TryParseOriginalSong(string line, string playlistCreator, 
                                          [NotNullWhen(true)] out string? result) {
            int index = line.IndexOf(ORIGINAL_SONG_SEPARATOR, 0);
            while (index >= 0) {
                var artistName = line.Substring(index + ORIGINAL_SONG_SEPARATOR.Length).TrimStart();
                if (artistName == playlistCreator) {
                    result = line.Substring(0, index);
                    return true;
                }
                index = line.IndexOf(ORIGINAL_SONG_SEPARATOR, index);
            }
            result = null;
            return false;
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