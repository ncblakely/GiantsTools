namespace Giants.EffectCompiler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Giants.BinTools.Macro;
    using Giants.BinTools.Symbol;
    using Newtonsoft.Json;
    using NLog;

    public class FxMacroDefinitionTable
    {
        private static readonly Logger logger = LogManager.GetLogger(nameof(FxMacroDefinitionTable));

        [JsonProperty]
        public SymbolTable SymbolTable { get; } = new SymbolTable();

        [JsonProperty]
        public IDictionary<int, FxMacroDefinition> MacroDefinitions { get; } = new Dictionary<int, FxMacroDefinition>();

        public FxMacroDefinitionTable(MacroDefinitionTable macroDefinitionTable)
        {
            this.SymbolTable = macroDefinitionTable.SymbolTable;

            this.ProcessMacroDefinitions(macroDefinitionTable.MacroDefinitions);
        }

        [JsonConstructor]
        internal FxMacroDefinitionTable() { }

        private void ProcessMacroDefinitions(IList<MacroDefinition> macroDefinitions)
        {
            // Try to map each opcode we know about to a pre-processed form of the macro definition with conditional branches eliminated
            foreach (int opcode in this.SymbolTable.GetSymbolGroup(KnownSymbolGroupNames.Fx).Values)
            {
                FxMacroDefinition fxMacroDefinition = this.GetFxMacroDefinition(macroDefinitions, opcode);
                if (fxMacroDefinition != null)
                {
                    this.MacroDefinitions[opcode] = fxMacroDefinition;
                }
                else
                {
                    logger.Warn($"Opcode {opcode} has no macro defined");
                }
            }
        }

        private FxMacroDefinition GetFxMacroDefinition(IList<MacroDefinition> macroDefinitions, int opcode)
        {
            foreach (var macroDefinition in macroDefinitions
                .Where(x => x.Groups.ContainsKey(KnownMacroGroupNames.FxDefGroup) && x.Groups[KnownMacroGroupNames.FxDefGroup].Any()))
            {
                var fxDefGroup = macroDefinition.Groups[KnownMacroGroupNames.FxDefGroup];
                for (int lineIndex = 0; lineIndex < fxDefGroup.Count; lineIndex++)
                {
                    if (!(fxDefGroup[lineIndex] is DataDefinitionMacroLine line))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(line.ConstantName)
                        && Convert.ToInt32(line.Constant) == opcode)
                    {
                        logger.Debug($"Matched opcode {opcode} to '{line.ConstantName}'");
                        return new FxMacroDefinition
                        {
                            Opcode = opcode,
                            Name = macroDefinition.Name,
                            FxDefGroup = SelectOptimalBranch(fxDefGroup.Skip(lineIndex + 1).ToList()).ToList()
                        };

                        // TODO: Handle macros with instructions after the conditional (if they exist)
                    }
                }
            }

            return null;
        }

        private static IEnumerable<MacroLine> SelectOptimalBranch(List<MacroLine> macroLines)
        {
            var outLines = new List<MacroLine>();

            int startIndex = 0;
            while (startIndex < macroLines.Count)
            {
                if (!IsConditional(macroLines[startIndex]))
                {
                    outLines.Add(macroLines[startIndex]);
                }
                else
                {
                    break;
                }

                startIndex++;
            }

            if (startIndex == macroLines.Count)
            {
                // No branches
                return outLines;
            }

            int longestBranchLength = 0;
            int branchIndex = startIndex;
            IEnumerable<MacroLine> branchLines = null;
            while (branchIndex >= 0)
            {
                var argSet = new HashSet<int>();

                int endIndex = 0;
                for (int i = branchIndex + 1; i < macroLines.Count; i++)
                {
                    if (IsOpcodeDefinition(macroLines[i]))
                    {
                        return outLines;
                    }

                    if (IsConditional(macroLines[i]))
                    {
                        endIndex = i;
                        break;
                    }

                    if (macroLines[i] is DataDefinitionMacroLine dataDefinitionLine)
                    {
                        argSet.Add(dataDefinitionLine.ArgumentIndex);
                    }
                }

                if (argSet.Count > longestBranchLength)
                {
                    longestBranchLength = branchIndex;
                    branchLines = macroLines.Skip(branchIndex + 1).Take(endIndex - branchIndex - 1);
                }

                branchIndex = macroLines.FindIndex(branchIndex + 1, l => l is IfMacroLine || l is ElseMacroLine);
            }

            if (branchLines != null)
            {
                outLines.AddRange(branchLines);
            }

            return outLines;
        }

        private static bool IsOpcodeDefinition(MacroLine line)
        {
            return line is DataDefinitionMacroLine dataDefinitionMacroLine
                && !string.IsNullOrWhiteSpace(dataDefinitionMacroLine.ConstantName)
                && !string.IsNullOrWhiteSpace(dataDefinitionMacroLine.Constant);
        }

        private static bool IsConditional(MacroLine line)
        {
            return line is IfMacroLine || line is ElseMacroLine || line is EndIfMacroLine;
        }
    }
}
