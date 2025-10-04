using Winit.Modules.Notification.BL.Interfaces.DataServices.General;
using Winit.Modules.Notification.Model.Interfaces;
using Winit.Modules.SMS.DL.Interfaces;
using Winit.Modules.SMS.Model.Classes;
using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.Notification.BL.Classes.DataServices.General
{
    public class NotificationGeneralDataServiceBL : INotificationGeneralDataServiceBL
    {
        private readonly ISMSDL _smsDL;
        public NotificationGeneralDataServiceBL(ISMSDL sMSDL)
        {
            _smsDL = sMSDL;
        }
        public async Task<object> SendSms(INotificationRequest notificationRequest)
        {
            try
            {
                // Logic to fetch data based on the notificationRequest
                Console.WriteLine("Fetching data for request...");
                //Converting into ISmsRequest
                ISms smsRequest = new Sms
                {
                    UID = notificationRequest.UniqueUID,
                    Content = $"Dear Sir/Madam,Please use OTP {notificationRequest.LinkedItemUID} to proceed with registration on CMI- Saarthi.Thank You Carrier Midea India Pvt Ltd.",
                    Sender = "",
                    MessageType = "Transactional",
                    Priority = 1,
                    RequestStatus = "Pending",
                    RequestTime = DateTime.Now,
                    Receivers = new List<Winit.Modules.SMS.Model.Classes.SmsModelReceiver>
                    {
                        new Winit.Modules.SMS.Model.Classes.SmsModelReceiver
                        {
                            Receiver = notificationRequest.Receiver.FirstOrDefault()
                        },
                    }
                };
                await _smsDL.SendOtp(smsRequest);
                return await Task.FromResult(notificationRequest);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        Task<object> INotificationGeneralDataServiceBL.GetDataAsync(INotificationRequest notificationRequest)
        {
            throw new NotImplementedException();
        }
    }
}
