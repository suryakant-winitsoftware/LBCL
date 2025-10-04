using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.RabbitMQQueue.BL.Interfaces
{
    public interface IRabbitMQLogBL
    {
        Task<int> InsertAppRequestInfo(Winit.Modules.Syncing.Model.Interfaces.IAppRequest appRequest);
        Task<int> UpdateLogByStepAsync(string UID, string Step, bool StepResult, bool IsFailed, string comments);
        Task PostToRabbitMQQueue(List<Winit.Modules.Syncing.Model.Interfaces.IAppRequest> appRequests);
    }
}
