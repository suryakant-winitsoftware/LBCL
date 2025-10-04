using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.Model.Constants;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Services;

namespace WINITMobile.Pages.Startday
{
    partial class MessageOfTheDay
    {
        public string SuccessMessage { get; set; }

        protected override void OnInitialized()
        {
            SuccessMessage = "Success is not a DESTINATION. Success is a JOURNEY";
            LoadResources(null, _languageService.SelectedCulture);
        }

        protected async void Continue()
        {
            try
            {
                if (_appUser.SelectedRoute == null)
                {
                    var routeSelectionItems = CommonFunctions.ConvertToSelectionItems<IRoute>(_appUser.JPRoutes, new List<string>
                { "UID","Code","Name"});
                    if (_appUser.SelectedRoute != null)
                    {
                        routeSelectionItems.Find(e => e.UID == _appUser.SelectedRoute.UID).IsSelected = true;
                    }
                    await _dropdownService.ShowMobilePopUpDropDown(new DropDownOptions
                    {
                        DataSource = routeSelectionItems,
                        OnSelect = async (eventArgs) =>
                        {
                            await HandelRouteSelect(eventArgs);
                        },
                        OkBtnTxt = @Localizer["proceed"],
                        Title = @Localizer["route_selection"]
                    });

                }
                else
                {
                    StartBackgroundSync();
                    NavigateTo("DashBoard");
                }
            }
            catch (Exception ex)
            {
            }
        }


        private async Task HandelRouteSelect(DropDownEvent dropDownEvent)
        {
            try
            {
                await _beatHistoryViewModel.OnRouteDD_Select(dropDownEvent);
                StartBackgroundSync();
                NavigateTo("CustomerList");
            }
            catch (CustomException ex)
            {
                await _alertService.ShowErrorAlert(Enum.GetName(ex.Status), ex.Message);
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["there_is_some_error_while_processing_request."]);
            }
        }

        public void StartBackgroundSync(List<string> tableGroups = null)
        {
            Task.Run(async () =>
            {
                try
                {
                    await SyncDataInBackGround(tableGroups);
                }
                catch (Exception ex)
                {
                    // Log the exception since it won't bubble up
                    // Logger.LogError(ex, "Background sync failed");
                    Console.WriteLine($"Background sync error: {ex.Message}");
                }
            });
        }
        public async Task SyncDataInBackGround(List<string> tableGroups = null)
        {
            bool result = await _imageUploadService.PostPendingImagesToServer();

            // Use passed tableGroups or default to FileSys and Merchandiser
            var groups = tableGroups ?? new List<string>
            {
                DbTableGroup.FileSys,
                DbTableGroup.Master
            };

            // Use the reusable method from base class (no alerts since this is part of checkout flow)
            var res = await UploadDataSilent(groups);
        }
    }
}


