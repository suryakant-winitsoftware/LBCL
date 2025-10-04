using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class RouteDeliveryProfileModel
    {
        [Required(ErrorMessage = "Default Route is required.")]
        public string DefaultRun { get; set; }

        [Required(ErrorMessage = "Building Delivery Code is required.")]
        public string BuildingDeliveryCode { get; set; }

        [Required(ErrorMessage = "Delivery Information is required.")]
        public string DeliveryInformation { get; set; }

        public bool IsStopDelivery { get; set; }

        public bool IsTemperatureCheck { get; set; }
    }
}
