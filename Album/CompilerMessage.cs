using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Album {
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CompilerMessage {
        NoPlaylistCreatorDeclFound,
        PushXTooSmall,
        PushXTooLarge,
        Push1,
        InvalidOriginalSongName,
        DuplicateOriginalSong,
        UnknownOriginalSong,
        UnusedOriginalSong,
        UnreachableCode
    }
}