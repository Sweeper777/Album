using System;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace Album.Syntax {
    public class SongManifest {

        [JsonProperty("SongNames")]
        public Dictionary<string, LineType> SongNames { get; }

        [JsonProperty("SpecialPushes")]
        public Dictionary<string, int> SpecialPushes { get; }
        
        [JsonProperty("SpecialSongs")]
        public SpecialSongs SpecialSongs { get; }

        public SongManifest(Dictionary<string, LineType> songNames,
                            Dictionary<string, int> specialPushes,
                            SpecialSongs specialSongs) {
            SongNames = songNames;
            SpecialPushes = specialPushes;
            SpecialSongs = specialSongs;
        }
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
    }
}