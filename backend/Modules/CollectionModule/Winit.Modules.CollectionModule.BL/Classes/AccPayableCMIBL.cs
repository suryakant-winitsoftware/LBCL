using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.DL.Classes;
using Winit.Modules.CollectionModule.DL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Modules.Store.DL.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Winit.Modules.CollectionModule.BL.Classes
{
    public class AccPayableCMIBL : Interfaces.IAccPayableCMIBL
    {
        protected readonly DL.Interfaces.IAccPayableCMIDL _accPayableCMIDL;

        public AccPayableCMIBL(DL.Interfaces.IAccPayableCMIDL accPayableCMIDL)
        {
            _accPayableCMIDL = accPayableCMIDL;
        }

       public async Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableCMI>> GetAccPayableCMIDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string jobPositionUID)
        {
            return await _accPayableCMIDL.GetAccPayableCMIDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired,jobPositionUID);
        }
        public async Task<Model.Interfaces.IAccPayableMaster> GetAccPayableMasterByUID(string uID)
        {
            return await _accPayableCMIDL.GetAccPayableMasterByUID(uID);
        }
        public async Task<List<OutstandingInvoiceView>> OutSTandingInvoicesByStoreCode(string storeCode, int pageNumber, int pageSize)
        {
            return await _accPayableCMIDL.OutSTandingInvoicesByStoreCode( storeCode,pageNumber,pageSize);
        }

    }

}
