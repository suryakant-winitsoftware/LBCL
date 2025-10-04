using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.RabbitMQQueue.DL.Interfaces
{
    public interface IRabbitMQLogDL
    {
        Task<int> InsertAppRequestInfo(Winit.Modules.Syncing.Model.Interfaces.IAppRequest appRequest);
        Task<int> UpdateLogByStepAsync(string UID, string Step, bool StepResult, bool IsFailed, string comments);
    }
}
