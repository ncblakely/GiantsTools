namespace Giants.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AutoMapper;

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
                            cfg.CreateMap<DataContract.ServerInfo, Services.ServerInfo>();
                            cfg.CreateMap<Services.ServerInfo, DataContract.ServerInfo>();
                        });

                        Instance = new AutoMapper.Mapper(config);
                    }
                }
            }

            return Instance;
        }
    }
}
