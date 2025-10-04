using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Common.BL.Classes
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public string Title { get; }
        public string Body { get; }
        public string ImageUrl { get; }
        public string NotificationType { get; }
        public string RelativePath { get; }

        public MessageReceivedEventArgs(string title, string body, string imageUrl, string notificationType, string relativePath)
        {
            Title = title;
            Body = body;
            ImageUrl = imageUrl;
            NotificationType = notificationType;
            RelativePath = relativePath;
        }
    }
}
