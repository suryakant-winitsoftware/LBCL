using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Common
{
    public class CommonResponse
    {
        public CommonResponse()
        {
            StatusCode = Enums.ResponseStatus.Failure;
            StatusMessage = CommonConstant.FAILURE_TEXT;
        }
        public Enums.ResponseStatus StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Message { get; set; }
    }
}
