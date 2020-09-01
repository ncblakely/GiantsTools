namespace Giants.Services.Services
{
    using System.IO;
    using System.Threading.Tasks;

    public interface ICrashReportService
    {
        Task UploadMinidumpToSentry(string fileName, Stream stream);
        Task ProcessReport(string fileName, string senderIpAddress, Stream stream);
    }
}
