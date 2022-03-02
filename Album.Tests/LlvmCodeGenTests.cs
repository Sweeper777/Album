using NUnit.Framework;
using Album.CodeGen.LLVM;
using System.IO;
using System.Diagnostics;

namespace Album.Tests {
    [Timeout(10000)]
    public class LlvmCodeGenTests {
        

        [TestCase("Album.Tests.50plus1000minus7.album", "1043", false)]
        [TestCase("Album.Tests.arithmetic.album", "23", false)]
        [TestCase("Album.Tests.branching.album", "10", false)]
        [TestCase("Album.Tests.stack.album", "1 3 2 4", false)]
        [TestCase("Album.Tests.overflow.album", "-2147483648 2147483647", false)]
        [TestCase("Album.Tests.empty.album", "", false)]
        [TestCase("Album.Tests.50plus1000minus7.album", "1043", true)]
        [TestCase("Album.Tests.arithmetic.album", "23", true)]
        [TestCase("Album.Tests.branching.album", "10", true)]
        [TestCase("Album.Tests.stack.album", "1 3 2 4", true)]
        [TestCase("Album.Tests.overflow.album", "-2147483648 2147483647", true)]
        [TestCase("Album.Tests.empty.album", "", true)]
        public void ProgramProducesCorrectOutput(string programResourceName, string expectedOutput, bool optimised) {
            using var stream = typeof(CecilCodeGeneratorTests).Assembly.GetManifestResourceStream(programResourceName);
            Assert.IsNotNull(stream);
            LlvmCodeGenerator codegen = new();
            AlbumCompiler compiler = new(codegen);
            compiler.EnableOptimisation = optimised;
            compiler.Compile(stream!);
            Assert.AreEqual(expectedOutput, RunLlvmIr(codegen, 0).Trim());
        }

        [TestCase("Album.Tests.pop_empty.album", false)]
        [TestCase("Album.Tests.clear.album", false)]
        [TestCase("Album.Tests.pop_empty.album", true)]
        [TestCase("Album.Tests.clear.album", true)]
        public void ProgramTerminatesWhenPoppingEmptyStack(string programResourceName, bool optimised) {
            using var stream = typeof(CecilCodeGeneratorTests).Assembly.GetManifestResourceStream(programResourceName);
            Assert.IsNotNull(stream);
            LlvmCodeGenerator codegen = new();
            AlbumCompiler compiler = new(codegen);
            compiler.EnableOptimisation = optimised;
            compiler.Compile(stream!);
            RunLlvmIr(codegen, 1);
        }

        private string RunLlvmIr(LlvmCodeGenerator codeGenerator, int expectedExitCode) {
            codeGenerator.WriteGeneratedModuleTo("TestOutput.ll");
            using var compileProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"clang",
                    Arguments = "-o TestOutput TestOutput.ll",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = true,
                    WorkingDirectory = ""
                }
            };

            compileProcess.Start();
            compileProcess.WaitForExit();
            using var executeProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"./TestOutput",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = ""
                }
            };

            executeProcess.Start();
            executeProcess.WaitForExit();
            string output = executeProcess.StandardOutput.ReadToEnd();
            File.Delete("TestOutput");
            File.Delete("TestOutput.ll");
            Assert.AreEqual(expectedExitCode, executeProcess.ExitCode);
            return output;
        }
    }
}