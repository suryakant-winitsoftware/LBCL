using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Notification.BL.Interfaces;
using Winit.Modules.Notification.BL.Interfaces.DataServices.Return;
using Winit.Modules.Notification.Model.Interfaces;

namespace Winit.Modules.Notification.BL.Classes.DataServices.Return
{
    public class NotificationReturnDataServiceBL : INotificationReturnDataServiceBL
    {
        public async Task<object> GetDataAsync(INotificationRequest notificationRequest)
        {
            // Logic to fetch data based on the notificationRequest
            Console.WriteLine("Fetching data for request...");
            return await Task.FromResult(notificationRequest);
        }
    }
}
