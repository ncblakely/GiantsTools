namespace Giants.BinTools.Macro
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Giants.BinTools.Symbol;
    using NLog;

    public class MacroDefinition
    {
        private static readonly Logger logger = LogManager.GetLogger(nameof(MacroDefinition));

        private static readonly string[] SplitCharacters = new string[] { " ", "\t" };

        public IDictionary<string, IList<MacroLine>> Groups { get; }

        public string Name { get; set; }

        public MacroDefinition(string name, IDictionary<string, IList<MacroLine>> groups = null)
        {
            this.Name = name;
            this.Groups = groups ?? new Dictionary<string, IList<MacroLine>>();
        }

        public void Read(StreamReader reader, SymbolTable symbolTable)
        {
            string activeGroup = string.Empty;

            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    throw new InvalidOperationException($"Unexpected end of macro definition in '{this.Name}'");
                }

                line = line.Trim();
                if (line.StartsWith(";") || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith(MacroInstruction.MacroDefinitionEnd))
                {
                    break;
                }

                string[] opcodeData = line.Split(SplitCharacters, StringSplitOptions.RemoveEmptyEntries);

                var macroLine = MacroLineFactory.Read(opcodeData, symbolTable);
                if (macroLine == null)
                {
                    continue;
                }

                if (!this.Groups.Any() && !(macroLine is GroupUseMacroLine))
                {
                    logger.Warn($"Warning: expected 'groupuse' for macro {this.Name}; this may be a bug");

                    // Try to recover
                    this.Groups.TryAdd("-MISSING-", new List<MacroLine>());
                    activeGroup = "-MISSING-";
                }

                if (macroLine is GroupUseMacroLine groupUseMacroLine)
                {
                    this.Groups[groupUseMacroLine.GroupName] = new List<MacroLine>();
                    activeGroup = groupUseMacroLine.GroupName;
                }
                else
                {
                    this.Groups[activeGroup].Add(macroLine);
                }
            }
        }
    }
}
