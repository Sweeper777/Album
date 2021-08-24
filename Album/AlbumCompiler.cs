using System.Collections.Generic;
using System.Linq;
using System.IO;
using Album.CodeGen;
using System.Text;
using Album.Syntax;
using Album.Semantics;

namespace Album {
    public class AlbumCompiler {

        public CodeGenerator CodeGenerator { get; set;}


        public SemanticAnalyser Analyser { get; set; } = new();

        public CodeOptimiser Optimiser { get; set; } = new();

        public ISongManifest SongManifest { get; set; }

        public bool EnableOptimisation { get; set; } = false;

        public IEnumerable<CompilerOutput> Outputs { get; set; } = Enumerable.Empty<CompilerOutput>();
        
        public AlbumCompiler(CodeGenerator codeGenerator)
        {
            CodeGenerator = codeGenerator;
            using var s = typeof(Program).Assembly.GetManifestResourceStream("Album.songManifest.json") ??
                throw new FileNotFoundException("Unable to read default song manifest");
            SongManifest = Album.Syntax.SongManifest.FromStream(s) ?? 
                            throw new IOException("Unable to read default song manifest");
        }

        public WarningLevel WarningLevel { get; set; } = WarningLevel.Warning;

        public void Compile(Stream source) {
            AlbumParser parser = new(SongManifest, source);
            IList<LineInfo> lines = parser.Parse();
            if (EnableOptimisation) {
                lines = Optimiser.Optimise(lines);
            }
            Analyser.Analyse(lines);

            IEnumerable<CompilerOutput> allOutputs = 
                parser.Outputs.Union(Analyser.Outputs)
                    .Where(x => !(WarningLevel == WarningLevel.None && x.Type == CompilerOutputType.Warning))
                    .ToList();
            if (WarningLevel == WarningLevel.Error) {
                foreach (var output in allOutputs) {
                    if (output.Type == CompilerOutputType.Warning) {
                        output.Type = CompilerOutputType.Error;
                    }
                }
            }
            
            Outputs = allOutputs;

            if (!allOutputs.Any(x => x.Type == CompilerOutputType.Error)) {
                CodeGenerator.GenerateCode(lines);
            }
        }

        public void Compile(string source) {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(source));
            stream.Position = 0;
            Compile(stream);
        }
    }
}