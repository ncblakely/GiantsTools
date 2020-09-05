namespace Giants.DataContract.V1
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
                   this.Build == info.Build &&
                   this.Major == info.Major &&
                   this.Minor == info.Minor &&
                   this.Revision == info.Revision;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Build, this.Major, this.Minor, this.Revision);
        }

        public Version ToVersion()
        {
            return new Version(this.Major, this.Minor, this.Build, this.Revision);
        }
    }
}
