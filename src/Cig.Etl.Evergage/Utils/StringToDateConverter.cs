using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Cig.Etl.Evergage.Utils
{
	public class StringToDateConverter : ITypeConverter
	{
		public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
		{
			DateTime dateTime;
			if (DateTime.TryParse(text, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out dateTime))
			{
				return dateTime;
			}

			return null;
		}

		public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
		{
			return value.ToString();
		}
	}
}
