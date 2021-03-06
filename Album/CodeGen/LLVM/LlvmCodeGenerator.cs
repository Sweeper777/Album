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
        private LLVMValueRef exitFunction;
        private LLVMValueRef fpValue;
        private LLVMValueRef spValue;

        LLVMBasicBlockRef firstBlock;
        LLVMBasicBlockRef lastBlock;

        private ControlFlowGraph? cfg;

        private Dictionary<BasicBlock, LLVMBasicBlockRef> bbMap = new();
        private Dictionary<string, LLVMBasicBlockRef> bbNameMap = new();

        private const long StackSize = 2000;
        public bool HasGenerated => generatedModule.Pointer != default;

        private void LLVMSetup() {
            var module = ModuleCreateWithName("AlbumPlaylist");

            mallocFunction = module.AddBuiltinFunction("malloc",
                Int8Type().Pointer(), Int64Type()
            );

            freeFunction = module.AddBuiltinFunction("free",
                VoidType(), PointerType(Int8Type(), 0)
            );

            putcharFunction = module.AddBuiltinFunction("putchar",
                Int32Type(), Int32Type()
            );

            printfFunction = AddFunction(module, "printf", FunctionType(
                Int32Type(), new[] { Int8Type().Pointer() }, true
            ));
            SetLinkage(printfFunction, LLVMLinkage.LLVMExternalLinkage);

            memmoveFunction = module.AddBuiltinFunction("memmove",
                Int8Type().Pointer(), Int8Type().Pointer(), Int8Type().Pointer(), Int64Type()
            );

            getcharFunction = module.AddBuiltinFunction("getchar", Int32Type());

            exitFunction = module.AddBuiltinFunction("exit", VoidType(), Int32Type());

            builder = CreateBuilder();
            spValue = module.AddGlobalVariable("sp", Int32Type().Pointer(), ConstNull(Int32Type().Pointer()));
            fpValue = module.AddGlobalVariable("fp", Int32Type().Pointer(), ConstNull(Int32Type().Pointer()));

            var intFormat = module.AddGlobalString("intFormat", "%d \0");

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
                var mainBlock = AppendBasicBlock(popFunction, "");
                var popBlock = AppendBasicBlock(popFunction, "");
                var exitBlock = AppendBasicBlock(popFunction, "");
                PositionBuilderAtEnd(builder, mainBlock);
                var sp = BuildLoad(builder, spValue, "");
                var fp = BuildLoad(builder, fpValue, "");
                var bottom = BuildInBoundsGEP(builder, fp, new[] { StackSize.ToLlvmValue() }, "");
                var comparison = BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, sp, bottom, "");
                BuildCondBr(builder, comparison, exitBlock, popBlock);

                PositionBuilderAtEnd(builder, popBlock);
                var load = BuildLoad(builder, spValue, "");
                var stackValue = BuildLoad(builder, load, "");
                var stackTop = BuildInBoundsGEP(builder, load, new[] { 1L.ToLlvmValue() }, "");
                BuildStore(builder, stackTop, spValue);
                BuildRet(builder, stackValue);
                PositionBuilderAtEnd(builder, exitBlock);
                BuildCall(builder, exitFunction, new[] { 1.ToLlvmValue() }, "");
                BuildRet(builder, 0.ToLlvmValue());
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

                BuildCondBr(builder, comparison, cycleBlock, retBlock);
                
                PositionBuilderAtEnd(builder, retBlock);
                BuildRetVoid(builder);

                PositionBuilderAtEnd(builder, cycleBlock);
                var bottomValue = BuildLoad(builder, bottom, "");
                var spBytePointer = BuildBitCast(builder, sp, Int8Type().Pointer(), "");
                var nextAfterTop = BuildInBoundsGEP(builder, sp, new[] { 1L.ToLlvmValue() }, "");
                var nextAfterTopBytePointer = BuildBitCast(builder, nextAfterTop, Int8Type().Pointer(), "");
                var bottomPointerToInt = BuildPtrToInt(builder, bottom, Int64Type(), "");
                var spPointerToInt = BuildPtrToInt(builder, sp, Int64Type(), "");
                var stackSize = BuildSub(builder, bottomPointerToInt, spPointerToInt, "");
                BuildCall(builder, memmoveFunction, new[] { 
                    nextAfterTopBytePointer, spBytePointer, stackSize
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

                BuildCondBr(builder, comparison, cycleBlock, retBlock);
                
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
                    spBytePointer, nextAfterTopBytePointer, stackSize
                 }, "");
                 sp = BuildLoad(builder, spValue, "");
                 BuildStore(builder, topValue, bottom);
                BuildRetVoid(builder);
            }

            void GenerateInputFunction() {
                inputFunction = AddFunction(module, "input", FunctionType(
                    VoidType(), Array.Empty<LLVMTypeRef>(), false
                ));
                SetLinkage(inputFunction, LLVMLinkage.LLVMExternalLinkage);
                PositionBuilderAtEnd(builder, AppendBasicBlock(inputFunction, ""));
                var input = BuildCall(builder, getcharFunction, Array.Empty<LLVMValueRef>(), "");
                BuildCall(builder, pushFunction, new[] { input }, "");
                BuildRetVoid(builder);
            }

            GeneratePushFunction();
            GeneratePopFunction();
            GenerateSetupFunction();
            GenerateOutputIntFunction();
            GenerateCycleFunction();
            GenerateRCycleFunction();
            GenerateInputFunction();

            mainFunction = AddFunction(module, "main", FunctionType(
                Int32Type(), Array.Empty<LLVMTypeRef>(), false
            ));
            SetLinkage(mainFunction, LLVMLinkage.LLVMExternalLinkage);
            
            generatedModule = module;
        }

        private void GenerateFirstBlock() {
            if (CountBasicBlocks(mainFunction) > 0) { 
                firstBlock = InsertBasicBlock(GetFirstBasicBlock(mainFunction), "");
            } else {
                firstBlock = AppendBasicBlock(mainFunction, "");
            }
            PositionBuilderAtEnd(builder, firstBlock);
            BuildCall(builder, setupFunction, Array.Empty<LLVMValueRef>(), "");
        }

        private void AddBasicBlocksFromCFG() {
            if (cfg is null) {
                throw new InvalidOperationException("CFG has not been created when AddBasicBlocksFromCFG is called!");
            }
            foreach (var basicBlock in cfg.BasicBlocks) {
                if (basicBlock.FirstLine is LineInfo label && label.IsOriginalSong(out var name)) {
                    var bb = AppendBasicBlock(mainFunction, name);
                    bbMap.Add(basicBlock, bb);
                    bbNameMap.Add(name, bb);
                } else {
                    var bb = AppendBasicBlock(mainFunction, "");
                    bbMap.Add(basicBlock, bb);
                }
            }
        }

        private void GenerateCodeForBasicBlock(LLVMBasicBlockRef llvmBasicBlock, BasicBlock basicBlock) {
            PositionBuilderAtEnd(builder, llvmBasicBlock);
            foreach (var line in basicBlock.Lines) {
                GenerateCodeForLine(line, llvmBasicBlock);
            }
            if (!basicBlock.Lines.Any (x => x.IsAnyBranch(out var _)) && 
                !basicBlock.Lines.Any (x => x.Type == LineType.Quit || x.Type == LineType.InfiniteLoop)) {
                var successors = basicBlock.ControlFlowGraph.Successors[basicBlock];
                if (!successors.Any()) {
                    BuildBr(builder, lastBlock);
                } else if (successors.Count > 1) {
                    throw new Exception("This should not be reached!");
                } else {
                    BuildBr(builder, bbMap[successors.Single()]);
                }
            }
        }

        private void GenerateCodeForLine(LineInfo line, LLVMBasicBlockRef basicBlock) {
            LLVMValueRef BuildPop() => BuildCall(builder, popFunction, Array.Empty<LLVMValueRef>(), "");

            void BuildPush(LLVMValueRef value) {
                BuildCall(builder, pushFunction, new[] { value }, "");
            }

            if (line.IsPush(out var x)) {
                BuildCall(builder, pushFunction, new[] { x.Value.ToLlvmValue() }, "");
            } else if (line.IsBranch(out var label)) {
                var operand = BuildCall(builder, popFunction, Array.Empty<LLVMValueRef>(), "");
                var comparison = BuildICmp(builder, LLVMIntPredicate.LLVMIntNE, operand, 0.ToLlvmValue(), "");
                BuildCondBr(builder, comparison, bbNameMap[label], GetNextBasicBlock(basicBlock));
            } else if (line.IsUnconditionalBranch(out label)) {
                BuildBr(builder, bbNameMap[label]);
            } else {
                LLVMValueRef operand1, operand2, result, operand;
                switch (line.Type) {
                    case LineType.Add:
                    operand1 = BuildPop();
                    operand2 = BuildPop();
                    result = BuildAdd(builder, operand1, operand2, "");
                    BuildPush(result);
                    break;
                    case LineType.Sub:
                    operand1 = BuildPop();
                    operand2 = BuildPop();
                    result = BuildSub(builder, operand2, operand1, "");
                    BuildPush(result);
                    break;
                    case LineType.OutputInt:
                    operand = BuildPop();
                    BuildPush(operand);
                    break;
                    case LineType.OutputChar:
                    operand = BuildPop();
                    BuildCall(builder, putcharFunction, new LLVMValueRef[] { operand }, "");
                    break;
                    case LineType.Input:
                    BuildCall(builder, inputFunction, Array.Empty<LLVMValueRef>(), "");
                    break;
                    case LineType.Double:
                    operand = BuildPop();
                    result = BuildMul(builder, operand, 2.ToLlvmValue(), "");
                    BuildPush(result);
                    break;
                    case LineType.Halve:
                    operand = BuildPop();
                    result = BuildAShr(builder, operand, 1.ToLlvmValue(), "");
                    BuildPush(result);
                    break;
                    case LineType.Clear:
                    var fp = BuildLoad(builder, fpValue, "");
                    var bottom = BuildInBoundsGEP(builder, fp, new[] { StackSize.ToLlvmValue() }, "");
                    BuildStore(builder, bottom, spValue);
                    break;
                    case LineType.Pop:
                    BuildPop();
                    break;
                    case LineType.Dup:
                    operand = BuildPop();
                    BuildPush(operand);
                    BuildPush(operand);
                    break;
                    case LineType.And:
                    operand1 = BuildPop();
                    operand2 = BuildPop();
                    result = BuildAnd(builder, operand1, operand2, "");
                    BuildPush(result);
                    break;
                    case LineType.Or:
                    operand1 = BuildPop();
                    operand2 = BuildPop();
                    result = BuildOr(builder, operand1, operand2, "");
                    BuildPush(result);
                    break;
                    case LineType.Eor:
                    operand1 = BuildPop();
                    operand2 = BuildPop();
                    result = BuildXor(builder, operand1, operand2, "");
                    BuildPush(result);
                    break;
                    case LineType.Swap:
                    operand1 = BuildPop();
                    operand2 = BuildPop();
                    BuildPush(operand1);
                    BuildPush(operand2);
                    break;
                    case LineType.Cycle:
                    BuildCall(builder, cycleFunction, Array.Empty<LLVMValueRef>(), "");
                    break;
                    case LineType.RCycle:
                    BuildCall(builder, rcycleFunction, Array.Empty<LLVMValueRef>(), "");
                    break;
                    case LineType.TopPositive:
                    operand = BuildPop();
                    result = BuildICmp(builder, LLVMIntPredicate.LLVMIntSGT, operand, 0.ToLlvmValue(), "");
                    BuildPush(BuildZExt(builder, result, Int32Type(), ""));
                    break;
                    case LineType.TopNegative:
                    operand = BuildPop();
                    result = BuildICmp(builder, LLVMIntPredicate.LLVMIntSLE, operand, 0.ToLlvmValue(), "");
                    BuildPush(BuildZExt(builder, result, Int32Type(), ""));
                    break;
                    case LineType.TopZero:
                    operand = BuildPop();
                    result = BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, operand, 0.ToLlvmValue(), "");
                    BuildPush(BuildZExt(builder, result, Int32Type(), ""));
                    break;
                    case LineType.InfiniteLoop:
                    var loopBlock1 = AppendBasicBlock(mainFunction, "");
                    var loopBlock2 = AppendBasicBlock(mainFunction, "");
                    BuildBr(builder, loopBlock1);
                    PositionBuilderAtEnd(builder, loopBlock1);
                    BuildBr(builder, loopBlock2);
                    PositionBuilderAtEnd(builder, loopBlock2);
                    BuildBr(builder, loopBlock1);
                    break;
                    case LineType.Quit:
                    BuildBr(builder, lastBlock);
                    break;
                }
            }
        }

        public override void GenerateCode(IEnumerable<LineInfo> lines)
        {
            LLVMSetup();
            if (!lines.Any()) {
                GenerateFirstBlock();
                GenerateLastBlock();
                PositionBuilderAtEnd(builder, firstBlock);
                BuildBr(builder, lastBlock);
                return;
            }
            var semanticAnalyser = new SemanticAnalyser();
            semanticAnalyser.Analyse(lines);
            if (semanticAnalyser.CFG is null) {
                throw new Exception("This should not happen");
            }
            cfg = semanticAnalyser.CFG;
            AddBasicBlocksFromCFG();
            GenerateFirstBlock();
            PositionBuilderAtEnd(builder, firstBlock);
            BuildBr(builder, bbMap[cfg.BasicBlocks.First()]);
            GenerateLastBlock();
            foreach (var bb in cfg.BasicBlocks) {
                GenerateCodeForBasicBlock(bbMap[bb], bb);
            }
        }

        private void GenerateLastBlock() {
            lastBlock = AppendBasicBlock(mainFunction, "");
            PositionBuilderAtEnd(builder, lastBlock);
            var load = BuildLoad(builder, BuildBitCast(builder, fpValue, Int8Type().Pointer().Pointer(), ""), "");
            BuildCall(builder, freeFunction, new[] { load }, "");
            BuildRet(builder, 0.ToLlvmValue());
        }

        public void WriteGeneratedModuleTo(string path) {
            VerifyModule(generatedModule, LLVMVerifierFailureAction.LLVMPrintMessageAction, out var message);
            Console.WriteLine(message);
            PrintModuleToFile(generatedModule, path, out message);
            Console.WriteLine(message);
        }
    }
}