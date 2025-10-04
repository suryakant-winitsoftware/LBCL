using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using WINITMobile.Pages.Base;

namespace WINITMobile.Shared
{
    public partial class CustomError : ComponentBase
    {
        [Parameter]
        public Exception Context { get; set; }
        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (Context != null)
            {
                LogError(Context);
            }
        }

        void LogError(Exception exception)
        {
            // Log the exception message
            Console.WriteLine($"An error occurred: {exception}");
        }
    }
}
