namespace Giants.BinTools.Macro
{
    public class IfMacroLine : MacroLine
    {
        public MacroLineType Type => MacroLineType.If;

        public string Condition { get; }

        public IfMacroLine(string[] tokens)
        {
            this.Condition = string.Join(" ", tokens[1..]);
        }

        internal IfMacroLine() { }
    }
}
