using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Album.CodeGen.Cecil;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reflection;

namespace Album.Benchmarks
{

    public class CompilationTime
    {

        private readonly AlbumCompiler compiler;
        private readonly string sourceCode;
        public CompilationTime()
        {
            compiler = new AlbumCompiler(new CecilCodeGenerator());
            using var streamReader = new StreamReader(
                typeof(CompilationTime).Assembly.GetManifestResourceStream("Album.Benchmarks.99_bottles_of_beer.album")
            );
            sourceCode = streamReader.ReadToEnd();
        }

        [Benchmark]
        public void CompileWithOptimisation() {
            compiler.EnableOptimisation = true;
            compiler.Compile(sourceCode);
        }

        [Benchmark]
        public void CompileWithoutOptimisation() {
            compiler.EnableOptimisation = false;
            compiler.Compile(sourceCode);
        }
    }

    public class ExecutionTime {

        public MethodInfo optimised, unoptimised;

        [GlobalSetup]
        public void GlobalSetup() {
            var codegen = new CecilCodeGenerator();
            var compiler = new AlbumCompiler(codegen) { EnableOptimisation = true };
            compiler.Compile(typeof(CompilationTime).Assembly.GetManifestResourceStream("Album.Benchmarks.99_bottles_of_beer.album"));
            codegen.GeneratedAssembly.EntryPoint.DeclaringType.Namespace = "Optimised";
            codegen.GeneratedAssembly.Name = new("Optimised", new(1, 0, 0, 0));
            MemoryStream stream = new MemoryStream();
            codegen.GeneratedAssembly.Write(stream);
            stream.Position = 0;
            Assembly optimisedAsm = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(stream);
            optimised = optimisedAsm.EntryPoint;

            compiler.EnableOptimisation = false;
            compiler.Compile(typeof(CompilationTime).Assembly.GetManifestResourceStream("Album.Benchmarks.99_bottles_of_beer.album"));
            codegen.GeneratedAssembly.EntryPoint.DeclaringType.Namespace = "Unoptimised";
            codegen.GeneratedAssembly.Name = new("Unoptimised", new(1, 0, 0, 0));
            stream = new MemoryStream();
            codegen.GeneratedAssembly.Write(stream);
            stream.Position = 0;
            Assembly unoptimisedAsm = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(stream);
            unoptimised = unoptimisedAsm.EntryPoint;
        }



        // public static Task<long> RunControl() => RunAssembly("Beer.dll");
    }
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
