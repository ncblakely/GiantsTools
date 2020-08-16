namespace Giants.Services.Services
{
    using Microsoft.Extensions.Configuration;

    public class DiscordService : IDiscordService
    {
        private readonly IConfiguration configuration;

        public DiscordService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GetDiscordUri()
        {
            return this.configuration["DiscordUri"];
        }
    }
}
