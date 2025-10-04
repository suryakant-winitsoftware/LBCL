using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL.Classes;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Common.BL.Interfaces
{
    public interface IAndroidBlazorCommunicationBridge
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public void SendMessageToBlazor(string title, string body, string imageUrl, string notificationType, string relativePath);
    }
}
