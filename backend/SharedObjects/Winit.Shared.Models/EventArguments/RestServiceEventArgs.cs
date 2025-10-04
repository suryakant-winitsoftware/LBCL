using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Winit.Shared.Models.Common;

namespace Winit.Shared.Models.EventArguments
{
    public class RestServiceEventArgs : EventArgs
    {
        public bool IsSuccess { get; set; }
        public bool ContinueFurther { get; set; }
        public string Message { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public Exception RestException { get; set; }
        public string RestResponse { get; set; }
        public CommonResponse CommonResponse { get; set; }

        public RestServiceEventArgs(bool isSuccess, bool continuefuther, string message, HttpStatusCode httpStatusCode,
            Exception exception, string response, CommonResponse commonResponse)
        {
            this.IsSuccess = isSuccess;
            this.ContinueFurther = continuefuther;
            this.Message = message;
            this.HttpStatusCode = httpStatusCode;
            this.RestException = exception;
            this.RestResponse = response;
            this.CommonResponse = commonResponse;

        }
    }
}
