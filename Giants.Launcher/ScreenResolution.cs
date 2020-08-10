using System;

namespace Giants.Launcher
{
    class ScreenResolution : IComparable
	{
		public int Width { get; set; }
		public int Height { get; set; }

		public ScreenResolution(int width, int height)
		{
            this.Width = width;
            this.Height = height;
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;

			ScreenResolution other = obj as ScreenResolution;
			if (other == null)
				throw new ArgumentException();

			if (this.Width > other.Width)
				return 1;
			else if (this.Width == other.Width && this.Height == other.Height)
				return 0;
			else
				return -1;
		}

		public override string ToString()
		{
			return string.Format("{0}x{1}", this.Width, this.Height);
		}

	}
}
