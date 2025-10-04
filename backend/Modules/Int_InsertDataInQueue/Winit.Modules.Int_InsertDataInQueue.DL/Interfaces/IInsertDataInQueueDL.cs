using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Int_InsertDataInQueue.Model.Interfaces;

namespace Winit.Modules.Int_InsertDataInQueue.DL.Interfaces
{
    public interface IInsertDataInQueueDL  
    {
        Task<int> InsertDataInQueue(IApiRequest Request);
    }
}
