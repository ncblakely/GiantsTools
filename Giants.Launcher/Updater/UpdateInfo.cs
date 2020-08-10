using System;
using System.IO;

namespace Giants.Launcher
{
    public class UpdateInfo
    {
        public Version VersionFrom { get; set; }
		public Version VersionTo { get; set; }
		public Uri DownloadUri 
		{ 
			get 
			{ 
				return this.downloadUri; 
			} 
			set 
			{
                this.downloadUri = value;
                this.FileName = Path.GetFileName(value.AbsoluteUri); 
			} 
		}
		public int FileSize { get; set; }
		public string FileName { get; set; }
		public UpdateType UpdateType { get; set; }

		private Uri downloadUri;
    }
}
