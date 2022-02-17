using LLVMSharp;
using static LLVMSharp.LLVM;
using System;

namespace Album.CodeGen.LLVM {
    static class LlvmExtensions {
        public static LLVMValueRef ToLlvmValue(this int x)
            => ConstInt(Int32Type(), (ulong)x, true);
        
    }
}