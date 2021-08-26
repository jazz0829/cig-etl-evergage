using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cig.Etl.Evergage.Jobs.Contracts;
using Microsoft.Extensions.Logging;

namespace Cig.Etl.Evergage
{
    public class Orchestrator : IOrchestrator
    {
        private readonly ILogger<Orchestrator> logger;
        private readonly IEnumerable<IJob> jobs;

        public Orchestrator(IEnumerable<IJob> jobs, ILogger<Orchestrator> logger)
        {
            this.jobs = jobs;
            this.logger = logger;
        }
        public async Task ExecuteAsync(Func<string, IDisposable> correlationContextFactory, Func<string> correlationIdFactory)
        {
            var waitAllTask =  Task.WhenAll(jobs.Select(j =>
            {
                using (correlationContextFactory(correlationIdFactory()))
                {
                    return j.ExecuteAsync();
                }
            }));
            try
            {
                await waitAllTask;
            }
            catch
            {
                if (waitAllTask.Exception != null)
                {
                    var allExceptions = waitAllTask.Exception.Flatten();
                    foreach (var innerException in allExceptions.InnerExceptions)
                    {
                        logger.LogError(innerException, innerException.Message);
                    }
                }
            }

        }
    }
}
