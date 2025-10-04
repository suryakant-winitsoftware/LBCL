using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Int_InsertDataInQueue.Model.Interfaces;

namespace Winit.Modules.Int_InsertDataInQueue.BL.Interfaces
{
    public interface IInsertDataInQueueBL
    {
        Task<int> InsertDataInQueue( IApiRequest Request);
    }
}
