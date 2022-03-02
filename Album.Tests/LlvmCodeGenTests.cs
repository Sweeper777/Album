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
        }
    }
}