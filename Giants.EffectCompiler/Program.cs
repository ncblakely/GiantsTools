namespace Giants.EffectCompiler
{
    using System;
    using System.IO;
    using System.Reflection;
    using Giants.BinTools.Macro;
    using Newtonsoft.Json;

    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode">The mode to operate in. Supported modes are: 'decompile', 'compile'.</param>
        /// <param name="input">The input path.</param>
        /// <param name="output">The output path.</param>
        /// <param name="definitionsPath">The path to the definitions file.</param>
        public static void Main(string mode, string input, string output, string definitionsPath)
        {
            if (string.IsNullOrEmpty(mode))
            {
                Console.WriteLine("--mode is required. Type --help for example usage.");
                return;
            }

            switch (mode.ToLowerInvariant())
            {
                case "generatedefinitions":
                    MacroDefinitionTable macroDefinitionTable = MacroDefinitionTable.GenerateFromLegacyBuildSystem(bldFilePath: input);
                    FxMacroDefinitionTable fxMacroDefinitionTable = new FxMacroDefinitionTable(macroDefinitionTable);
                    string serializedDefintions = JsonConvert.SerializeObject(fxMacroDefinitionTable, Formatting.Indented);
                    File.WriteAllText(output, serializedDefintions);
                    break;
                case "decompile":
                    var decompiler = new FxDecompiler(Utilities.LoadDefinitions(definitionsPath));
                    decompiler.Decompile(
                        inputPath: input,
                        outputPath: output);
                    break;
                case "compile":
                    var compiler = new FxCompiler(Utilities.LoadDefinitions(definitionsPath));
                    compiler.Compile(
                        inputPath: input,
                        outputPath: output);
                    break;
            }
        }
    }
}
