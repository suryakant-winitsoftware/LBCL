using System;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;

namespace Winit.UIComponents.Common
{
    public interface IAlertService
    {
        Task ShowAlert(string heading, string body, string button1Text, string button1Class, Func<object, Task> button1Action = null, string button2Text = "", 
            string button2Class = "", Func<object, Task> button2Action = null, string button3Text = "", string button3Class = "", 
            Func<object, Task> button3Action = null, object obj = null);

        Task ShowSuccessAlert(string heading, string body, Func<object, Task> okAction = null, string btnOkText = "Ok", object obj = null);

        Task ShowErrorAlert(string heading, string body, Func<object, Task> okAction = null, string btnOkText = "Ok", object obj = null);

        Task ShowConfirmationAlert(string heading, string body, Func<object, Task> confirmAction = null, 
            string btnYesText = "Yes", string btnNoText = "No", object obj = null);

        public Task<bool> ShowConfirmationReturnType(string heading, string body, string btnYesText = "Yes", string btnNoText = "No", object? obj = null);

        event Action<AlertOptions> OnShowAlert;
    }

    public class AlertOptions
    {
        public string Heading { get; set; }
        public string Body { get; set; }
        public string Button1Text { get; set; }
        public string Button1Class { get; set; }
        public Func<object,Task> Button1Action { get; set; }
        public string Button2Text { get; set; }
        public string Button2Class { get; set; }
        public Func<object, Task> Button2Action { get; set; }
        public string Button3Text { get; set; }
        public string Button3Class { get; set; }
        public Func<object, Task> Button3Action { get; set; }
        public AlertTypes AlertType { get; set; } = AlertTypes.Success;
        public string BackgroundColor { get; set; } = "bg-success";
        public object data { get; set; }
    }
}

