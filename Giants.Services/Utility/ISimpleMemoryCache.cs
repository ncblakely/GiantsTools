using System.Collections.Generic;
using System.Threading.Tasks;

namespace Giants.Services.Utility
{
    public interface ISimpleMemoryCache<TRecord>
    {
        Task<IEnumerable<TRecord>> GetItems();

        void Invalidate();
    }
}
