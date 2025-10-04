using System;
using System.Collections.Generic;
using System.Text;

namespace Winit.Modules.Base.BL
{
    public abstract class RestBase : BaseBL
    {
        public string _basePath = string.Empty;
        public RestBase(Model.RestBaseModel model, string basePath): base(model) {
            _basePath = basePath;
        }
        protected (bool Result, T t) RestPost<T>(string url, string request)
        {
            return RestPost<T>(url, request, 60000);
        }
        protected (bool Result, T t) RestPost<T>(string url, string request, int timeout)
        {
            return RestPost<T>(url, request, _basePath, timeout);
        }
        protected (bool Result, T t) RestPost<T>(string url, string request, string basepath)
        {
            return RestPost<T>(url, request, basepath, null, 60000);
        }
        protected (bool Result, T t) RestPost<T>(string url, string request, string basepath, int timeout)
        {
            return RestPost<T>(url, request, basepath, null, timeout);
        }
        protected (bool Result, T t) RestPost<T>(string url, string request, System.Collections.Specialized.NameValueCollection headers)
        {
            return RestPost<T>(url, request, _basePath, headers, 60000);
        }
        protected (bool Result, T t) RestPost<T>(string url, string request, string basepath, System.Collections.Specialized.NameValueCollection headers, int timeOut)
        {
            RestClientSync restClient = new RestClientSync();
            var resp = restClient.PostRestService(url, Shared.Models.Enums.HttpVerb.POST,
                    request, headers, basepath);
            if (!resp.IsSuccess)
            {
                _model.IsValid = false;
                _model.ErrorMessage = resp.Message;
                return (false, default(T));
            }
            return (true, Newtonsoft.Json.JsonConvert.DeserializeObject<T>(resp.RestResponse));
        }
        protected (bool Result, Winit.Shared.Models.EventArguments.RestServiceEventArgs t) RestPost(string url, string request)
        {
            return RestPost(url, request, _basePath);
        }
        protected (bool Result, Winit.Shared.Models.EventArguments.RestServiceEventArgs t) RestPost(string url, string request, int timeout)
        {
            return RestPost(url, request, _basePath, timeout);
        }
        protected (bool Result, Winit.Shared.Models.EventArguments.RestServiceEventArgs t) RestPost(string url, string request, string basepath)
        {
            return RestPost(url, request, basepath, 60000);
        }
        protected (bool Result, Winit.Shared.Models.EventArguments.RestServiceEventArgs t) RestPost(string url, string request, string basepath, int timeout)
        {
            return RestPost(url, request, basepath, null, timeout);
        }
        protected (bool Result, Winit.Shared.Models.EventArguments.RestServiceEventArgs t) RestPost(string url, string request, System.Collections.Specialized.NameValueCollection headers)
        {
            return RestPost(url, request, _basePath, headers, 60000);
        }
        protected (bool Result, Winit.Shared.Models.EventArguments.RestServiceEventArgs t) RestPost(string url, string request, string basepath, System.Collections.Specialized.NameValueCollection headers, int timeout)
        {
            RestClientSync restClient = new RestClientSync();
            var resp = restClient.PostRestService(url, Shared.Models.Enums.HttpVerb.POST,
                    request, headers, basepath, timeout);
            if (!resp.IsSuccess)
            {
                _model.IsValid = false;
                _model.ErrorMessage = resp.Message;
                return (false, resp);
            }
            return (true, resp);
        }
    }
}
