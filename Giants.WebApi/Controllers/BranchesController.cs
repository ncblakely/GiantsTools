using Giants.DataContract.V1;
using Giants.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Giants.WebApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    [Route("api/[controller]")]
    public class BranchesController : ControllerBase
    {
        private readonly IVersioningService versioningService;

        public BranchesController(IVersioningService branchService)
        {
            this.versioningService = branchService;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> GetBranches([FromQuery]string appName)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(appName);

            return await this.versioningService.GetBranches(appName);
        }
    }
}
