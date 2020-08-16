namespace Giants.Services
{
    using AutoMapper;
    using Giants.DataContract.V1;

    public static class Mapper
    {
        public static IMapper Instance { get; private set; }

        private static readonly object LockObject = new object();

        public static IMapper GetMapper()
        {
            if (Instance == null)
            {
                lock(LockObject)
                {
                    if (Instance == null)
                    {
                        var config = new MapperConfiguration(cfg => {
                            cfg.CreateMap<DataContract.V1.ServerInfo, ServerInfo>();
                            cfg.CreateMap<ServerInfo, DataContract.V1.ServerInfo>();
                            cfg.CreateMap<ServerInfo, ServerInfoWithHostAddress>();
                            cfg.CreateMap<VersionInfo, DataContract.V1.VersionInfo>();
                        });

                        Instance = new AutoMapper.Mapper(config);
                    }
                }
            }

            return Instance;
        }
    }
}
