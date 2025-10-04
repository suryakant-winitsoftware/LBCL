using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface IQPSSchemeBL
    {
        Task<List<IQPSSchemePO>> GetQPSSchemesByStoreUIDAndSKUUID(string storeUid, DateTime order_date, List<SKUFilter> filters);
        Task<List<IQPSSchemePO>> GetQPSSchemesByPOUID(string pouid, List<SKUFilter> filters);


    }
}
