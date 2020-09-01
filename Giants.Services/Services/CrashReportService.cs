﻿namespace Giants.Services.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class CrashReportService : ICrashReportService
    {
        private readonly BlobServiceClient blobServiceClient;
        private readonly IConfiguration configuration;
        private readonly ILogger<CrashReportService> logger;

        public CrashReportService(
            IConfiguration configuration,
            ILogger<CrashReportService> logger)
        {
            this.configuration = configuration;
            this.logger = logger;

            string blobConnectionString = configuration["BlobConnectionString"];
            this.blobServiceClient = new BlobServiceClient(blobConnectionString);
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
