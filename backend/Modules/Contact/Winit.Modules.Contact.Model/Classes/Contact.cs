using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Contact.Model.Interfaces;

namespace Winit.Modules.Contact.Model.Classes
{
    //public class Contact: BaseModel,IContact
    //{
    //    public string Title { get; set; }
    //    public string Name { get; set; }
    //    public string Phone { get; set; }
    //    public string PhoneExtension { get; set; }
    //    public string Description { get; set; }
    //    public string Designation { get; set; }
    //    public string Mobile { get; set; }
    //    public string Email { get; set; }
    //    public string Email2 { get; set; }
    //    public string Email3 { get; set; }
    //    public bool InvoiceForEmail1 { get; set; }
    //    public bool InvoiceForEmail2 { get; set; }
    //    public bool InvoiceForEmail3 { get; set; }
    //    public string Fax { get; set; }
    //    public string LinkedItemUID { get; set; }
    //    public string LinkedItemType { get; set; }
    //    public bool IsDefault { get; set; }
    //    public bool IsEditable { get; set; }
    //    public bool EnabledForInvoiceEmail { get; set; }
    //    public bool EnabledForDocketEmail { get; set; }
    //    public bool EnabledForPromoEmail { get; set; }
    //    public bool IsEmailCC { get; set; }
    //    public string Mobile2 { get; set; }
    //    public string Type { get; set; }
    //    public string SectionName { get; set; }

    //}

    public class Contact : BaseModel, IContact
    {
        [Column("title")]
        public string Title { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [Column("phone_extension")]
        public string PhoneExtension { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("designation")]
        public string Designation { get; set; }

        [Column("mobile")]
        public string Mobile { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("email2")]
        public string Email2 { get; set; }

        [Column("email3")]
        public string Email3 { get; set; }

        [Column("invoice_for_email1")]
        public bool InvoiceForEmail1 { get; set; }

        [Column("invoice_for_email2")]
        public bool InvoiceForEmail2 { get; set; }

        [Column("invoice_for_email3")]
        public bool InvoiceForEmail3 { get; set; }

        [Column("fax")]
        public string Fax { get; set; }

        [Column("linked_item_uid")]
        public string LinkedItemUID { get; set; }

        [Column("linked_item_type")]
        public string LinkedItemType { get; set; }

        [Column("is_default")]
        public bool IsDefault { get; set; }

        [Column("is_editable")]
        public bool IsEditable { get; set; }

        [Column("enabled_for_invoice_email")]
        public bool EnabledForInvoiceEmail { get; set; }

        [Column("enabled_for_docket_email")]
        public bool EnabledForDocketEmail { get; set; }

        [Column("enabled_for_promo_email")]
        public bool EnabledForPromoEmail { get; set; }

        [Column("is_email_cc")]
        public bool IsEmailCC { get; set; }

        [Column("mobile2")]
        public string Mobile2 { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("section_name")]
        public string SectionName { get; set; }

    }
}
