using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;

namespace Winit.Shared.Models.Common
{
    public class CustomException : System.Exception
    {
        public ExceptionStatus Status { get; set; }
        public CustomException() : base()
        {

        }

        public CustomException(ExceptionStatus status, string? message): base(message)
        {
            this.Status = status;
        }

        public CustomException(ExceptionStatus status, string? message, Exception? innerException)
            : base(message, innerException)
        {
            this.Status = status;
        }
    }
}
