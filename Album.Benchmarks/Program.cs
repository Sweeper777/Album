using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Album.CodeGen.Cecil;
using System.IO;
using System.Diagnostics;

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

    class Program
    {
        static void Main(string[] args)
        {
            // var summary = BenchmarkRunner.Run<CompilationTime>();

        }
    }
}
