using System.Collections.Generic;
using Album.Syntax;
using Mono.Cecil.Cil;
using Mono.Cecil;
using static Album.CodeGen.Cecil.CecilCodeGenerationStrategies;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

        public override void GenerateCode(IEnumerable<LineInfo> lines)
        {
            this.lines = lines;
            WillGenerateLines();
            foreach (var line in lines) {
                var strategy = strategies?.GetValueOrDefault(line.Type);
                if (strategy is null) {
                    continue;
                }
                if (strategy.SupportsLineType(line.Type)) {
                    strategy.GenerateCodeForSong(line);
                } else {
                    throw new InvalidOperationException($"Code Generation Strategy {strategy} Does Not Support Line Type {line.Type}!");
                }
            }
            DidGenerateLines();
        }
        private void WillGenerateLines() {
            GeneratedAssembly = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition("AlbumPlaylist", new Version(1, 0, 0, 0)), "AlbumPlaylist", ModuleKind.Console);

            var programType = new TypeDefinition("AlbumPlaylist", "Program",
                Mono.Cecil.TypeAttributes.Class | Mono.Cecil.TypeAttributes.Public, GeneratedModule.TypeSystem.Object);

            GeneratedModule.Types.Add(programType);

            var mainMethod = new MethodDefinition("Main",
                Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static, GeneratedModule.TypeSystem.Int32);

            var stackVar = new VariableDefinition(GeneratedModule.TypeSystem.Int32);
            var exitCodeVar = new VariableDefinition(GeneratedModule.TypeSystem.Int32);
            mainMethod.Body.Variables.Add(stackVar);
            mainMethod.Body.Variables.Add(exitCodeVar);
            programType.Methods.Add(mainMethod);

            il = mainMethod.Body.GetILProcessor();
            il.Emit(OpCodes.Newobj, GeneratedModule.ImportReference(typeof(LinkedList<int>).GetConstructor(new Type[] {})));
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_1);

            GeneratedAssembly.EntryPoint = mainMethod;

            InitialiseMethodReferences(GeneratedModule, il);
            InitialiseCodeGenStrategies(methodReferences, il);
        }

        [MemberNotNull(nameof(methodReferences))]
        private void InitialiseMethodReferences(ModuleDefinition module, ILProcessor il) {
            methodReferences = new() {
                ConsoleWriteChar = module.ImportReference(typeof(Console).GetMethod("Write", new[] { typeof(char) })),
                ConsoleWriteInt = module.ImportReference(typeof(Console).GetMethod("Write", new[] { typeof(int) })),
                LinkedListAddLast = module.ImportReference(typeof(LinkedList<int>).GetMethod("AddLast", new[] { typeof(int) })),
                LinkedListAddFirst = module.ImportReference(typeof(LinkedList<int>).GetMethod("AddFirst", new[] { typeof(int) })),
                ConsoleRead = module.ImportReference(typeof(Console).GetMethod("Read", new Type[] { })),
                LinkedListNodeValue = module.ImportReference(typeof(LinkedListNode<int>).GetProperty("Value")?.GetGetMethod()),
                LinkedListLast = module.ImportReference(typeof(LinkedList<int>).GetProperty("Last")?.GetGetMethod()),
                LinkedListFirst = module.ImportReference(typeof(LinkedList<int>).GetProperty("First")?.GetGetMethod()),
                LinkedListRemoveLast = module.ImportReference(typeof(LinkedList<int>).GetMethod("RemoveLast", new Type[] { })),
                LinkedListRemoveFirst = module.ImportReference(typeof(LinkedList<int>).GetMethod("RemoveFirst", new Type[] { })),
                LinkedListClear = module.ImportReference(typeof(LinkedList<int>).GetMethod("Clear", new Type[] { })),
                LinkedListAddBefore = module.ImportReference(typeof(LinkedList<int>).GetMethod("AddBefore", new[] { typeof(LinkedListNode<int>), typeof(int) })),
                LastInstruction = il.Create(OpCodes.Ldloc_1),
            };
        }

        private void InitialiseCodeGenStrategies(MethodReferenceProvider methodReferences, ILProcessor il) {
            Input inputStrategy = new(methodReferences, il);
            BranchOriginalSong branchStrategy = new(methodReferences, il);
            UnaryOperation unaryOpStrategy = new(methodReferences, il);
            Dup dupStrategy = new(methodReferences, il);
            Output outputStrategy = new(methodReferences, il);
            Pop popStrategy = new(methodReferences, il);
            Push pushStrategy = new(methodReferences, il);
            BinaryOperation binOpStrategy = new(methodReferences, il);
            Clear clearStrategy = new(methodReferences, il);
            Cycle cycleStrategy = new(methodReferences, il);
            Swap swapStrategy = new(methodReferences, il);
            InfiniteLoop infiniteLoopStrategy = new(methodReferences, il);
            Quit quitStrategy = new(methodReferences, il);
            strategies = new() {
                { LineType.Input, inputStrategy},
                { LineType.Branch, branchStrategy },
                { LineType.UnconditionalBranch, branchStrategy },
                { LineType.OriginalSong, branchStrategy },
                { LineType.TopPositive, unaryOpStrategy },
                { LineType.TopNegative, unaryOpStrategy },
                { LineType.TopZero, unaryOpStrategy },
                { LineType.Halve, unaryOpStrategy },
                { LineType.Double, unaryOpStrategy },
                { LineType.Dup, dupStrategy },
                { LineType.OutputChar, outputStrategy },
                { LineType.OutputInt, outputStrategy },
                { LineType.Pop, popStrategy },
                { LineType.Push, pushStrategy },
                { LineType.Sub, binOpStrategy },
                { LineType.Add, binOpStrategy },
                { LineType.And, binOpStrategy },
                { LineType.Or, binOpStrategy },
                { LineType.Eor, binOpStrategy },
                { LineType.Clear, clearStrategy },
                { LineType.Cycle, cycleStrategy },
                { LineType.RCycle, cycleStrategy },
                { LineType.Swap, swapStrategy },
                { LineType.Quit, quitStrategy },
                { LineType.InfiniteLoop, infiniteLoopStrategy }
            };
        }
        private void DidGenerateLines()
        {
            var catchEnd = methodReferences?.LastInstruction;
            il?.Emit(OpCodes.Pop);
            il?.Emit(OpCodes.Leave, catchEnd);
            var tryEnd = il?.Create(OpCodes.Pop);
            il?.Append(tryEnd);
            il?.Emit(OpCodes.Ldc_I4_1);
            il?.Emit(OpCodes.Stloc_1);
            il?.Emit(OpCodes.Leave, catchEnd);
            il?.Append(catchEnd);
            il?.Emit(OpCodes.Ret);

            ExceptionHandler handler = new ExceptionHandler(ExceptionHandlerType.Catch) {
                TryStart = il?.Body.Instructions.First (),
                TryEnd = tryEnd,
                HandlerStart = tryEnd,
                HandlerEnd = catchEnd,
                CatchType = GeneratedModule?.ImportReference(typeof(Exception)),
            };

            il?.Body.ExceptionHandlers.Add(handler);
        }
    }
}