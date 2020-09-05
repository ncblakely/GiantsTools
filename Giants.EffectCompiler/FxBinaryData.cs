namespace Giants.EffectCompiler
{
    using System;
    using System.IO;

    public class FxBinaryData
    {
        public const int CurrentVersion = 2;

        public int Version { get; private set; }

        public int DataSize { get; private set; }

        public int EffectCount { get; private set; }

        public byte[] Data { get; private set; }

        public FxBinaryData(Stream stream)
        {
            using var reader = new BinaryReader(stream);

            this.Version = reader.ReadInt32();
            if (this.Version != CurrentVersion)
            {
                throw new ArgumentException("The version number is incorrect.");
            }

            this.DataSize = reader.ReadInt32();
            this.EffectCount = reader.ReadInt32();
            this.Data = reader.ReadBytes(this.DataSize);
        }
    }
}
