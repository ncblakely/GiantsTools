namespace Giants.Services.Core
{
    using System;

    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
