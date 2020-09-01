namespace Giants.Services.Services
{
    using System.IO;
    using System.Threading.Tasks;

    public interface ICrashReportService
    {
        Task ProcessReport(string fileName, string senderIpAddress, Stream stream);
    }
}
