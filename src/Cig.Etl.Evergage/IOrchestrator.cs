using System;
using System.Threading.Tasks;

namespace Cig.Etl.Evergage
{
    public interface IOrchestrator
    {
        Task ExecuteAsync(Func<string, IDisposable> correlationContextFactory, Func<string> correlationIdFactory);
    }
}