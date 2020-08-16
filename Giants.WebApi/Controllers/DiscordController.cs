namespace Giants.WebApi.Controllers
{
    using Giants.DataContract.V1;
    using Giants.Services;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class DiscordController : ControllerBase
    {
        private readonly IDiscordService discordService;

        public DiscordController(
            IDiscordService discordService)
        {
            this.discordService = discordService;
        }

        [HttpGet]
        public DiscordStatus GetDiscordStatus()
        {
            return new DiscordStatus
            {
                DiscordUri = this.discordService.GetDiscordUri()
            };
        }
    }
}
