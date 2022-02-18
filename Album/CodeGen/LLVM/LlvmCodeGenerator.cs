using Album.Syntax;
using LLVMSharp;
using System;
using System.Linq;
using Album.Semantics;
using static LLVMSharp.LLVM;
using System.Collections.Generic;

namespace Album.CodeGen.LLVM {
    public class LlvmCodeGenerator : CodeGenerator
    {

        private LLVMModuleRef generatedModule;
        private LLVMBuilderRef builder;
        private LLVMValueRef mainFunction;
        private LLVMValueRef putcharFunction;
        private ControlFlowGraph? cfg;
        private void LLVMSetup() {
            var module = ModuleCreateWithName("AlbumPlaylist");
            putcharFunction = AddFunction(module, "putchar", FunctionType(
                Int32Type(), new[] { Int32Type() }, false
            ));
            SetLinkage(putcharFunction, LLVMLinkage.LLVMExternalLinkage);
            mainFunction = AddFunction(module, "main", FunctionType(
                Int32Type(), Array.Empty<LLVMTypeRef>(), false
            ));
            SetLinkage(mainFunction, LLVMLinkage.LLVMExternalLinkage);
            
            generatedModule = module;
            builder = CreateBuilder();
        }

        private void AddBasicBlocksFromCFG() {
            if (cfg is null) {
                throw new InvalidOperationException("CFG has not been created when AddBasicBlocksFromCFG is called!");
            }
            foreach (var basicBlock in cfg.BasicBlocks) {
                if (basicBlock.FirstLine is LineInfo label && label.IsOriginalSong(out var name)) {
                    AppendBasicBlock(mainFunction, name);
                } else {
                    AppendBasicBlock(mainFunction, "");
                }
            }
        }

        private void GenerateCodeForBasicBlock(LLVMBasicBlockRef llvmBasicBlock, BasicBlock basicBlock) {
            PositionBuilderAtEnd(builder, llvmBasicBlock);
            foreach (var line in basicBlock.Lines) {
                GenerateCodeForLine(line);
            }
            if (basicBlock.LastLine?.IsAnyBranch(out var _) != true || basicBlock.LastLine?.Type != LineType.Quit) {
                var successors = basicBlock.ControlFlowGraph.Successors[basicBlock];
                if (!successors.Any()) {
                    BuildRet(builder, 0.ToLlvmValue());
                } else if (successors.Count > 1) {
                    throw new Exception("This should not be reached!");
                } else {
                    BuildBr(builder, GetNextBasicBlock(llvmBasicBlock));
                }
            }
        }

    }
}