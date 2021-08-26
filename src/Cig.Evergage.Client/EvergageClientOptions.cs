using Cig.Evergage.Client.Contracts;

namespace Cig.Evergage.Client
{
    public class EvergageClientOptions : IHttpClientOptions
    {
        public string ApiKey { get; set; }
        public string BaseAddress { get; set; }
    }
}
