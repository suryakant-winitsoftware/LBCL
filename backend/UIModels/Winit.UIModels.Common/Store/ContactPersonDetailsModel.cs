using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Common.Store
{
    public class ContactPersonDetailsModel : BaseModel
    {
        public string Title { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "PhoneExtension is required.")]
        public string PhoneExtension { get; set; }


        public string Description { get; set; }


        public string Designation { get; set; }

        [Required(ErrorMessage = "Mobile is required.")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }


        public string Email2 { get; set; }


        public string Email3 { get; set; }


        public bool InvoiceForEmail1 { get; set; }


        public bool InvoiceForEmail2 { get; set; }


        public bool InvoiceForEmail3 { get; set; }

        [Required(ErrorMessage = "Fax Number is required.")]
        public string Fax { get; set; }


        public string LinkedItemUID { get; set; }


        public string LinkedItemType { get; set; }


        public bool IsDefault { get; set; }


        public bool IsEditable { get; set; }


        public bool EnabledForInvoiceEmail { get; set; }


        public bool EnabledForDocketEmail { get; set; }


        public bool EnabledForPromoEmail { get; set; }


        public bool IsEmailCC { get; set; }

        [Required(ErrorMessage = "Additional Mobiles Number is required.")]
        public string Mobile2 { get; set; }
    }
}
