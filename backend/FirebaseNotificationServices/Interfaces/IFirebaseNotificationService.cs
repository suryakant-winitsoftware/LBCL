using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseNotificationServices.Interfaces
{    
    public interface IFirebaseNotificationService
    {
        void SetFCMToken(string fcmToken);

        Task SendNotificationAsync(string title, string body, string token);

        Task SendNotificationAsync(string messageUid, string title, string body, string token);
    }

}
