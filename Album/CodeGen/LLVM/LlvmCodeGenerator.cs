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

    }
}