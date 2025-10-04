using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class ContactDetailsModel : BaseModel
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public string PhoneExtension { get; set; }

        public string Mobile { get; set; }
        public string Landline { get; set; }


        public string Email { get; set; }


        public string Email2 { get; set; }


        public string Email3 { get; set; }


        public bool InvoiceForEmail1 { get; set; }


        public bool InvoiceForEmail2 { get; set; }


        public bool InvoiceForEmail3 { get; set; }


        public string Fax { get; set; }


        public string LinkedItemUID { get; set; }


        public string LinkedItemType { get; set; }


        public bool IsDefault { get; set; }


        public bool IsEditable { get; set; }


        public bool EnabledForInvoiceEmail { get; set; }


        public bool EnabledForDocketEmail { get; set; }


        public bool EnabledForPromoEmail { get; set; }


        public bool IsEmailCC { get; set; }


        public string Mobile2 { get; set; }

    }
}
