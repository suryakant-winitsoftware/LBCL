namespace Winit.UIComponents.SnackBar.Services;

public class CommonToastOptions
{
    public int MaximumOpacity { get; set; } = 95;

    public int ShowTransitionDuration { get; set; } = 200;

    public int VisibleStateDuration { get; set; } = 5000;

    public int HideTransitionDuration { get; set; } = 200;
    
    public bool ShowCloseIcon { get; set; } = true;

    public bool RequireInteraction { get; set; } = false;

    public bool BackgroundBlurred { get; set; } = false;
    
    protected CommonToastOptions() { }

    protected CommonToastOptions(CommonToastOptions options)
    {
        MaximumOpacity = options.MaximumOpacity;
        ShowTransitionDuration = options.ShowTransitionDuration;
        VisibleStateDuration = options.VisibleStateDuration;
        HideTransitionDuration = options.HideTransitionDuration;
        ShowCloseIcon = options.ShowCloseIcon;
        RequireInteraction = options.RequireInteraction;
        BackgroundBlurred = options.BackgroundBlurred;
    }
}