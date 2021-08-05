using Mono.Cecil;
using System.Diagnostics.CodeAnalysis;

namespace Album.CodeGen.Cecil
{
    internal class MethodReferenceProvider
    {
        [DisallowNull]
        public MethodReference? LinkedListAddLast { get; init; }
        [DisallowNull]
        public MethodReference? LinkedListAddFirst { get; init; }
        [DisallowNull]
        public MethodReference? LinkedListAddBefore { get; init; }
        [DisallowNull]
        public MethodReference? LinkedListRemoveLast { get; init; }
        [DisallowNull]
        public MethodReference? LinkedListRemoveFirst { get; init; }
        [DisallowNull]
        public MethodReference? LinkedListClear { get; init; }
        [DisallowNull]
        public MethodReference? LinkedListLast { get; init; }
        [DisallowNull]
        public MethodReference? LinkedListFirst { get; init; }
        [DisallowNull]
        public MethodReference? LinkedListNodeValue { get; init; }
        [DisallowNull]
        public MethodReference? ConsoleRead { get; init; }
        [DisallowNull]
        public MethodReference? ConsoleWriteChar { get; init; }
        [DisallowNull]
        public MethodReference? ConsoleWriteInt { get; init; }
        [DisallowNull]
        public MethodReference? EnvironmentExit { get; init; }
    }
}