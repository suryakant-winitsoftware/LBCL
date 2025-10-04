using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class CustomerCreationExpirationModel
    {
        public DateTime? CustomerStartDate { get; set; }
        [Required (ErrorMessage ="Created by is Required")]
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public DateTime? CustomerEndDate { get; set; }
    }
}
