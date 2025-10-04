using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerServices.Interfaces
{
    public interface ISalesOrderWorkerService
    {
        Task OnMessageReceived(string obj);
        Task PrepareSalesOrderAsyncWithJSONMessageNew(string message);

    }
}
