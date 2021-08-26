using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cig.Evergage.Client
{
    public class EvergageClientBatcher
    {
        public int PageSize { get; }
        public string SegmentId { get; }

        public EvergageClientBatcher(string segmentId, int pageSize = 100)
        {
            PageSize = pageSize;
            SegmentId = segmentId;
        }
        public async Task<IEnumerable<T>> MakeBatchRequestAndAggregate<T>(Func<string, int, int, Task<IEnumerable<T>>> httpFunc, Func<IEnumerable<T>, IEnumerable<T>> postProcessor)
        {
            var pageNumber = 0;
            var aggregatedResult = new List<T>();
            List<T> batchResult;
            do
            {
                batchResult = (await httpFunc(SegmentId, pageNumber, PageSize)).ToList();
                aggregatedResult.AddRange(batchResult);
                pageNumber++;
            } while (batchResult.Count > 0);

            if (postProcessor != null)
                return postProcessor(aggregatedResult);
            else
            {
                return aggregatedResult;
            }
        }
    }
}
