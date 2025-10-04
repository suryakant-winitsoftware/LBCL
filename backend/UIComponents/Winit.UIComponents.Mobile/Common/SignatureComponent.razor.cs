using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Resources;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;
namespace Winit.UIComponents.Mobile.Common;

public partial class SignatureComponent : ComponentBase
{
    [Parameter]
    public EventCallback<string> OnSaveClick { get; set; }
    [Parameter]
    public byte[] Signature { get; set; } = Array.Empty<byte>();
    [Parameter]
    public bool DisableSaveBtn { get; set; }
    [Parameter]
    public EventCallback<byte[]> OnSignatureInput { get; set; }
    [Parameter]
    public string FileName { get; set; } = string.Empty;
    [Parameter]
    public string FolderPath { get; set; } = string.Empty;
    private async Task Save()
    {
        string[] imgArray = System.Text.Encoding.UTF8.GetString(Signature).Split(",");
        string base64String = string.Empty;
        if (imgArray.Length == 2)
        {
            base64String = imgArray[1];
        }
        await OnSaveClick.InvokeAsync(base64String);
    }
    private async Task OnSignatureChange(byte[] signature) 
    {
        await OnSignatureInput.InvokeAsync(signature);
    }
    protected override async Task OnInitializedAsync()
    {

        LoadResources(null, _languageService.SelectedCulture);
    }
    protected void LoadResources(object sender, string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
        Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
    }
}
