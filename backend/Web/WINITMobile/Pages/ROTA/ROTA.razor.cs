using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using WINITMobile.Pages.Base;
using Winit.Modules.Merchandiser.BL.Interfaces;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace WINITMobile.Pages.ROTA
{
    public partial class ROTA : BaseComponentBase
    {
        [Inject]
        private IROTAActivityBL RotaActivityBL { get; set; }

        [Parameter]
        public string EmpUID { get; set; }

        [Parameter]
        public string JobPositionUID { get; set; }

        private List<ScheduleInfo> weekSchedule;
        private DateTime startOfWeek;
        private DateTime endOfWeek;
        private bool isLoading = true;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadScheduleData();
        }

        private async Task LoadScheduleData()
        {
            try
            {
                isLoading = true;
                errorMessage = null;

                // Calculate week range
                var today = DateTime.Now.Date;
                startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1); // Start from Monday
                endOfWeek = startOfWeek.AddDays(6); // End on Sunday

                // Initialize empty schedule for the week
                weekSchedule = Enumerable.Range(0, 7)
                    .Select(offset => new ScheduleInfo 
                    { 
                        Date = startOfWeek.AddDays(offset),
                        ActivityType = "No Activity" // Default value
                    })
                    .ToList();

                

                if (string.IsNullOrEmpty(JobPositionUID))
                {
                    JobPositionUID = _appUser.SelectedJobPosition?.UID;
                }

                // Fetch ROTA activities for the week
                var rotaActivities = await RotaActivityBL.GetByJobPositionAndDateRange(
                    JobPositionUID,
                    startOfWeek,
                    endOfWeek
                );

                // Update schedule with actual activities
                foreach (var activity in rotaActivities)
                {
                    var scheduleItem = weekSchedule.FirstOrDefault(s => s.Date.Date == activity.RotaDate.Date);
                    if (scheduleItem != null)
                    {
                        scheduleItem.ActivityType = activity.RotaActivityName;
                        scheduleItem.RotaActivityUID = activity.UID;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading schedule: {ex.Message}";
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        public async Task RefreshSchedule()
        {
            await LoadScheduleData();
        }
    }

    public class ScheduleInfo
    {
        public DateTime Date { get; set; }
        public string ActivityType { get; set; }
        public string RotaActivityUID { get; set; }
    }
} 