using System.Collections.Generic;
using Album.Syntax;
using Mono.Cecil.Cil;
using Mono.Cecil;
using static Album.CodeGen.Cecil.CecilCodeGenerationStrategies;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Album.CodeGen.Cecil
{
    public class CecilCodeGenerator : CodeGenerator
    {
        public AssemblyDefinition? GeneratedAssembly { get; private set; }
        public ModuleDefinition? GeneratedModule => GeneratedAssembly?.MainModule;

        private ILProcessor? il;

        public CecilCodeGenerator(IEnumerable<LineInfo> lines) : base(lines) {
        }

        private Dictionary<LineType, CecilCodeGenerationStrategy>? strategies;
        private MethodReferenceProvider? methodReferences;

        protected override ICodeGenerationStrategy? GetCodeGenerationStrategyForSong(LineType type)
            => strategies?.GetValueOrDefault(type);

        protected override void WillGenerateLines() {
            GeneratedAssembly = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition("AlbumPlaylist", new Version(1, 0, 0, 0)), "AlbumPlaylist", ModuleKind.Console);

            var programType = new TypeDefinition("AlbumPlaylist", "Program",
                Mono.Cecil.TypeAttributes.Class | Mono.Cecil.TypeAttributes.Public, GeneratedModule?.TypeSystem.Object);

            GeneratedModule?.Types.Add(programType);

            var mainMethod = new MethodDefinition("Main",
                Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static, GeneratedModule?.TypeSystem.Void);

            var stackVar = new VariableDefinition(GeneratedModule?.TypeSystem.Int32);
            mainMethod.Body.Variables.Add(stackVar);
            programType.Methods.Add(mainMethod);

            var argsParameter = new ParameterDefinition("args",
                Mono.Cecil.ParameterAttributes.None, GeneratedModule?.ImportReference(typeof(string[])));

            mainMethod.Parameters.Add(argsParameter);

            il = mainMethod.Body.GetILProcessor();
            il.Emit(OpCodes.Newobj, GeneratedModule?.ImportReference(typeof(LinkedList<int>).GetConstructor(new Type[] {})));

            GeneratedAssembly.EntryPoint = mainMethod;

            InitialiseMethodReferences();
            InitialiseCodeGenStrategies(methodReferences, il);
        }

        [MemberNotNull(nameof(methodReferences))]
        private void InitialiseMethodReferences() {
            methodReferences = new() {
                ConsoleWrite = GeneratedModule?.ImportReference(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })),
                LinkedListAddLast = GeneratedModule?.ImportReference(typeof(LinkedList<int>).GetMethod("AddLast", new[] { typeof(int) })),
                ConsoleRead = GeneratedModule?.ImportReference(typeof(Console).GetMethod("Read", new Type[] { })),
                LinkedListNodeValue = GeneratedModule?.ImportReference(typeof(LinkedListNode<int>).GetProperty("Value")?.GetGetMethod()),
                LinkedListLast = GeneratedModule?.ImportReference(typeof(LinkedList<int>).GetProperty("Last")?.GetGetMethod()),
                LinkedListRemoveLast = GeneratedModule?.ImportReference(typeof(LinkedList<int>).GetMethod("RemoveLast", new Type[] { })),
            };
        }

        private void InitialiseCodeGenStrategies(MethodReferenceProvider methodReferences, ILProcessor il) {
            strategies = new() {
                { LineType.Input, new Input(methodReferences, il) }
            };
        }
        protected override void DidGenerateLines()
        {
            il?.Emit(OpCodes.Pop);
            il?.Emit(OpCodes.Ret);
        }
    }
}