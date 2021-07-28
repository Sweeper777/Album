using NUnit.Framework;
using Album.Syntax;
using System.Linq;

namespace Album.Tests {
    [Timeout(1000)]
    public class SongManifestLoaderTests {
        private const string MANIFEST_WTIH_DUPLICATE_SONGS = @"
{
    ""SongNames"": {
        ""voracity, by myth & roid"": ""Input"",
        ""VORACITY, by myth & roid"": ""Input""
    },
    ""SpecialSongs"": {
        ""PushX"": {
            ""StartWith"": null,
            ""EndWith"": null
        },
        ""Branch"": {
            ""StartWith"": null,
            ""EndWith"": null
        }
    },
    ""SpecialPushes"": {}
}
        ";

        private const string MANIFEST_WTIH_NO_SPECIAL_SONGS = @"
{
    ""SongNames"": {
        ""voracity, by myth & roid"": ""Input""
    },
    ""SpecialSongs"": {
        
    },
    ""SpecialPushes"": {}
}
        ";

        private const string MANIFEST_WTIH_MISSING_KEYS = @"
{
    ""SongNames"": {
        ""voracity, by myth & roid"": ""Input""
    }
}
        ";

        private const string MANIFEST_WITH_INVALID_SONGS1 = @"
{
    ""SongNames"": {
        ""voracity, by myth & roid"": ""Input"",
        ""foo"": ""Foo""
    },
    ""SpecialSongs"": {
        ""PushX"": {
            ""StartWith"": null,
            ""EndWith"": null
        },
        ""Branch"": {
            ""StartWith"": null,
            ""EndWith"": null
        }
    },
    ""SpecialPushes"": {}
}
        ";

        private const string MANIFEST_WITH_INVALID_SONGS2 = @"
{
    ""SongNames"": {
        ""voracity, by myth & roid"": ""Input"",
        ""foo"": ""Push""
    },
    ""SpecialSongs"": {
        ""PushX"": {
            ""StartWith"": null,
            ""EndWith"": null
        },
        ""Branch"": {
            ""StartWith"": null,
            ""EndWith"": null
        }
    },
    ""SpecialPushes"": {}
}
        ";

    }
}