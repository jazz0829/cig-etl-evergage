using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cig.Evergage.Client
{
    public class SegmentResponse
    {
        public string Name { get; set; }
        public string AccountName { get; set; }
        [JsonConverter(typeof(MicrosecondEpochConverter))]
        public DateTime SegmentJoined { get; set; }
        public Dictionary<string, string> SegmentInputValues { get; set; }
    }
}
