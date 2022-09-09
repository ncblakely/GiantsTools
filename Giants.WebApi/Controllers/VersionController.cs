using AutoMapper;
using Giants.DataContract.Contracts.V1;
using Giants.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System;
using System.Threading.Tasks;

namespace Giants.WebApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    [Route("api/[controller]")]
    public class VersionController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IVersioningService versioningService;
        private const string VersionWriteScope = "App.Write";

        public VersionController(
            IMapper mapper,
            IVersioningService versioningService)
        {
            this.mapper = mapper;
            this.versioningService = versioningService;
        }

        [HttpGet]
        public async Task<DataContract.V1.VersionInfo> GetVersionInfo(string appName)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(appName);

            VersionInfo versionInfo = await this.versioningService.GetVersionInfo(appName, BranchConstants.DefaultBranchName);

            if (versionInfo == null)
            {
                throw new ArgumentException($"No version information for {appName} found.", appName);
            }

            return this.mapper.Map<DataContract.V1.VersionInfo>(versionInfo);
        }

        [HttpGet]
        [MapToApiVersion("1.1")]
        public async Task<DataContract.V1.VersionInfo> GetVersionInfo(string appName, string branchName)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(appName);
            ArgumentUtility.CheckStringForNullOrEmpty(branchName);

            VersionInfo versionInfo = await this.versioningService.GetVersionInfo(appName, branchName);

            if (versionInfo == null)
            {
                throw new ArgumentException($"No version information for {appName} ({branchName}) found.", appName);
            }

            return this.mapper.Map<DataContract.V1.VersionInfo>(versionInfo);
        }

        [Authorize]
        [RequiredScopeOrAppPermission(
            AcceptedAppPermission = new[] { VersionWriteScope }) ]
        [HttpPost]
        public async Task UpdateVersionInfo([FromBody] VersionInfoUpdate versionInfoUpdate)
        {
            ArgumentUtility.CheckForNull(versionInfoUpdate);

            await this.versioningService.UpdateVersionInfo(
                appName: versionInfoUpdate.AppName,
                appVersion: versionInfoUpdate.AppVersion,
                fileName: versionInfoUpdate.FileName,
                branchName: versionInfoUpdate.BranchName,
                force: versionInfoUpdate.ForceUpdate);
        }
    }
}
