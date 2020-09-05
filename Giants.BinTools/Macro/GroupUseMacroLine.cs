namespace Giants.BinTools.Macro
{
    public class GroupUseMacroLine : MacroLine
    {
        public MacroLineType Type => MacroLineType.GroupUse;

        public string GroupName { get; set; }

        public GroupUseMacroLine(string[] tokens)
        {
            this.GroupName = tokens[1];
        }

        internal GroupUseMacroLine() { }
    }
}
