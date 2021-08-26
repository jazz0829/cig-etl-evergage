using Cig.Etl.Evergage.Utils;
using CsvHelper.Configuration.Attributes;
using System;

namespace Cig.Etl.Evergage.Model
{
	public class ConsultancySurvey
	{
		[Index(0)]
		public string UserId { get; set; }

		[Index(1)]
		[TypeConverter(typeof(StringToDateConverter))]
		public DateTime TimeStamp { get; set; }

		[Index(2)]
		public string Solution { get; set; }

		[Index(3)]
		public string RequestId { get; set; }

	    [Index(4)]
        public string ContactId { get; set; }

        [Index(5)]
		public string Question4 { get; set; }

		[Index(6)]
		public string Question5 { get; set; }

		[Index(7)]
		public string Question6 { get; set; }

		[Index(8)]		
		public string Question7 { get; set; }

		[Index(9)]		
		public string Question8 { get; set; }

		[Index(10)]
		public string Question1 { get; set; }

		[Index(11)]
		public string Question2 { get; set; }

	    [Index(12)]
	    public string Question9 { get; set; } // map to question6

        [Index(13)]
	    public string Question9Verbatim { get; set; } // map to question7
    }
}
