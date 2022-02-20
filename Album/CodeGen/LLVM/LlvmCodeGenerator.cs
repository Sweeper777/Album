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
        private LLVMValueRef mallocFunction;
        private LLVMValueRef freeFunction;
        private LLVMValueRef printfFunction;
        private LLVMValueRef memmoveFunction;
        private LLVMValueRef getcharFunction;
        private LLVMValueRef pushFunction;
        private LLVMValueRef popFunction;
        private LLVMValueRef setupFunction;
        private LLVMValueRef outputIntFunction;
        private LLVMValueRef cycleFunction;
        private LLVMValueRef rcycleFunction;
        private LLVMValueRef inputFunction;
        private LLVMValueRef fpValue;
        private LLVMValueRef spValue;

        LLVMBasicBlockRef firstBlock;
        LLVMBasicBlockRef lastBlock;

        private ControlFlowGraph? cfg;

        private Dictionary<BasicBlock, LLVMBasicBlockRef> bbMap = new();

        private const long StackSize = 2000;
        public bool HasGenerated => generatedModule.Pointer != default;

        private void LLVMSetup() {
            var module = ModuleCreateWithName("AlbumPlaylist");

            mallocFunction = AddFunction(module, "malloc", FunctionType(
                PointerType(Int8Type(), 0), new[] { Int64Type() }, false
            ));
            SetLinkage(mallocFunction, LLVMLinkage.LLVMExternalLinkage);

            freeFunction = AddFunction(module, "free", FunctionType(
                VoidType(), new[] { PointerType(Int8Type(), 0) }, false
            ));
            SetLinkage(freeFunction, LLVMLinkage.LLVMExternalLinkage);

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

        private void GenerateCodeForLine(LineInfo line) {
            
        }

        public override void GenerateCode(IEnumerable<LineInfo> lines)
        {
            LLVMSetup();
            if (!lines.Any()) {
                return;
            }
            var semanticAnalyser = new SemanticAnalyser();
            semanticAnalyser.Analyse(lines);
            if (semanticAnalyser.CFG is null) {
                throw new Exception("This should not happen");
            }
            cfg = semanticAnalyser.CFG;
            AddBasicBlocksFromCFG();

            GetBasicBlocks(mainFunction).Zip(cfg.BasicBlocks).ToList()
                .ForEach(x => GenerateCodeForBasicBlock(x.First, x.Second));

             
            LLVMFinisher();
        }

        private void LLVMFinisher() {

        }
    }
}