using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Notification.Model.Interfaces;

namespace Winit.Modules.Notification.BL.Interfaces.DataServices.General
{
    public interface INotificationGeneralDataServiceBL
    {
        Task<object> SendSms(INotificationRequest notificationRequest);
        Task<object> GetDataAsync(INotificationRequest notificationRequest);
    }
}
