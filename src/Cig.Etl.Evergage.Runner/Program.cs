using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cig.Etl.Evergage.Configuration;
using Cig.Etl.Evergage.Configuration.Contracts;
using Cig.Etl.Evergage.Jobs;
using Cig.Etl.Evergage.Jobs.Contracts;
using Cig.Evergage.Client;
using Cig.Evergage.Client.Contracts;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Cig.Etl.Evergage.Runner
{
	[Command(Name = "evergageimport", Description = "evergage import project")]
	[HelpOption]
	internal class Program
	{
		private readonly IOrchestrator orchestrator;

		static int Main(string[] args)
		{
			using (var serviceProvider = ConfigureServices())
			{
				var app = new CommandLineApplication<Program>();
				app.Conventions
					.UseDefaultConventions()
					.UseConstructorInjection(serviceProvider);

				return app.Execute(args);
			}

		}

		private static ServiceProvider ConfigureServices()
		{
			var configurationBuilder = new ConfigurationBuilder()
				.SetBasePath(Environment.CurrentDirectory)
				.AddJsonFile("config.json");
			var configuration = configurationBuilder.Build();

			var services = new ServiceCollection();

			services.AddSingleton<ILoggerFactory, LoggerFactory>();
			services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
			services.AddLogging((builder) => builder.SetMinimumLevel(LogLevel.Trace));

			services.AddOptions();
			services.Configure<List<Setting>>(configuration.GetSection("Jobs"));
			services.Configure<EvergageClientOptions>(configuration.GetSection("Evergage"));
			services.AddSingleton<IAwsConfiguration>(new AwsConfiguration(configuration.GetSection("AWS")));

			services.AddHttpClient<IEvergageClient, EvergageClient>();
			services.AddSingleton<IJob, AllSurveys>();
			services.AddSingleton<IJob, CancellationSurvey>();
			services.AddSingleton<IJob, ConsultancySurvey>();
			//Removed the Relationship for now because the Survey Id has changed
			//services.AddSingleton<IJob, RelationshipSurvey>();
			services.AddSingleton<IJob, NewRelationshipSurvey>();
			services.AddSingleton<IOrchestrator, Orchestrator>();

			var serviceProvider =  services.BuildServiceProvider();

			var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

			//configure NLog
			loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
			NLog.LogManager.LoadConfiguration("nlog.config");

			return serviceProvider;
		}

		public Program(IOrchestrator orchestrator)
		{
			this.orchestrator = orchestrator;
		}
	   
		private async Task OnExecuteAsync()
		{
			Func<string, IDisposable> correlationContextFactory = (correlationId) => MappedDiagnosticsLogicalContext.SetScoped("CorrelationId", correlationId);
			Func<string> correlationIdFactory = () => Guid.NewGuid().ToString();

			try
			{
				await this.orchestrator.ExecuteAsync(correlationContextFactory, correlationIdFactory);
			}
			finally
			{
				NLog.LogManager.Shutdown();
			}
		}
	}
}
