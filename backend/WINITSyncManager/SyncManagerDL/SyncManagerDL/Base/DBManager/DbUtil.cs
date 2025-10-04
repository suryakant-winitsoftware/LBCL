using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Base.DBManager
{
    public class DbUtil
    {
        /// <summary>
        /// Escapes an input string for database processing, that is, 
        /// surround it with quotes and change any quote in the string to 
        /// two adjacent quotes (i.e. escape it). 
        /// If input string is null or empty a NULL string is returned.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>Escaped output string.</returns>
        protected string Escape(string s)
        {
            if (String.IsNullOrEmpty(s))
                return "NULL";
            else
                return "'" + s.Trim().Replace("'", "''") + "'";
        }

        /// <summary>
        /// Escapes an input string for database processing, that is, 
        /// surround it with quotes and change any quote in the string to 
        /// two adjacent quotes (i.e. escape it). 
        /// Also trims string at a given maximum length.
        /// If input string is null or empty a NULL string is returned.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <param name="maxLength">Maximum length of output string.</param>
        /// <returns>Escaped output string.</returns>
        protected string Escape(string s, int maxLength)
        {
            if (String.IsNullOrEmpty(s))
                return "NULL";
            else
            {
                s = s.Trim();
                if (s.Length > maxLength) s = s.Substring(0, maxLength - 1);
                return "'" + s.Trim().Replace("'", "''") + "'";
            }
        }

        /// <summary>
        /// converts an object to double value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>double</returns>
        protected double ToDouble(object value)
        {
            double retValue = 0;

            if (value != DBNull.Value)
            {
                double.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }

        /// <summary>
        /// converts an object to decimal
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>decimal</returns>
        protected decimal ToDecimal(object value)
        {
            decimal retValue = 0;

            if (value != DBNull.Value)
            {
                decimal.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }

        /// <summary>
        /// converts an object to float
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>float</returns>
        protected float ToFloat(object value)
        {
            float retValue = 0;

            if (value != DBNull.Value)
            {
                float.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }
        /// <summary>
        ///  converts an object to integer value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>int</returns>
        protected int ToInteger(object value)
        {
            int retValue = 0;

            if (value != DBNull.Value)
            {
                int.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }
        public static Int32 GetIntValue(object objTest)
        {
            Int32 functionReturnValue = default(Int32);
            try
            {
                if (((objTest != null)) && ((!object.ReferenceEquals(objTest, System.DBNull.Value))))
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
        /// <summary>
        ///  converts an object to long value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>System.Int64</returns>
        protected long ToLong(object value)
        {
            long retValue = 0;

            if (value != DBNull.Value)
            {
                long.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }

        /// <summary>
        ///  converts an object to string value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>string</returns>
        protected string ToString(object value)
        {
            string retValue = "";

            if (value != DBNull.Value)
            {
                retValue = value.ToString();
            }

            return retValue;
        }
        protected string ToStringNullable(object value)
        {
            string retValue = null;

            if (value != DBNull.Value)
            {
                retValue = value.ToString();
            }
            return retValue;
        }

        /// <summary>
        ///  converts an object to boolean value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>bool</returns>
        protected bool ToBoolean(object value)
        {
            bool retValue = false;

            if (value != DBNull.Value)
            {
                try
                {
                    value = Convert.ToBoolean(value);
                }
                catch (Exception ex) { }
                bool.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }

        /// <summary>
        ///  converts an object to datetime value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>System.DateTime</returns>
        protected DateTime ToDateTime(object value)
        {
            DateTime retValue = new DateTime();

            if (value != DBNull.Value)
            {
                DateTime.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }
        protected DateTime? ToDateTimeNullable(object value)
        {
            DateTime dt = ToDateTime(value);
            if (dt == DateTime.MinValue)
            {
                return null;
            }
            else
            {
                return dt;
            }
        }
        protected TimeSpan ToTimeSpan(object value)
        {
            TimeSpan retValue = new TimeSpan();

            if (value != DBNull.Value)
            {
                TimeSpan.TryParse(value.ToString(), out retValue);
            }

            return retValue;

        }

        /// <summary>
        ///  converts an object to bigint value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>Int32</returns>
        protected Int64 ToBigInteger(object value)
        {
            int retValue = 0;

            if (value != DBNull.Value)
            {
                try
                {
                    retValue = int.Parse(value.ToString());
                }
                catch (Exception ex)
                {
                }
            }

            return retValue;
        }
        protected string GetDateTimeInFormatForSqlite(DateTime? date)
        {
            if (date == null)
            {
                return null;
            }
            else
            {
                return date.Value.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        public DateTime GetDate(string date)
        {
            DateTime ReturnDate;
            try
            {
                DateTime.TryParse(date, out ReturnDate);
            }
            catch { ReturnDate = DateTime.Now; }
            return ReturnDate;
        }
        public string GetDateTimeInFormat(DateTime? date, string format = null)
        {
            if (date == null)
            {
                return "N/A";
            }
            else if (date == DateTime.MinValue)
            {
                return "N/A";
            }
            else
            {
                if (string.IsNullOrEmpty(format))
                {
                    format = "dd MMM, yyyy";
                }
                return date.Value.ToString(format);
            }
        }
        public string GetDateTimeInFormat(string date, string format = null)
        {
            DateTime? dt = GetDate(date);
            return GetDateTimeInFormat(dt, format);
        }
        protected bool ValidateDataSet(System.Data.DataSet set)
        {
            return (set != null && set.Tables != null && set.Tables.Count > 0);
        }
        protected bool ValidateDataRowCollection(System.Data.DataRowCollection rows)
        {
            return (rows != null && rows.Count > 0);
        }
        protected int GetBooleanToNumber(bool obj)
        {
            if (obj)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        protected List<T> SerializeDatasetToList<T>(System.Data.DataSet set)
        {
            if (!ValidateDataSet(set))
                return new List<T>();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(Newtonsoft.Json.JsonConvert.SerializeObject(set.Tables[0]),
                new Newtonsoft.Json.JsonSerializerSettings()
                {
                    //DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.IgnoreAndPopulate, 
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
                });
        }
        protected List<T> SerializeDataRowCollectionToList<T>(System.Data.DataRowCollection rows)
        {
            if (!ValidateDataRowCollection(rows))
                return new List<T>();
            System.Data.DataTable dt = new System.Data.DataTable();
            foreach (System.Data.DataRow dr in rows)
            {
                dt.ImportRow(dr);
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(Newtonsoft.Json.JsonConvert.SerializeObject(dt),
                new Newtonsoft.Json.JsonSerializerSettings()
                {
                    //DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.IgnoreAndPopulate
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore
                });
        }
        protected T SerializeDataRowToObject<T>(System.Data.DataRow rows)
        {
            if (rows == null)
                return default(T);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Newtonsoft.Json.JsonConvert.SerializeObject(rows),
                new Newtonsoft.Json.JsonSerializerSettings()
                {
                    //DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.IgnoreAndPopulate
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore
                });
        }
    }
}
