namespace Giants.BinTools.Macro
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;

    public class MacroLineJsonConverter : CustomCreationConverter<MacroLine>
    {
        private MacroLineType type;
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(MacroLine));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.ReadFrom(reader);
            this.type = jObject["Type"].ToObject<MacroLineType>();
            return base.ReadJson(jObject.CreateReader(), objectType, existingValue, serializer);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override MacroLine Create(Type objectType)
        {
            switch (this.type)
            {
                case MacroLineType.DataDefinition: return new DataDefinitionMacroLine();
                case MacroLineType.Else: return new ElseMacroLine();
                case MacroLineType.EndIf: return new EndIfMacroLine();
                case MacroLineType.GroupUse: return new GroupUseMacroLine();
                case MacroLineType.If: return new IfMacroLine();
            }

            throw new NotSupportedException();
        }
    }
}
