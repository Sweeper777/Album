using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Album {
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CompilerMessage {
        NoPlaylistCreatorDeclFound
    }
}