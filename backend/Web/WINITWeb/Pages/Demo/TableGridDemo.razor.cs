using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Modules.Base.BL.Helper.Classes;
using Winit.Modules.Base.BL.Helper.Interfaces;
using WinIt.Pages.Base;

namespace WinIt.Pages.Demo
{
    public partial class TableGridDemo : BaseComponentBase
    {
        List<ISKU> products;
        List<IStore> customers;
        protected override async Task OnInitializedAsync()
        {
            products = GetSKUs();
            customers = GetStores();
        }
        public List<ISKU> GetSKUs()
        {
            List<ISKU> skus = new List<ISKU>();
            skus.Add(new SKU { Code = "SKU1", Name = "SKU Name1" });
            skus.Add(new SKU { Code = "SKU2", Name = "SKU Name2" });
            skus.Add(new SKU { Code = "SKU3", Name = "SKU Name3" });
            skus.Add(new SKU { Code = "SKU4", Name = "SKU Name4" });
            skus.Add(new SKU { Code = "SKU5", Name = "SKU Name5" });
            skus.Add(new SKU { Code = "SKU6", Name = "SKU Name6" });
            return skus;
        }
        public List<IStore> GetStores()
        {
            List<IStore> stores = new List<IStore>();
            stores.Add(new Winit.Modules.Store.Model.Classes.Store { Code = "Store1", Name = "Store Name1" });
            stores.Add(new Winit.Modules.Store.Model.Classes.Store { Code = "Store2", Name = "Store Name2" });
            stores.Add(new Winit.Modules.Store.Model.Classes.Store { Code = "Store3", Name = "Store Name3" });
            stores.Add(new Winit.Modules.Store.Model.Classes.Store { Code = "Store4", Name = "Store Name4" });
            stores.Add(new Winit.Modules.Store.Model.Classes.Store { Code = "Store5", Name = "Store Name5" });
            stores.Add(new Winit.Modules.Store.Model.Classes.Store { Code = "Store6", Name = "Store Name6" });
            return stores;
        }

        List<DataGridColumn> productColumns;
        List<DataGridColumn> customerColumns;
        protected override void OnInitialized()
        {
            productColumns = new List<DataGridColumn>
        {
            new DataGridColumn { Header = "SKU Code", GetValue = s => ((SKU)s).Code, IsSortable = false, SortField = "Code" },
            new DataGridColumn { Header = "SKU Code1", GetValue = s => ((SKU)s).Code, IsSortable = false, SortField = "Code" },
            new DataGridColumn { Header = "SKU Name", GetValue = s => ((SKU)s).Name, IsSortable = true, SortField = "Name" },
            new DataGridColumn { Header = "Is Active", GetValue = s => ((SKU)s).IsActive },
            new DataGridColumn
            {
                Header = "Actions",
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                       ButtonType=ButtonTypes.Image,
                        URL = "https://qa-fonterra.winitsoftware.com/assets/Images/logo123.png",
                        Action = item => HandleAction2_Customer((Winit.Modules.Store.Model.Classes.Store)item)
                    },
                    new ButtonAction
                    {
                        Text = "Action 2",
                         ButtonType=ButtonTypes.Text,
                        Action = item => HandleAction2_Product((SKU)item)
                    },
                    new ButtonAction
                    {
                        GetValue = s => ((SKU)s).Code,
                         ButtonType=ButtonTypes.Url,
                        Action = item => HandleAction2_Product((SKU)item)
                    },
                }
            }
        };
         
            customerColumns = new List<DataGridColumn>
        {
            new DataGridColumn { Header = "Store Code", GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s).Code, IsSortable = true, SortField = "Code" },
            new DataGridColumn { Header = "Store Name", GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s).Name, IsSortable = true, SortField = "Name" },
            new DataGridColumn { Header = "Is Active", GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s).IsActive },
            new DataGridColumn
            {
                Header = "Actions",
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType=ButtonTypes.Text,
                        Text = "Action 1",
                        Action = item => HandleAction1_Customer((Winit.Modules.Store.Model.Classes.Store)item)
                    },
                    new ButtonAction
                    {
                         ButtonType=ButtonTypes.Image,
                        URL = "https://qa-fonterra.winitsoftware.com/assets/Images/logo123.png",
                        Action = item => HandleAction2_Customer((Winit.Modules.Store.Model.Classes.Store)item)
                    },
                }
            }
        };
            
            base.OnInitialized();
        }

        
        private async void Product_OnSort(SortCriteria sortCriteria)
        {
            ISortHelper sortHelper = new SortHelper();
            products = await sortHelper.Sort(products, sortCriteria);
        }
        private async void Customer_OnSort(SortCriteria sortCriteria)
        {
            ISortHelper sortHelper = new SortHelper();
            customers = await sortHelper.Sort(customers, sortCriteria);
        }
        private void HandleAction1_Product(SKU item)
        {
        }
        private void HandleAction2_Product(SKU item)
        {
        }
        private  void HandleAction1_Customer(Winit.Modules.Store.Model.Classes.Store store)
        {
        }
        private  void HandleAction2_Customer(Winit.Modules.Store.Model.Classes.Store store)
        {
        }
        private async void Product_AfterCheckBoxSelection(HashSet<object> hashSet)
        {

        }

        private async void Product_OnPageChange(int pageNumber)
        {
        }
    }
}
