namespace Giants.EffectCompiler.Tests.Integration
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DecompileCompileTests
    {
        private const string ProjectDirectoryPath = @"..\..\..\";

        [TestMethod]
        public void TestDecompileCompile()
        {
            // Verify round-trip of compiled file

            Guid testIdentifier = Guid.NewGuid();
            string textOutputPath = @$"Temp\fx_{testIdentifier}.txt";
            string binaryOutputPath = @$"Temp\fx_{testIdentifier}.bin";

            try
            {
                var definitionTable = Utilities.LoadDefinitions();
                var fxDecompiler = new FxDecompiler(definitionTable);
                fxDecompiler.Decompile(
                    Path.Combine(ProjectDirectoryPath, @"TestResources\fx.bin"),
                    Path.Combine(ProjectDirectoryPath, textOutputPath));

                var fxCompiler = new FxCompiler(definitionTable);
                fxCompiler.Compile(
                    Path.Combine(ProjectDirectoryPath, textOutputPath),
                    Path.Combine(ProjectDirectoryPath, binaryOutputPath));

                using var originalFile = new BinaryReader(File.Open(Path.Combine(ProjectDirectoryPath, @"TestResources\fx.bin"), FileMode.Open));
                using var recompiledFile = new BinaryReader(File.Open(Path.Combine(ProjectDirectoryPath, binaryOutputPath), FileMode.Open));

                if (originalFile.BaseStream.Length != recompiledFile.BaseStream.Length)
                {
                    throw new InvalidOperationException("File sizes do not match.");
                }

                while (originalFile.BaseStream.Position != originalFile.BaseStream.Length)
                {
                    byte b1 = originalFile.ReadByte();
                    byte b2 = recompiledFile.ReadByte();

                    Assert.AreEqual(b1, b2);
                }
            }
            finally
            {
                File.Delete(Path.Combine(ProjectDirectoryPath, textOutputPath));
                File.Delete(Path.Combine(ProjectDirectoryPath, binaryOutputPath));
            }
        }
    }
}
