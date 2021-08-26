namespace Cig.Evergage.Client.Contracts
{
    public interface IHttpClientOptions
    {
        string ApiKey { get; set; }
        string BaseAddress { get; set; }
    }
}