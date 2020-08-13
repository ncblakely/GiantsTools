using System;
using System.IO;

namespace Giants.Launcher
{
    partial class RendererInterop
    {
        public class Capabilities : IComparable
        {
            [Flags]
            public enum RendererFlag
            {
                LowBitDepthAllowed = 0x1,

                // Multisampling support flags:
                MSAA2x = 0x2,
                MSAA4x = 0x4,
                MSAA8x = 0x8,
                MSAA16x = 0x10,

                // Other options:
                VSync = 0x20,
                TripleBuffer = 0x40,
            };

            public Capabilities(string filePath, ref RendererInterop.GFXCapabilityInfo gfxCaps)
            {
                this.FilePath = filePath;
                this.FileName = Path.GetFileName(filePath);
                this.MaxAnisotropy = gfxCaps.maxAnisotropy;
                this.Flags = (RendererFlag)gfxCaps.flags;
                this.Priority = gfxCaps.priority;
                this.Name = gfxCaps.rendererName;
            }

            public override string ToString()
            {
                return string.Format("{0} ({1})", this.Name, Path.GetFileName(this.FilePath));
            }

            public int CompareTo(object obj)
            {
                if (obj == null) return 1;

                Capabilities other = obj as Capabilities;
                if (other != null)
                    return this.Priority.CompareTo(other.Priority);
                else
                    throw new ArgumentException();
            }

            public string FilePath { get; private set; }
            public string FileName { get; private set; }
            public int MaxAnisotropy { get; private set; }
            public RendererFlag Flags { get; private set; }
            public int Priority { get; private set; }
            public string Name { get; private set; }
        }
    }
}
