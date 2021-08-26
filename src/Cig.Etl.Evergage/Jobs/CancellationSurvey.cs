using System.Collections.Generic;
using Cig.Etl.Evergage.Configuration;
using Cig.Etl.Evergage.Configuration.Contracts;
using Cig.Evergage.Client.Contracts;
using Eol.Cig.Etl.Kinesis.Producer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cig.Etl.Evergage.Jobs
{
    public class CancellationSurvey : Job<CancellationSurvey>
    {
        public CancellationSurvey(
            IOptions<List<Setting>> jobsOptions,
            IAwsConfiguration awsConfiguration,
            IEvergageClient evergageClient,
            ILogger<CancellationSurvey> jobLogger,
            ILogger<KinesisWriter> kinesisLogger) : base(jobsOptions, awsConfiguration, evergageClient, jobLogger, kinesisLogger)
        {
        }
    }
}