using System;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Album.Syntax {
    public class SongManifest : ISongManifest{

        [JsonProperty("SongNames")]
        [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<LineType>))]
        [JsonRequired]
        public Dictionary<string, LineType> SongNames { get; }

        [JsonProperty("SpecialPushes")]
        [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<int>))]
        [JsonRequired]
        public Dictionary<string, int> SpecialPushes { get; }
        
        [JsonProperty("SpecialSongs")]
        [JsonRequired]
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
            try {
                var result = serializer.Deserialize<SongManifest>(jsonTextReader);
                if (result?.IsValid() == true) {
                    return result;
                } else {
                    return null;
                }
            } catch (JsonSerializationException) {
                return null;
            }
        }

        public static SongManifest? FromFile(string path) {
            using var fileStream = File.OpenRead(path);
            return FromStream(fileStream);
        }

        public static SongManifest? FromText(string text) {
            try {
                var result = JsonConvert.DeserializeObject<SongManifest>(text);
                if (result?.IsValid() == true) {
                    return result;
                } else {
                    return null;
                }
            } catch (JsonSerializationException) {
                return null;
            }
        }

        public bool IsValid()
            => !SongNames.Values.Contains(LineType.Push) &&
                !SongNames.Values.Contains(LineType.Branch) &&
                !SongNames.Values.Contains(LineType.OriginalSong) &&
                !SongNames.Values.Contains(LineType.UnconditionalBranch);

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
        [JsonRequired]
        public SpecialSongInfo PushX { get; }

        [JsonProperty("Branch")]
        [JsonRequired]
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
        public string? StartWith { get; }

        [JsonProperty("EndWith")]
        public string? EndWith { get; }

        public SpecialSongInfo(string? startWith, string? endWith) {
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

    public class CaseInsensitiveDictionaryConverter<TValue> : JsonConverter {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, TValue>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JToken obj = JToken.ReadFrom(reader);                
            if (objectType == typeof(Dictionary<string, TValue>))
            {
                Dictionary<string, TValue> result = new Dictionary<string, TValue>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var property in ((JObject)obj).Properties()) {
                    if (result.ContainsKey(property.Name)) {
                        throw new JsonSerializationException();
                    } else {
                        result.Add(property.Name, 
                                    serializer.Deserialize<TValue>(property.Value.CreateReader()) ??
                                    throw new JsonSerializationException()
                                    );
                    }
                }
                return result;
            }
            return obj.ToObject(objectType) ?? throw new JsonSerializationException();
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            => throw new NotImplementedException();
    }
}