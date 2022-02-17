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
    }
}