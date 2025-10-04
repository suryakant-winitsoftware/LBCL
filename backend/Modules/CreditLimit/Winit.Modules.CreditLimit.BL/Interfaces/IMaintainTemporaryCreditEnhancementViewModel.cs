using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CreditLimit.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CreditLimit.BL.Interfaces;

public interface IMaintainTemporaryCreditEnhancementViewModel
{
    public List<ITemporaryCredit> TemporaryCreditEnhancementList { get; set; }
    public int PageNo { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    Task PopulateViewModel();
    public List<ISelectionItem> ChannelPartnerList { get; set; }
    public List<ISelectionItem> DivisionsList { get; set; }
    public List<IStoreCreditLimit> CreditLimits { get; set; }
    public List<SortCriteria> SortCriterias { get; set; }
    public List<FilterCriteria> FilterCriterias { get; set; }
    public List<IFileSys> CreditEnhancementFileSysList { get; set; } 
    Task PopulateChannelPartners();
    Task PopulateDivisionSelectionList(string uid);
    Task<bool> SaveTemporaryCreditRequest(ITemporaryCredit temporaryCreditEnhancementDetails);
    Task GetTemporaryCreditRequestDetailsByUID(string temporaryCreditEnhancementUID);
    Task GetCreditLimitsByChannelPartnerAndDivision(string? storeUID, string divisionOrgUID);
    public List<ISelectionItem> TemporaryCreditEnhancementRequestselectionItems { get; set; }
    public ITemporaryCredit TemporaryCreditEnhancementDetails { get; set; }
    public int? _requestDays { get; }
    public List<ISelectionItem> StatusSelectionItems { get; set; }
    Task ApplyFilter(List<FilterCriteria> filters, List<SortCriteria> sorts);
    Task OnPageIndexChanged(int pageIndex);
}
