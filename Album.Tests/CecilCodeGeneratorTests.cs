using NUnit.Framework;
using Album.Syntax;
using Album.CodeGen.Cecil;
using Mono.Cecil;
using System.Diagnostics;

namespace Album.Tests {
    [Timeout(1000)]
    public class CecilCodeGeneratorTests {
        

        [TestCase("Album.Tests.50plus1000minus7.album", "1043")]
        public void ProgramProducesCorrectOutput(string programResourceName, string expectedOutput) {
            using var stream = typeof(CecilCodeGeneratorTests).Assembly.GetManifestResourceStream(programResourceName);
            Assert.IsNotNull(stream);
            CecilCodeGenerator codegen = new();
            AlbumCompiler compiler = new(codegen);
            compiler.Compile(stream!);
            Assert.NotNull(codegen.GeneratedAssembly);
            Assert.AreEqual(expectedOutput, RunAssembly(codegen.GeneratedAssembly!, 0).Trim());
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