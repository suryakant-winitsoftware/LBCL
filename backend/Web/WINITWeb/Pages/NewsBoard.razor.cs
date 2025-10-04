using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.NewsActivity.Models.Constants;
using Winit.Modules.Store.Model.Constants;
using Winit.Shared.Models.Common;

namespace WinIt.Pages
{
    public partial class NewsBoard
    {
        //Links of images
        public string Url1 = "https://carriermideaindia.com/wp-content/uploads/2023/07/Interview-Sanjay-Mahajan-2pages_page-00011-scaled.jpg";
        public string Url2 = "https://carriermideaindia.com/wp-content/uploads/2023/04/Advertisement-News-Images-744x604_21.jpg";
        public string Url3 = "https://carriermideaindia.com/wp-content/uploads/2023/04/Advertisement-News-Images-744x604_24-1.jpg";
        public string Url4 = "https://carriermideaindia.com/wp-content/uploads/2023/03/Advertisement-News-Images-744x604_13.jpg";
        public string Url5 = "https://carriermideaindia.com/wp-content/uploads/2023/03/Advertisement-News-Images-744x604_22.jpg";
        public string Url6 = "https://carriermideaindia.com/wp-content/uploads/2023/03/Advertisement-News-Images-744x604_12.jpg";
        public string Url7 = "https://carriermideaindia.com/wp-content/uploads/2023/03/Advertisement-News-Images-744x604_16.jpg";
        public string Url8 = "https://carriermideaindia.com/wp-content/uploads/2023/03/Advertisement-News-Images-744x604_22-1.jpg";
        public string Url9 = "https://carriermideaindia.com/wp-content/uploads/2023/02/Advertisement-News-Images-744x604_01.jpg";
        public string Url10 = "https://carriermideaindia.com/wp-content/uploads/2023/02/Advertisement-News-Images-744x604_02.jpg";
        public string Url11 = "https://carriermideaindia.com/wp-content/uploads/2023/02/Advertisement-News-Images-744x604_04.jpg";
        public string Url12 = "https://carriermideaindia.com/wp-content/uploads/2023/07/Magazine-ads-BW-Businessworld_Carrier-02-9-scaled.jpg";
        public string Url13 = "https://carriermideaindia.com/wp-content/uploads/2023/07/Magazine-ads-BW-Businessworld_Carrier-01-8-scaled.jpg";
        public string Url14 = "https://carriermideaindia.com/wp-content/uploads/2018/03/Advertisement-News-Images-744x604_14.jpg";
        public string Url15 = "https://carriermideaindia.com/wp-content/uploads/2018/02/Advertisement-News-Images-744x604_17-17.jpg";
        public string Url16 = "https://carriermideaindia.com/wp-content/uploads/2018/02/Advertisement-News-Images-744x604_17-15.jpg";
        public string Url17 = "https://carriermideaindia.com/wp-content/uploads/2017/04/ad-thumb-7-1.jpg";
        public string Url18 = "https://carriermideaindia.com/wp-content/uploads/2017/04/ad-thumb-9.jpg";
        public string Url19 = "https://carriermideaindia.com/wp-content/uploads/2017/04/ad-thumb-8.jpg";
        public string Url20 = "https://carriermideaindia.com/wp-content/uploads/2017/04/Advertisement-News-Images-744x604_08.jpg";
        public string Url21 = "https://carriermideaindia.com/wp-content/uploads/2016/04/Advertisement-News-Images-744x604_19.jpg";
        public string Url22 = "https://carriermideaindia.com/wp-content/uploads/2015/04/Advertisement-News-Images-744x604_07.jpg";
        public string Url23 = "https://carriermideaindia.com/wp-content/uploads/2014/04/Advertisement-News-Images-744x604_06.jpg";
        public string Url24 = "https://carriermideaindia.com/wp-content/uploads/2013/04/Advertisement-News-Images-744x604_05.jpg";
        public string Url25 = "https://carriermideaindia.com/wp-content/uploads/2023/04/ad-thumb-12.jpg";
        public string Url26 = "https://carriermideaindia.com/wp-content/uploads/2012/04/Advertisement-News-Images-744x604_18.jpg";










        public string VUrl1 = "https://carriermideaindia.com/wp-content/themes/twentytwentyone-child/media/I-need-Smart-Living-iNeedMyCarrier-_-XCEL-Series-_-Gujrati-_-Carrier-Midea-India-_-20-Sec.mp4";
        public string VUrl2 = "https://carriermideaindia.com/wp-content/themes/twentytwentyone-child/media/I-need-my-X-Factor-iNeedMyCarrier-_-XCEL-Series-_-Gujrati-_-Carrier-Midea-India-_-20-Sec.mp4";
        public string VUrl3 = "https://carriermideaindia.com/wp-content/themes/twentytwentyone-child/media/I-need-my-Savings-iNeedMyCarrier-_-XCEL-Series-_-Gujrati-_-Carrier-Midea-India-_-20-Sec.mp4";
        public string VUrl4 = "https://carriermideaindia.com/wp-content/themes/twentytwentyone-child/media/I-need-my-Memories-iNeedMyCarrier-_-XCEL-Series-_-Gujrati-_-Carrier-Midea-India-_-20-Sec.mp4";


        public List<ISelectionItem> TabItems { get; set; } = new List<ISelectionItem>();
        public string TabName { get; set; } = NewsActivityConstants.news;
        public string Title { get; set; }
        public bool ShowPopUp { get; set; }
        public bool IsImgTag { get; set; }
        public bool IsVideoTag { get; set; }
        public string ImageSrc { get; set; }
        public string VideoSrc { get; set; }

        protected override async Task OnInitializedAsync()
        {
            TabItems.Add(new SelectionItem() { Code = NewsActivityConstants.news, Label = "News", IsSelected = true });
            TabItems.Add(new SelectionItem() { Code = NewsActivityConstants.advertisement, Label = "Advertisement" });
            TabItems.Add(new SelectionItem() { Code = NewsActivityConstants.businesscommunication, Label = "Business Communication" });
            TabName = NewsActivityConstants.news;
            try
            {
                ShowLoader();
                await _viewModel.PopulateviewModel(isFilesysNeeded: true);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                HideLoader();
            }


        }
        public async Task OnTabSelect(ISelectionItem selectionItem)
        {
            try
            {
                TabItems.ForEach(item => item.IsSelected = false);
                selectionItem.IsSelected = !selectionItem.IsSelected;
                TabName = selectionItem.Code;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                StateHasChanged();
            }
        }
        public void OnImageClick(string Url)
        {
            try
            {
                ShowPopUp = true;
                IsImgTag = true;
                Title = "";
                ImageSrc = Url;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                IsVideoTag = false;
                StateHasChanged();
            }
        }
        public void OnVideoClick(string Url)
        {
            try
            {
                ShowPopUp = true;
                IsVideoTag = true;
                Title = "";
                VideoSrc = Url;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                IsImgTag = false;
                StateHasChanged();
            }
        }
    }
}
