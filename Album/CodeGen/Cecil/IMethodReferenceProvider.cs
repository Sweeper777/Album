using Mono.Cecil;

namespace Album.CodeGen.Cecil
{
    internal interface IMethodReferenceProvider
    {
        MethodReference LinkedListAddLast { get; }
        MethodReference LinkedListRemoveLast { get; }
        MethodReference LinkedListLast { get; }
        MethodReference LinkedListNodeValue { get; }
        MethodReference ConsoleRead { get; }
        MethodReference ConsoleWrite { get; }
    }
}