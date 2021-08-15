using Album.Syntax;
using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace Album.CodeGen.Cecil
{
    internal partial class CecilCodeGenerationStrategies {
        public class BranchOriginalSong : CecilCodeGenerationStrategy
        {
            private Dictionary<string, Instruction> originalSongs = new();

            public BranchOriginalSong(MethodReferenceProvider methods, ILProcessor ilProcessor) 
                : base(methods, ilProcessor) {
            }

            public override void GenerateCodeForSong(LineInfo line)
            {
                if (line.IsBranch(out string? originalSong)) {
                    GenerateBranch(originalSong);
                } else if (line.IsOriginalSong(out originalSong)) {
                    GenerateOriginalSong(originalSong);
                } else if (line.IsUnconditionalBranch(out originalSong)) {
                    GenerateUnconditionalBranch(originalSong);
                }
            }

            private void GenerateBranch(string originalSong) {
                Instruction? target = originalSongs.GetValueOrDefault(originalSong);
                if (target is null) {
                    target = ILProcessor.Create(OpCodes.Nop);
                    originalSongs.Add(originalSong, target);
                }
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListLast);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListNodeValue);
                ILProcessor.Emit(OpCodes.Stloc_0);
                ILProcessor.Emit(OpCodes.Dup);
                ILProcessor.Emit(OpCodes.Callvirt, methods.LinkedListRemoveLast);
                ILProcessor.Emit(OpCodes.Ldloc_0);
                ILProcessor.Emit(OpCodes.Brtrue, target);
            }

            private void GenerateUnconditionalBranch(string originalSong) {
                Instruction? target = originalSongs.GetValueOrDefault(originalSong);
                if (target is null) {
                    target = ILProcessor.Create(OpCodes.Nop);
                    originalSongs.Add(originalSong, target);
                }
                ILProcessor.Emit(OpCodes.Br, target);
            }

            private void GenerateOriginalSong(string name) {
                Instruction? existing = originalSongs.GetValueOrDefault(name);
                if (existing is null) {
                    existing = ILProcessor.Create(OpCodes.Nop);
                    originalSongs.Add(name, existing);
                }
                ILProcessor.Append(existing);
            }

            public override bool SupportsLineType(LineType type)
                => type == LineType.Branch || type == LineType.OriginalSong || type == LineType.UnconditionalBranch;
        }
    }
}