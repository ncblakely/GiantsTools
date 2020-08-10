using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Giants.Launcher
{
    class RendererInterop
    {
#pragma warning disable 649
        public struct GFXCapabilityInfo
        {
            public int maxAnisotropy;
			public uint flags;
            public int priority;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string rendererName;
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


		public static List<Capabilities> GetCompatibleRenderers(string gamePath)
		{
			DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(gamePath));

			List<Capabilities> Capabilities = new List<Capabilities>(); 

			// Search current directory for compatible renderers:
			foreach (FileInfo file in dir.GetFiles("gg_*.dll"))
			{
				try
				{
					// Make interop call to native renderer DLLs to get capability info
					RendererInterop.GFXCapabilityInfo interopCaps = new RendererInterop.GFXCapabilityInfo();
					string path = Path.Combine(file.DirectoryName, file.Name);
					if (RendererInterop.GetRendererCapabilities(path, ref interopCaps))
					{

						Capabilities caps = new Capabilities(path, ref interopCaps);
						Capabilities.Add(caps);
						//cmbRenderer.Items.Add(caps);
					}


				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			return Capabilities;

			// Select highest priority renderer
			//cmbRenderer.SelectedItem = _RendererCaps.Max();
		}

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
				FilePath = filePath;
				FileName = Path.GetFileName(filePath);
				MaxAnisotropy = gfxCaps.maxAnisotropy;
				Flags = (RendererFlag)gfxCaps.flags;
				Priority = gfxCaps.priority;
				Name = gfxCaps.rendererName;
			}

			public override string ToString()
			{
				return string.Format("{0} ({1})", Name, Path.GetFileName(FilePath));
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
			public string FileName { get; private set;  }
			public int MaxAnisotropy { get; private set; }
			public RendererFlag Flags { get; private set; }
			public int Priority { get; private set; }
			public string Name { get; private set; }
		}
    }
}
