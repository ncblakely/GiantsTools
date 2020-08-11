namespace Giants.DataContract
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class AppVersion
    {
        [Required]
        public int Build { get; set; }

        [Required]
        public int Major { get; set; }

        [Required]
        public int Minor { get; set; }

        [Required]
        public int Revision { get; set; }

        public override bool Equals(object obj)
        {
            return obj is AppVersion info &&
                   Build == info.Build &&
                   Major == info.Major &&
                   Minor == info.Minor &&
                   Revision == info.Revision;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Build, Major, Minor, Revision);
        }

        public Version ToVersion()
        {
            return new Version(this.Major, this.Minor, this.Build, this.Revision);
        }
    }
}
