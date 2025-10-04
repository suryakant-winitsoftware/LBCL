using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace Winit.UIModels.Common.Product
{

    public class StoreDataModel
    {


        [Required]
        public string Code { get; set; }

        [Required]
        public string Number { get; set; }

        public string Name { get; set; }

        [Required]
        public string AliasName { get; set; }

        [Required]
        public string LegalName { get; set; }

        [Required]
        public string Type { get; set; }



        [Required]
        public int Status { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public string StoreClass { get; set; }

        [Required]
        public string StoreRating { get; set; }

        [Required]
        public bool IsBlocked { get; set; }

        [Required]
        public string BlockedReasonCode { get; set; }

        [Required]
        public string BlockedReasonDescription { get; set; }



        [Required]
        public string Source { get; set; }

        public object storeCredit { get; set; } // Replace "object" with the actual data type if known

        [Required]
        public string vk { get; set; }

        public object vk2 { get; set; } // Replace "object" with the actual data type if known

        [Required]
        public int Id { get; set; }



        [Required]
        public int SS { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedTime { get; set; }

        [Required]
        public string ModifiedBy { get; set; }

        [Required]
        public DateTime ModifiedTime { get; set; }

        [Required]
        public DateTime ServerAddTime { get; set; }

        [Required]
        public DateTime ServerModifiedTime { get; set; }
    }
    public class MaintainProductClass
    {
    }
}
