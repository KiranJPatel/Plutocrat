using System.Collections.Generic;
using Plutocrat.Core.Helpers;

namespace Plutocrat.Core.Interfaces
{
    public interface ISettingsLoader
    {
        int BuyInterval { get; }

        int SellInterval { get; }

        bool Test { get; }

        string RedisUrl { get; }
        
        List<ExhangeConfig> ExhangeConfigurations { get; }
    }
}