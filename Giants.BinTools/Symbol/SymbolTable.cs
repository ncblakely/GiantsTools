namespace Giants.BinTools.Symbol
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public class SymbolTable
    {
        private static readonly HashSet<string> ExcludedSymbols = new HashSet<string>() { "FxBinVersion", "SfxVersionOld", "SfxVersion1", "SfxVersion2" };

        [JsonProperty(nameof(symbols))]
        private IDictionary<string, CaseInsensitiveDictionary<int>> symbols = new Dictionary<string, CaseInsensitiveDictionary<int>>(StringComparer.OrdinalIgnoreCase);

        private IDictionary<string, IDictionary<int, string>> reverseSymbolLookup = new Dictionary<string, IDictionary<int, string>>(StringComparer.OrdinalIgnoreCase);

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            foreach (var symbolGroup in this.symbols)
            {
                this.reverseSymbolLookup.Add(symbolGroup.Key, new Dictionary<int, string>());
                foreach (var symbol in this.symbols[symbolGroup.Key])
                {
                    this.reverseSymbolLookup[symbolGroup.Key].Add(symbol.Value, symbol.Key);
                }
            }
        }

        public void AddSymbol(string symbolGroup, string symbolName, int symbolValue)
        {
            if (ExcludedSymbols.Contains(symbolName))
            {
                return;
            }

            if (!this.symbols.ContainsKey(symbolGroup))
            {
                this.symbols.Add(symbolGroup, new CaseInsensitiveDictionary<int>());
            }

            if (!this.reverseSymbolLookup.ContainsKey(symbolGroup))
            {
                this.reverseSymbolLookup.Add(symbolGroup, new Dictionary<int, string>());
            }

            this.symbols[symbolGroup].Add(symbolName, symbolValue);
            this.reverseSymbolLookup[symbolGroup].Add(symbolValue, symbolName);
        }

        public bool ContainsKey(string symbolGroup, string key)
        {
            if (!string.IsNullOrEmpty(symbolGroup) && !string.IsNullOrEmpty(key))
            {
                return this.symbols.ContainsKey(symbolGroup)
                    && this.symbols[symbolGroup].ContainsKey(key);
            }

            return false;
        }

        public bool TryGetSymbolFromName(string symbolGroup, string symbolName, out int symbolValue)
        {
            return this.symbols[symbolGroup].TryGetValue(symbolName, out symbolValue);
        }

        public bool TryGetSymbolFromId(string symbolGroup, int symbolValue, out string symbolName)
        {
            return this.reverseSymbolLookup[symbolGroup].TryGetValue(symbolValue, out symbolName);
        }

        public IDictionary<string, int> GetSymbolGroup(string symbolGroup)
        {
            return this.symbols[symbolGroup];
        }

        private class CaseInsensitiveDictionary<TValue> : Dictionary<string, TValue>
        {
            public CaseInsensitiveDictionary()
                : base(StringComparer.OrdinalIgnoreCase) { }
        }
    }
}
