using System;

namespace Album {

    [Flags]
    public enum OptimisationOptions{
        None = 0,
        EvaluateCompileTimeConstants = 1 << 0,
        DetectUnconditionalBranches = 1 << 1,
        DetectUnnecessaryBranches = 1 << 2
    }
}