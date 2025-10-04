using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;

using Winit.UIComponents.Common.Language;
using static Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection;

namespace WinIt.Pages.Collection.ExcelUploadDetails
{
    public partial class Parent
    {
        private bool ShowModalPopUp { get; set; } = false;
        [Parameter]
        public List<Collections> People { get; set; } = new List<Collections>();
        [Parameter]
        public EventCallback<List<AccCollectionAllotment>> OnDataSent { get; set; }

        [Parameter]
        public List<string> Recipt { get; set; } = new List<string>();
        [Parameter]
        public EventCallback<(IAccCollectionAllotment, string, Collections)> OnDataSend { get; set; }
        public List<AccCollectionAllotment> obj = new List<AccCollectionAllotment>();
        [Parameter] public EventCallback _showDiv { get; set; }
        public int trace { get; set; } = 0;


        protected override void OnInitialized()
        {

            try
            {
                LoadResources(null, _languageService.SelectedCulture);
                ShowModal();
            }
            catch (Exception ex)
            {

            }
        }
        
        private void HandleDropdownSelection(string item, IAccCollectionAllotment accCollectionNew, Collections accCollectionNew1)
        {
            try
            {
                ++trace;
                var data = (accCollectionNew, item, accCollectionNew1);
                OnDataSend.InvokeAsync(data);
            }
            catch (Exception ex)
            {

            }
        }

        private void ShowModal()
        {
            ShowModalPopUp = true;
        }
        private void Close()
        {
            ShowModalPopUp = false;
            _showDiv.InvokeAsync();
        }
        private async Task Save()
        {
            
                ShowModalPopUp = false;
                trace = 0;
            
            
        }
    }
}
