using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class QPSSchemeProducts : IQPSSchemeProducts
    {
        public int Id { get; set; }
        public string PromoOrderItemUID { get; set; }
        public string OrderType { get; set; }
        public string OrderTypeCode { get; set; }
        public string SKUGroupType { get; set; }
        public string SKUGroupTypeCode { get; set; }
        public string SelectedValue { get; set; }
        public string SelectedValueCode { get; set; }
        public bool IsNewProduct {  get; set; }
        public bool IsSKU {  get; set; }
        public ActionType ActionType { get; set; }
    }
}
