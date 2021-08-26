namespace Cig.Etl.Evergage.Configuration.Contracts
{
    public interface ISetting
    {
        string Name { get; set; }
        string SegmentId { get; set; }
        string DestinationConnectionString { get; set; }
        string DestinationTable { get; set; }
        string ConfigurationTable { get; set; }
        int BatchSize { get; set; }
    }
}