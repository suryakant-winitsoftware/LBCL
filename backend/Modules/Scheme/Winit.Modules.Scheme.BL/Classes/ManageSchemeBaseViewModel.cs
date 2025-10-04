using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Enums;
using static iTextSharp.text.pdf.AcroFields;

namespace Winit.Modules.Scheme.BL.Classes
{
    public abstract class ManageSchemeBaseViewModel : IManageSchemeViewModel
    {
        public bool StateRestored {  get; set; }
        public List<IManageScheme> ManageSchemes { get; set; } = [];
        public List<Winit.Shared.Models.Common.ISelectionItem> ChannelPartner { get; set; } = [];
        protected Winit.Shared.Models.Common.PagingRequest PagingRequest = new Shared.Models.Common.PagingRequest()
        {
            IsCountRequired = true,
            FilterCriterias = []
        };
        public List<ISchemeExtendHistory> SchemeExtendHistories { get; set; } = [];
        public abstract Task PopulateViewModel();
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        public async Task OnPageChange(int pageNumber)
        {
            PagingRequest.PageNumber = CurrentPage = pageNumber;

            await GetAllSchemes();
        }
        public async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {

            PagingRequest.FilterCriterias!.Clear();
            if (filterCriteria is not null)
            {

                foreach (var item in filterCriteria)
                {
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        switch (item.Key)
                        {
                            case nameof(IManageScheme.ValidFrom):
                                PagingRequest.FilterCriterias.Add(
                                new FilterCriteria(nameof(IManageScheme.ValidFrom), CommonFunctions.GetDate(item.Value),
                               FilterType.GreaterThanOrEqual));
                                break;
                            case nameof(IManageScheme.ValidUpto):
                                PagingRequest.FilterCriterias.Add(
                                new FilterCriteria(nameof(IManageScheme.ValidUpto), CommonFunctions.GetDate(item.Value),
                               FilterType.LessThanOrEqual));
                                break;
                            case SchemeConstants.ShowInactive:
                                bool isInActive = CommonFunctions.GetBooleanValue(item.Value);
                                if (isInActive)
                                {
                                    List<string> status = [SchemeConstants.Rejected, SchemeConstants.Expired];
                                    PagingRequest.FilterCriterias.Add(new(name: nameof(IManageScheme.Status), status, FilterType.In));
                                }
                                else
                                {
                                    List<string> status = [SchemeConstants.Approved, SchemeConstants.Pending];
                                    PagingRequest.FilterCriterias.Add(new(name: nameof(IManageScheme.Status), status, FilterType.In));
                                }
                                break;

                            case nameof(IManageScheme.Code):
                                PagingRequest.FilterCriterias.Add(
                                new FilterCriteria(nameof(IManageScheme.Code), item.Value, FilterType.Like));
                                break;
                            default:
                                PagingRequest.FilterCriterias.Add(new FilterCriteria(item.Key, item.Value, FilterType.Equal));
                                break;
                        }
                    }

                }

            }
            PagingRequest.PageNumber = 1;
            await GetAllSchemes();
        }
        protected abstract Task GetAllSchemes();
        public abstract Task GetschemeExtendHistoryBySchemeUID(string schemeUID);
    }
}
