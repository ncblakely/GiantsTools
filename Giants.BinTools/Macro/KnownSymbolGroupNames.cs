namespace Giants.BinTools.Macro
{
    using System;
    using System.Collections.Generic;

    public static class KnownSymbolGroupNames
    {
        public const string Sfx = "sfx";
        public const string Fx = "Fx";
        public const string Object = "ObjObj";
        public const string ObjectData = "ObjData";
        public const string ObjectGroup = "ObjGroup";

        public static readonly IList<string> Names = new[] { Sfx, Fx, Object, ObjectData, ObjectGroup };

        public static string GetGroupName(string str)
        {
            foreach (string groupName in Names)
            {
                if (str.StartsWith(groupName, StringComparison.OrdinalIgnoreCase))
                {
                    return groupName;
                }
            }

            return null;
        }
    }
}
