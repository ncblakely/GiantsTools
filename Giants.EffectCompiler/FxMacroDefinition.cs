namespace Giants.EffectCompiler
{
    using System.Collections.Generic;
    using Giants.BinTools.Macro;

    public class FxMacroDefinition
    {
        public int Opcode { get; set; }

        public string Name { get; set; }

        public IEnumerable<MacroLine> FxDefGroup { get; set; }
    }
}
