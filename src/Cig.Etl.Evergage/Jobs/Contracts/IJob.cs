using System.Threading.Tasks;

namespace Cig.Etl.Evergage.Jobs.Contracts
{
    public interface IJob
    {
        string Name { get; }
        Task ExecuteAsync();
    }
}