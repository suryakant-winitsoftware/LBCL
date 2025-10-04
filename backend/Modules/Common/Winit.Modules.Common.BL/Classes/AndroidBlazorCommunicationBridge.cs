using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Common.BL.Classes
{
    public class AndroidBlazorCommunicationBridge : IAndroidBlazorCommunicationBridge
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        // Method to send message from platform-specific code to Blazor
        public void SendMessageToBlazor(string title, string body, string imageUrl, string notificationType, string relativePath)
        {
            // Raise the MessageReceived event
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(title, body, imageUrl, notificationType, relativePath));
        }
    }
}
