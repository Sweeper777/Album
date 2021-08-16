using NUnit.Framework;
using Album.Syntax;
using Album.CodeGen.Cecil;
using Mono.Cecil;
using System.Diagnostics;

namespace Album.Tests {
    [Timeout(1000)]
    public class CecilCodeGeneratorTests {
        

        [TestCase("Album.Tests.50plus1000minus7.album", "1043", false)]
        [TestCase("Album.Tests.arithmetic.album", "23", false)]
        [TestCase("Album.Tests.branching.album", "10", false)]
        [TestCase("Album.Tests.stack.album", "1 3 2 4", false)]
        [TestCase("Album.Tests.50plus1000minus7.album", "1043", true)]
        [TestCase("Album.Tests.arithmetic.album", "23", true)]
        [TestCase("Album.Tests.branching.album", "10", true)]
        [TestCase("Album.Tests.stack.album", "1 3 2 4", true)]
        public void ProgramProducesCorrectOutput(string programResourceName, string expectedOutput, bool optimised) {
            using var stream = typeof(CecilCodeGeneratorTests).Assembly.GetManifestResourceStream(programResourceName);
            Assert.IsNotNull(stream);
            CecilCodeGenerator codegen = new();
            AlbumCompiler compiler = new(codegen);
            compiler.EnableOptimisation = optimised;
            compiler.Compile(stream!);
            Assert.NotNull(codegen.GeneratedAssembly);
            Assert.AreEqual(expectedOutput, RunAssembly(codegen.GeneratedAssembly!, 0).Trim());
        }

        [TestCase("Album.Tests.pop_empty.album", false)]
        [TestCase("Album.Tests.clear.album", false)]
        [TestCase("Album.Tests.pop_empty.album", true)]
        [TestCase("Album.Tests.clear.album", true)]
        public void ProgramTerminatesWhenPoppingEmptyStack(string programResourceName, bool optimised) {
            using var stream = typeof(CecilCodeGeneratorTests).Assembly.GetManifestResourceStream(programResourceName);
            Assert.IsNotNull(stream);
            CecilCodeGenerator codegen = new();
            AlbumCompiler compiler = new(codegen);
            compiler.EnableOptimisation = optimised;
            compiler.Compile(stream!);
            Assert.NotNull(codegen.GeneratedAssembly);
            RunAssembly(codegen.GeneratedAssembly!, 1);
        }

        private string RunAssembly(AssemblyDefinition asmDef, int expectedExitCode) {
            asmDef.Write("TestOutput.exe");
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"dotnet",
                    Arguments = "TestOutput.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = ""
                }
            };

            proc.Start();
            proc.WaitForExit();
            string output = proc.StandardOutput.ReadToEnd();
            Assert.AreEqual(expectedExitCode, proc.ExitCode);
            return output;
        }
    }
}