using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Classes;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.CreditLimit.BL.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.CreditLimit.Model.Interfaces;
using System.Runtime.InteropServices.Marshalling;
using Newtonsoft.Json;
using Winit.Modules.CreditLimit.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CreditLimit.BL.Classes;

public abstract class MaintainTemporaryCreditEnhancementBaseViewModel : IMaintainTemporaryCreditEnhancementViewModel
{
    private IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private Winit.Modules.Base.BL.ApiService _apiService;
    private readonly IAppSetting _appSetting;
    private List<string> _propertiesToSearch = new List<string>();
    private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
    public List<ITemporaryCredit> TemporaryCreditEnhancementList { get; set; }
    public List<ISelectionItem> ChannelPartnerList { get; set; }
    public List<ISelectionItem> DivisionsList { get; set; }
    public List<ISelectionItem> TemporaryCreditEnhancementRequestselectionItems { get; set; }
    public List<ISelectionItem> StatusSelectionItems { get; set; }
    public ITemporaryCredit TemporaryCreditEnhancementDetails { get; set; }
    public int? _requestDays => (int)(TemporaryCreditEnhancementDetails?.RequestAmountDays ?? 0);
    public List<IStoreCreditLimit> CreditLimits { get; set; }
    public List<FilterCriteria> FilterCriterias { get; set; }
    public List<SortCriteria> SortCriterias { get; set; }
    public List<IFileSys> CreditEnhancementFileSysList { get; set; } = new List<IFileSys>();
    public int PageNo { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public MaintainTemporaryCreditEnhancementBaseViewModel(IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        IAppUser appUser,
        IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
        Winit.Modules.Base.BL.ApiService apiService,
        IAppSetting appSetting
        )
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _apiService = apiService;
        _appSetting = appSetting;
        _appConfigs = appConfigs;
        _appUser = appUser;
        CreditLimits = [];
        _propertiesToSearch.Add("Code");
        _propertiesToSearch.Add("Name");
        TemporaryCreditEnhancementList = new List<ITemporaryCredit>();
        ChannelPartnerList = new List<ISelectionItem>();
        DivisionsList = new List<ISelectionItem>();
        TemporaryCreditEnhancementRequestselectionItems = new List<ISelectionItem>();
        StatusSelectionItems = new List<ISelectionItem>();
        SortCriterias = new List<SortCriteria>();
        FilterCriterias = new List<FilterCriteria>();
        TemporaryCreditEnhancementDetails = new TemporaryCredit();
        TemporaryCreditEnhancementDetails.UID = Guid.NewGuid().ToString();
    }
    public async virtual Task PopulateViewModel()
    {
        await GetTemporaryCreditEnhancementDetails();
        await PopulateChannelPartners();
        PopulateStatusSelectionList();
    }
    public async virtual Task PopulateChannelPartners()
    {
        await GetChannelPartnersList();
        //await GetDivisionsList();
        await GetTemporaryCreditEnhancementRequestselectionList();
    }

    public async virtual Task PopulateDivisionSelectionList(string uid)
    {
        await GetDivisionsList(uid);
    }
    private async Task GetTemporaryCreditEnhancementRequestselectionList()
    {
        TemporaryCreditEnhancementRequestselectionItems.Clear();
        TemporaryCreditEnhancementRequestselectionItems.AddRange(new List<ISelectionItem>
        {
            new SelectionItem
            {
                UID = "Credit Limit",
                Code = "Credit Limit",
                Label = "Credit Limit",
                ExtData = null,
                IsSelected = false
            },
            new SelectionItem
            {
                UID = "Aging Days",
                Code = "Aging Days",
                Label = "Aging Days",
                ExtData = null,
                IsSelected = false
            }
        });
    }

    private void PopulateStatusSelectionList()
    {
        StatusSelectionItems.Clear();
        StatusSelectionItems.AddRange(new List<ISelectionItem>
        {
            new SelectionItem
            {
                UID = "Pending",
                Code = "Pending",
                Label = "Pending",
            },
            new SelectionItem
            {
                UID = "Approved",
                Code = "Approved",
                Label = "Approved",
            },
            new SelectionItem
            {
                UID = "Rejected",
                Code = "rejected",
                Label = "Rejected",
            }
        });
    }
    private async Task GetChannelPartnersList()
    {
        ChannelPartnerList.Clear();
        var newChannelpartners = await GetChannelPartners();
        if (newChannelpartners != null && newChannelpartners.Any())
        {
            ChannelPartnerList.AddRange(CommonFunctions.ConvertToSelectionItems(newChannelpartners, e => e.UID, e => e.Code, e => $"[{e.Code}] {e.Name}"));
        }
    }
    private async Task GetDivisionsList(string uid)
    {
        DivisionsList.Clear();
        var newDivisions = await GetDivisionsListForCreditEnhancement(uid);
        if (newDivisions != null && newDivisions.Any())
        {
            DivisionsList.AddRange(CommonFunctions.ConvertToSelectionItems(newDivisions, e => e.UID, e => e.Code, e => $"{e.Name}"));
        }
    }
    public async Task GetCreditLimitsByChannelPartnerAndDivision(string? storeUID, string divisionOrgUID)
    {
        CreditLimits = await GetCreditLimitsByChannelPartnerAndDivisionUID(storeUID, string.Empty);
    }


    public async Task<bool> SaveTemporaryCreditRequest(ITemporaryCredit temporaryCredit)
    {
        temporaryCredit.CreditData = JsonConvert.SerializeObject(CreditLimits);
        temporaryCredit.RequestDate = DateTime.Now;
        temporaryCredit.CalenderEndDate = _appUser.CalenderPeriods.Max(s => s.EndDate);
        temporaryCredit.MaxDays = _appSetting.TempCreditLimitMaxDays;
        if (!await SaveTemporaryCreditRequestAsync(temporaryCredit))
        {
            return false;
        }
        else if (!await SaveTemporaryCreditForOracle(temporaryCredit))
        {
            return false;
        }
        else
        {
            return true;
        }

    }

    public async virtual Task GetTemporaryCreditRequestDetailsByUID(string uid)
    {
        TemporaryCreditEnhancementDetails = await GetTemporaryCreditRequestDetailsUID(uid);
        if (TemporaryCreditEnhancementDetails != null && !string.IsNullOrEmpty(TemporaryCreditEnhancementDetails.CreditData))
        {
            CreditLimits.Clear();
            CreditLimits.AddRange(JsonConvert.DeserializeObject<List<StoreCreditLimit>>(TemporaryCreditEnhancementDetails.CreditData));
        }
    }
    private async Task GetTemporaryCreditEnhancementDetails()
    {
        var newTemporaryCreditEnhancementDetails = await GetTemporaryCreditEnhancementDetailsData();
        TemporaryCreditEnhancementList.Clear();
        TemporaryCreditEnhancementList.AddRange(newTemporaryCreditEnhancementDetails);
    }

    public abstract Task<List<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit>> GetTemporaryCreditEnhancementDetailsData();
    public abstract Task<List<IStore>> GetChannelPartnersListForTempCreditEnhancement();
    public abstract Task<List<IOrg>> GetDivisionsListForCreditEnhancement(string uid);
    public abstract Task<bool> SaveTemporaryCreditRequestAsync(ITemporaryCredit temporaryCreditEnhancementDetails);
    public abstract Task<bool> SaveTemporaryCreditForOracle(ITemporaryCredit temporaryCreditEnhancementDetails);
    public abstract Task<ITemporaryCredit> GetTemporaryCreditRequestDetailsUID(string uid);
    public abstract Task<List<IStoreCreditLimit>> GetCreditLimitsByChannelPartnerAndDivisionUID(string? storeUID, string divisionOrgUID);
    public abstract Task<List<IStore>?> GetChannelPartners();

    public async Task ApplyFilter(List<FilterCriteria> filters, List<SortCriteria> sorts)
    {
        FilterCriterias.Clear();
        SortCriterias.Clear();
        FilterCriterias.AddRange(filters);
        SortCriterias.AddRange(sorts);
        TemporaryCreditEnhancementList.Clear();
        var data = await GetTemporaryCreditEnhancementDetailsData();
        if (data != null && data.Any())
        {
            TemporaryCreditEnhancementList.AddRange(data);
        }
    }

    public async Task OnPageIndexChanged(int pageIndex)
    {
        PageNo = pageIndex;
        TemporaryCreditEnhancementList.Clear();
        var data = await GetTemporaryCreditEnhancementDetailsData();
        if (data != null && data.Any())
        {
            TemporaryCreditEnhancementList.AddRange(data);
        }
    }


}
