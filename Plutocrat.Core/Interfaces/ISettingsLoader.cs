using System.Collections.Generic;
using Plutocrat.Core.Helpers;

namespace Plutocrat.Core.Interfaces
{
    public interface ISettingsLoader
    {
        int BuyInterval { get; }

        int PlacedOrderManagementInterval { get; }

        int DownTrendNotificationJobInterval { get; }

        bool Test { get; }

        string RedisUrl { get; }
        
        List<ExhangeConfig> ExhangeConfigurations { get; }
    }
}