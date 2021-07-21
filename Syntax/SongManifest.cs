using System;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace Album.Syntax {
    public class SongManifest {

        [JsonProperty("SongNames")]
        public Dictionary<string, LineType> SongNames { get; set; }

        [JsonProperty("SpecialPushes")]
        public Dictionary<string, int> SpecialPushes { get; set; }
        
        [JsonProperty("SpecialSongs")]
        public SpecialSongs SpecialSongs { get; set; }
    }

    public class SpecialSongs {
        [JsonProperty("PushX")]
        public SpecialSongInfo PushX { get; set; }

        [JsonProperty("Branch")]
        public SpecialSongInfo Branch { get; set; }
    }

    public class SpecialSongInfo {
        [JsonProperty("StartWith")]
        public string StartWith { get; set; }

        [JsonProperty("EndWith")]
        public string EndWith { get; set; }
    }
}