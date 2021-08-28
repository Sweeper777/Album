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

    public class ExecutionTime {
        private void RunAssembly(string executableName) {
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"dotnet",
                    Arguments = executableName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = ""
                }
            };

            proc.Start();
            proc.WaitForExit();
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var codegen = new CecilCodeGenerator();
            var compiler = new AlbumCompiler(codegen) { EnableOptimisation = true };
            compiler.Compile(typeof(CompilationTime).Assembly.GetManifestResourceStream("Album.Benchmarks.99_bottles_of_beer.album"));
            codegen.GeneratedAssembly.Write("optimised.dll");
            compiler.EnableOptimisation = false;
            compiler.Compile(typeof(CompilationTime).Assembly.GetManifestResourceStream("Album.Benchmarks.99_bottles_of_beer.album"));
            codegen.GeneratedAssembly.Write("unoptimised.dll");
            string runtimeConfigContents;
            using (var stream = new StreamReader(typeof(AlbumCompiler).Assembly.GetManifestResourceStream("Album.runtimeconfig_template.json")!)) {
                runtimeConfigContents = stream.ReadToEnd();
            }
            File.WriteAllText("optimised.runtimeconfig.json", runtimeConfigContents);
            File.WriteAllText("unoptimised.runtimeconfig.json", runtimeConfigContents);
        }

        [Benchmark]
        public void RunWithOptimisation() {
            RunAssembly("optimised.dll");
        }

        [Benchmark]
        public void RunWithoutOptimisation() {
            RunAssembly("unoptimised.dll");
        }

        [Benchmark]
        public void RunControl() {
            RunAssembly("Beer.dll");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            // var summary = BenchmarkRunner.Run<CompilationTime>();

            var summary = BenchmarkRunner.Run<ExecutionTime>();
        }
    }
}
