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

            printfFunction = AddFunction(module, "printf", FunctionType(
                Int32Type(), new[] { Int8Type().Pointer() }, true
            ));
            SetLinkage(printfFunction, LLVMLinkage.LLVMExternalLinkage);

            memmoveFunction = AddFunction(module, "memmove", FunctionType(
                Int8Type().Pointer(), new[] { Int8Type().Pointer(), Int8Type().Pointer(), Int64Type() }, false
            ));
            SetLinkage(memmoveFunction, LLVMLinkage.LLVMExternalLinkage);

            getcharFunction = AddFunction(module, "getchar", FunctionType(
                Int32Type(), Array.Empty<LLVMTypeRef>(), false
            ));
            SetLinkage(getcharFunction, LLVMLinkage.LLVMExternalLinkage);

            builder = CreateBuilder();
            spValue = AddGlobal(module, PointerType(Int32Type(), 0), "sp");
            SetInitializer(spValue, ConstNull(PointerType(Int32Type(), 0)));
            SetLinkage(spValue, LLVMLinkage.LLVMCommonLinkage);

            fpValue = AddGlobal(module, PointerType(Int32Type(), 0), "fp");
            SetInitializer(fpValue, ConstNull(PointerType(Int32Type(), 0)));
            SetLinkage(fpValue, LLVMLinkage.LLVMCommonLinkage);

            var intFormat = AddGlobal(module, ArrayType(Int8Type(), 4), "intFormat");
            SetInitializer(intFormat, ConstString("%d \0", 4, true));
            SetLinkage(intFormat, LLVMLinkage.LLVMLinkerPrivateLinkage);

            void GeneratePushFunction() {
                pushFunction = AddFunction(module, "push", FunctionType(
                    VoidType(), new[] { Int32Type() }, false
                ));
                SetLinkage(pushFunction, LLVMLinkage.LLVMExternalLinkage);
                PositionBuilderAtEnd(builder, AppendBasicBlock(pushFunction, ""));
                var load = BuildLoad(builder, spValue, "");
                var stackTop = BuildInBoundsGEP(builder, load, new[] { (-1).ToLlvmValue() }, "");
                BuildStore(builder, stackTop, spValue);
                BuildStore(builder, GetParam(pushFunction, 0), stackTop);
                BuildRetVoid(builder);
            }

            void GeneratePopFunction() {
                popFunction = AddFunction(module, "pop", FunctionType(
                    Int32Type(), Array.Empty<LLVMTypeRef>(), false
                ));
                SetLinkage(popFunction, LLVMLinkage.LLVMExternalLinkage);
                PositionBuilderAtEnd(builder, AppendBasicBlock(popFunction, ""));
                var load = BuildLoad(builder, spValue, "");
                var stackValue = BuildLoad(builder, load, "");
                var stackTop = BuildInBoundsGEP(builder, load, new[] { 1L.ToLlvmValue() }, "");
                BuildStore(builder, stackTop, spValue);
                BuildRet(builder, stackValue);
            }

            void GenerateSetupFunction() {
                setupFunction = AddFunction(module, "setup", FunctionType(
                    VoidType(), Array.Empty<LLVMTypeRef>(), false
                ));
                SetLinkage(setupFunction, LLVMLinkage.LLVMExternalLinkage);
                PositionBuilderAtEnd(builder, AppendBasicBlock(setupFunction, ""));
                var bytePointer = Int8Type().Pointer().Pointer();
                var malloc = BuildCall(builder, mallocFunction, new[] { (StackSize * 4).ToLlvmValue() }, "");
                BuildStore(builder, malloc, BuildBitCast(builder, fpValue, bytePointer, ""));
                var stackBottom = BuildInBoundsGEP(builder, malloc, new[] { (StackSize * 4).ToLlvmValue() }, "");
                BuildStore(builder, stackBottom, BuildBitCast(builder, spValue, bytePointer, ""));
                BuildRetVoid(builder);
            }

            void GenerateOutputIntFunction() {
                outputIntFunction = AddFunction(module, "outputInt", FunctionType(
                    VoidType(), new[] { Int32Type() }, false
                ));
                SetLinkage(outputIntFunction, LLVMLinkage.LLVMExternalLinkage);
                PositionBuilderAtEnd(builder, AppendBasicBlock(outputIntFunction, ""));
                BuildCall(builder, printfFunction, 
                    new[] { 
                        BuildInBoundsGEP(builder, intFormat, new[] { 0L.ToLlvmValue(), 0L.ToLlvmValue() }, ""), 
                        GetParam(outputIntFunction, 0) 
                    }, 
                    "");
                BuildRetVoid(builder);
            }

            void GenerateCycleFunction() {
                cycleFunction = AddFunction(module, "cycle", FunctionType(
                    VoidType(), Array.Empty<LLVMTypeRef>(), false
                ));
                SetLinkage(cycleFunction, LLVMLinkage.LLVMExternalLinkage);
                PositionBuilderAtEnd(builder, AppendBasicBlock(cycleFunction, ""));
                var sp = BuildLoad(builder, spValue, "");
                var fp = BuildLoad(builder, fpValue, "");
                var bottom = BuildInBoundsGEP(builder, fp, new[] { (StackSize - 1).ToLlvmValue() }, "");
                var comparison = BuildICmp(builder, LLVMIntPredicate.LLVMIntULT, sp, bottom, "");
                
                var cycleBlock = AppendBasicBlock(cycleFunction, "");
                var retBlock = AppendBasicBlock(cycleFunction, "");

                BuildCondBr(builder, comparison, retBlock, cycleBlock);
                
                PositionBuilderAtEnd(builder, retBlock);
                BuildRetVoid(builder);

                PositionBuilderAtEnd(builder, cycleBlock);
                var bottomValue = BuildLoad(builder, bottom, "");
                var bottomBytePointer = BuildBitCast(builder, bottom, Int8Type().Pointer(), "");
                var nextAfterTop = BuildInBoundsGEP(builder, sp, new[] { 1L.ToLlvmValue() }, "");
                var nextAfterTopBytePointer = BuildBitCast(builder, nextAfterTop, Int8Type().Pointer(), "");
                var bottomPointerToInt = BuildPtrToInt(builder, bottom, Int64Type(), "");
                var spPointerToInt = BuildPtrToInt(builder, sp, Int64Type(), "");
                var stackSize = BuildSub(builder, bottomPointerToInt, spPointerToInt, "");
                BuildCall(builder, memmoveFunction, new[] { 
                    bottomBytePointer, nextAfterTopBytePointer, stackSize
                 }, "");
                 sp = BuildLoad(builder, spValue, "");
                 BuildStore(builder, bottomValue, sp);
                BuildRetVoid(builder);
            }

            void GenerateRCycleFunction() {
                rcycleFunction = AddFunction(module, "rcycle", FunctionType(
                    VoidType(), Array.Empty<LLVMTypeRef>(), false
                ));
                SetLinkage(rcycleFunction, LLVMLinkage.LLVMExternalLinkage);
                PositionBuilderAtEnd(builder, AppendBasicBlock(rcycleFunction, ""));
                var sp = BuildLoad(builder, spValue, "");
                var fp = BuildLoad(builder, fpValue, "");
                var bottom = BuildInBoundsGEP(builder, fp, new[] { (StackSize - 1).ToLlvmValue() }, "");
                var comparison = BuildICmp(builder, LLVMIntPredicate.LLVMIntULT, sp, bottom, "");
                
                var cycleBlock = AppendBasicBlock(rcycleFunction, "");
                var retBlock = AppendBasicBlock(rcycleFunction, "");

                BuildCondBr(builder, comparison, retBlock, cycleBlock);
                
                PositionBuilderAtEnd(builder, retBlock);
                BuildRetVoid(builder);

                PositionBuilderAtEnd(builder, cycleBlock);
                var topValue = BuildLoad(builder, sp, "");
                var nextAfterTop = BuildInBoundsGEP(builder, sp, new[] { 1L.ToLlvmValue() }, "");
                var nextAfterTopBytePointer = BuildBitCast(builder, nextAfterTop, Int8Type().Pointer(), "");
                var spBytePointer = BuildBitCast(builder, sp, Int8Type().Pointer(), "");
                var bottomPointerToInt = BuildPtrToInt(builder, bottom, Int64Type(), "");
                var spPointerToInt = BuildPtrToInt(builder, sp, Int64Type(), "");
                var stackSize = BuildSub(builder, bottomPointerToInt, spPointerToInt, "");
                BuildCall(builder, memmoveFunction, new[] { 
                    nextAfterTopBytePointer, spBytePointer, stackSize
                 }, "");
                 sp = BuildLoad(builder, spValue, "");
                 BuildStore(builder, topValue, bottom);
                BuildRetVoid(builder);
            }

            mainFunction = AddFunction(module, "main", FunctionType(
                Int32Type(), Array.Empty<LLVMTypeRef>(), false
            ));
            SetLinkage(mainFunction, LLVMLinkage.LLVMExternalLinkage);
            
            generatedModule = module;
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