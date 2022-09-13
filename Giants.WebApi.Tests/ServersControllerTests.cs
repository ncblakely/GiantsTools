using Autofac;
using Giants.DataContract.V1;
using Giants.Services;
using Giants.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using YamlDotNet.Core.Tokens;

namespace Giants.WebApi.Tests
{
    [TestClass]
    public class ServersControllerTests
    {
        private const string ServersRoute = "/api/servers";

        private readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
        private static WebApplicationFactory<Program>? application;
        private static HttpClient? client;

        private class TestRegistrations : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder
                    .RegisterType<InMemoryServerRegistryStore>()
                    .As<IServerRegistryStore>()
                    .SingleInstance();
            }
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            application = new WebApplicationFactory<Program>()
                                    .WithWebHostBuilder(builder =>
                                    {
                                        builder.ConfigureTestServices(x => 
                                        {
                                            Program.AddAdditionalRegistrations(new[] { new TestRegistrations() });
                                        });
                                    });

            client = application.CreateClient();
        }

        [TestMethod]
        public async Task GetServersWithNoRegistrations()
        {
            using var response = await client!.GetAsync(ServersRoute);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<IEnumerable<ServerInfoWithHostAddress>>(responseContent, options);
            Assert.IsNotNull(responseObject);

            Assert.AreEqual(0, responseObject.Count());
        }

        [TestMethod]
        public async Task AddAndGetServerRegistration()
        {
            var serverInfo = new DataContract.V1.ServerInfo
            {
                GameName = "Giants",
                Version = new AppVersion() {  Build = 1, Major = 1, Minor = 1, Revision = 1 },
                SessionName = "Session name",
                MapName = "Map name",
                GameState = "openplaying",
                GameType = "Game type",
                PlayerInfo = new List<PlayerInfo>()
                {
                    new PlayerInfo
                    {
                        Name = "test player",
                        TeamName = "Green",
                    }
                }

            };

            var postContent = JsonContent.Create(serverInfo);

            using var postResponse = await client!.PostAsync(ServersRoute, postContent);
            postResponse.EnsureSuccessStatusCode();

            using var getResponse = await client!.GetAsync(ServersRoute);
            getResponse.EnsureSuccessStatusCode();

            var responseContent = await getResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<IEnumerable<ServerInfoWithHostAddress>>(responseContent, options);

            Assert.IsNotNull(responseObject);
            Assert.AreEqual(1, responseObject.Count());

            var returnedServerInfo = responseObject.First();
            Assert.AreEqual(serverInfo.GameName, returnedServerInfo.GameName);
            Assert.AreEqual(serverInfo.Version, returnedServerInfo.Version);
            Assert.AreEqual(serverInfo.SessionName, returnedServerInfo.SessionName);
            Assert.AreEqual(serverInfo.MapName, returnedServerInfo.MapName);
            Assert.AreEqual(serverInfo.GameState, returnedServerInfo.GameState);
            Assert.AreEqual(serverInfo.GameType, returnedServerInfo.GameType);
            Assert.IsTrue(serverInfo.PlayerInfo.SequenceEqual(returnedServerInfo.PlayerInfo));
        }
    }
}
