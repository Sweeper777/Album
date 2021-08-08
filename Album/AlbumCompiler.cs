using System.Collections.Generic;
using System.Linq;
using System.IO;
using Album.CodeGen;
using Album.CodeGen.Cecil;
using Album.Syntax;
using Album.Semantics;

namespace Album {
    public class AlbumCompiler {

        public CodeGenerator CodeGenerator { get; set;}
        public SongManifest SongManifest { get; set; }
        
        public AlbumCompiler(CodeGenerator codeGenerator)
        {
            CodeGenerator = codeGenerator;
            using var s = typeof(Program).Assembly.GetManifestResourceStream("Album.songManifest.json") ??
                throw new FileNotFoundException("Unable to read default song manifest");
            SongManifest = SongManifest.FromStream(s) ?? throw new IOException("Unable to read default song manifest");
        }

        // TODO: compiler options

        public void Compile(Stream source) {
            AlbumParser parser = new(SongManifest, source);
            IList<LineInfo> lines = parser.Parse();
            foreach (var output in parser.Outputs) {
                DisplayOutput?.Invoke(output);
            }
            SemanticAnalyser analyser = new(lines);
            foreach (var output in analyser.Outputs) {
                DisplayOutput?.Invoke(output);
            }
            if (!parser.Outputs.Union(analyser.Outputs).Any(x => x.Type == CompilerOutputType.Error)) {
                CodeGenerator.GenerateCode(lines);
            }
        }

        public event CompilerOutputHandler? DisplayOutput;
    }

    public delegate void CompilerOutputHandler(CompilerOutput output);
}