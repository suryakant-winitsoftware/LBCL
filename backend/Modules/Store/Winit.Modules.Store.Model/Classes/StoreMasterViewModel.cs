using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Currency.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreMasterViewModel
    {
       public Winit.Modules.Store.Model.Classes.StoreMaster StoreMaster { get; set; }
        public List<StandardListSource> standardListSources { get; set; }
        public StandardListSource selectedOrg { get; set; }
        public List<OrgCurrency> OrgCurrencies { get; set; }
        public OrgCurrency SelectedOrgCurrency { get; set; }


    }

}
