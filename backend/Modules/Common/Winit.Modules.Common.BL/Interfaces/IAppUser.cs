using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Calender.Models.Interfaces;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Common.BL.Interfaces
{
    public interface IAppUser
    {
        public bool IsCheckedIn { get; set; }

        // by niranjan - this is for check-out
        public bool IsCustomerCallPage { get; set; }
        public bool IsMultiCurrency { get; set; }
        string DefaultLocationTypeUID { get; set; }
        public bool IsDashBoardPage { get; set; }
        public bool IsPreviousBeatOpen { get; set; }
        public DateTime JourneyStartDate { get; set; }

        public List<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> AvailableJobPositionList { get; set; }

        public Shared.Models.Enums.UserType UserType { get; set; }
        public List<Winit.Modules.Common.Model.Interfaces.IMyRole> MyRoles { get; set; }
        public Winit.Modules.Common.Model.Interfaces.IMyRole SelectedRole { get; set; }
        public List<Winit.Modules.Vehicle.Model.Interfaces.IVehicleStatus> Vehicles { get; set; }
        public Winit.Modules.Vehicle.Model.Interfaces.IVehicleStatus Vehicle { get; set; }
        public List<Winit.Modules.Org.Model.Interfaces.IOrg> OrgList { get; set; }
        public Winit.Modules.Org.Model.Interfaces.IOrg CurrentOrg { get; set; }
        //public Winit.Modules.Currency.Model.Interfaces.ICurrency OrgCurrency { get; set; }
        //public List<Winit.Modules.Currency.Model.Interfaces.ICurrency> OrgCurrencyList { get; set; }
        //public List<Winit.Modules.Currency.Model.Interfaces.ICurrency> CurrencyList { get; set; }

        public string OrgSeqCode { get; set; }
        public string StartDayStatus { get; set; }
        public string RemoteCollectionReason { get; set; }
        public bool IsRemoteCollection { get; set; }

        void PopulateAppUser(Emp.Model.Interfaces.IEmp emp, UserType userType, List<JobPosition.Model.Interfaces.IJobPosition> availableJobPositionList, JobPosition.Model.Interfaces.IJobPosition selectedJobPositionList);

        // niranjan 
        public Winit.Modules.Store.Model.Interfaces.IStoreItemView SelectedCustomer { get; set; }
        public Winit.Modules.JourneyPlan.Model.Interfaces.IJPBeatHistory SelectedBeatHistory { get; set; }
        public List<Winit.Modules.JourneyPlan.Model.Interfaces.IJPBeatHistory> BeatHistories { get; set; }
        public Winit.Modules.Route.Model.Interfaces.IRoute SelectedRoute { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRoute> Routes { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRoute> JPRoutes { get; set; }

        public Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney UserJourney { get; set; }

        public DateTime CurrentJpDate { get; set; }

        public Emp.Model.Interfaces.IEmp Emp { get; set; }
        public Emp.Model.Interfaces.IEmpInfo EmpInfo { get; set; }

        public Winit.Modules.JobPosition.Model.Interfaces.IJobPosition SelectedJobPosition { get; set; }

        public List<Currency.Model.Interfaces.IOrgCurrency> OrgCurrencyList { get; set; }
        public Currency.Model.Interfaces.IOrgCurrency DefaultOrgCurrency { get; set; }
        public List<string> OrgUIDs { get; set; }
        public List<ISelectionItem>? ProductDivisionSelectionItems { get; set; }
        public Dictionary<string, List<ISelectionItem>> ReasonDictionary { get; set; }
        IRole Role { get; set; }
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
        public string DefaultBatchNumber { get; set; }
        public string DefaultStockVersion { get; set; }
        List<ICalender> CalenderPeriods { get; set; }
        List<string>? AsmDivisions { get; set; }
        public Task SetStartDayAction();
        public List<IApprovalRuleMaster> ApprovalRuleMaster { get; set; }
    }
}
