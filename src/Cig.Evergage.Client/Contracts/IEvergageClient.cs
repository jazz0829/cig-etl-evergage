using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cig.Evergage.Client.Contracts
{
    public interface IEvergageClient
    {
        Task<IEnumerable<T>> Get<T>(string segmentId, int pageNr, int pageSize);
		Task<string> GetSurveyData();
		Task<string> GetRelationshipSurveyData(string surveyId);
	}
}