using System;
using System.IO;

namespace Giants.Launcher
{
    public class UpdateInfo
    {
        public int FileSize { get; set; }
        public string FilePath { get; set; }
        public ApplicationType ApplicationType { get; set; }
    }
}
