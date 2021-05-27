using System.Collections.Generic;
using System.Threading.Tasks;
using Plutocrat.Core.Helpers;

namespace Plutocrat.Core.Interfaces
{
    public interface IDatabaseHandler
    {
        Task<string> AddToDatabase(Order order);

        Task<Order> GetFromDatabase(string key);

        Task<IEnumerable<string>> GetAllKeys();
        
        void FlushDatabase();
    }
}