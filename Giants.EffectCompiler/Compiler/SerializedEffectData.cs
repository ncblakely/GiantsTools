namespace Giants.EffectCompiler
{
    using System.Collections.Generic;

    public class SerializedEffectData
    {
        public byte[] Data { get; set; }

        public int TableOfContentsSize { get; set; }

        public IList<ContentEntry> TableOfContents { get; set; } = new List<ContentEntry>();
    }
}
