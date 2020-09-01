namespace Giants.Services.Services
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class CrashReportService : ICrashReportService
    {
        private readonly BlobServiceClient blobServiceClient;
        private readonly IConfiguration configuration;
        private readonly ILogger<CrashReportService> logger;
        private readonly IHttpClientFactory clientFactory;

        private const string SentryMinidumpUploadFileKey = "upload_file_minidump";

        public CrashReportService(
            IConfiguration configuration,
            ILogger<CrashReportService> logger,
            IHttpClientFactory clientFactory)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.clientFactory = clientFactory;

            string blobConnectionString = configuration["BlobConnectionString"];
            this.blobServiceClient = new BlobServiceClient(blobConnectionString);
        }

        public async Task UploadMinidumpToSentry(string fileName, Stream stream)
        {
            string minidumpUri = this.configuration["SentryMinidumpUri"];
            if (string.IsNullOrEmpty(minidumpUri))
            {
                throw new InvalidOperationException("Minidump URI is not defined.");
            }

            var httpClient = this.clientFactory.CreateClient("Sentry");

            using var zipArchive = new ZipArchive(stream);
            var zipEntry = zipArchive.Entries.FirstOrDefault(e => e.Name == "crashdump.dmp");
            if (zipEntry == null)
            {
                throw new InvalidOperationException("No crash dump found in archive.");
            }

            using var dumpStream = zipEntry.Open();
            using var formData = new MultipartFormDataContent
            {
                { new StreamContent(dumpStream), SentryMinidumpUploadFileKey, fileName }
            };
            var response = await httpClient.PostAsync(minidumpUri, formData).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }
        }

        public async Task ProcessReport(string fileName, string senderIpAddress, Stream stream)
        {
            this.logger.LogInformation("Processing crash report file {FileName} from {IP}", fileName, senderIpAddress);

            var containerClient = this.blobServiceClient.GetBlobContainerClient(
                this.configuration["CrashBlobContainerName"]);

            string blobPath = this.GetBlobPath(fileName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            this.logger.LogInformation("Saving {FileName} to path: {Path}", fileName, blobPath);

            await blobClient.UploadAsync(stream).ConfigureAwait(false);
        }

        private string GetBlobPath(string fileName)
        {
            DateTime dateTime = DateTime.Now;
            return $"{dateTime.Year}/{dateTime.Month}/{dateTime.Day}/{fileName}";
        }
    }
}
