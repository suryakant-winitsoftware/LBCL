using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Int_InsertDataInQueue.BL.Interfaces;
using Winit.Modules.Int_InsertDataInQueue.Model.Interfaces;

namespace Winit.Modules.Int_InsertDataInQueue.BL.Classes
{
    public class InsertDataInQueueBL : IInsertDataInQueueBL
    {
        protected readonly DL.Interfaces.IInsertDataInQueueDL _InsertDataInQueueRepository = null;
        public InsertDataInQueueBL(DL.Interfaces.IInsertDataInQueueDL InsertDataInQueueRepository)
        {
            _InsertDataInQueueRepository = InsertDataInQueueRepository;
        }
        public async Task<int> InsertDataInQueue(IApiRequest Request)
        {
            return await _InsertDataInQueueRepository.InsertDataInQueue(Request);
        }
    }
}
