using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LitJson;

namespace Client01
{
	public class JsonUtil
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Member functions

		public static bool GetParameter(JsonData jsonData, string sKey, out string sParameter)
		{
			sParameter = "";

			if (jsonData == null)
				throw new ArgumentNullException("jsonData");

			if (!jsonData.IsObject)
				throw new ArgumentException("jsonData is not object");

			if (sKey == null)
				throw new ArgumentNullException("sKey");

			if (!jsonData.ContainsKey(sKey))
				return false;

			if (jsonData[sKey].IsObject || jsonData[sKey].IsArray)
				sParameter = jsonData[sKey].ToJson();
			else
				sParameter = jsonData[sKey].ToString();

			return true;
		}

		public static bool GetParameter(JsonData jsonData, string sKey, out int nParameter)
		{
			nParameter = -1;

			if (jsonData == null)
				throw new ArgumentNullException("jsonData");

			if (!jsonData.IsObject)
				throw new ArgumentException("jsonData is not object");

			if (sKey == null)
				throw new ArgumentNullException("sKey");

			if (!jsonData.ContainsKey(sKey))
				return false;

			return int.TryParse(jsonData[sKey].ToString(), out nParameter);
		}
	}
}
