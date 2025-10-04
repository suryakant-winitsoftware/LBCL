using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIComponents.Web
{
    public  class CommonFunctions
    {
       
        public static string GetDataFromResponse(string jsonString)
        {
            // Parse the JSON string into a JObject
            JObject jsonObject = JObject.Parse(jsonString);
            if (jsonObject != null)
            {
                // Get the value of the "Data" key
                JToken dataValue = jsonObject["Data"];
                if (dataValue != null)
                {
                    return dataValue.ToString();
                }
            }
            return jsonString;
        }
    }
}
