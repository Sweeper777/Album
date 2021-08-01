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
            GeneratedAssembly = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition("AlbumPlaylist", new Version(1, 0, 0, 0)), "AlbumPlaylist", ModuleKind.Console);

            var programType = new TypeDefinition("AlbumPlaylist", "Program",
                Mono.Cecil.TypeAttributes.Class | Mono.Cecil.TypeAttributes.Public, GeneratedModule.TypeSystem.Object);

            GeneratedModule.Types.Add(programType);

            consoleWrite = GeneratedModule.ImportReference(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));
            linkedListAddLast = GeneratedModule.ImportReference(typeof(LinkedList<int>).GetMethod("AddLast", new[] { typeof(int) }));
            consoleRead = GeneratedModule.ImportReference(typeof(Console).GetMethod("Read", new Type[] { }));
            linkedListNodeValue = GeneratedModule.ImportReference(typeof(LinkedListNode<int>).GetProperty("Value")?.GetGetMethod());
            linkedListLast = GeneratedModule.ImportReference(typeof(LinkedList<int>).GetProperty("Last")?.GetGetMethod());
            linkedListRemoveLast = GeneratedModule.ImportReference(typeof(LinkedList<int>).GetMethod("RemoveLast", new Type[] { }));

            var mainMethod = new MethodDefinition("Main",
                Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static, GeneratedModule.TypeSystem.Void);

            var stackVar = new VariableDefinition(GeneratedModule.TypeSystem.Int32);
            mainMethod.Body.Variables.Add(stackVar);
            programType.Methods.Add(mainMethod);

            var argsParameter = new ParameterDefinition("args",
                Mono.Cecil.ParameterAttributes.None, GeneratedModule.ImportReference(typeof(string[])));

            mainMethod.Parameters.Add(argsParameter);

            il = mainMethod.Body.GetILProcessor();
            il.Emit(OpCodes.Newobj, GeneratedModule.ImportReference(typeof(LinkedList<int>).GetConstructor(new Type[] {})));

            GeneratedAssembly.EntryPoint = mainMethod;

            strategies = new() {
                { LineType.Input, new Input(this, il) }
            };
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