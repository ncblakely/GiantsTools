﻿using System.Threading.Tasks;
using AutoMapper;
using Giants.Services;
using Microsoft.AspNetCore.Mvc;

namespace Giants.WebApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
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
        public async Task<DataContract.V1.VersionInfo> GetVersionInfo(string appName)
        {
            Services.VersionInfo versionInfo = await this.updaterService.GetVersionInfo(appName);

            return this.mapper.Map<DataContract.V1.VersionInfo>(versionInfo);
        }
    }
}
