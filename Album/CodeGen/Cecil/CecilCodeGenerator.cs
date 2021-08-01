using System.Collections.Generic;
using Album.Syntax;
using Mono.Cecil.Cil;
using Mono.Cecil;
using static Album.CodeGen.Cecil.CecilCodeGenerationStrategies;
using System;

namespace Album.CodeGen.Cecil
{
    public class CecilCodeGenerator : CodeGenerator, IMethodReferenceProvider
    {
        public AssemblyDefinition GeneratedAssembly { get; }
        public ModuleDefinition GeneratedModule => GeneratedAssembly.MainModule;

        private ILProcessor il;

        public CecilCodeGenerator(IEnumerable<LineInfo> lines) : base(lines)
        {
        }

        private readonly Dictionary<LineType, CecilCodeGenerationStrategy> strategies;
        private readonly MethodReference linkedListAddLast;
        private readonly MethodReference linkedListRemoveLast;
        private readonly MethodReference linkedListLast;
        private readonly MethodReference linkedListNodeValue;
        private readonly MethodReference consoleRead;
        private readonly MethodReference consoleWrite;

        MethodReference IMethodReferenceProvider.LinkedListAddLast => linkedListAddLast;

        MethodReference IMethodReferenceProvider.LinkedListRemoveLast => linkedListRemoveLast;
        MethodReference IMethodReferenceProvider.LinkedListLast => linkedListLast;
        MethodReference IMethodReferenceProvider.LinkedListNodeValue => linkedListNodeValue;
        MethodReference IMethodReferenceProvider.ConsoleRead => consoleRead;
        MethodReference IMethodReferenceProvider.ConsoleWrite => consoleWrite;

        protected override ICodeGenerationStrategy? GetCodeGenerationStrategyForSong(LineType type)
            => strategies.GetValueOrDefault(type);
    }
}