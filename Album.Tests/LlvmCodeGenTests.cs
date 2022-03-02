using NUnit.Framework;
using Album.CodeGen.LLVM;
using System.IO;
using System.Diagnostics;

namespace Album.Tests {
    [Timeout(10000)]
    public class LlvmCodeGenTests {
        

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