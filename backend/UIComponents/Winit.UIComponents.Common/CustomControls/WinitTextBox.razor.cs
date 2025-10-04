using Microsoft.AspNetCore.Components;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Enums;
namespace Winit.UIComponents.Common.CustomControls;

public partial class WinitTextBox : ComponentBase, IDisposable
{
    private System.Threading.Timer? _timer;
    private ElementReference _inputElement;
    [Parameter]
    public EventCallback<string> OnDebounceIntervalElapsed { get; set; }
    [Parameter]
    public string Placeholder { get; set; } = "";
    [Parameter]
    public int MaxLength { get; set; } = 250;
    [Parameter]
    public int DebounceInterval { get; set; } = 300;
    [Parameter]
    public InputType KeyboardType { get; set; } = InputType.Text;
    [Parameter]
    public string? Value { get; set; }
    [Parameter]
    public bool IsForSearch { get; set; }
    [Parameter]
    public bool IsHideClearButton { get; set; }
    [Parameter]
    public bool Disabled { get; set; }
    [Parameter]
    public int NoOfDecimal { get; set; } = 2;
    protected override void OnInitialized()
    {
        base.OnInitialized();

        // Initialize the timer
        _timer = new System.Threading.Timer(DebounceCallback, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
    }
    public void UpdateValue(string value)
    {
        Value = value;
        DebounceInput();
        FocusInput(); // Focus the input after clearing the value
    }
    private void DebounceCallback(object state)
    {
        _ = InvokeAsync(() => OnDebounceIntervalElapsed.InvokeAsync(Value));
    }
    public void Dispose()
    {
        _timer?.Dispose();
        _timer = null;
    }
    private void ClearInput()
    {
        Value = string.Empty;
        DebounceInput();
        FocusInput(); // Focus the input after clearing the value
    }
    private void DebounceInput()
    {
        _ = _timer.Change(DebounceInterval, System.Threading.Timeout.Infinite);
    }

    private void DebouncedInputValueChanged(ChangeEventArgs e)
    {
        string? s = e.Value?.ToString();
        if (!string.IsNullOrEmpty(s) && InputType.Numeric == KeyboardType)
        {
            decimal val = CommonFunctions.GetDecimalValue(s);
            Value = val == 0 ? "" : CommonFunctions.RoundForSystem(val, NoOfDecimal).ToString();
        }
        else
        {
            Value = s;
        }
        // Call the debounce method when the input value changes
        DebounceInput();
    }
    private void FocusInput()
    {
        // Focus on the input element using its reference
        _ = _inputElement.FocusAsync();
    }
    private string GetTypeAttribute(InputType inputType)
    {
        return inputType switch
        {
            InputType.Numeric => "number",
            InputType.Email => "email",
            InputType.URL => "url",
            _ => "text"
        };
    }
    //private string GetInputAttributes(InputType inputType)
    //{
    //    if (inputType == InputType.Numeric)
    //    {
    //        return "inputmode=\"numeric\" pattern=\"[0-9]*\"";
    //    }
    //    return string.Empty;
    //}
    private Dictionary<string, object> GetInputAttributes()
    {
        Dictionary<string, object> attributes = [];
        if (KeyboardType == InputType.Numeric)
        {
            attributes["inputmode"] = "numeric";
            attributes["pattern"] = "[0-9]*";
        }
        return attributes;
    }
    private string GetValue
    {
        get
        {
            if (InputType.Numeric == KeyboardType)
            {
                decimal val = CommonFunctions.GetDecimalValue(Value);
                return val == 0 ? "" : CommonFunctions.RoundForSystem(val, NoOfDecimal).ToString();
            }
            return string.IsNullOrEmpty(Value) ? "" : Value;
        }
    }

}
