using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Album.Syntax {
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LineType {
        Comment,
        Input,
        OutputInt,
        OutputChar,
        Push,
        Double,
        Halve,
        Clear,
        Pop,
        Dup,
        Branch,
        Add,
        And,
        Or,
        Sub,
        Eor,
        Quit,
        Swap,
        Cycle,
        RCycle,
        OriginalSong,
        TopPositive,
        TopNegative,
        TopZero,
        InfiniteLoop,

        UnconditionalBranch
    }
}