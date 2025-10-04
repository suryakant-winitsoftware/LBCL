using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Calender.Models.Interfaces;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Common.BL.Classes
{
    public class AppUser : Interfaces.IAppUser
    {
        public bool IsCheckedIn { get; set; }
        public bool IsCustomerCallPage { get; set; }
        public string DefaultLocationTypeUID { get; set; }
        public bool IsDashBoardPage { get; set; }
        public bool IsPreviousBeatOpen { get; set; }
        public DateTime JourneyStartDate { get; set; }
        public List<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> AvailableJobPositionList { get; set; }
        public bool IsMultiCurrency { get; set; } = true;
        public UserType UserType { get; set; }
        //srinadh
        public List<Winit.Modules.Common.Model.Interfaces.IMyRole> MyRoles { get; set; }
        public Winit.Modules.Common.Model.Interfaces.IMyRole SelectedRole { get; set; }
        public List<Winit.Modules.Vehicle.Model.Interfaces.IVehicleStatus> Vehicles { get; set; }
        public Winit.Modules.Vehicle.Model.Interfaces.IVehicleStatus Vehicle { get; set; }
        public Winit.Modules.Org.Model.Interfaces.IOrg SelectedOrg { get; set; }
        public List<Winit.Modules.Org.Model.Interfaces.IOrg> OrgList { get; set; }
        public List<string> OrgUIDs { get; set; } = new List<string>();
        public List<ISelectionItem>? ProductDivisionSelectionItems { get; set; }
        public Winit.Modules.Org.Model.Interfaces.IOrg CurrentOrg { get; set; }
        //public Winit.Modules.Currency.Model.Interfaces.ICurrency OrgCurrency { get; set; }
        //public List<Winit.Modules.Currency.Model.Interfaces.ICurrency> OrgCurrencyList { get; set; }
        //public List<Winit.Modules.Currency.Model.Interfaces.ICurrency> CurrencyList { get; set; }
        //public List<Winit.Modules.Currency.Model.Interfaces.ExchangeRate> ExchangeRateList { get; set; }
        //public Winit.Modules.Currency.Model.Interfaces.ExchangeRate DefaultExchangeRate { get; set; } // Populate this based on FromCurrencyID = OrgCurrency.UID
        public string OrgSeqCode { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRoute> JPRoutes { get; set; }
        // niranjan 
        public Winit.Modules.Store.Model.Interfaces.IStoreItemView SelectedCustomer { get; set; }
        public Winit.Modules.JourneyPlan.Model.Interfaces.IJPBeatHistory SelectedBeatHistory { get; set; }
        public List<Winit.Modules.JourneyPlan.Model.Interfaces.IJPBeatHistory> BeatHistories { get; set; }
        public Winit.Modules.Route.Model.Interfaces.IRoute SelectedRoute { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRoute> Routes { get; set; }

        public Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney UserJourney { get; set; }

        public DateTime CurrentJpDate { get; set; } = DateTime.Today.Date;

        public Emp.Model.Interfaces.IEmp Emp { get; set; }
        public Emp.Model.Interfaces.IEmpInfo EmpInfo { get; set; }

        public Winit.Modules.JobPosition.Model.Interfaces.IJobPosition SelectedJobPosition { get; set; }
        public List<Winit.Modules.Currency.Model.Interfaces.IOrgCurrency> OrgCurrencyList { get; set; }
        public Winit.Modules.Currency.Model.Interfaces.IOrgCurrency DefaultOrgCurrency { get; set; }

        public string StartDayStatus { get; set; }
        public bool IsRemoteCollection { get; set; }
        public string RemoteCollectionReason { get; set; }
        public Dictionary<string, List<ISelectionItem>> ReasonDictionary { get; set; }
        public IRole Role { get; set; }
        public bool IsBeatStartedFirstTime { get; set; }
        public int AlreadyVisitedCustomerCountForRoute { get; set; }
        public int AlreadyCollectedLoadReqeustCountForRoute { get; set; }
        public DateTime CurrentUserDate { get; set; }
        public string UserJourneyVehicleUID { get; set; }
        public bool IsAllRouteJPRCompleted { get; set; }
        public Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion> DMSPromotionDictionary { get; set; }
        public List<string> ApplicablePromotionUIDs { get; set; }
        public Dictionary<string, List<string>> StorePromotionMapDictionary { get; set; }
        public Dictionary<string, Tax.Model.Interfaces.ITax> TaxDictionary { get; set; }
        public string DefaultBatchNumber { get; set; } = "B1";
        public string DefaultStockVersion { get; set; } = "V1";
        //end
        public List<ICalender> CalenderPeriods { get; set; }
        public List<string>? AsmDivisions { get; set; }
        public List<IApprovalRuleMaster> ApprovalRuleMaster { get; set; }
        public void PopulateAppUser(Emp.Model.Interfaces.IEmp emp, UserType userType,
            List<JobPosition.Model.Interfaces.IJobPosition> availableJobPositionList, JobPosition.Model.Interfaces.IJobPosition selectedJobPositionList)
        {
            Emp = emp;
            UserType = userType;
            AvailableJobPositionList = availableJobPositionList;
            SelectedJobPosition = selectedJobPositionList;
        }
        public async Task SetStartDayAction()
        {
            string startDayStatus = Model.Constants.StartDayStatus.BLANK;
            if (UserJourney == null)
            {
                startDayStatus = Model.Constants.StartDayStatus.START_DAY;
            }
            else if (!CommonFunctions.IsDateNull(UserJourney.JourneyStartTime) && CommonFunctions.IsDateNull(UserJourney.JourneyEndTime))
            {
                if (SelectedJobPosition.HasEOT == true)
                {
                    if (CommonFunctions.GetTimeDifferenceInDay(UserJourney.JourneyStartTime.Value, CurrentUserDate) == 0)
                    {
                        //StartDay == today
                        startDayStatus = Model.Constants.StartDayStatus.CONTINUE;
                    }
                    else if (CommonFunctions.GetTimeDifferenceInDay(UserJourney.JourneyStartTime.Value, CurrentUserDate) > 0)
                    {
                        //StartDay < today
                        startDayStatus = Model.Constants.StartDayStatus.EOT;
                    }
                }
                else
                {
                    startDayStatus = Model.Constants.StartDayStatus.CONTINUE;
                }
            }
            else if (!CommonFunctions.IsDateNull(UserJourney.JourneyStartTime) && !CommonFunctions.IsDateNull(UserJourney.JourneyEndTime))
            {
                if (CommonFunctions.GetTimeDifferenceInDay(UserJourney.JourneyStartTime.Value, CurrentUserDate) == 0)
                {
                    //StartDay == today
                    startDayStatus = Model.Constants.StartDayStatus.EOT_DONE;
                }
                else if (CommonFunctions.GetTimeDifferenceInDay(UserJourney.JourneyStartTime.Value, CurrentUserDate) > 0)
                {
                    //StartDay < today
                    startDayStatus = Model.Constants.StartDayStatus.START_DAY;
                }
            }
            else
            {
                startDayStatus = Model.Constants.StartDayStatus.BLANK;
            }
            StartDayStatus = startDayStatus;
            await Task.CompletedTask;
        }
    }
}
