namespace Giants.EffectCompiler
{
    using System;
    using System.IO;
    using System.Reflection;
    using Giants.BinTools.Macro;
    using Giants.BinTools.Symbol;
    using Newtonsoft.Json;

    public static class Utilities
    {
        public static readonly string[] SplitCharacters = new string[] { " ", "\t" };

        /// <summary>
        /// Gets the symbolic value of an FX name.
        /// </summary>
        public static int GetFxSymbolValue(SymbolTable symbolTable, string symbolName)
        {
            if (!symbolTable.TryGetSymbolFromName(
                KnownSymbolGroupNames.Fx,
                symbolName,
                out int symbolValue))
            {
                throw new InvalidOperationException($"No symbol definition for '{symbolName}' found");
            }

            return symbolValue;
        }

        public static FxMacroDefinitionTable LoadDefinitions(string definitionPath = null)
        {
            if (string.IsNullOrEmpty(definitionPath))
            {
                using var definitionStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(Program).Namespace}.Definitions.json");
                if (definitionStream == null)
                {
                    throw new InvalidOperationException("Could not load the definition resource.");
                }

                using var streamReader = new StreamReader(definitionStream);
                string serializedTable = streamReader.ReadToEnd();
                return JsonConvert.DeserializeObject<FxMacroDefinitionTable>(serializedTable);
            }

            MacroDefinitionTable macroDefinitionTable = MacroDefinitionTable.GenerateFromLegacyBuildSystem(bldFilePath: definitionPath);
            return new FxMacroDefinitionTable(macroDefinitionTable);
        }
    }
}
