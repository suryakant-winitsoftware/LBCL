using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreAdditionalInfoCMIRetailingCityMonthlySales:IStoreAdditionalInfoCMIRetailingCityMonthlySales
    {
        public int? Sn { get; set; }
        public string CityName { get; set; }
        public string CitySD1 { get; set; }
        public string CitySD2 { get; set; }
        public string CitySD3 { get; set; }
        public string CitySD4 { get; set; }
        public string CitySD5 { get; set; }
        public string CityAMS1 { get; set; }
        public string CityAMS2 { get; set; }
        public string CityAMS3 { get; set; }
        public string CityAMS4 { get; set; }
        public string CityAMS5 { get; set; }
        public string AvgMonthlySales { get; set; }

    }
    public class StoreAdditionalInfoCMIRetailingCityMonthlySales1 : IStoreAdditionalInfoCMIRetailingCityMonthlySales1
    {
        public int? Sn { get; set; }
        public string CityName { get; set; }
        public string AvgMonthlySales { get; set; }

    }
}
