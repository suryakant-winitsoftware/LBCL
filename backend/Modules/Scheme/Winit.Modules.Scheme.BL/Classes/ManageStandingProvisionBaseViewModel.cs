using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.BL.Classes;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes
{
    public abstract class ManageStandingProvisionBaseViewModel : IManageStandingProvisionViewModel
    {
        protected Winit.Modules.Common.Model.Interfaces.IDataManager _dataManager;
        public List<IStandingProvisionScheme> StandingProvisionSchemes { get; set; } = [];
        public List<ISchemeExtendHistory> SchemeExtendHistories { get; set; } = [];
        protected PagingRequest PagingRequest = new PagingRequest()
        {

            FilterCriterias = [],
            IsCountRequired = true,
        };
        protected SortCriteria DefaultSortCriteria = new SortCriteria(nameof(IStandingProvisionScheme.ModifiedTime), SortDirection.Desc);
        public List<ISKUGroup> SKUGroup { get; set; } = [];
        public List<ISKUGroupType> SKUGroupType { get; set; } = [];
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }

        public virtual async Task PopulateViewModel()
        {

        }
        public virtual async Task GetStandingProvisionDetails()
        {

        }
        protected void SetTableData()
        {
            foreach (var sku in StandingProvisionSchemes)
            {
                sku.SKUCategoryName = SKUGroup.Find(p => p.Code == sku.SKUCategoryCode)?.Name ?? "NA";
                sku.SKUTonnageName = SKUGroup.Find(p => p.Code == sku.SKUTonnageCode)?.Name ?? "NA";
                sku.SKUTypeName = SKUGroup.Find(p => p.Code == sku.SKUTypeCode)?.Name ?? "NA";
                sku.StarRatingName = SKUGroup.Find(p => p.Code == sku.StarRatingCode)?.Name ?? "NA";
                sku.SKUSeriesName = SKUGroup.Find(p => p.Code == sku.SKUSeriesCode)?.Name ?? "NA";
                sku.StarCapacityName = SKUGroup.Find(p => p.Code == sku.StarCapacityCode)?.Name ?? "NA";
            }
        }
        public async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            PagingRequest.FilterCriterias = [];
            foreach (var filter in filterCriteria)
            {
                if (!string.IsNullOrEmpty(filter.Value))
                {
                    switch (filter.Key)
                    {
                        case nameof(IStandingProvisionScheme.Code):
                            PagingRequest.FilterCriterias.Add(new(filter.Key, filter.Value, FilterType.Like));
                            break;
                        case nameof(IStandingProvisionScheme.Description):
                            PagingRequest.FilterCriterias.Add(new(filter.Key, filter.Value, FilterType.Like));
                            break;
                        case nameof(IStandingProvisionScheme.StartDate):
                            PagingRequest.FilterCriterias.Add(new(filter.Key, filter.Value, FilterType.GreaterThanOrEqual));
                            break;
                        case nameof(IStandingProvisionScheme.EndDate):
                            PagingRequest.FilterCriterias.Add(new(filter.Key, filter.Value, FilterType.LessThanOrEqual));
                            break;
                        default:
                            PagingRequest.FilterCriterias.Add(new(filter.Key, filter.Value, FilterType.Equal));
                            break;
                    }
                }
            }
            await GetStandingProvisionDetails();
        }
        public async Task OnPageChange(int pageNumber)
        {
            PagingRequest.PageNumber = pageNumber;
            await GetStandingProvisionDetails();
        }


        public abstract Task GetschemeExtendHistoryBySchemeUID(string standingProvisionUID);

    }
}
