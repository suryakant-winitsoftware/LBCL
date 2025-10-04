using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Winit.Shared.CommonUtilities.Common;

namespace WINITMobile.Pages;

public partial class MultiSignature
{
    [Parameter]
    public string Signature1Heading { get; set; }
    [Parameter]
    public string Signature2Heading { get; set; }
    [Parameter]
    public string Signature1FolderPath { get; set; }
    [Parameter]
    public string Signature2FolderPath { get; set; }
    [Parameter]
    public string Signature1FileName { get; set; }
    [Parameter]
    public string Signature2FileName { get; set; }
    [Parameter]
    public string Signature1Footer { get; set; }
    [Parameter]
    public string Signature2Footer { get; set; }
    [Parameter]
    public EventCallback OnCloseClick { get; set; }
    [Parameter]
    public EventCallback OnProceedClick { get; set; }
    [Parameter]
    public byte[] Signature1 { get; set; } = Array.Empty<byte>();
    [Parameter]
    public byte[] Signature2 { get; set; } = Array.Empty<byte>();
    [Parameter]
    public bool IsLPONumberNeeded { get; set; }
    [Parameter]
    public string LPONumber { get; set; } = string.Empty;
    [Parameter]
    public EventCallback<string> HandleLPONumber { get; set; } 
    public async Task HandleSignature1Save(string base64String)
    {
        await CommonFunctions.SaveBase64StringToFile(base64String, Signature1FolderPath, Signature1FileName);
    }
    public async Task HandleSignature2Save(string base64String)
    {
        await CommonFunctions.SaveBase64StringToFile(base64String, Signature2FolderPath, Signature2FileName);
    }

    private async Task Handle_ProceedClick()
    {
        if (Signature1 == null || Signature2 == null || Signature1.IsNullOrEmpty() || Signature2.IsNullOrEmpty())
        {
            return;
        }
        string[] userImgArray = System.Text.Encoding.UTF8.GetString(Signature1).Split(",");
        string userbase64String = string.Empty;
        if (userImgArray.Length == 2)
        {
            userbase64String = userImgArray[1];
        }
        string[] storeImgArray = System.Text.Encoding.UTF8.GetString(Signature2).Split(",");
        string Customerbase64String = string.Empty;
        if (storeImgArray.Length == 2)
        {
            Customerbase64String = storeImgArray[1];
        }
        if (string.IsNullOrEmpty(userbase64String))
        {
            await _alertService.ShowErrorAlert("", @Localizer["missing_salesman_signature"]);
            return;
        }
        if (string.IsNullOrEmpty(Customerbase64String))
        {
            await _alertService.ShowErrorAlert("", @Localizer["missing_customer_signature"]);
            return;
        }
        await HandleSignature1Save(Customerbase64String);
        await HandleSignature2Save(userbase64String);
        await HandleLPONumber.InvokeAsync(LPONumber);
        await OnProceedClick.InvokeAsync((userbase64String, Customerbase64String));
    }
    protected override async Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
    }
}
