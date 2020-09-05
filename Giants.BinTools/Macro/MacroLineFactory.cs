namespace Giants.BinTools.Macro
{
    using Giants.BinTools.Symbol;

    public static class MacroLineFactory
    {
        public static MacroLine Read(string[] tokens, SymbolTable symbolTable)
        {
            string instruction = tokens[0].ToLowerInvariant().Trim();
            return instruction switch
            {
                MacroInstruction.GroupUse => new GroupUseMacroLine(tokens),
                MacroInstruction.DefByte => new DataDefinitionMacroLine(instruction, tokens, symbolTable),
                MacroInstruction.DefFloat => new DataDefinitionMacroLine(instruction, tokens, symbolTable),
                MacroInstruction.DefLong => new DataDefinitionMacroLine(instruction, tokens, symbolTable),
                MacroInstruction.Name0cs => new DataDefinitionMacroLine(instruction, tokens, symbolTable),
                MacroInstruction.If => new IfMacroLine(tokens),
                MacroInstruction.Else => new ElseMacroLine(),
                MacroInstruction.EndIf => new EndIfMacroLine(),
                _ => null,
            };
        }
    }
}
