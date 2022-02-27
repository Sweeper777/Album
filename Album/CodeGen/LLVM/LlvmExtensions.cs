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

        public static LLVMValueRef AddBuiltinFunction(
            this LLVMModuleRef module,
            string name,
            LLVMTypeRef returnType,
            params LLVMTypeRef[] parameterTypes
        ) {
            var func = AddFunction(module, name, FunctionType(
                returnType, parameterTypes, false
            ));
            SetLinkage(func, LLVMLinkage.LLVMExternalLinkage);
            return func;
        }

        public static LLVMValueRef AddGlobalVariable(
            this LLVMModuleRef module,
            string name,
            LLVMTypeRef type,
            LLVMValueRef initialValue
        ) {
            var value = AddGlobal(module, type, name);
            SetInitializer(value, initialValue);
            SetLinkage(value, LLVMLinkage.LLVMCommonLinkage);
            return value;
        }

        public static LLVMValueRef AddGlobalString(
            this LLVMModuleRef module,
            string name,
            string initialValue
        ) {
            var value = AddGlobal(module, ArrayType(Int8Type(), 4), name);
            SetInitializer(value, ConstString(initialValue, (uint)initialValue.Length, true));
            SetLinkage(value, LLVMLinkage.LLVMLinkerPrivateLinkage);
            return value;
        }
    }
}