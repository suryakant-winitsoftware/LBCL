using Microsoft.AspNetCore.Components;
using Winit.Modules.Base.Model;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.JourneyPlan.Model.Classes
{
    public class JPBeatHistory:BeatHistory,IJPBeatHistory
    {
        public string Status { get; set; }
        public bool IsLastRoute { get; set; }
        public string ActionButtonTextLabel { get; set; }
        public string ActionButtonText { get; set; }
        public string ExecutionStatus { get; set; }
        public DateTime JourneyStartDate { get; set; }
        
        public void UpdatedMyStatus()
        {
            // Status Update
            if (CommonFunctions.IsDateNull(StartTime))
            {
                Status = CommonConstant.NOT_STARTED;
            }
            if (!CommonFunctions.IsDateNull(StartTime) && CommonFunctions.IsDateNull(EndTime))
            {
                Status = CommonConstant.NOT_COMPLETED;
            }
            else if (!CommonFunctions.IsDateNull(StartTime) && !CommonFunctions.IsDateNull(EndTime))
            {
                Status = CommonConstant.COMPLETED;
            }
            UpdatedMyExecutionStatus();
        }
        public void UpdatedMyExecutionStatus()
        {
            // Execution Status Update
            if (Status.Equals(CommonConstant.NOT_COMPLETED) && VisitDate.Date < JourneyStartDate/*DateTime.Now.Date*/)
            {
                ExecutionStatus = CommonConstant.CONFIRM;
                ActionButtonText = CommonConstant.STOP;
                ActionButtonTextLabel = CommonConstant.STARTED; //"Started";
                /* 
                 * Show Stop Button
                 * In case user click on list to see customer open popup for user confirmation with message "Do you want to continue or close this beat". 
                 * If User will select continue then update ExecutionStatus = "Enabled"
                 * If User will select close then close the Beat by updating EndDate as Current Date and Update ExecutionStatus = "Completed"
                 * Refresh View
                 */
            }
            else if (Status.Equals(CommonConstant.NOT_COMPLETED) && VisitDate.Date == JourneyStartDate/*DateTime.Now.Date*/)
            {
                ExecutionStatus = CommonConstant.ENABLED;
                ActionButtonText = CommonConstant.STOP;
                ActionButtonTextLabel = CommonConstant.STARTED; // "Started";
                /*
                 * Show Stop Button
                 * On Stop Refresh View
                 */
            }
            else if (Status.Equals(CommonConstant.NOT_STARTED) && VisitDate.Date == JourneyStartDate.Date /*DateTime.Now.Date*/)
            {
                ExecutionStatus = CommonConstant.READY;
                ActionButtonText = CommonConstant.START;
                ActionButtonTextLabel = CommonConstant.NOT_STARTED;// "Not Started";
                /*
                 * Show Start Button
                 */
            }
            else if (Status.Equals(CommonConstant.COMPLETED) && VisitDate.Date == JourneyStartDate /*DateTime.Now.Date*/)
            {
                ExecutionStatus = CommonConstant.DISABLED;
                ActionButtonText = CommonConstant.BLANK;
                ActionButtonTextLabel = CommonConstant.COMPLETED; //"Completed";
                /*
                 * Don't show any button
                 */
            }
            else
            {
                ExecutionStatus = CommonConstant.DISABLED;
                ActionButtonText = CommonConstant.BLANK;
                ActionButtonTextLabel = CommonConstant.BLANK; //"Completed";
                //ActionButtonTextLabel = CommonConstant.COMPLETED; //"Completed";
                /*
                 * Don't show any button
                 */
            }
        }
    }
}
