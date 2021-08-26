using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cig.Etl.Evergage.Model
{
	public class AllSurvey
	{
		public string user_id { get; set; }
		public string survey_id { get; set; }
		public string survey_name { get; set; }
		public string page_name { get; set; }
		public string question_id { get; set; }
		public string element_id { get; set; }
		public string element_name { get; set; }
		public string element_type { get; set; }
		public string survey_updated_ts { get; set; }
		public string started_ts { get; set; }
		public string response_ts { get; set; }
		public string element_title { get; set; }
		public string answer { get; set; }
	}
}