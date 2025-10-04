using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Interfaces
{
    public interface IQPSSchemeDL
    {
        
        Task<List<IQPSSchemePO>> GetQPSSchemesByStoreUIDAndSKUUID(string orgUid, DateTime order_date, List<SKUFilter> filters);
        Task<List<IQPSSchemePO>> GetQPSSchemesByPOUID(string pouid, List<SKUFilter> filters);



    }
}
