using Newtonsoft.Json;

namespace Giants.BinTools.Macro
{
    [JsonConverter(typeof(MacroLineJsonConverter))]
    public interface MacroLine
    {
        public MacroLineType Type { get; }
    }
}
