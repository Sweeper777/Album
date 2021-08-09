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

        public WarningLevel WarningLevel { get; set; } = WarningLevel.Warning;

        public void Compile(Stream source) {
            AlbumParser parser = new(SongManifest, source);
            IList<LineInfo> lines = parser.Parse();
            SemanticAnalyser analyser = new(lines);

            IEnumerable<CompilerOutput> allOutputs = 
                parser.Outputs.Union(analyser.Outputs)
                    .Where(x => WarningLevel == WarningLevel.None && x.Type == CompilerOutputType.Warning)
                    .ToList();
            if (WarningLevel == WarningLevel.Error) {
                foreach (var output in allOutputs) {
                    if (output.Type == CompilerOutputType.Warning) {
                        output.Type = CompilerOutputType.Error;
                    }
                }
            }
            

            if (!allOutputs.Any(x => x.Type == CompilerOutputType.Error)) {
                CodeGenerator.GenerateCode(lines);
            }
        }
    }
}