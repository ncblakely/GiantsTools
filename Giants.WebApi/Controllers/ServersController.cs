using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Giants.DataContract.V1;
using Giants.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Giants.Web.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    [Route("api/[controller]")]
    public class ServersController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<ServersController> logger;
        private readonly IServerRegistryService serverRegistryService;

        public ServersController(
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ServersController> logger,
            IServerRegistryService serverRegistryService)
        {
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
            this.serverRegistryService = serverRegistryService;
        }

        [HttpDelete]
        public async Task DeleteServer()
        {
            string requestIpAddress = this.GetRequestIpAddress();

            this.logger.LogInformation("Deleting server from {IPAddress}", requestIpAddress);

            await this.serverRegistryService.DeleteServer(requestIpAddress);
        }

        [HttpDelete]
        [MapToApiVersion("1.1")]
        public async Task DeleteServer(string gameName, int port)
        {
            string requestIpAddress = this.GetRequestIpAddress();

            this.logger.LogInformation("Deleting server from {IPAddress}", requestIpAddress);

            await this.serverRegistryService.DeleteServer(requestIpAddress, gameName, port);
        }

        [HttpGet]
        public async Task<IEnumerable<ServerInfoWithHostAddress>> GetServers()
        {
            IEnumerable<Services.ServerInfo> serverInfo = await this.serverRegistryService.GetAllServers();

            IMapper mapper = Services.Mapper.GetMapper();
            var mappedServers = serverInfo
                .Select(x => 
                {
                    var serverInfo = mapper.Map<ServerInfoWithHostAddress>(x);
                    serverInfo.HostIpAddress = x.HostIpAddress;
                    return serverInfo;
                })
                .ToList();

            string requestIpAddress = this.GetRequestIpAddress();
            this.logger.LogInformation("Returning {Count} servers to {IPAddress}", mappedServers.Count, requestIpAddress);

            return mappedServers;
        }

        [HttpPost]
        public async Task AddServer([FromBody] DataContract.V1.ServerInfo serverInfo)
        {
            ArgumentUtility.CheckForNull(serverInfo, nameof(serverInfo));

            string requestIpAddress = this.GetRequestIpAddress();

            this.logger.LogInformation("Request to add server from {IPAddress}", requestIpAddress);

            var serverInfoEntity = this.mapper.Map<Services.ServerInfo>(serverInfo);
            serverInfoEntity.HostIpAddress = requestIpAddress.ToString();
            serverInfoEntity.LastHeartbeat = DateTime.UtcNow;

            await this.serverRegistryService.AddServer(serverInfoEntity);

            this.logger.LogInformation("Added server successfully for {IPAddress}", requestIpAddress);
        }

        private string GetRequestIpAddress()
        {
            return this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "<no ip>";
        }
    }
}
