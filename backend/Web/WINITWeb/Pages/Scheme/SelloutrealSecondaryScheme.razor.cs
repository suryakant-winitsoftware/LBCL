using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Shared.Models.Common;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;

namespace WinIt.Pages.Scheme
{
    public partial class SelloutrealSecondaryScheme : ComponentBase
    {
        List<DataGridColumn> productColumns = new List<DataGridColumn>()
        {
           new DataGridColumn { Header ="Product category",  IsSortable = false},
           new DataGridColumn { Header ="TYPE",  IsSortable = false},
           new DataGridColumn { Header ="Star Rating",  IsSortable = false},
           new DataGridColumn { Header ="Model category",  IsSortable = false},
           new DataGridColumn { Header ="Excluded Models",  IsSortable = false},
           new DataGridColumn { Header ="Slab",  IsSortable = false},
           new DataGridColumn { Header ="Reward",  IsSortable = false},
           new DataGridColumn { Header ="Max Cap",  IsSortable = false},
           new DataGridColumn { Header ="Actions",  IsSortable = false},
        };

        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Sell out Real Secondary Scheme",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Manage Scheme",IsClickable=true,URL="ManageScheme"},
                new BreadCrumModel(){SlNo=1,Text="Add Scheme",IsClickable=true,URL="AddScheme"},
                new BreadCrumModel(){SlNo=1,Text="Sell Out Scheme",IsClickable=true,URL="SellOutScheme"},
                new BreadCrumModel(){SlNo=1,Text="Sell out Real Secondary Scheme",IsClickable=false,URL="SellOutScheme"},
            }
        };

        protected override void OnInitialized()
        {

            base.OnInitialized();
        }
    }
}
