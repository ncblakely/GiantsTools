using System.Threading.Tasks;
using AutoMapper;
using Giants.DataContract;
using Giants.Services;
using Microsoft.AspNetCore.Mvc;

namespace Giants.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VersionController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IUpdaterService updaterService;

        public VersionController(
            IMapper mapper,
            IUpdaterService updaterService)
        {
            this.mapper = mapper;
            this.updaterService = updaterService;
        }

        [HttpGet]
        public async Task<DataContract.VersionInfo> GetVersionInfo(string appName)
        {
            Services.VersionInfo versionInfo = await this.updaterService.GetVersionInfo(appName);

            return mapper.Map<DataContract.VersionInfo>(versionInfo);
        }
    }
}
