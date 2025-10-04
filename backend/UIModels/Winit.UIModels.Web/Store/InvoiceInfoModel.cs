using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class InvoiceInfoModel :BaseModel
    {
        [Required(ErrorMessage = "Invoice Frequency is required.")]
        public string InvoiceFrequency { get; set; }

        [Required(ErrorMessage = "Ageing Cycle is required.")]
        public string AgeingCycle { get; set; }

        [Required(ErrorMessage = "Invoice Format is required.")]
        public string InvoiceFormat { get; set; }

        [Required(ErrorMessage = "Invoice Delivery Method is required.")]
        public string InvoiceDeliveryMethod { get; set; }

        public bool DisplayDeliveryDocket { get; set; }
        public bool ShowCustPO { get; set; }
        public bool DisplayPrice { get; set; }

        
        public string AssetEnabled { get; set; }

      
        public string SurveyEnabled { get; set; }

       
        public string Depot { get; set; }

        [Required(ErrorMessage = "Bank Account is required.")]
        public string BankAccount { get; set; }

        [Required(ErrorMessage = "Drawer is required.")]
        public string Drawer { get; set; }

       
        public string EducationServicesLtd { get; set; }

       
        public string Bank { get; set; }

       
        public string BankOfNewZealand { get; set; }

        public DateTime? InvoiceStartDate { get; set; }

        [Required(ErrorMessage = "Invoice Text is required.")]
        public string InvoiceText { get; set; }

        public DateTime? InvoiceEndDate { get; set; }
    }
}
