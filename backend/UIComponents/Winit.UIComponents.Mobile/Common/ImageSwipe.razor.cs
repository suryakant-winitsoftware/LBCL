using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Winit.UIComponents.Mobile.Common;

partial class ImageSwipe
{
    [Parameter]
    public List<string> Images { get; set; }
    private string CurrentImage = string.Empty;
    private ElementReference ImgRef;
    private bool IsJSInvokeNeeded = false;
    protected override void OnInitialized()
    {
        if (Images is not null && Images.Any())
        {
            //Images = Images.Select(e => Path.Combine(_appConfigs.ApiDataBaseUrl, e)).ToList();
            CurrentImage = Images.First();
        }
        StateHasChanged();
    }
    private async Task HandleSwipe(SwipeDirection swipeDirection)
    {
        if (swipeDirection == SwipeDirection.LeftToRight)
        {
            int index = Images.IndexOf(CurrentImage);
            if (index == 0)
            {
                return;
            }
            else
            {
                CurrentImage = Images[index - 1];
                IsJSInvokeNeeded = true;
            }
        }
        else if (swipeDirection == SwipeDirection.RightToLeft)
        {
            int index = Images.IndexOf(CurrentImage);
            if (index == Images.Count - 1)
            {
                return;
            }
            else
            {
                CurrentImage = Images[index + 1];
                IsJSInvokeNeeded = true;
            }
        }
        StateHasChanged();
    }
    private async Task HandleImageChange(string img)
    {
        CurrentImage =  img;
        IsJSInvokeNeeded = true;
        StateHasChanged();
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        //if (IsJSInvokeNeeded)
        //{
        await _jSRunTime.InvokeVoidAsync("hammerIt", ImgRef);
        IsJSInvokeNeeded = false;
        //}
    }
}
