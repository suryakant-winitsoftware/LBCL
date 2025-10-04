using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.Syncing.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models;
using Winit.Modules.RabbitMQQueue.Model.Classes;
using Winit.Modules.Syncing.Model.Interfaces;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.IdentityModel.Tokens;

namespace WINITMobile.Pages.Notification
{
    public partial class NotificationHandler : ComponentBase
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();
            _androidBlazorCommunicationBridge.MessageReceived -= _androidBlazorCommunicationBridge_MessageReceived;
            _androidBlazorCommunicationBridge.MessageReceived += _androidBlazorCommunicationBridge_MessageReceived;
        }

        private async void _androidBlazorCommunicationBridge_MessageReceived(object sender, Winit.Modules.Common.BL.Classes.MessageReceivedEventArgs e)
        {
            switch (e.NotificationType)
            {
                case NotificationType.Alert:
                    _ = InvokeAsync(() =>
                    {
                        _alertService.ShowErrorAlert(e.Title, e.Body);
                        StateHasChanged(); // Ensure UI reflects changes
                    });
                    break;
                case NotificationType.Navigate:
                    _navigationManager.NavigateTo(e.RelativePath);
                    break;
                case NotificationType.NavigateWithAlert:
                    _ = InvokeAsync(() =>
                    {
                        _alertService.ShowErrorAlert(e.Title, e.Body, NotificationAfterNotificationRead, "Ok", e.RelativePath);
                        StateHasChanged(); // Ensure UI reflects changes
                    });
                    break;
                case NotificationType.InternalUploadStatus:
                    _ = InvokeAsync(async () =>
                    {
                        try
                        {
                            //_alertService.ShowErrorAlert(e.Title, e.Body, NotificationAfterNotificationRead, "Ok", e.RelativePath);
                            //StateHasChanged(); // Ensure UI reflects changes
                            TrxStatusDco trxStatusDco = JsonConvert.DeserializeObject<TrxStatusDco>(e.Body);
                            if (trxStatusDco == null || trxStatusDco.Status == 0 || string.IsNullOrEmpty(trxStatusDco.ResponseUID))
                            {
                                return;
                            }
                            List<IAppRequest> appRequests = (List<IAppRequest>)await _appRequestBL.SelectAppRequestByUID(new List<string> { trxStatusDco.ResponseUID });
                            if (appRequests == null || appRequests.Count == 0)
                            {
                                return;
                            }
                            IAppRequest appRequest = appRequests.FirstOrDefault();
                            Dictionary<string, List<string>>? requestUIDDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(appRequest.RequestUIDs);
                            if (requestUIDDictionary == null || requestUIDDictionary.Count == 0)
                            {
                                return;
                            }
                            await _mobileDataSyncDL.UpdateSSForUIDs(requestUIDDictionary, SSValues.Zero);
                            await _appRequestBL.UpdateAppRequest_IsNotificationReceived(new List<string> { appRequest.UID}, true);
                        }
                        catch (Exception ex)
                        {
                            await _alertService.ShowErrorAlert("Error", ex.Message, NotificationAfterNotificationRead, "Ok", e.RelativePath);
                            StateHasChanged(); // Ensure UI reflects changes
                        }
                    });
                    break;
                default:
                    break;
            }

        }
        async Task NotificationAfterNotificationRead(object obj)
        {
            if(obj == null || string.IsNullOrEmpty((string)obj))
            {
                return;
            }
            _navigationManager.NavigateTo(CommonFunctions.GetStringValue(obj));
            await Task.CompletedTask;
            // Your asynchronous code here
        }
        // Dispose method
        public void Dispose()
        {
            _androidBlazorCommunicationBridge.MessageReceived -= _androidBlazorCommunicationBridge_MessageReceived;
        }
    }
}
