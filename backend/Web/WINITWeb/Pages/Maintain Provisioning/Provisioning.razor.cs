using Microsoft.IdentityModel.Tokens;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.CreditLimit.Model.Classes;
using Winit.Modules.CreditLimit.Model.Interfaces;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.DougnutGraph;
using Winit.UIModels.Common.GraphModels;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;

namespace WinIt.Pages.Maintain_Provisioning;

public partial class Provisioning : BaseComponentBase
{
    public bool IsInitialised { get; set; } = false;
    public bool IsChannelPartnerSelected { get; set; } = false;
    public List<DoughnutModel> Doughnutdata = new List<DoughnutModel>();
    public List<DataGridColumn> DataGridColumns { get; set; }
    public Doughnut? DoughNut;
    private string AvailableBalanceAmount = "0";
    IDataService dataService = new DataServiceModel()
    {
        HeaderText = "Provision Dashboard",
        BreadcrumList = new List<IBreadCrum>()
        {
            new BreadCrumModel(){SlNo=1,Text="Provision Dashboard"},
        }
    };
    protected override async Task OnInitializedAsync()
    {
        ShowLoader();
        GenerateGridColumns();
        await provisioningHeaderViewModel.PopulateHeaderViewModel();
        IsInitialised = true;
        HideLoader();
    }
    public async Task OnChannelPartnerSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent.SelectionItems.Any(item => item.IsSelected))
        {
            var selectedItem = dropDownEvent.SelectionItems.FirstOrDefault(item => item.IsSelected);
            if (!string.IsNullOrEmpty(selectedItem.UID))
            {
                await provisioningHeaderViewModel.GetProvisioningHeaderViewData(selectedItem!.UID);
                await provisioningItemViewModel.GetProvisionItemViewData(selectedItem.UID);
                GenerateGridColumnsDoughnutGraph();
                IsChannelPartnerSelected = true;
                if (DoughNut is not null)
                {
                    await DoughNut?.LoadDoughnutChart();
                }
                StateHasChanged();
            }
            else
            {
                IsChannelPartnerSelected = false;
            }
        }
        else
        {
            IsChannelPartnerSelected = false;
            ShowErrorSnackBar("Info :", "Please select channel partner for provisioning");
        }
    }
    private void GenerateGridColumns()
    {
        DataGridColumns = new List<DataGridColumn>
        {
            new DataGridColumn { Header = "Channel Partner" , GetValue = item =>{
                var provisionItem = (IProvisionItemView)item;
                var matchingPartner = provisioningHeaderViewModel.ChannelPartnerList
                            .Where(p => p.UID == provisionItem.ChannelPartner)
                            .FirstOrDefault();
                return "["+matchingPartner?.Code+"] " + matchingPartner?.Label ?? "";
                }
             },
            new DataGridColumn { Header = "Date" , GetValue = item => ((IProvisionItemView)item).OrderDate.ToString("dd/MM/yyyy") ?? "N/A"},
            new DataGridColumn { Header = "Amount" , GetValue = item => ((IProvisionItemView)item).Amount.ToString("F2")?? "N/A"},
            new DataGridColumn { Header = "Type" , GetValue = item => ((IProvisionItemView)item).Type ?? "N/A"},
            new DataGridColumn { Header = "Transaction code " , GetValue = item => ((IProvisionItemView)item).TransactionCode ?? "N/A"},
            new DataGridColumn { Header = "Model #" , GetValue = item => ((IProvisionItemView)item).ModelNo ?? "N/A" },
            new DataGridColumn { Header = "Qty" , GetValue = item => ((IProvisionItemView) item).Qty.ToString() ?? "N/A"},
            new DataGridColumn { Header = "Amount / SKU" , GetValue = item => ((IProvisionItemView) item).UnitAmount.ToString() ?? "N/A"},
            new DataGridColumn { Header = "Description" , GetValue = item => ((IProvisionItemView)item).Description ?? "N/A"},
            //new DataGridColumn { Header = "Action", IsButtonColumn = true,
            //ButtonActions = new List<ButtonAction>
            //    {
            //        new ButtonAction
            //        {
            //            ButtonType = ButtonTypes.Image,
            //            URL = "Images/view.png",
            //            Action = item => OnViewClick((IProvisionItemView)item)
            //        },
            //    }},
        };
    }
    private void GenerateGridColumnsDoughnutGraph()
    {
        Doughnutdata.Clear();
        Doughnutdata.AddRange(
        new List<DoughnutModel>(){
        new DoughnutModel { Label = "P3 (HO)", Value = CommonFunctions.RoundForSystem( provisioningHeaderViewModel.ProvisionHeaderViewDetails.P2Amount), Color = "#FF5733" },  // Orange0
        new DoughnutModel { Label = "P2 (Branch)", Value = CommonFunctions.RoundForSystem( provisioningHeaderViewModel.ProvisionHeaderViewDetails.P3Amount), Color = "#33FF57" },  // Green
        new DoughnutModel { Label = "S (HO - Standing)", Value = CommonFunctions.RoundForSystem( provisioningHeaderViewModel.ProvisionHeaderViewDetails.HoAmount), Color = "#3357FF" },  // Blue
        });
        if (DoughNut is not null)
        {
            DoughNut.CentreValue = _iAppUser.DefaultOrgCurrency?.Symbol + provisioningHeaderViewModel.ProvisionHeaderViewDetails.BalanceAmount.ToString("N2");
        }
    }
    private void OnViewClick(IProvisionItemView item)
    {
        //_navigationManager.NavigateTo($"ViewProvisioningDetails?ProvisionItemUID={item.UID}");
    }
}
