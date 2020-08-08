namespace Giants.Services.Core
{
    using System;

    public class DefaultDateTimeProvider : IDateTimeProvider
    {
        public static DefaultDateTimeProvider Instance { get; } = new DefaultDateTimeProvider();

        public DateTime UtcNow => DateTime.UtcNow;
    }
}
