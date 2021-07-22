using System;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace Album.Syntax {
    public interface ISongManifest {
        LineType GetFixedLineType(string songName);
        int? GetFixedPushAmount(string songName);
        SpecialSongInfo PushXSyntax { get; }
        SpecialSongInfo BranchSyntax { get; }
    }
}