using Microsoft.AspNetCore.Components;
using Winit.Shared.Models.Enums;

namespace Winit.UIComponents.Common
{
    public class AlertService : IAlertService
    {
        public event Action<AlertOptions> OnShowAlert;

        public async Task ShowAlert(string heading, string body, string button1Text, string button1Class, Func<object,Task> button1Action, 
            string button2Text, string button2Class, Func<object, Task> button2Action, string button3Text, string button3Class, 
            Func<object, Task> button3Action, object obj = null)
        {
            var options = new AlertOptions
            {
                Heading = heading,
                Body = body,
                Button1Text = button1Text,
                Button1Class = button1Class,
                Button1Action = button1Action,
                Button2Text = button2Text,
                Button2Class = button2Class,
                Button2Action = button2Action,
                Button3Text = button3Text,
                Button3Class = button3Class,
                Button3Action = button3Action,
                AlertType = AlertTypes.Info,
                BackgroundColor = "alert-info",
                data = obj
            };

            OnShowAlert?.Invoke(options);
        }

        public async Task ShowSuccessAlert(string heading, string body, Func<object, Task> okAction, string btnOkText = "Ok", object obj = null)
        {
            var options = new AlertOptions
            {
                Heading = heading,
                Body = body,
                AlertType = AlertTypes.Success,
                BackgroundColor = "alert-success",
                Button1Text = btnOkText,
                Button1Action = okAction,
                data = obj
            };

            OnShowAlert?.Invoke(options);
        }

        public async Task ShowErrorAlert(string heading, string body, Func<object, Task> okAction, string btnOkText = "Ok", object obj = null)
        {
            var options = new AlertOptions
            {
                Heading = heading,
                Body = body,
                AlertType = AlertTypes.Error,
                BackgroundColor = "bg-warning",
                Button1Text = btnOkText,
                Button1Action = okAction,
                data = obj
            };

            OnShowAlert?.Invoke(options);
        }

        public async Task ShowConfirmationAlert(string heading, string body, Func<object, Task> confirmAction, string btnYesText = "Yes", 
            string btnNoText = "No", object obj = null)
        {
            var options = new AlertOptions
            {
                Heading = heading,
                Body = body,
                Button1Text = btnYesText,
                Button1Class = "btn-primary",
                Button1Action = confirmAction,
                Button2Text = btnNoText,
                Button2Class = "btn-secondary",
                AlertType = AlertTypes.Warning,
                BackgroundColor = "alert-danger",
                data = obj
            };

            OnShowAlert?.Invoke(options);
        }

        public  Task<bool> ShowConfirmationReturnType(string heading, string body,  string btnYesText = "Yes", string btnNoText = "No",
            object obj = null)
        {
            var tcs = new TaskCompletionSource<bool>();

            var options = new AlertOptions
            {
                Heading = heading,
                Body = body,
                Button1Text = btnYesText,
                Button1Class = "btn-primary",
                Button1Action = async (param) =>
                {
                   
                    tcs.SetResult(true);
                },
                Button2Text = btnNoText,
                Button2Class = "btn-secondary",
                Button2Action = (param) =>
                {
                    tcs.SetResult(false);
                    return Task.CompletedTask;
                },
                AlertType = AlertTypes.Warning,
                BackgroundColor = "alert-danger",
                data = obj
            };

            OnShowAlert?.Invoke(options);

            return  tcs.Task;
        }


    }
}
