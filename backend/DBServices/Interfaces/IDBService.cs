using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FirebaseReport.Models.Interfaces;
using Winit.Shared.Models;

namespace DBServices.Interfaces
{
    public interface IDBService
    {
        Task<int> UpdateLogByStepAsync(string UID, string Step, bool StepResult, bool IsFailed, string comments);
        Task<string> GenerateLogUID(string linkeditemuid, string linkeditemtype, string body, string? usercode = null, string? customercode = null, string? UID = null);
        Task<int> LogNotificationSent(string ReqUID, string linkeditemuid, string linkeditemtype, string title, string body, DateTime ondate);
        Task<string> MoveLogToRepost(string messageUID);
        Task<Dictionary<string, dynamic>> UPSertLogs(List<LogEntry> logList);
    }
}
