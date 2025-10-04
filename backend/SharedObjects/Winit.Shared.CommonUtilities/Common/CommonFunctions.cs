using ClosedXML.Excel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;

//using DinkToPdf
//using DinkToPdf.Contracts;
using System.Text.RegularExpressions;
using System.Web;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Shared.CommonUtilities.Common
{
    public class CommonFunctions
    {
        private NavigationManager _navigationManager;
        IJSRuntime _jSRuntime;
        //private IServiceProvider _serviceProvider;
        public CommonFunctions(NavigationManager navigationManager, IJSRuntime jSRuntime)
        {
            _navigationManager = navigationManager;
            _jSRuntime = jSRuntime;
            //_serviceProvider = serviceProvider;
        }
        public CommonFunctions()
        {

        }
        public string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(_navigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
        }
        //public string GetDataFromResponse(string jsonString)
        //{
        //    // Parse the JSON string into a JObject
        //    JObject jsonObject = JObject.Parse(jsonString);
        //    if (jsonObject != null)
        //    {
        //        // Get the value of the "Data" key
        //        JToken dataValue = jsonObject["Data"];
        //        if (dataValue != null)
        //        {
        //            return dataValue.ToString();
        //        }
        //    }
        //    return jsonString;
        //}
        public string GetDataFromResponse(string jsonString, string propertyToGetObject = "Data")
        {
            // Parse the JSON string into a JObject
            JObject jsonObject = JObject.Parse(jsonString);
            if (jsonObject != null)
            {
                // Get the value of the "any" key
                try
                {
                    JToken dataValue = jsonObject[propertyToGetObject];
                    if (dataValue != null)
                    {
                        return dataValue.ToString();
                    }
                }
                catch
                {

                }
            }
            return jsonString;
        }
        public static string GetEncodedStringValue(string val)
        {
            return HttpUtility.UrlEncode(val);
        }
        public static string GetDecodedStringValue(string val)
        {
            return HttpUtility.UrlDecode(val);
        }
        public void SessionExpire()
        {
            _navigationManager.NavigateTo("logout");
        }

        public async Task<string> DownloadFileAsync(string fileUrl, string folderPath, string fileName)
        {
            try
            {
                // Create the folder if it doesn't exist
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Combine the folder path and file name to get the full file path
                string filePath = Path.Combine(folderPath, fileName);

                using (var httpClient = new HttpClient())
                {
                    // Download the file from the URL
                    byte[] fileData = await httpClient.GetByteArrayAsync(fileUrl);

                    // Write the downloaded file data to the specified path
                    await File.WriteAllBytesAsync(filePath, fileData);
                }

                return filePath; // Return the full path to the downloaded file
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during file download or saving
                Console.WriteLine($"Error downloading file: {ex.Message}");
                return null; // Return null to indicate failure
            }
        }
        public async Task<bool> ZIPFolder(string physicalUrl, string relativePath)
        {
            bool status = false;
            try
            {
                string fullUrl = $"{physicalUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(fullUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        // Get the base directory from the relative path
                        string baseDirectory = Path.GetDirectoryName(relativePath);
                        if (string.IsNullOrEmpty(baseDirectory))
                        {
                            throw new Exception("Invalid relative path: Directory path could not be resolved.");
                        }

                        // Ensure the directory exists
                        if (!Directory.Exists(baseDirectory))
                        {
                            Directory.CreateDirectory(baseDirectory);
                        }

                        // Construct the file paths
                        string fileName = Path.GetFileName(relativePath);
                        string downloadedFilePath = Path.Combine(baseDirectory, fileName);
                        string zipFilePath = Path.Combine(baseDirectory, Path.GetFileNameWithoutExtension(fileName) + ".zip");

                        // Download the file to the base directory
                        using (var fs = new FileStream(downloadedFilePath, FileMode.Create, FileAccess.Write))
                        {
                            await response.Content.CopyToAsync(fs);
                        }

                        // Initial delay to allow file to be released
                        await Task.Delay(2000);
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();

                        // Create the ZIP file in the same directory with retry logic
                        int retryCount = 0;
                        int maxRetries = 10;
                        Exception lastException = null;
                        
                        while (retryCount < maxRetries)
                        {
                            try
                            {
                                // Wait before attempting (especially on retries)
                                if (retryCount > 0)
                                {
                                    await Task.Delay(2000);
                                    GC.Collect();
                                    GC.WaitForPendingFinalizers();
                                    GC.Collect();
                                }
                                
                                using (FileStream zipFileStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write))
                                using (ZipArchive zip = new ZipArchive(zipFileStream, ZipArchiveMode.Create, false))
                                {
                                    // Use FileShare.ReadWrite when reading the source file
                                    using (FileStream sourceStream = new FileStream(downloadedFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                    {
                                        var entry = zip.CreateEntry(fileName, CompressionLevel.Optimal);
                                        using (var entryStream = entry.Open())
                                        {
                                            await sourceStream.CopyToAsync(entryStream);
                                        }
                                    }
                                    status = true;
                                    break; // Success, exit the retry loop
                                }
                            }
                            catch (IOException ioEx) when (ioEx.HResult == -2147024864) // File in use error
                            {
                                lastException = ioEx;
                                retryCount++;
                                Console.WriteLine($"File locked, retry {retryCount}/{maxRetries}: {ioEx.Message}");
                                
                                if (retryCount >= maxRetries)
                                {
                                    throw new Exception($"Failed to create ZIP after {maxRetries} attempts. File may be locked.", lastException);
                                }
                            }
                            catch (Exception)
                            {
                                throw; // Other exceptions, don't retry
                            }
                        }

                        //File.Delete(downloadedFilePath);
                    }
                    else
                    {
                        throw new Exception($"File not found at URL: {fullUrl}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing zip file: {ex.Message}", ex);
            }
            return status;
        }
        public async Task<string> DownloadAndExtractZipAsync(string zipUrl, string folderPath, string extractedFileName)
        {
            try
            {
                // Create the folder if it doesn't exist
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Define the file name for the downloaded ZIP file
                string zipFileName = Path.Combine(folderPath, "downloaded.zip");

                // Download the ZIP file from the URL
                using (var httpClient = new HttpClient())
                {
                    byte[] zipData = await httpClient.GetByteArrayAsync(zipUrl);
                    await File.WriteAllBytesAsync(zipFileName, zipData);
                }

                // Extract the contents of the ZIP file to the specified folder
                ZipFile.ExtractToDirectory(zipFileName, folderPath, true);

                // Get the list of files extracted from the ZIP
                string[] extractedFiles = Directory.GetFiles(folderPath);

                // Find the first extracted file (you can implement more specific logic here)
                string originalExtractedFilePath = null;
                foreach (var extractedFile in extractedFiles)
                {
                    if (Path.GetFileName(extractedFile) != "downloaded.zip")
                    {
                        originalExtractedFilePath = extractedFile;
                        break;
                    }
                }

                // Define the full path to the extracted file with the specified name
                string finalExtractedFilePath = Path.Combine(folderPath, extractedFileName);

                // If a file was found, rename it to the specified name
                if (!string.IsNullOrEmpty(originalExtractedFilePath))
                {
                    if (File.Exists(finalExtractedFilePath))
                    {
                        File.Delete(finalExtractedFilePath);
                    }
                    File.Move(originalExtractedFilePath, finalExtractedFilePath);
                }

                // Delete the downloaded ZIP file
                File.Delete(zipFileName);

                // Return the full path to the renamed file, or null if no valid file was found
                return !string.IsNullOrEmpty(originalExtractedFilePath) ? finalExtractedFilePath : null;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during download, extraction, or file operations
                Console.WriteLine($"Error: {ex.Message}");
                return null; // Return null to indicate failure
            }
        }
        public List<T> GetDataInRange<T>(List<T> data, int startIndex, int endIndex)
        {
            // Check for invalid input
            if (data == null || startIndex < 0 || endIndex < 0 || startIndex > endIndex)
            {
                return data; // Return null to indicate an error condition
            }

            // Use LINQ to get data in the specified range
            List<T> result = data.Skip(startIndex).Take(endIndex - startIndex + 1).ToList();
            return result;
        }
        public static bool GetBooleanValue(object objStatus)
        {
            bool returnVal = false;

            if (((objStatus != null)) && ((!object.ReferenceEquals(objStatus, System.DBNull.Value))))
            {
                if (objStatus.ToString().ToLower() is "1" or "y" or "active" or "true")
                {
                    returnVal = true;
                }
            }
            return returnVal;
        }
        public static string GetBooleanValueInYesOrNO(object objStatus)
        {
            string returnVal = "No";

            if (((objStatus != null)) && ((!object.ReferenceEquals(objStatus, System.DBNull.Value))))
            {
                if (objStatus.ToString().ToLower() is "1" or "y" or "active" or "true")
                {
                    returnVal = "Yes";
                }
            }
            return returnVal;
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
        public static decimal GetDecimalValue(object objTest)
        {
            decimal functionReturnValue = 0;
            try
            {
                if (objTest != null)
                {
                    decimal.TryParse(objTest.ToString(), out functionReturnValue);
                }
            }
            catch (Exception ex)
            {
                functionReturnValue = 0;
            }
            return functionReturnValue;
        }
        public static decimal RoundForSystem(object input, int NoOfDecimal = 2, bool IsNegativeShowEnable = true)
        {
            decimal finalValue = GetDecimalValue(input);
            if (!IsNegativeShowEnable && finalValue < 0)
            {
                finalValue = 0;
            }
            return System.Math.Round(finalValue, NoOfDecimal, MidpointRounding.AwayFromZero);
        }
        public static string GetDateTimeInFormatForSqlite(DateTime? date)
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
        public string GetDateOnlyInFormat(string value)
        {
            try
            {
                string dateValueString = value;
                DateTime dateValue;

                if (DateTime.TryParse(dateValueString, out dateValue))
                {
                    return dateValue.ToString("yyyy-MM-dd");
                    // Use the formattedDate as needed
                }
                return value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static string GetDateOnlyInFormatForSqlite(DateTime? date)
        {
            if (date == null)
            {
                return null;
            }
            else
            {
                return date.Value.ToString("yyyy-MM-dd");
            }
        }
        //public static string GetDateInFormat(DateTime? date)
        //{
        //    if (date == null)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return date.Value.ToString("MMM dd, yyyy");
        //    }
        //}
        public static int GetBooleanInBeat(bool obj)
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
        public static string GetStringValueNA(object objTest)
        {
            try
            {
                if (string.IsNullOrEmpty(objTest.ToString()))
                {
                    return "N/A";
                }
                else
                {
                    return objTest.ToString();
                }
            }
            catch (Exception ex)
            {
                //CommonFunctions.LogError(ex, WINIT.ErrorLog.LogSeverity.Error);
            }
            return "N/A";
        }
        public static long getCurrentDateInMillisUTC()
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            return (long)(DateTime.Now - epoch).TotalMilliseconds;
        }
        public static string GetStringInNumberFormatExcludingZero(object objTest)
        {
            decimal functionReturnValue = 0;
            try
            {
                if (objTest != null)
                {
                    decimal.TryParse(objTest.ToString(), out functionReturnValue);
                }
            }
            catch (Exception ex)
            {
                functionReturnValue = 0;
            }
            return functionReturnValue.ToString("#,##0.##", new System.Globalization.CultureInfo("hi-IN"));
        }
        public static string GetStringInNumberFormat(object objTest)
        {
            decimal functionReturnValue = 0;
            try
            {
                if (objTest != null)
                {
                    decimal.TryParse(objTest.ToString(), out functionReturnValue);
                }
            }
            catch (Exception ex)
            {
                functionReturnValue = 0;
            }
            return functionReturnValue.ToString("#,##0.00", System.Globalization.CultureInfo.InvariantCulture);
        }
        public static int GetTimeDifferenceInDay(DateTime startDate, DateTime? endDate)
        {
            if (endDate == null || startDate.Equals(DateTime.MinValue) || endDate.Equals(DateTime.MinValue))
            {
                return 0;
            }
            else
            {
                return (endDate.Value.Date - startDate.Date).Days;
            }
        }
        public static float GetFloatValue(object objTest)
        {
            float functionReturnValue = 0;
            try
            {
                if (objTest != null)
                {
                    float.TryParse(objTest.ToString(), out functionReturnValue);
                }
            }
            catch (Exception ex)
            {
                //CommonFunctions.LogError(ex, WINIT.ErrorLog.LogSeverity.Error);
                functionReturnValue = 0.0f;
            }
            return functionReturnValue;
        }
        public static string GetUniqueTimeTick()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
        public static string GetTwoDecimalValueAfterPoint(object input)
        {

            var value = System.Math.Round(GetDecimalValue(input), 2, MidpointRounding.AwayFromZero);
            return DoFormat(value);
        }
        public static string RoundForSystemWithoutZero(object input, int noOfDecimal, bool isNegativeShowEnable = true)
        {
            var value = RoundForSystem(input, noOfDecimal, isNegativeShowEnable);
            return DoFormat(value);
        }
        public static long GetDateInMillis(DateTime date)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            return (long)(date - epoch).TotalMilliseconds;
        }
        public static DateTime GetDate(string date)
        {
            DateTime ReturnDate;
            try
            {
                DateTime.TryParse(date, out ReturnDate);
            }
            catch { ReturnDate = DateTime.Now; }
            return ReturnDate;
        }
        public static DateTime GetDate(string date, bool isDateonly = false)
        {
            DateTime ReturnDate;
            try
            {
                DateTime.TryParse(date, out ReturnDate);
                if (isDateonly && ReturnDate != null)
                {
                    ReturnDate = ReturnDate.Date;
                }
            }
            catch { ReturnDate = DateTime.Now; }
            return ReturnDate;
        }
        public static string DoFormat(decimal myNumber)
        {
            var s = string.Format("{0:0.00}", myNumber);

            if (s.EndsWith("00"))
            {
                return ((long)myNumber).ToString();
            }
            else
            {
                return s;
            }
        }
        public static string GetStringValue(object objTest)
        {
            try
            {
                return (objTest != null) && (!object.ReferenceEquals(objTest, System.DBNull.Value)) ? objTest.ToString() : "";
            }
            catch (Exception ex) { }
            return "";
        }
        public static string GetDateTimeInFormat(DateTime? date, string format = null)
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
        public static DateTime GetFirstDayOfMonth(DateTime dtDate)
        {
            DateTime dtFrom = dtDate;
            dtFrom = dtFrom.AddDays(-(dtFrom.Day - 1));
            dtFrom = new DateTime(dtFrom.Year, dtFrom.Month, dtFrom.Day, 0, 0, 0);
            return dtFrom;
        }
        public static DateTime GetLastDayOfMonth(DateTime dtDate)
        {
            DateTime dtTo = new DateTime(dtDate.Year, dtDate.Month, 1);
            dtTo = dtTo.AddMonths(1);
            dtTo = dtTo.AddDays(-(dtTo.Day));
            dtTo = new DateTime(dtTo.Year, dtTo.Month, dtTo.Day, 23, 59, 59);
            return dtTo;
        }
        public static bool ValidateStringByCondition(string value1, string value2, ConditionType conditionType)
        {
            bool retValue = true;
            value1 = value1 != null ? value1.ToLower() : "";
            value2 = value2 != null ? value2.ToLower() : "";
            switch (conditionType)
            {
                case ConditionType.EQUAL:
                    retValue = value1.Equals(value2);
                    break;
                case ConditionType.CONTAINS:
                    retValue = value1.Contains(value2);
                    break;
                default:
                    retValue = true;
                    break;
            }
            return retValue;
        }
        public static bool IsDateNull(Nullable<DateTime> dt)
        {
            if (dt == null || dt.Equals(DateTime.MinValue))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void LogToConsole(string message)
        {
            Console.WriteLine(message);
        }
        public static void WriteLogToFile(string message)
        {
            //string directory = Context.AppContext.AppConstant.DOWNLOAD_ERROR_LOG_PATH;
            //var directoryName = System.IO.Path.GetDirectoryName(directory);
            //string fileName = directoryName + "WINITLog_" + DateTime.Now.Date.ToString("dd_MMM_yyyy") + ".txt";
            //if (!System.IO.Directory.Exists(directoryName))
            //    System.IO.Directory.CreateDirectory(directoryName);
            //System.IO.File.WriteAllText(fileName, Environment.NewLine + message);
        }

        public Nest.QueryContainer CreateElasticsearchQuery(List<Winit.Shared.Models.Enums.FilterCriteria> filterCriterias)
        {
            var mustQueries = new List<Nest.QueryContainer>();
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                foreach (var criteria in filterCriterias)
                {
                    Nest.QueryContainer query = null;

                    string fieldNameInCamelCase = ToCamelCase(criteria.Name);

                    switch (criteria.Type)
                    {
                        case Winit.Shared.Models.Enums.FilterType.Equal:
                            query = new Nest.MatchQuery
                            {
                                Field = fieldNameInCamelCase,
                                Query = CommonFunctions.GetStringValue(criteria.Value)
                            };
                            break;

                        case Winit.Shared.Models.Enums.FilterType.NotEqual:
                            query = new Nest.BoolQuery
                            {
                                MustNot = new List<Nest.QueryContainer>
                        {
                            new Nest.MatchQuery
                            {
                                Field = fieldNameInCamelCase,
                                Query = CommonFunctions.GetStringValue(criteria.Value)
                            }
                        }
                            };
                            break;

                        case Winit.Shared.Models.Enums.FilterType.Like:
                            query = new Nest.WildcardQuery
                            {
                                Field = fieldNameInCamelCase,
                                Value = $"*{criteria.Value}*",
                                // CaseInsensitive = true
                            };
                            break;
                    }

                    if (query != null)
                    {
                        mustQueries.Add(query);
                    }
                }
            }

            var boolQuery = new Nest.BoolQuery
            {
                Must = mustQueries
            };

            return boolQuery;
        }

        private string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            char firstChar = char.ToLower(input[0]);
            string camelCase = firstChar + input.Substring(1);

            return camelCase;
        }
        public static object ParseValueToModelFieldDataType<T>(string fieldName, string fieldValueAsString) where T : class
        {
            // Get the field information based on the field name
            //        var propertyInfo = typeof(T).GetInterfaces()
            //.SelectMany(interfaceType => interfaceType.GetProperties())
            //.FirstOrDefault(p => p.Name == fieldName);

            var propertyInfo = typeof(T).GetProperty(fieldName);
            if (propertyInfo != null)
            {
                // Get the data type of the field
                var fieldType = propertyInfo.PropertyType;
                try
                {
                    // Parse the string value to the appropriate data type
                    if (fieldType == typeof(string))
                    {
                        return fieldValueAsString;
                    }
                    object parsedValue = Convert.ChangeType(fieldValueAsString, fieldType);
                    // For reference types, explicitly cast to the property type
                    if (!fieldType.IsValueType)
                    {
                        parsedValue = fieldType.IsAssignableFrom(parsedValue.GetType()) ? parsedValue : null;
                    }
                    return parsedValue;
                }
                catch (Exception ex)
                {
                    // Handle conversion errors (e.g., invalid format)
                    Console.WriteLine($"Error converting value for field '{fieldName}': {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        private static Timer debounceTimer;

        public static void Debounce<T>(Action<T> action, T parameter, TimeSpan debounceDelay)
        {
            // Reset the timer on every input
            debounceTimer?.Dispose();

            // Create a new timer with the specified delay
            debounceTimer = new Timer(state => action.Invoke(parameter), null, debounceDelay, Timeout.InfiniteTimeSpan);
        }
        public static List<dynamic> ToDynamic(DataTable dt)
        {
            var dynamicDt = new List<dynamic>();
            foreach (DataRow row in dt.Rows)
            {
                dynamic dyn = new ExpandoObject();
                dynamicDt.Add(dyn);
                foreach (DataColumn column in dt.Columns)
                {
                    var dic = (IDictionary<string, object>)dyn;
                    dic[column.ColumnName] = row[column];
                }
            }
            return dynamicDt;
        }
        public IEnumerable<T> ReadJsonFile<T>(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string jsonContent = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<IEnumerable<T>>(jsonContent);
                }
                else
                {
                    // Handle file not found or other errors
                    return Enumerable.Empty<T>();
                }
            }
            catch (Exception ex)
            {
                // Handle deserialization errors
                throw new Exception("Error reading JSON file.", ex);
            }
        }
        public static List<ISelectionItem> ConvertToSelectionItems<T>(List<T>? Items, List<string>? fieldNames = null)
        {
            List<ISelectionItem> selectionItems = new List<ISelectionItem>();
            if (Items != null && Items.Any())
            {
                foreach (var item in Items)
                {
                    ISelectionItem selectionItem = new SelectionItem();
                    selectionItem.UID = item?.GetPropertyValue<string>(fieldNames != null ? fieldNames[0] : "UID") ?? string.Empty;
                    selectionItem.Code = item?.GetPropertyValue<string>(fieldNames != null ? fieldNames[1] : "Code");
                    selectionItem.Label = item?.GetPropertyValue<string>(fieldNames != null ? fieldNames[2] : "Label");
                    selectionItem.ExtData = item?.GetPropertyValue<string>(fieldNames != null ? fieldNames.Count == 4 ? fieldNames[3] : string.Empty : "ExtData");
                    selectionItems.Add(selectionItem);
                }
            }
            return selectionItems;
        }
        public static List<ISelectionItem> ConvertToSelectionItems<T>(List<T> Items, Func<T, string> uid, Predicate<T> isSelected, Func<T, string>? code = null,
            Func<T, string>? label = null, Func<T, object>? extData = null)
        {
            return Items.Select(e =>
            {
                return new SelectionItem
                {
                    UID = uid(e),
                    Code = code(e) ?? string.Empty,
                    Label = label(e) ?? string.Empty,
                    IsSelected = isSelected(e),
                    ExtData = extData?.Invoke(e)
                };
            }).ToList<ISelectionItem>();
        }
        public static List<ISelectionItem> ConvertToSelectionItems<T>(List<T> Items, Func<T, string> uid, Func<T, string>? code = null,
            Func<T, string>? label = null, Func<T, object>? extData = null)
        {
            return Items.Select(e =>
            {
                return new SelectionItem
                {
                    UID = uid.Invoke(e),
                    Code = code?.Invoke(e),
                    Label = label?.Invoke(e),
                    ExtData = extData?.Invoke(e)
                };
            }).ToList<ISelectionItem>();
        }
        public static void SavesDataToFile(string jsonData, string folderPath, string fileName)
        {
            // Get the current date and time
            DateTime now = DateTime.Now;

            // Ensure that the directory exists
            Directory.CreateDirectory(folderPath);

            // Combine the directory path with the file name to get the full file path
            string filePath = Path.Combine(folderPath, fileName);

            // Write the JSON content to the file
            File.WriteAllText(filePath, jsonData);
        }
        public static void LogSyncLogJsonToFile(string jsonData, string identifier, string folderName)
        {
            try
            {
                // Get the current date and time
                DateTime now = DateTime.Now;

                // Create the directory path based on the current date and the employee UID
                string directoryPath = Path.Combine("Data", folderName, now.Year.ToString(), now.Month.ToString(), now.Day.ToString(), identifier);

                // Construct the file name using EmployeeUID and current timestamp
                string fileName = $"{identifier}_{now:yyyyMMddHHmmssfff}.json";

                CommonFunctions.SavesDataToFile(jsonData, directoryPath, fileName);
            }
            catch
            {
                // Don't return any exception
            }
        }

        //    public static void DownloadPDF()
        //    {
        //        var converter = new SynchronizedConverter(new PdfTools());

        //        // HTML string content
        //        var htmlContent = "<html><body><h1>Hello, World!</h1></body></html>";

        //        // Convert HTML string to PDF
        //        var doc = new HtmlToPdfDocument()
        //        {
        //            GlobalSettings = {
        //    ColorMode = ColorMode.Color,
        //    Orientation = Orientation.Portrait,
        //    PaperSize = PaperKind.A4,
        //},
        //            Objects = {
        //    new ObjectSettings() {
        //        PagesCount = true,
        //        HtmlContent = htmlContent,
        //        WebSettings = { DefaultEncoding = "utf-8" },
        //        HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
        //        FooterSettings = { FontSize = 9, Line = true, Center = "Footer" }
        //    }
        //}
        //        };

        //        // Convert to PDF
        //        var pdfBytes = converter.Convert(doc);

        //        // Save PDF to file or send it as a response
        //        File.WriteAllBytes("example.pdf", pdfBytes);
        //    }
        public static async Task SaveBase64StringToFile(string base64String, string folderPath, string fileName)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64String)))
            {
                using (FileStream fs = new FileStream(Path.Combine(folderPath, fileName), FileMode.Create))
                {
                    await ms.CopyToAsync(fs);
                }
            }
        }
        public static List<dynamic>? ConvertJTokenListToType(IServiceProvider serviceProvider, List<dynamic> list, string modelName)
        {
            Type? type = Type.GetType(modelName);
            if (type == null)
            {
                return null;
            }
            Type? targetType = serviceProvider.GetRequiredService(type).GetType();
            if (targetType == null)
            {
                return null;
            }

            var convertedList = new List<dynamic>();

            foreach (var jToken in list)
            {
                // Convert JToken to the target type
                var convertedItem = ((JToken)jToken).ToObject(targetType);
                if (convertedItem == null)
                {
                    continue;
                }
                convertedList.Add(convertedItem);
            }

            return convertedList;
        }
        public static int GetYearMonth(DateTime? dateTime)
        {
            if (dateTime == null || dateTime == DateTime.MinValue)
            {
                dateTime = DateTime.Now;
            }
            return CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(dateTime, "yyMM"));
        }

        public static string FormatNumber(decimal number)
        {

            if (number % 1 == 0)
            {
                return ((int)number).ToString();
            }
            else
            {
                return number.ToString();
            }

        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            try
            {
                // Use a regular expression to validate the email format
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return regex.IsMatch(email);
            }
            catch (Exception)
            {
                return false;
            }
        }
        #region Convert to Json
        public static string ConvertToJson<T>(T data)
        {
            return JsonConvert.SerializeObject(data);

        }
        #endregion


        #region Compare Two Object Values
        public static Dictionary<string, (object? OldValue, object? NewValue)> CompareObjects<T>(T original, T updated, string parentName = "", bool shouldCompareAllField = false)
        {
            try
            {
                var changesDict = new Dictionary<string, (object? OldValue, object? NewValue)>();

                if (original == null && updated == null)
                    return changesDict;

                if (original == null || updated == null)
                {
                    changesDict.Add(parentName, (null, updated));
                    return changesDict;
                }

                if (IsPrimitiveOrSimpleType(original.GetType()) || IsPrimitiveOrSimpleType(updated.GetType()))
                {
                    if (!Equals(original, updated))
                    {
                        changesDict.Add(parentName, (original, updated));
                    }
                    return changesDict;
                }

                // Handle collections (arrays/lists)
                if (original is IEnumerable<object> originalList && updated is IEnumerable<object> updatedList)
                {
                    CompareCollections(originalList, updatedList, parentName, changesDict);
                }
                else
                {
                    var properties = original.GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        var originalValue = property.GetValue(original);
                        var updatedValue = property.GetValue(updated);

                        if (originalValue == null && updatedValue == null) continue;

                        // Get the Column attribute if present
                        var columnAttr = (ColumnAttribute)Attribute.GetCustomAttribute(property, typeof(ColumnAttribute));
                        if (!shouldCompareAllField && columnAttr == null)
                        {
                            continue; // Skip this property and move to the next
                        }
                        var columnName = columnAttr != null ? columnAttr.Name : property.Name;
                        var propertyName = string.IsNullOrEmpty(parentName) ? columnName : $"{parentName}.{columnName}";

                        // Handle JSON or complex types
                        if (IsJson(originalValue) && IsJson(updatedValue))
                        {
                            CompareJsonValues(originalValue, updatedValue, propertyName, changesDict);
                        }
                        // Handle objects/classes
                        else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                        {
                            var nestedChanges = CompareObjects(originalValue, updatedValue, propertyName);
                            AddNestedChanges(nestedChanges, changesDict);
                        }
                        // Handle collections
                        else if (originalValue is IEnumerable<object> origList && updatedValue is IEnumerable<object> updaList)
                        {
                            CompareCollections(origList, updaList, propertyName, changesDict);
                        }
                        // Handle primitive values
                        else if (!Equals(originalValue, updatedValue))
                        {
                            if (string.IsNullOrEmpty(propertyName))
                            {
                                throw new ArgumentException("Property name should not be null or empty.");
                            }
                            changesDict.Add(propertyName, (originalValue, updatedValue));
                        }
                    }
                }

                return changesDict;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return default;
            }
        }



        private static void CompareCollections(IEnumerable<object> originalList, IEnumerable<object> updatedList, string parentName, Dictionary<string, (object OldValue, object NewValue)> changesDict)
        {
            int index = 0;
            foreach (var (origItem, updatedItem) in originalList.Zip(updatedList, Tuple.Create))
            {
                var nestedChanges = CompareObjects(origItem, updatedItem, $"{parentName}[{index}]");
                AddNestedChanges(nestedChanges, changesDict);
                index++;
            }

            // Handle unequal list lengths
            if (originalList.Count() != updatedList.Count())
            {
                changesDict.Add($"{parentName}", ("list length mismatch", "list length mismatch"));
            }
        }

        private static void CompareJsonValues(object originalValue, object updatedValue, string propertyName, Dictionary<string, (object OldValue, object NewValue)> changesDict)
        {
            var originalJsonToken = JToken.Parse(originalValue.ToString());
            var updatedJsonToken = JToken.Parse(updatedValue.ToString());

            if (!JToken.DeepEquals(originalJsonToken, updatedJsonToken))
            {
                changesDict.Add(propertyName, (originalValue.ToString() ?? "null", updatedValue.ToString() ?? "null"));
            }
        }

        private static void AddNestedChanges(Dictionary<string, (object OldValue, object NewValue)> nestedChanges, Dictionary<string, (object OldValue, object NewValue)> changesDict)
        {
            foreach (var change in nestedChanges)
            {
                changesDict.Add(change.Key, change.Value);
            }
        }

        public static bool IsJson(object value)
        {
            if (value is string strValue)
            {
                strValue = strValue.Trim();
                return (strValue.StartsWith("{") && strValue.EndsWith("}")) ||
                       (strValue.StartsWith("[") && strValue.EndsWith("]"));
            }
            return value is JObject || value is JArray;
        }
        private static bool IsPrimitiveOrSimpleType(Type type)
        {
            return type.IsPrimitive || type.IsValueType || type == typeof(string);
        }
        #endregion

        #region ChangeRequest
        public static List<ChangeRecords> GetChangedData(Dictionary<string, (object OldValue, object NewValue)> changes)
        {
            List<ChangeRecords> ChangeRecord = new List<ChangeRecords>();

            foreach (var change in changes)
            {
                ChangeRecord.Add(new ChangeRecords
                {
                    FieldName = change.Key,
                    OldValue = change.Value.OldValue,
                    NewValue = change.Value.NewValue,
                });
            }
            return ChangeRecord;
        }

        #endregion

        public static string FormatNumberInIndianStyle(decimal number, int noOfDecimal = 2, string symbol = "₹ ")
        {
            // Create a new NumberFormatInfo object
            // Create a new NumberFormatInfo object
            NumberFormatInfo nfi = new NumberFormatInfo();

            // Define the grouping for the Indian numbering system
            nfi.NumberGroupSizes = new[] { 3, 2 }; // 3 for the first group, 2 for subsequent groups
            nfi.NumberGroupSeparator = ","; // Use comma as the separator
            nfi.CurrencySymbol = ""; // Remove the currency symbol temporarily (we'll add it manually)

            // Format the number using the custom NumberFormatInfo
            // "{0:C}" formats as currency
            decimal roundOffNumber = decimal.Parse(RoundForSystemWithoutZero(number, noOfDecimal));
            string formattedNumber = roundOffNumber.ToString("N", nfi); // "N" for number format
            string result = (number < 0 ? "-" : "") + symbol + formattedNumber.TrimStart('-'); // Manually handle the sign and prepend the symbol
            return result;
        }
        public static string FormatNumberInIndianStyleWithoutSymbol(decimal number, int noOfDecimal = 2, string symbol = "")
        {
            // Create a new NumberFormatInfo object
            // Create a new NumberFormatInfo object
            NumberFormatInfo nfi = new NumberFormatInfo();

            // Define the grouping for the Indian numbering system
            nfi.NumberGroupSizes = new[] { 3, 2 }; // 3 for the first group, 2 for subsequent groups
            nfi.NumberGroupSeparator = ","; // Use comma as the separator
            nfi.CurrencySymbol = ""; // Remove the currency symbol temporarily (we'll add it manually)

            // Format the number using the custom NumberFormatInfo
            // "{0:C}" formats as currency
            decimal roundOffNumber = decimal.Parse(RoundForSystemWithoutZero(number, noOfDecimal));
            string formattedNumber = roundOffNumber.ToString("N", nfi); // "N" for number format
            string result = (number < 0 ? "-" : "") + symbol + formattedNumber.TrimStart('-'); // Manually handle the sign and prepend the symbol
            return result;
        }
        public static string ConvertToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string result = Regex.Replace(input, "([a-z])([A-Z])", "$1_$2");
            return result.ToLower();
        }
        public string MaskSensitiveData(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            if (input.Contains("@")) // Email Masking
            {
                var parts = input.Split('@');
                if (parts[0].Length <= 3)
                    return new string('*', parts[0].Length) + "@" + parts[1];

                return parts[0].Substring(0, 4) + "********" + "@" + parts[1];
            }
            else if (input.All(char.IsDigit) && input.Length >= 8) // Phone Number Masking
            {
                return input.Substring(0, 2) + new string('*', input.Length - 4) + input.Substring(input.Length - 2);
            }

            return input; // Return as-is if not an email or phone number
        }
        #region Convert true and false into Yes and No
        public static string ConvertBoolToYesNo(bool value)
        {
            return value ? "Yes" : "No";
        }
        #endregion

        #region convert dateTime into dd MM, yyyy HH:mm:ss
        public static string FormatDateTimeTo(DateTime dateTime)
        {
            return dateTime.ToString("dd MM, yyyy HH:mm:ss");
        }
        #endregion


        public async Task ExportToExcelAsync<T>(List<T> data, Dictionary<string, string> columnMappings, string fileName)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sheet1");
                if (data == null || data.Count == 0)
                {
                    var cell = worksheet.Cell(1, 1).SetValue("No Record");
                    cell.Style.Font.FontColor = XLColor.Red;
                    cell.Style.Font.Bold = true;
                }
                else
                {
                    // Get properties of the object dynamically
                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                          .ToDictionary(p => p.Name, p => p);

                    // Add headers based on provided column mappings
                    int colIndex = 1;
                    foreach (var column in columnMappings)
                    {
                        worksheet.Cell(1, colIndex).Value = column.Value; // Excel column name
                        colIndex++;
                    }

                    // Populate data rows
                    for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
                    {
                        colIndex = 1;
                        foreach (var column in columnMappings)
                        {
                            if (properties.TryGetValue(column.Key, out var prop))
                            {
                                var value = prop.GetValue(data[rowIndex]);
                                worksheet.Cell(rowIndex + 2, colIndex).Value = value?.ToString() ?? "";
                            }
                            colIndex++;
                        }
                    }
                }

                // Convert to base64 for Blazor download
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var bytes = stream.ToArray();
                    string base64 = Convert.ToBase64String(bytes);
                    // Trigger download in Blazor using JS Interop
                    await _jSRuntime.InvokeVoidAsync("eval", $"var a = document.createElement('a'); a.href = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,{base64}'; a.download = '{fileName}.xlsx'; a.click();");
                }
            }
        }
        public static int ExtractNumericQuantity(string openingBalance)
        {
            if (string.IsNullOrWhiteSpace(openingBalance))
                return 0;

            // Match a decimal number at the beginning of the string
            var match = Regex.Match(openingBalance.Trim(), @"^\d+(\.\d+)?");

            if (match.Success && decimal.TryParse(match.Value, out decimal result))
            {
                return (int)result; // or use Math.Round(result), if rounding is needed
            }

            return 0;
        }
    }


}
