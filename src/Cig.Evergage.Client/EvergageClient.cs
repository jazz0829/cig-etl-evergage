using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Cig.Evergage.Client.Contracts;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Cig.Evergage.Client
{
    public class EvergageClient : IEvergageClient
    {
        private readonly EvergageClientOptions ClientOptions;
        private HttpClient HttpClient { get; }
        public EvergageClient(HttpClient client, IOptions<EvergageClientOptions> options)
        {
            this.ClientOptions = options.Value;
            this.HttpClient = client;
            this.HttpClient.BaseAddress = new Uri(this.ClientOptions.BaseAddress);
        }
        public async Task<IEnumerable<T>> Get<T>(string segmentId, int pageNr, int pageSize)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get, 
                $"/api/dataset/engage/users.json?_at={this.ClientOptions.ApiKey}&filter={segmentId}&page={pageNr}&pageSize={pageSize}");
            var response = await this.HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<T>>(json);
        }

		public async Task<string> GetSurveyData()
		{
			var request = new HttpRequestMessage(HttpMethod.Get,
				$"/api/dataset/engage/survey/3IirQ/export?_at={ClientOptions.ApiKey}&timeRange=2019-01-01.." + DateTime.UtcNow.ToString("yyyy-MM-dd"));

			var response = await HttpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsStringAsync();
		}

		public async Task<string> GetRelationshipSurveyData(string surveyId)
		{
			var request = new HttpRequestMessage(HttpMethod.Get,
				$"/api/dataset/engage/survey/{surveyId}/export?_at={ClientOptions.ApiKey}&timeRange=2019-05-15.." + DateTime.UtcNow.ToString("yyyy-MM-dd"));
			var response = await HttpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsStringAsync();
		}
	}
}
