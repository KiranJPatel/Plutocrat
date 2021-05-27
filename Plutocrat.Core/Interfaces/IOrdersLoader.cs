using System.Collections.Generic;
using Plutocrat.Core.Helpers;

namespace Plutocrat.Core.Interfaces
{
    public interface IOrdersLoader
    {
         List<OrderConfiguration> Orders { get; set; }
    }
}