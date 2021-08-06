using NUnit.Framework;
using Album.Syntax;
using Album.CodeGen.Cecil;
using Mono.Cecil;
using System.Diagnostics;

namespace Album.Tests {
    [Timeout(1000)]
    public class CecilCodeGeneratorTests {
        
        private string RunAssembly(AssemblyDefinition asmDef) {
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
            Assert.AreEqual(0, proc.ExitCode);
            return output;
        }
    }
}