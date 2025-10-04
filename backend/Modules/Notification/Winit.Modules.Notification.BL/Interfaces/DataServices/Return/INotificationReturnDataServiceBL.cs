using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Notification.Model.Interfaces;

namespace Winit.Modules.Notification.BL.Interfaces.DataServices.Return
{
    public interface INotificationReturnDataServiceBL
    {
        Task<object> GetDataAsync(INotificationRequest notificationRequest);
    }
}
