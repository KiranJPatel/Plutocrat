using System;
using System.Threading.Tasks;
using Autofac;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using Plutocrat.Core.DI;
using Plutocrat.Core.Helpers;
using Plutocrat.Core.Interfaces;

namespace Plutocrat.Server
{
    internal static class Program
    {
        private static ILogger _logger;

        private static void Main(string[] args)
        {
            var container = AutofacBootstrapper.Init();
            var PlutocratService = container.Resolve<IPlutocratService>();
            var settings = container.Resolve<ISettingsLoader>();
            _logger = container.Resolve<ILogger>();

            //Task.Run(async () => await Plutocrat.Core.Helpers.TelegramBot.Main()).Wait();

            Plutocrat.Core.Helpers.Utils.InitializeDependencies();

            JobManager.Initialize(new PlutocratRegistry(PlutocratService, settings));

            JobManager.JobException += HandleJobException;

            Console.ReadKey();
            JobManager.Stop();
        }

        private static void HandleJobException(JobExceptionInfo info)
        {
            _logger.LogError($"An error just happened with the scheduled job {info.Name}: {info.Exception}");
            
            JobManager.Stop();
        }
    }
}