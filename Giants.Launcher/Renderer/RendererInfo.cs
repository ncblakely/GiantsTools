using System;
using System.Collections.Generic;
using System.IO;

namespace Giants.Launcher
{
    public class RendererInfo : IComparable
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

        public RendererInfo(string filePath, ref RendererInterop.GFXCapabilityInfo gfxCaps)
        {
            this.FilePath = filePath;
            this.FileName = Path.GetFileName(filePath);
            this.MaxAnisotropy = gfxCaps.maxAnisotropy;
            this.Flags = (RendererFlag)gfxCaps.flags;
            this.Priority = gfxCaps.priority;
            this.Name = gfxCaps.rendererName;

            if (this.Flags.HasFlag(RendererFlag.MSAA16x))
            {
                this.MaxAntialiasing = 16;
            }
            else if (this.Flags.HasFlag(RendererFlag.MSAA8x))
            {
                this.MaxAntialiasing = 8;
            }
            else if (this.Flags.HasFlag(RendererFlag.MSAA4x))
            {
                this.MaxAntialiasing = 4;
            }
            else if (this.Flags.HasFlag(RendererFlag.MSAA2x))
            {
                this.MaxAntialiasing = 2;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            RendererInfo other = obj as RendererInfo;
            if (other != null)
                return this.Priority.CompareTo(other.Priority);
            else
                throw new ArgumentException();
        }

        public override bool Equals(object obj)
        {
            return obj is RendererInfo info &&
                   this.FilePath == info.FilePath &&
                   this.FileName == info.FileName &&
                   this.MaxAnisotropy == info.MaxAnisotropy &&
                   this.MaxAntialiasing == info.MaxAntialiasing &&
                   this.Flags == info.Flags &&
                   this.Priority == info.Priority &&
                   this.Name == info.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = 300496696;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.FilePath);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.FileName);
            hashCode = hashCode * -1521134295 + this.MaxAnisotropy.GetHashCode();
            hashCode = hashCode * -1521134295 + this.MaxAntialiasing.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Flags.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Priority.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Name);
            return hashCode;
        }

        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public int MaxAnisotropy { get; private set; }
        public int MaxAntialiasing { get; private set; }
        public RendererFlag Flags { get; private set; }
        public int Priority { get; private set; }
        public string Name { get; set; }
    }
}
