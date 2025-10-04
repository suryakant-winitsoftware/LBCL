using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.util;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class QPSSchemeBL : IQPSSchemeBL
    {
        IQPSSchemeDL _qpsSchemeDL;
       
        public QPSSchemeBL(IQPSSchemeDL qpsSchemeDL)
        {
            _qpsSchemeDL = qpsSchemeDL;
           
        }
        
        public async Task<List<IQPSSchemePO>> GetQPSSchemesByStoreUIDAndSKUUID(string storeUid, DateTime order_date, List<SKUFilter> filters)
        {
            return await _qpsSchemeDL.GetQPSSchemesByStoreUIDAndSKUUID(storeUid, order_date, filters);
        }
        public async Task<List<IQPSSchemePO>> GetQPSSchemesByPOUID(string pouid, List<SKUFilter> filters)
        {
            return await _qpsSchemeDL.GetQPSSchemesByPOUID(pouid, filters);
        }

    }
}
