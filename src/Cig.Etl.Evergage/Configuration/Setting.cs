using Cig.Etl.Evergage.Configuration.Contracts;

namespace Cig.Etl.Evergage.Configuration
{
    public class Setting : ISetting
    {
        public string Name { get; set; }
        public string SegmentId { get; set; }
        public string DestinationConnectionString { get; set; }
        public string DestinationTable { get; set; }
        public string ConfigurationTable { get; set; }
        public int BatchSize { get; set; }
        public string DutchSurveyId { get; set; }
        public string EnglishSurveyId { get; set; }
        public string FlemishSurveyId { get; set; }
        public string SourceFolder { get; set; }
    }
}
