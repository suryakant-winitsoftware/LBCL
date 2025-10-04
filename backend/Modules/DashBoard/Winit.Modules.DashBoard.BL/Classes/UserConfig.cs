using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Currency.BL.Interfaces;
using Winit.Modules.Emp.BL.Classes;
using Winit.Modules.Emp.BL.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JobPosition.BL.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Currency.BL.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Role.BL.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Modules.User.Model.Classes;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.DashBoard.BL.Classes;

public class UserConfig : MobileBase
{
    private readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;
    private readonly IEmpBL _empbl;
    private readonly IEmpInfoBL _empInfobl;
    private readonly Winit.Modules.JourneyPlan.BL.Interfaces.IBeatHistoryBL _beatHistoryBL;
    private readonly Winit.Modules.Currency.BL.Interfaces.ICurrencyBL _currencyBL;
    private readonly Winit.Modules.JobPosition.BL.Interfaces.IJobPositionBL _jobpositionBL;
    private readonly Winit.Modules.Setting.BL.Interfaces.ISettingBL _settingBL;
    private readonly ApiService _apiService;
    private readonly IAppUser _iAppUser;
    private readonly IAppConfig _appConfigs;
    private readonly Winit.Modules.Role.BL.Interfaces.IMenu _menu;
    private readonly Winit.Modules.Promotion.BL.Interfaces.IPromotionBL _promotionBL;
    private readonly Winit.Modules.Tax.BL.Interfaces.ITaxMasterBL _taxMasterBL;
    private readonly IRoleBL _roleBL;
    private readonly Winit.Modules.Org.BL.Interfaces.IOrgBL _orgBL;


    public UserConfig(IAppUser appUser,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        IEmpBL empBL, IEmpInfoBL empInfoBL,
        IAppSetting appSetting,
        IBeatHistoryBL beatHistory,
        ICurrencyBL currencyBL,
        IJobPositionBL jobPositionBL,
        ISettingBL settingBL,
        ApiService apiService,
        IAppUser iAppUser,
        IAppConfig appConfigs,
        Winit.Modules.Role.BL.Interfaces.IMenu menu,
        Winit.Modules.Promotion.BL.Interfaces.IPromotionBL promotionBL,
        Winit.Modules.Tax.BL.Interfaces.ITaxMasterBL taxMasterBL,
        IRoleBL roleBL,
        Winit.Modules.Org.BL.Interfaces.IOrgBL orgBL)
        : base(serviceProvider, configuration)
    {
        _appUser = appUser;
        _appSetting = appSetting;
        _empbl = empBL;
        _empInfobl = empInfoBL;
        _beatHistoryBL = beatHistory;
        _currencyBL = currencyBL;
        _jobpositionBL = jobPositionBL;
        _settingBL = settingBL;
        _apiService = apiService;
        _iAppUser = iAppUser;
        _appConfigs = appConfigs;
        _appConfigs = appConfigs;
        _menu = menu;
        _promotionBL = promotionBL;
        _taxMasterBL = taxMasterBL;
        _roleBL = roleBL;
        _orgBL = orgBL;
    }
    public async Task InitiateData(string loginId)
    {
        try
        {
            //await GetAppuser(loginId);
            //  _appUser.Emp = await _empbl.GetEmpByLoginId(loginId); commented by prem ,Emp getting from GetAppUser
            await SetEmpInfo();
            await LoadSettingMaster();
            //await GetSelectedRoute();
            //await GetSelectedBeatHistory();
            //await SelectedJobPosition();
            await GetSetOrgCurrencyList();
            //await GetReasonsFromListHeader();
            //await SetVehicle();
            await InitializeRoute();
            await SetRole();
            await SetUserJourney();
            _appUser.JourneyStartDate = DateTime.Now;
            await SetApplicablePromotionUIDs();
            await SetDMSPromotionDictionary();
            await SetStorePromotionMap();
            await SetTaxDictionary();
            await SetOrgHierarchy(_appUser.SelectedJobPosition.OrgUID);
            await GetSelectedBeatHistory();
            //await GetCurrencyDetails();
        }
        catch (Exception ex)
        {
        }
    }
    public async Task GetAppuser(string loginId)
    {

        _iAppUser.Emp = await _empbl.GetEmpByLoginId(loginId);
        if (_iAppUser.Emp != null)
        {
            _iAppUser.SelectedJobPosition = await _jobpositionBL.SelectJobPositionByEmpUID(_iAppUser.Emp.UID);
        }
        //_iAppUser.Role = await _roleBL.SelectRolesByUID(_iAppUser.SelectedJobPosition.UserRoleUID);
        // await _menu.PopulateMenuData();

    }
    async Task InitializeRoute()
    {
        _appUser.Routes = await GetRoutes();
        if (_appUser.Routes != null &&
            _appUser.Routes.Any())
        {
            _appUser.SelectedRoute = _appUser.Routes.FirstOrDefault();
        }
    }
    public Task<List<IRoute>> GetRoutes()
    {
        //string query = $@"SELECT * FROM Route JP WHERE 
        //        Is_Active = 1 AND JP.Vehicle_UID = @VehicleUID";
        string query = $@"SELECT * FROM Route R WHERE 
                R.Is_Active = true AND R.Job_Position_UID=@JobPositionUID";
        IDictionary<string, object?> param = new Dictionary<string, object?>
            {
                { "@JobPositionUID", _appUser.SelectedJobPosition?.UID },
                {   "@VehicleUID", _appUser?.Vehicle?.UID }
            };
        return GetList<IRoute>(query, param);
    }


    public async Task GetSetOrgCurrencyList()
    {
        if (_currencyBL != null && _appUser != null && _appUser.SelectedJobPosition != null)
        {
            IEnumerable<Currency.Model.Interfaces.IOrgCurrency> currencyList =
                await _currencyBL.GetOrgCurrencyListBySelectedOrg(_appUser.SelectedJobPosition.OrgUID);
            _appUser.OrgCurrencyList = currencyList.ToList();

            if (_appUser.OrgCurrencyList == null || _appUser.OrgCurrencyList.Count == 0)
            {
                return;
            }
            _appUser.DefaultOrgCurrency = _appUser.OrgCurrencyList.FirstOrDefault(e => e.IsPrimary = true);
        }
        else
        {
            // here handle failure
        }
    }
    public async Task LoadSettingMaster()
    {
        var data = await _settingBL.SelectAllSettingDetails(null, 1, 10000, null, false);
        _appSetting.PopulateSettings(data.PagedData); // Call only once at the time of successful login
    }
    //public async Task GetUserJourney()
    //{
    //    _appUser.UserJourney = await GetSampleUserJourney();
    //}
    public async Task SetUserJourney()
    {
        try
        {
            string query = string.Format(@"SELECT uj.id AS Id, uj.uid AS UID, uj.created_by AS CreatedBy, 
                                    uj.created_time AS CreatedTime, uj.modified_by AS ModifiedBy, 
                                    uj.modified_time AS ModifiedTime, uj.server_add_time AS ServerAddTime,
                                    uj.server_modified_time AS ServerModifiedTime, 
                                    uj.job_position_uid AS JobPositionUID, uj.emp_uid AS EmpUID,
                                    uj.journey_start_time AS JourneyStartTime, uj.journey_end_time 
                                    AS JourneyEndTime, uj.start_odometer_reading AS StartOdometerReading,
                                    uj.end_odometer_reading AS EndOdometerReading, 
                                    uj.journey_time AS JourneyTime, uj.vehicle_uid AS VehicleUID, uj.eot_status
                                    AS EotStatus, uj.reopened_by AS ReopenedBy, uj.has_audit_completed AS
                                    HasAuditCompleted, uj.ss AS SS, uj.beat_history_uid AS BeatHistoryUID,
                                    uj.wh_stock_request_uid AS WhStockRequestUID,
                                    uj.is_synchronizing AS IsSynchronizing, uj.has_internet AS HasInternet,
                                    uj.internet_type AS InternetType, uj.download_speed AS DownloadSpeed,
                                    uj.upload_speed AS UploadSpeed, uj.has_mobile_network AS HasMobileNetwork, 
                                    uj.is_location_enabled AS IsLocationEnabled, uj.battery_percentage_target AS 
                                    BatteryPercentageTarget, uj.battery_percentage_available AS BatteryPercentageAvailable, 
                                    uj.attendance_status AS AttendanceStatus, uj.attendance_latitude AS AttendanceLatitude, 
                                    uj.attendance_longitude AS AttendanceLongitude, uj.attendance_address AS AttendanceAddress
                                    FROM User_Journey uj WHERE uj.Job_Position_UID = '{0}'
                                ORDER BY uj.Journey_Start_Time DESC LIMIT 1",
                                _appUser.SelectedJobPosition.UID);
            _appUser.UserJourney = await ExecuteSingleAsync<IUserJourney>(query);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task GetSelectedRoute()
    {
        _appUser.SelectedRoute = _appUser.Routes?.First();
    }

    public async Task GetSelectedBeatHistory()
    {
        //var beatHistory = await _beatHistoryBL.GetSelectedBeatHistoryByRouteUID(_appUser.SelectedRoute.UID);
        Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"RouteUID", _appUser?.SelectedRoute?.UID}
                };
        //var sql = @"SELECT
        //          Id, UID, Created_By, CreatedTime, ModifiedBy, 
        //          ModifiedTime, ServerAddTime, ServerModifiedTime, UserJourneyUID, 
        //          RouteUID, StartTime, EndTime, JobPositionUID, LoginId, 
        //          VisitDate, LocationUID, PlannedStartTime, PlannedEndTime, 
        //          PlannedStoreVisits, UnPlannedStoreVisits, ZeroSalesStoreVisits, 
        //          MSLStoreVisits, SkippedStoreVisits, ActualStoreVisits, Coverage,
        //          ACoverage, TCoverage, InvoiceStatus, Notes, InvoiceFinalizationDate, 
        //          RouteWHOrgUID, CFDTime, HasAuditCompleted, WHStockAuditUID, ss,
        //          DefaultJobPositionUID, UserJourneyVehicleUID
        //      FROM BeatHistory
        //      WHERE RouteUID = @RouteUID AND date(VisitDate) = date('now')";
        var sql = @"SELECT  
    id, uid, created_by, created_time, modified_by,  
    modified_time, server_add_time, server_modified_time, user_journey_uid,  
    route_uid, start_time, end_time, job_position_uid, login_id,  
    visit_date, location_uid, planned_start_time, planned_end_time,  
    planned_store_visits, unplanned_store_visits, zero_sales_store_visits,  
    msl_store_visits, skipped_store_visits, actual_store_visits, coverage,  
    a_coverage, t_coverage, invoice_status, notes, invoice_finalization_date,  
    route_wh_org_uid, cfd_time, has_audit_completed, wh_stock_audit_uid, ss,  
    default_job_position_uid, user_journey_vehicle_uid  
FROM beat_history  
WHERE route_uid = @RouteUID AND DATE(visit_date) = DATE('now', 'localtime');";
        Winit.Modules.JourneyPlan.Model.Interfaces.IJPBeatHistory selectedBeatHistory = await ExecuteSingleAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IJPBeatHistory>(sql, parameters);

        _appUser.SelectedBeatHistory = selectedBeatHistory;
    }
    public async Task SelectedJobPosition()

    {
        _appUser.SelectedJobPosition = await _jobpositionBL.SelectJobPositionByEmpUID(_appUser.Emp.UID);
    }
    public async Task<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney> GetSampleUserJourney()
    {


        Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney sampleUserJourney = new Winit.Modules.JourneyPlan.Model.Classes.UserJourney
        {
            UID = "UID1",
            JobPositionUID = "Driver2",
            EmpUID = "7ee9f49f-26ea-4e89-8264-674094d805e1",
            JourneyStartTime = DateTime.Now,
            JourneyEndTime = null,
            StartOdometerReading = 1,
            EndOdometerReading = 0,
            JourneyTime = "0",
            EOTStatus = "Pending",
            ReOpenedBy = string.Empty,
            HasAuditCompleted = false,
            BeatHistoryUID = null,
            WHStockRequestUID = null

        };

        return await Task.FromResult(sampleUserJourney);
    }

    private async Task SetEmpInfo()
    {
        string sql = "SELECT * FROM Emp_Info WHERE Emp_UID = @EmpUID";
        IDictionary<string, object?> parms = new Dictionary<string, object?>
        {
            {"EmpUID",_appUser.Emp.UID }
        };
            _appUser.EmpInfo = await ExecuteSingleAsync<IEmpInfo>(sql, parms);
    }

    private async Task SetVehicle()
    {
        string sql = "Select * from Vehicle where UID=@UID";
        IDictionary<string, object?> parms = new Dictionary<string, object?>
        {
            {"UID","11df6959-1298-4618-8361-ef69d559ae06" }
        };
        _appUser.Vehicle = await ExecuteSingleAsync<IVehicleStatus>(sql, parms);
    }

    private async Task SetRole()
    {
        IMyRole myRole = new MyRole
        {
            JobPositionUID = "FBNZBU",
            RoleCode = "Role-01",
            RoleName = "Admin"
        };
        _appUser.SelectedRole = myRole;
    }
    public async Task SetApplicablePromotionUIDs()
    {
        _appUser.ApplicablePromotionUIDs = await _promotionBL.GetApplicablePromotionUIDs(new List<string>() { _appUser.SelectedJobPosition.OrgUID });
    }
    public async Task SetDMSPromotionDictionary()
    {
        if (_appUser.ApplicablePromotionUIDs != null && _appUser.ApplicablePromotionUIDs.Count > 0)
        {
            _appUser.DMSPromotionDictionary = await _promotionBL.GetDMSPromotionByPromotionUIDs(_appUser.ApplicablePromotionUIDs);
        }
    }
    public async Task SetStorePromotionMap()
    {
        if (_appUser.ApplicablePromotionUIDs != null && _appUser.ApplicablePromotionUIDs.Count > 0)
        {
            _appUser.StorePromotionMapDictionary = await _promotionBL.LoadStorePromotionMap(new List<string>() { _appUser.SelectedJobPosition.OrgUID });
        }
    }
    public async Task SetTaxDictionary()
    {
        List<ITaxMaster> taxMasters = await _taxMasterBL.GetTaxMaster(new List<string>() { _appUser.SelectedJobPosition.OrgUID });
        if (taxMasters == null || taxMasters.Count == 0)
        {
            return;
        }
        Dictionary<string, ITax> taxDictionary = new Dictionary<string, ITax>();
        foreach (ITaxMaster taxMaster in taxMasters)
        {
            taxDictionary[taxMaster.Tax.UID] = taxMaster.Tax;
        }
        _appUser.TaxDictionary = taxDictionary;
    }
    public async Task SetOrgHierarchy(string org_uid)
    {
        _appUser.OrgUIDs
              = await _orgBL.GetOrgHierarchyParentUIDsByOrgUID(new List<string> { org_uid });
    }
    //public async Task GetCurrencyDetails()
    //{
    //    var pagedResponse = await _currencyBL.GetCurrencyDetails(null, 0, 0, null, true);
    //    if (pagedResponse != null && pagedResponse.PagedData != null)
    //    {
    //        _appUser.CurrencyList = pagedResponse.PagedData.ToList();
    //    }
    //}
}

