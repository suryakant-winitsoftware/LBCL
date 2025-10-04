using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class StoreRendererModel
    {
        public bool IsCustomerInformationRendered { get; set; } 
        public bool IsContactDetailsRendered { get; set; } 
        public bool IsContactPersonDetailsRendered { get; set; } 
        public bool IsBillToAddressRendered { get; set; } 
        public bool IsShipToAddressRendered { get; set; }
        public bool IsOrganisationConfigurationRendered { get; set; }
        public bool IsAwayPeriodRendered { get; set; } 
        public bool IsDocumentRendered { get; set; }
        public bool IsAdditionalRendered { get; set; }
    }
}
