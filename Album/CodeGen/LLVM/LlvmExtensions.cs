using LLVMSharp;
using static LLVMSharp.LLVM;
using System;

namespace Album.CodeGen.LLVM {
    static class LlvmExtensions {
        public static LLVMValueRef ToLlvmValue(this int x)
            => ConstInt(Int32Type(), (ulong)x, true);
        public static LLVMValueRef ToLlvmValue(this long x)
            => ConstInt(Int64Type(), (ulong)x, true);
        
        public static LLVMTypeRef Pointer(this LLVMTypeRef type)
            => PointerType(type, 0);

    }
}