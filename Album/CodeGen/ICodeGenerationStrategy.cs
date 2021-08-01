using Album.Syntax;

namespace Album.CodeGen
{
    public interface ICodeGenerationStrategy {
        void GenerateCodeForSong(LineInfo line);

        bool SupportsLineType(LineType type);
    }
}