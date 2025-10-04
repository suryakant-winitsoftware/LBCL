using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.Notification.BL.Interfaces.Common
{
    public interface INotificationSMSServiceBL
    {
        Task SendSMSAsync(ISms emailData);
    }
}
