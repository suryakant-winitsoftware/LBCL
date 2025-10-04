using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Classes;
using Winit.Modules.SalesOrder.Model.UIClasses;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.BL.UIClasses
{
    public class SalesOrderFilter :FilterHelper
    {
        public List<ISalesOrderItemView> ApplyAttributeFilter(List<ISalesOrderItemView> salesOrderItemView, string attributeName, List<FilterCriteria> attributeValues)
        {
            if (attributeValues != null && attributeValues.Count > 0)
            {
                List<object> selectedValues = attributeValues.Select(e => e.Value).ToList();
                return salesOrderItemView.Where(e => e.Attributes.ContainsKey(attributeName) && selectedValues.Contains(e.Attributes[attributeName].Code)).ToList();
            }
            return salesOrderItemView;
        }
    }
}
