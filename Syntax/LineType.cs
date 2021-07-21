using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Album.Syntax {
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LineType {
        Unknown,
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
        Label,
        TopPositive,
        TopNegative,
        TopZero,
        InfiniteLoop,
    }
}