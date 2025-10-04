using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Modules.AwayPeriod.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using Winit.UIModels.Web.Store;

namespace Winit.UIComponents.Web.Store.AddNewStore
{
    public partial class AwayPeriod : ComponentBase
    {

        [Parameter] public IAwayPeriod? _AwayPeriod { get; set; }
        [Parameter] public List<IAwayPeriod> AwayPeriodList { get; set; }

        [Parameter] public string StoreUID { get; set; }
        [Parameter] public bool isNewStore { get; set; }
        [Parameter] public EventCallback<IAwayPeriod?> SaveOrUpdateAwayPeriod { get; set; }

        HttpClient http = new HttpClient();
        public bool isEdited { get; set; } = false;

        List<DataGridColumn> Columns;
        public bool IsComponentEdited()
        {
            return isEdited;
        }
        private void ChangeIsEditedOrNot()
        {
            isEdited = true;
        }
        protected override void OnInitialized()
        {
            _AwayPeriod.FromDate=DateTime.Now;
            _AwayPeriod.ToDate=DateTime.Now;

            Columns = new()
            {
                new DataGridColumn(){Header=@Localizer["from_date"],GetValue=s=>CommonFunctions.GetDateTimeInFormat(((IAwayPeriod)s).FromDate) },
                new DataGridColumn(){Header=@Localizer["to_date"],GetValue=s=>CommonFunctions.GetDateTimeInFormat(((IAwayPeriod)s).ToDate) },
                new DataGridColumn(){Header=@Localizer["description"],GetValue=s=>CommonFunctions.GetStringValue(((IAwayPeriod)s).Description) },
            };

            base.OnInitialized();
        }
      
        protected async Task SaveOrUpdate()
        {
            if (_AwayPeriod.FromDate > _AwayPeriod.ToDate)
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["to_date_should_not_be_less_than_from_date"]);
                return;
            }
            await SaveOrUpdateAwayPeriod.InvokeAsync(_AwayPeriod);
        }
        protected void Edit(IAwayPeriod away)
        {
            _AwayPeriod = away;
        }
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
        }
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
    }
}
