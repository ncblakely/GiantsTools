namespace Giants.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class UpdaterService
    {
        private readonly IUpdaterStore updaterStore;

        public UpdaterService(IUpdaterStore updaterStore)
        {
            this.updaterStore = updaterStore;
        }
    }
}
