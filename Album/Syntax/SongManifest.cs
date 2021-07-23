using System;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace Album.Syntax {
    public class SongManifest : ISongManifest{

        [JsonProperty("SongNames")]
        public Dictionary<string, LineType> SongNames { get; }

        [JsonProperty("SpecialPushes")]
        public Dictionary<string, int> SpecialPushes { get; }
        
        [JsonProperty("SpecialSongs")]
        public SpecialSongs SpecialSongs { get; }

        public SpecialSongInfo PushXSyntax => SpecialSongs.PushX;

        public SpecialSongInfo BranchSyntax => SpecialSongs.Branch;

        public SongManifest(Dictionary<string, LineType> songNames,
                            Dictionary<string, int> specialPushes,
                            SpecialSongs specialSongs) {
            SongNames = songNames;
            SpecialPushes = specialPushes;
            SpecialSongs = specialSongs;
        }

        public static SongManifest? FromStream(Stream stream) {
            var reader = new StreamReader(stream);
            var serializer = new JsonSerializer();
            var jsonTextReader = new JsonTextReader(reader);
            return serializer.Deserialize<SongManifest>(jsonTextReader);
        }

        public static SongManifest? FromFile(string path) {
            using var fileStream = File.OpenRead(path);
            return FromStream(fileStream);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SongNames, SpecialPushes, SpecialSongs);
        }

        public override bool Equals(object? obj)
        {
            return obj is SongManifest manifest &&
                   EqualityComparer<Dictionary<string, LineType>>.Default.Equals(SongNames, manifest.SongNames) &&
                   EqualityComparer<Dictionary<string, int>>.Default.Equals(SpecialPushes, manifest.SpecialPushes) &&
                   EqualityComparer<SpecialSongs>.Default.Equals(SpecialSongs, manifest.SpecialSongs);
        }

        public LineType GetFixedLineType(string songName)
            => SongNames.GetValueOrDefault(songName);

        public int? GetFixedPushAmount(string songName)
            => SpecialPushes.TryGetValue(songName, out int value) ? value : null;
    }

    public class SpecialSongs {
        [JsonProperty("PushX")]
        public SpecialSongInfo PushX { get; }

        [JsonProperty("Branch")]
        public SpecialSongInfo Branch { get; }

        public SpecialSongs(SpecialSongInfo pushX, SpecialSongInfo branch) {
            PushX = pushX;
            Branch = branch;
        }

        public override bool Equals(object? obj)
        {
            return obj is SpecialSongs songs &&
                   EqualityComparer<SpecialSongInfo>.Default.Equals(PushX, songs.PushX) &&
                   EqualityComparer<SpecialSongInfo>.Default.Equals(Branch, songs.Branch);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PushX, Branch);
        }
    }

    public class SpecialSongInfo {
        [JsonProperty("StartWith")]
        public string StartWith { get; }

        [JsonProperty("EndWith")]
        public string EndWith { get; }

        public SpecialSongInfo(string startWith, string endWith) {
            StartWith = startWith;
            EndWith = endWith;
        }

        public override bool Equals(object? obj)
        {
            return obj is SpecialSongInfo info &&
                   StartWith == info.StartWith &&
                   EndWith == info.EndWith;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartWith, EndWith);
        }
    }
}