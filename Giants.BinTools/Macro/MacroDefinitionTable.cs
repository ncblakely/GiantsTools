namespace Giants.BinTools.Macro
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Giants.BinTools.Symbol;
    using NLog;

    public class MacroDefinitionTable
    {
        private static readonly Logger logger = LogManager.GetLogger(nameof(MacroDefinition));

        private static readonly string[] SplitCharacters = new string[] { " ", "\t" };

        private readonly HashSet<string> includedFiles = new HashSet<string>();

        public SymbolTable SymbolTable { get; } = new SymbolTable();

        public IList<MacroDefinition> MacroDefinitions { get; } = new List<MacroDefinition>();

        public static MacroDefinitionTable GenerateFromLegacyBuildSystem(string bldFilePath)
        {
            var table = new MacroDefinitionTable();
            table.ProcessFile(bldFilePath);

            return table;
        }

        private void ProcessFile(string bldFilePath)
        {
            using FileStream stream = File.OpenRead(bldFilePath);
            using StreamReader streamReader = new StreamReader(stream);

            string currentLine;
            do
            {
                currentLine = streamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }

                currentLine = currentLine.Trim();
                string[] tokens = currentLine.Split(SplitCharacters, StringSplitOptions.RemoveEmptyEntries);

                if (currentLine.StartsWith(";") || !tokens.Any())
                {
                    continue;
                }
                else if (tokens[0].Equals(MacroInstruction.MacroDefinitionStart, StringComparison.OrdinalIgnoreCase))
                {
                    MacroDefinition macroDefinition = this.ReadDefinition(currentLine, streamReader);
                    this.MacroDefinitions.Add(macroDefinition);
                }
                else if (tokens[0].Equals(MacroInstruction.IncludeFile, StringComparison.OrdinalIgnoreCase))
                {
                    string includeFile = tokens[1];
                    if (this.includedFiles.Contains(includeFile))
                    {
                        continue;
                    }

                    string directory = Directory.GetParent(bldFilePath).FullName;
                    string includeFilePath = Path.Combine(directory, includeFile);
                    if (string.IsNullOrEmpty(Path.GetExtension(includeFilePath)))
                    {
                        includeFilePath = includeFilePath + ".bld";
                    }

                    this.ProcessFile(includeFilePath);
                }
                else if (tokens[0].Equals(MacroInstruction.Define, StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(tokens[2], NumberStyles.Number, CultureInfo.InvariantCulture, out int symbolValue))
                    {
                        string groupName = KnownSymbolGroupNames.GetGroupName(tokens[1]);
                        if (groupName != null)
                        {
                            this.SymbolTable.AddSymbol(groupName, tokens[1], symbolValue);
                        }
                    }
                    else
                    {
                        logger.Warn($"Failed to parse symbol '{tokens[2]}'; only integer constants are supported");
                    }
                }

            } while (currentLine != null);
        }

        MacroDefinition ReadDefinition(string line, StreamReader reader)
        {
            string[] opcodeData = line.Split(SplitCharacters, StringSplitOptions.RemoveEmptyEntries);

            // Save definition name
            string macroName = opcodeData[1];

            var macroDefinition = new MacroDefinition(macroName);
            macroDefinition.Read(reader, this.SymbolTable);

            return macroDefinition;
        }

        private MacroDefinitionTable() { }
    }
}
