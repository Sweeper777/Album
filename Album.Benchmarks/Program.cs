using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Album.CodeGen.Cecil;
using System.IO;
using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Engines;
using System.Linq;

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

        public MethodInfo optimised, unoptimised, control;

        [GlobalSetup]
        public void GlobalSetup() {
            var codegen = new CecilCodeGenerator();
            var compiler = new AlbumCompiler(codegen) { EnableOptimisation = true };
            compiler.Compile(typeof(CompilationTime).Assembly.GetManifestResourceStream("Album.Benchmarks.99_bottles_of_beer.album"));
            codegen.GeneratedAssembly.EntryPoint.DeclaringType.Namespace = "Optimised";
            codegen.GeneratedAssembly.Name = new("Optimised", new(1, 0, 0, 0));
            Stream stream = new MemoryStream();
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

            stream = typeof(ExecutionTime).Assembly.GetManifestResourceStream("Album.Benchmarks.99BottlesControl.dll");
            Assembly controlAsm = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(stream);
            control = controlAsm.EntryPoint;
        }

        [Benchmark]
        public void RunWithOptimisation() {
            var stdout = Console.Out;
            Console.SetOut(new StringWriter());
            optimised.Invoke(null, new object[0]);
            Console.SetOut(stdout);
        }

        [Benchmark]
        public void RunWithoutOptimisation() {
            var stdout = Console.Out;
            Console.SetOut(new StringWriter());
            unoptimised.Invoke(null, new object[0]);
            Console.SetOut(stdout);
        }

        [Benchmark]
        public void RunControl() {
            var stdout = Console.Out;
            Console.SetOut(new StringWriter());
            control.Invoke(null, new object[] { null });
            Console.SetOut(stdout);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ExecutionTime>(new Config());
            summary = BenchmarkRunner.Run<CompilationTime>(new Config());
        }
    }

    public class Config : ManualConfig
    {
        public Config()
        {
            AddExporter(RPlotExporter.Default);
            AddExporter(HtmlExporter.Default);
            AddLogger(ConsoleLogger.Default);
            AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());
            AddJob(Job.Default, 
                    Job.Default.WithStrategy(RunStrategy.ColdStart).WithId("ColdStart"));
            
        }
    }
}
