using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Common
{
    public class Validation
    {
        public bool IsValidated { get; set; }
        public string ErrorMessage { get; set; }
        public Validation(bool isValidated, string errorMessage=null) 
        {
            this.IsValidated = isValidated;
            this.ErrorMessage = errorMessage;
        }

    }
}
