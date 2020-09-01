namespace Giants.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Giants.Services.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class CrashReportsController : ControllerBase
    {
        private readonly ICrashReportService crashReportService;
        private readonly IHttpContextAccessor httpContextAccessor;

        private const long MaximumSizeInBytes = 5242880; // 5MB

        public CrashReportsController(
            ICrashReportService crashReportService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.crashReportService = crashReportService;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public async Task Upload()
        {
            this.ValidateFiles(this.Request.Form.Files);

            var file = this.Request.Form.Files.First();
            using (var stream = file.OpenReadStream())
            {
                await this.crashReportService.ProcessReport(file.FileName, this.GetRequestIpAddress(), stream).ConfigureAwait(false);
            }
        }

        private void ValidateFiles(IEnumerable<IFormFile> formFiles)
        {
            if (formFiles.Count() != 1)
            {
                // We only expect one .zip file
                throw new ArgumentException("Only one file is accepted.", nameof(formFiles));
            }

            var file = this.Request.Form.Files.First();
            if (file.Length > MaximumSizeInBytes)
            {
                throw new ArgumentException("File too large.", nameof(formFiles));
            }

            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            if (!Guid.TryParse(fileName, out Guid _) || file.Name != "crashrpt")
            {
                throw new ArgumentException("Unexpected file name.", nameof(formFiles));
            }
        }

        private string GetRequestIpAddress()
        {
            return this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
