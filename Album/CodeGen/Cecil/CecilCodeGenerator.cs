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
        private AssemblyDefinition? generatedAssembly;
        
            
        [DisallowNull]
        public AssemblyDefinition? GeneratedAssembly { 
            get => generatedAssembly;
            [MemberNotNull(nameof(GeneratedModule))]
            private set {
                generatedAssembly = value;
                GeneratedModule = value.MainModule;
            }
        }

        public ModuleDefinition? GeneratedModule { get; private set; }

        private ILProcessor? il;

        private Dictionary<LineType, CecilCodeGenerationStrategy>? strategies;
        private MethodReferenceProvider? methodReferences;

        protected override ICodeGenerationStrategy? GetCodeGenerationStrategyForSong(LineType type)
            => strategies?.GetValueOrDefault(type);

        protected override void WillGenerateLines() {
            GeneratedAssembly = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition("AlbumPlaylist", new Version(1, 0, 0, 0)), "AlbumPlaylist", ModuleKind.Console);

            var programType = new TypeDefinition("AlbumPlaylist", "Program",
                Mono.Cecil.TypeAttributes.Class | Mono.Cecil.TypeAttributes.Public, GeneratedModule.TypeSystem.Object);

            GeneratedModule.Types.Add(programType);

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

            InitialiseMethodReferences(GeneratedModule);
            InitialiseCodeGenStrategies(methodReferences, il);
        }

        [MemberNotNull(nameof(methodReferences))]
        private void InitialiseMethodReferences(ModuleDefinition module) {
            methodReferences = new() {
                ConsoleWriteChar = module.ImportReference(typeof(Console).GetMethod("Write", new[] { typeof(char) })),
                ConsoleWriteInt = module.ImportReference(typeof(Console).GetMethod("Write", new[] { typeof(int) })),
                LinkedListAddLast = module.ImportReference(typeof(LinkedList<int>).GetMethod("AddLast", new[] { typeof(int) })),
                ConsoleRead = module.ImportReference(typeof(Console).GetMethod("Read", new Type[] { })),
                LinkedListNodeValue = module.ImportReference(typeof(LinkedListNode<int>).GetProperty("Value")?.GetGetMethod()),
                LinkedListLast = module.ImportReference(typeof(LinkedList<int>).GetProperty("Last")?.GetGetMethod()),
                LinkedListRemoveLast = module.ImportReference(typeof(LinkedList<int>).GetMethod("RemoveLast", new Type[] { })),
            };
        }

        private void InitialiseCodeGenStrategies(MethodReferenceProvider methodReferences, ILProcessor il) {
            strategies = new() {
                { LineType.Input, new Input(methodReferences, il) },
                { LineType.Branch, new BranchOriginalSong(methodReferences, il) },
                { LineType.OriginalSong, new BranchOriginalSong(methodReferences, il) },
                { LineType.TopPositive, new Conditionals(methodReferences, il) },
                { LineType.TopNegative, new Conditionals(methodReferences, il) },
                { LineType.TopZero, new Conditionals(methodReferences, il) },
                { LineType.Dup, new Dup(methodReferences, il) },
                { LineType.OutputChar, new Output(methodReferences, il) },
                { LineType.OutputInt, new Output(methodReferences, il) },
                { LineType.Pop, new Pop(methodReferences, il) },
                { LineType.Push, new Push(methodReferences, il) },
                { LineType.Sub, new Sub(methodReferences, il) },
            };
        }
        protected override void DidGenerateLines()
        {
            il?.Emit(OpCodes.Pop);
            il?.Emit(OpCodes.Ret);
        }
    }
}