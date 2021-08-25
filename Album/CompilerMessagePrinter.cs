using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System;

namespace Album {
    public interface ICompilerMessagePrinter {
        void Print(CompilerOutput output);
    }
}