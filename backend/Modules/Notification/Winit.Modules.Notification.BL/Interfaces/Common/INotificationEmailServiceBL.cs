using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Email.Model.Interfaces;

namespace Winit.Modules.Notification.BL.Interfaces.Common
{
    public interface INotificationEmailServiceBL
    {
        Task SendEmailAsync(IMailRequest mailRequest);
    }
}
