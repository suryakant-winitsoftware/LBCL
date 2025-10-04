using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITSyncManager.Common
{
    public class CommonFunctions
    {
        private readonly IConfiguration _Configuration;

        public CommonFunctions(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        public string BaseURL
        {
            get { return GetStringValue(_Configuration["AppSettings:WinitBaseURL"] ?? ""); }
        }

        public static string GetStringValue(object? objTest)
        {
            try
            {
                return (objTest != null) && (!object.ReferenceEquals(objTest, DBNull.Value)) ? objTest.ToString() : "";
            }
            catch { }
            return "";
        }
        public static Int32 GetIntValue(object objTest)
        {
            Int32 functionReturnValue = default(Int32);
            try
            {
                if (((objTest != null)) && ((!object.ReferenceEquals(objTest, DBNull.Value))))
                {
                    functionReturnValue = Convert.ToInt32(objTest);
                }
                else
                {
                    functionReturnValue = 0;
                }

            }
            catch (Exception ex)
            {
                functionReturnValue = 0;
            }

            return functionReturnValue;

        }
        public static long GetLongValue(object input)
        {
            if (input == null)
                return 0;
            string strValue = input.ToString();

            if (long.TryParse(strValue, out long result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }

    }
}
