namespace Giants.Services.Services
{
    using Microsoft.Extensions.Configuration;

    public class CommunityService : ICommunityService
    {
        private readonly IConfiguration configuration;

        public CommunityService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GetDiscordUri()
        {
            return this.configuration["DiscordUri"];
        }
    }
}
