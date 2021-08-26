using System;
using Cig.Etl.Evergage.Configuration.Contracts;
using Microsoft.Extensions.Configuration;

namespace Cig.Etl.Evergage.Configuration
{
    public class AwsConfiguration : IAwsConfiguration
    {
        public string AwsAccessKeyId { get; set; }
        public string AwsSecretAccessKey { get; set; }
        public string AwsKinesisStreamName { get; set; }
        public string S3Prefix { get; set; }
        public bool IsStreamingEnabled { get; set; }

        public AwsConfiguration(IConfigurationSection configuration)
        {
            this.AwsAccessKeyId = configuration["AwsAccessKeyId"];
            this.AwsSecretAccessKey = configuration["AwsSecretAccessKey"];
            this.AwsKinesisStreamName = configuration["AwsKinesisStreamName"];
            this.S3Prefix = configuration["S3Prefix"];
            this.IsStreamingEnabled = Boolean.Parse(configuration["IsStreamingEnabled"]);
        }
    }
}
