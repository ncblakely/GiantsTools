using Giants.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Giants.WebApi.Tests
{
    [TestClass]
    public class BranchesControllerTests
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
        public async Task GetBranchesReturnsExpectedBranches()
        {
            using (var response = await client!.GetAsync("api/branches?appName=Giants"))
            {
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<IEnumerable<string>>(responseContent, options);

                Assert.IsNotNull(responseObject);
                Assert.AreEqual(2, responseObject.Count());
                Assert.IsTrue(responseObject.Any(x => x == "Release"));
                Assert.IsTrue(responseObject.Any(x => x == "Beta"));
            }
        }


    }
}
