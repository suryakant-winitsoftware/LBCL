using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Winit.Modules.Base.BL
{
    public class AppExp : System.ApplicationException
    {
        public Winit.Shared.Models.EventArguments.RestServiceEventArgs ExceptionArgs { get; set; }
        public AppExp(Winit.Shared.Models.EventArguments.RestServiceEventArgs exp)
        {
            ExceptionArgs = exp;
        }
    }
    public class RestClientSync
    {
        public int TimeOut { get; set; }
        public string EndPoint { get; set; }
        public Shared.Models.Enums.HttpVerb Method { get; set; }
        public string ContentType { get; set; }
        public string PostData { get; set; }
        public System.Collections.Specialized.NameValueCollection headers { get; set; }
        public RestClientSync()
        {
            EndPoint = "";
            Method = Shared.Models.Enums.HttpVerb.GET;
            ContentType = "text/xml";
            PostData = "";
            headers = null;
        }
        private Winit.Shared.Models.EventArguments.RestServiceEventArgs MakeRequest()
        {
            return MakeRequest("");
        }
        private Winit.Shared.Models.EventArguments.RestServiceEventArgs MakeRequest(string parameters)
        {
            JObject serviceResponse = null;
            Winit.Shared.Models.EventArguments.RestServiceEventArgs restServiceEventArgs = null;
            var responseValue = string.Empty;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);

                request.Method = Method.ToString();

                //request.ContentLength = 0;
                request.ContentType = ContentType;
                request.Headers.Add(headers);
                request.Timeout = TimeOut;//120000;

                if (!string.IsNullOrEmpty(PostData) && Method == Shared.Models.Enums.HttpVerb.POST)
                {
                    var encoding = new System.Text.UTF8Encoding();
                    //var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);
                    var bytes = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(PostData);
                    request.ContentLength = bytes.Length;
                    request.Timeout = TimeOut;// 120000;

                    using (var writeStream = request.GetRequestStream())
                    {
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var message = String.Format("Received HTTP {0}", response.StatusCode);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                        restServiceEventArgs = new Winit.Shared.Models.EventArguments.RestServiceEventArgs(false, false, message, response.StatusCode, new ApplicationException(message), null, null);
                        //throw new AppExp(restServiceEventArgs);
                    }
                    else
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            if (responseStream != null)
                                using (var reader = new System.IO.StreamReader(responseStream))
                                {
                                    responseValue = reader.ReadToEnd();
                                }
                        }
                        if (responseValue != null && responseValue != "null")
                        {
                            serviceResponse = JsonConvert.DeserializeObject<JObject>(responseValue);
                            if (serviceResponse != null)
                            {
                                if (serviceResponse["ResponseStatus"] != null)
                                {
                                    Winit.Shared.Models.Common.CommonResponse commonResponse = serviceResponse["ResponseStatus"].ToObject<Winit.Shared.Models.Common.CommonResponse>();
                                    if (commonResponse != null && commonResponse.StatusCode == Winit.Shared.Models.Enums.ResponseStatus.Success)
                                    {
                                        restServiceEventArgs = new Winit.Shared.Models.EventArguments.RestServiceEventArgs(true, true, message, response.StatusCode, null, responseValue, commonResponse);
                                        return restServiceEventArgs;
                                    }
                                    else if (commonResponse != null && commonResponse.StatusCode == Winit.Shared.Models.Enums.ResponseStatus.AnotherDeviceLogin)
                                    {
                                        message = String.Format("Error message : {0},{1}", commonResponse.StatusMessage, commonResponse.Message);
                                        restServiceEventArgs = new Winit.Shared.Models.EventArguments.RestServiceEventArgs(true, false, message, response.StatusCode, null, responseValue, commonResponse);
                                        //throw new AppExp(restServiceEventArgs);
                                    }
                                    else if (commonResponse != null && commonResponse.StatusCode == Winit.Shared.Models.Enums.ResponseStatus.Failure)
                                    {
                                        message = String.Format("Error message : {0},{1}", commonResponse.StatusMessage, commonResponse.Message);
                                        restServiceEventArgs = new Winit.Shared.Models.EventArguments.RestServiceEventArgs(true, false, message, response.StatusCode, null, responseValue, commonResponse);
                                        //throw new AppExp(restServiceEventArgs);
                                    }
                                }
                                else
                                {
                                    restServiceEventArgs = new Winit.Shared.Models.EventArguments.RestServiceEventArgs(true, true, message, response.StatusCode, null, responseValue, null);
                                    return restServiceEventArgs;
                                }
                            }
                            else
                            {
                                restServiceEventArgs = new Winit.Shared.Models.EventArguments.RestServiceEventArgs(false, false, "No response", HttpStatusCode.NoContent, null, responseValue, null);
                                //throw new AppExp(restServiceEventArgs);
                            }
                        }
                        else
                        {
                            restServiceEventArgs = new Winit.Shared.Models.EventArguments.RestServiceEventArgs(false, false, "No response", HttpStatusCode.NoContent, null, responseValue, null);
                            //throw new AppExp(restServiceEventArgs);
                        }
                    }
                }
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.Timeout)
            {
                restServiceEventArgs = new Winit.Shared.Models.EventArguments.RestServiceEventArgs(false, false, ex.Message, HttpStatusCode.RequestTimeout, ex, null, null);
                return restServiceEventArgs;
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.NameResolutionFailure)
            {
                restServiceEventArgs = new Winit.Shared.Models.EventArguments.RestServiceEventArgs(false, false, "Please reset internet connection and retry, still problem persist Contact Support.", HttpStatusCode.RequestTimeout, ex, null, null);
                return restServiceEventArgs;
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.ConnectFailure)
            {
                restServiceEventArgs = new Winit.Shared.Models.EventArguments.RestServiceEventArgs(false, false, "Please check internet connection and retry", HttpStatusCode.FailedDependency, ex, null, null);
                return restServiceEventArgs;
            }
            catch (Exception ex)
            {
                restServiceEventArgs = new Winit.Shared.Models.EventArguments.RestServiceEventArgs(false, false, ex.Message, HttpStatusCode.InternalServerError, ex, null, null);
                return restServiceEventArgs;
            }
            restServiceEventArgs = new Winit.Shared.Models.EventArguments.RestServiceEventArgs(true, true, "", HttpStatusCode.OK, null, responseValue, null);
            return restServiceEventArgs;
        }
        public Winit.Shared.Models.EventArguments.RestServiceEventArgs PostRestService(string serviceName, Shared.Models.Enums.HttpVerb methodType, string jsonData, System.Collections.Specialized.NameValueCollection headers,
            string BasePath = "", int timeOut = 60000)
        {
            if (headers == null)
            {
                headers = new System.Collections.Specialized.NameValueCollection();
            }
            if (string.IsNullOrEmpty(BasePath))
            {
                this.EndPoint = "" + serviceName; 
            }
            else
            {
                this.EndPoint = BasePath + serviceName; 
            }

            this.Method = methodType; //HttpVerb.POST;
            this.ContentType = "application/json";
            this.PostData = jsonData;
            this.headers = headers;
            this.TimeOut = timeOut;
            return this.MakeRequest();
        }
    }
}
