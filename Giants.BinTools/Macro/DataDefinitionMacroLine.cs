namespace Giants.BinTools.Macro
{
    using System;
    using System.IO;
    using Giants.BinTools;
    using Giants.BinTools.Symbol;
    using Newtonsoft.Json;

    public class DataDefinitionMacroLine : MacroLine
    {
        public MacroLineType Type => MacroLineType.DataDefinition;

        [JsonProperty]
        public string Instruction { get; private set; }

        [JsonProperty]
        public string Constant { get; private set; }

        [JsonProperty]
        public string ConstantName { get; private set; }

        [JsonProperty]
        public string ArgumentPrefix { get; private set; }

        [JsonProperty]
        public int ArgumentIndex { get; private set; }

        public DataDefinitionMacroLine(string instruction, string[] lineData, SymbolTable symbolTable)
        {
            this.Instruction = instruction;

            string data = lineData[1];

            int argIndexRef = data.IndexOf("\\");
            if (argIndexRef > 0)
            {
                this.ArgumentPrefix = data[0..argIndexRef];
                data = data[argIndexRef..];
            }

            if (data.StartsWith("\\"))
            {
                string index = data[1..];
                if (index != "#")
                {
                    this.ArgumentIndex = Convert.ToInt32(data[1..]);
                }
            }
            else
            {
                string groupName = KnownSymbolGroupNames.GetGroupName(data);
                if (groupName != null)
                {
                    this.ConstantName = data;
                    if (symbolTable.TryGetSymbolFromName(groupName, data, out int symbolValue))
                    {
                        this.Constant = symbolValue.ToString();
                    }
                }

                if (this.Constant == null)
                {
                    this.Constant = data;
                }
            }
        }

        internal DataDefinitionMacroLine() { }

        public string Deserialize(BinaryReader reader, SymbolTable symbolTable)
        {
            string value = this.Instruction switch
            {
                MacroInstruction.DefByte => reader.ReadByte().ToString(),
                MacroInstruction.DefFloat => reader.ReadSingle().ToString(),
                MacroInstruction.DefLong => reader.ReadInt32().ToString(),
                MacroInstruction.Name0cs => reader.ReadCString(),
                _ => throw new NotSupportedException()
            };

            if (!string.IsNullOrEmpty(this.ArgumentPrefix))
            {
                if (int.TryParse(value, out int intValue) && symbolTable.TryGetSymbolFromId(this.ArgumentPrefix, intValue, out string symbolName))
                {
                    return symbolName[this.ArgumentPrefix.Length..];
                }
            }

            return value;
        }

        public void Serialize(string token, SymbolTable symbolTable, BinaryWriter binaryWriter)
        {
            if (!string.IsNullOrEmpty(this.ArgumentPrefix))
            {
                if (symbolTable.TryGetSymbolFromName(this.ArgumentPrefix, this.ArgumentPrefix + token, out int symbolValue))
                {
                    token = symbolValue.ToString();
                }
            }

            switch (this.Instruction)
            {
                case MacroInstruction.DefByte: binaryWriter.Write((byte)Convert.ChangeType(token, typeof(byte))); break;
                case MacroInstruction.DefFloat: binaryWriter.Write((float)Convert.ChangeType(token, typeof(float))); break;
                case MacroInstruction.DefLong: binaryWriter.Write((int)Convert.ChangeType(token, typeof(int))); break;
                case MacroInstruction.Name0cs: binaryWriter.WriteCString(token); break;
                default: throw new NotSupportedException();
            }
        }
    }
}
