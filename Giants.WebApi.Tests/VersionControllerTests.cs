using Giants.DataContract.V1;
using Giants.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace Giants.WebApi.Tests
{
    [TestClass]
    public class VersionControllerTests
    {
        private readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
        private static WebApplicationFactory<Program>? application;
        private static HttpClient? client;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            application = new WebApplicationFactory<Program>()
                                    .WithWebHostBuilder(builder =>
                                    {
                                    });

            client = application.CreateClient();
        }

        [TestMethod]
        public async Task GetVersionSupportsApiVersion1_0()
        {
            using (var response = await client!.GetAsync("api/version?appName=Giants"))
            {
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<VersionInfo>(responseContent, options);
                Assert.IsNotNull(responseObject);

                Assert.AreEqual("Giants", responseObject.AppName);
                Assert.IsNotNull(responseObject.Version);
                Assert.IsTrue(responseObject.InstallerUri.ToString().StartsWith("https://giants.blob.core.windows.net/public/GPatch"));
                Assert.AreEqual("Release", responseObject.BranchName);
            }
        }

        [TestMethod]
        public async Task GetLauncherVersionSupportsApiVersion1_0()
        {
            using (var response = await client!.GetAsync("api/version?appName=GiantsLauncher"))
            {
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<VersionInfo>(responseContent, options);
                Assert.IsNotNull(responseObject);

                Assert.AreEqual("GiantsLauncher", responseObject.AppName);
                Assert.IsNotNull(responseObject.Version);
                Assert.IsTrue(responseObject.InstallerUri.ToString().StartsWith("about:blank"));
                Assert.AreEqual("Release", responseObject.BranchName);
            }
        }

        [TestMethod]
        public async Task GetVersionFiltersByBranchNameCorrectly()
        {
            using (var response = await client!.GetAsync("api/version?appName=Giants&branchName=Release&apiVersion=1.1"))
            {
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<VersionInfo>(responseContent, options);
                Assert.IsNotNull(responseObject);

                Assert.AreEqual("Giants", responseObject.AppName);
                Assert.IsNotNull(responseObject.Version);
                Assert.IsTrue(responseObject.InstallerUri.ToString().StartsWith("https://giants.blob.core.windows.net/public/GPatch"));
                Assert.AreEqual("Release", responseObject.BranchName);
            }

            using (var response = await client!.GetAsync("api/version?appName=Giants&branchName=Beta&apiVersion=1.1"))
            {
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<VersionInfo>(responseContent, options);
                Assert.IsNotNull(responseObject);

                Assert.AreEqual("Giants", responseObject.AppName);
                Assert.IsNotNull(responseObject.Version);
                Assert.IsTrue(responseObject.InstallerUri.ToString().StartsWith("https://giants.blob.core.windows.net/public/GPatch"));
                Assert.AreEqual("Beta", responseObject.BranchName);
            }
        }

        [TestMethod]
        public async Task GetLauncherVersionFiltersByBranchNameCorrectly()
        {
            using (var response = await client!.GetAsync("api/version?appName=GiantsLauncher&branchName=Release&apiVersion=1.1"))
            {
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<VersionInfo>(responseContent, options);
                Assert.IsNotNull(responseObject);

                Assert.AreEqual("GiantsLauncher", responseObject.AppName);
                Assert.IsNotNull(responseObject.Version);
                Assert.IsTrue(responseObject.InstallerUri.ToString().StartsWith("about:blank"));
                Assert.AreEqual("Release", responseObject.BranchName);
            }

            using (var response = await client!.GetAsync("api/version?appName=GiantsLauncher&branchName=Beta&apiVersion=1.1"))
            {
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<VersionInfo>(responseContent, options);
                Assert.IsNotNull(responseObject);

                Assert.AreEqual("GiantsLauncher", responseObject.AppName);
                Assert.IsNotNull(responseObject.Version);
                Assert.IsTrue(responseObject.InstallerUri.ToString().StartsWith("about:blank"));
                Assert.AreEqual("Beta", responseObject.BranchName);
            }
        }
    }
}
