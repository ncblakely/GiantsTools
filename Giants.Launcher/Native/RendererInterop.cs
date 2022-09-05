using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Giants.Launcher
{
    public class RendererInterop
    {
#pragma warning disable 649
        public struct GFXCapabilityInfo
        {
            public int maxAnisotropy;
            public uint flags;
            public int priority;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string rendererName;
            public int interfaceVersion;
        };
#pragma warning restore 649

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool GFXGetCapabilities(ref GFXCapabilityInfo outCapabilities);

        /// <summary>
        /// Makes interop call to native renderer DLL to obtain capability information.
        /// </summary>
        /// <returns>True if the given renderer is supported by the system.</returns>
        public static bool GetRendererCapabilities(string dllPath, ref GFXCapabilityInfo capabilities)
        {
            bool rendererSupported = false;
            IntPtr pDll = NativeMethods.LoadLibrary(dllPath);
            if (pDll == IntPtr.Zero)
                throw new System.Exception(string.Format("LoadLibrary() for {0} failed", dllPath));

            IntPtr pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "GFXGetCapabilities");
            if (pAddressOfFunctionToCall == IntPtr.Zero)
                return false;

            var prcGetCapabilities = (GFXGetCapabilities)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(GFXGetCapabilities));
            rendererSupported = prcGetCapabilities(ref capabilities);

            NativeMethods.FreeLibrary(pDll);

            return rendererSupported;
        }


        public static List<RendererInfo> GetCompatibleRenderers(string gamePath)
        {
            var dir = new DirectoryInfo(
                Path.GetDirectoryName(gamePath));
            var capabilities = new List<RendererInfo>();

            // Search current directory for compatible renderers:
            foreach (FileInfo file in dir.GetFiles("gg_*.dll"))
            {
                try
                {
                    NativeMethods.SetDllDirectory(Environment.CurrentDirectory);

                    // Make interop call to native renderer DLLs to get capability info
                    var interopCaps = new RendererInterop.GFXCapabilityInfo();
                    string path = Path.Combine(file.DirectoryName, file.Name);
                    if (GetRendererCapabilities(path, ref interopCaps) && interopCaps.priority >= 0)
                    {
                        RendererInfo caps = new RendererInfo(path, ref interopCaps);
                        capabilities.Add(caps);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return capabilities;
        }
    }
}
