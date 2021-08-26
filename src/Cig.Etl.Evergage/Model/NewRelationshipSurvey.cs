using Cig.Etl.Evergage.Utils;
using CsvHelper.Configuration.Attributes;
using System;

namespace Cig.Etl.Evergage.Model
{
	public class NewRelationshipSurvey
	{
		[Index(0)]
		public string User_Id { get; set; }
		[Index(1)]
		[TypeConverter(typeof(StringToDateConverter))]
		public DateTime TimeStamp { get; set; }
		[Index(2)] public string Question41 { get; set; }
		[Index(3)] public string Question33 { get; set; }
		[Index(4)] public string Question34 { get; set; }
		[Index(5)] public string Question35 { get; set; }
		[Index(6)] public string Question36 { get; set; }
		[Index(7)] public string Question37 { get; set; }
		[Index(8)] public string Question4 { get; set; }
		[Index(9)] public string Question1 { get; set; }
		[Index(10)] public string Question3 { get; set; }
		[Index(11)] public string Question5 { get; set; }
		[Index(12)] public string Question6 { get; set; }
		[Index(13)] public string Question7 { get; set; }
		[Index(14)] public string Question8 { get; set; }
		[Index(15)] public string Question9 { get; set; }
		[Index(16)] public string Question10 { get; set; }
		[Index(17)] public string Question11 { get; set; }
		[Index(18)] public string Question12 { get; set; }
		[Index(19)] public string Question13 { get; set; }
		[Index(20)] public string Question14 { get; set; }
		[Index(21)] public string Question15 { get; set; }
		[Index(22)] public string Question16 { get; set; }
		[Index(23)] public string Question17 { get; set; }
		[Index(24)] public string Question18 { get; set; }
		[Index(25)] public string Question19 { get; set; }
		[Index(26)] public string Question20 { get; set; }
		[Index(27)] public string Question21 { get; set; }
		[Index(28)] public string Question22 { get; set; }
		[Index(29)] public string Question23 { get; set; }
		[Index(30)] public string Question24 { get; set; }
		[Index(31)] public string Question25 { get; set; }
		[Index(32)] public string Question26 { get; set; }
		[Index(33)] public string Clone_of_Question27 { get; set; }
		[Index(34)] public string Question27 { get; set; }
		[Index(35)] public string Question28 { get; set; }
		[Index(36)] public string Question29 { get; set; }
		[Index(37)] public string Clone_of_Question30 { get; set; }
		[Index(38)] public string Question30 { get; set; }
		[Index(39)] public string Question31 { get; set; }
		[Index(40)] public string Question32 { get; set; }
		[Index(41)] public string Question38 { get; set; }
		[Index(42)] public string Clone_of_Question3 { get; set; }
		[Index(43)] public string Question2 { get; set; }
		[Index(44)] public string Clone_of_Question3_ { get; set; }
	}
}
