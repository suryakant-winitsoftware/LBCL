using Microsoft.Playwright;
using Xunit;
using System.Threading.Tasks;
using System;

namespace WINITXUnitTest.SystemTest
{
    public class PlaywrightTests : IAsyncLifetime
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IPage _page;
        private const string BaseUrl = "http://localhost:5289/"; // Update this to match your application URL

        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false // Set to true in production
            });
            _page = await _browser.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            await _page.CloseAsync();
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        [Fact]
        public async Task Application_ShouldLoadSuccessfully()
        {
            // Navigate to the application
            await _page.GotoAsync(BaseUrl);

            // Wait for the page to be loaded
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Take a screenshot for debugging
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "screenshot.png"
            });

            // Verify the page loaded successfully
            var title = await _page.TitleAsync();
            Assert.NotNull(title);
        }

        [Fact]
        public async Task Navigation_ShouldWork()
        {
            // Navigate to the application
            await _page.GotoAsync(BaseUrl);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Wait for navigation menu to be visible
            await _page.WaitForSelectorAsync("nav");

            // Click on a navigation item (update the selector based on your application)
            await _page.ClickAsync("nav a:first-child");

            // Wait for navigation to complete
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the URL changed
            var currentUrl = _page.Url;
            Assert.NotEqual(BaseUrl, currentUrl);
        }

        [Fact]
        public async Task Form_ShouldSubmitSuccessfully()
        {
            // Navigate to the application
            await _page.GotoAsync(BaseUrl);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Wait for a form to be visible (update the selector based on your application)
            await _page.WaitForSelectorAsync("form");

            // Fill in form fields (update selectors and values based on your application)
            await _page.FillAsync("input[name='username']", "admin");
            await _page.FillAsync("input[name='password']", "password");

            // Submit the form
            await _page.ClickAsync("button[type='submit']");

            // Wait for the form submission to complete
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the form submission was successful
            // Add appropriate assertions based on your application's behavior
        }

        [Fact]
        public async Task CreateNewCollection_ShouldWork()
        {
            // Navigate to the application
            await _page.GotoAsync(BaseUrl);

            // Login
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "User Name" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "User Name" }).FillAsync("admin");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "User Name" }).PressAsync("Tab");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("password");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

            // Wait for login to complete
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Wait for the main content to be visible
            await _page.WaitForSelectorAsync("main", new() { 
                State = WaitForSelectorState.Visible,
                Timeout = 60000 // 60 seconds timeout
            });

            // Take a screenshot for debugging
            await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "after-login.png" });

            // Try multiple selectors for the Add New Collection link
            var addNewCollectionLink = await _page.WaitForSelectorAsync("a:has-text('Add New Collection'), [href*='collection'], .nav-link:has-text('Add New Collection')", new() {
                State = WaitForSelectorState.Visible,
                Timeout = 60000
            });
            
            if (addNewCollectionLink != null)
            {
                await addNewCollectionLink.ClickAsync();
            }
            else
            {
                // Take a screenshot if link is not found
                await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "link-not-found.png" });
                throw new Exception("Add New Collection link not found after login");
            }

            // Wait for the new collection page to load
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Select Payment Mode
            await _page.GetByRole(AriaRole.Button, new() { Name = "Select PaymentMode" }).ClickAsync();
            await _page.GetByRole(AriaRole.Cell, new() { Name = "Select All" }).First.ClickAsync();
            await _page.GetByRole(AriaRole.Cell, new() { Name = "Select All" }).First.ClickAsync();
            await _page.GetByRole(AriaRole.Row, new() { Name = "Cash" }).GetByRole(AriaRole.Cell).First.ClickAsync();
            await _page.GetByRole(AriaRole.Row, new() { Name = "Cash" }).Locator("#checkbox").CheckAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Done" }).ClickAsync();

            // Fill in collection details
            await _page.GetByRole(AriaRole.Combobox).SelectOptionAsync(new[] { "INVOICE|" });
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Enter Amount" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Enter Amount" }).FillAsync("1");

            // Select customer and payment details
            await _page.GetByText("Customer Name*CUST2(CUST2) (ADMIN)ADMIN Payment Mode 1items selected × Select").ClickAsync();
            await _page.Locator(".cls_btn_radio_input").CheckAsync();
            await _page.Locator(".cls_section_div_purchase2 > .cls_select2_button").ClickAsync();
            await _page.Locator(".cls_section_div_purchase2 > .cls_select2_button").FillAsync("1");

            // Click Cash Amount and Create
            await _page.Locator("#app div").Filter(new() { HasText = "Cash Amount:" }).Nth(3).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Ok" }).ClickAsync();

            // Reset zoom and verify collection
            await _page.Locator("body").PressAsync("ControlOrMeta+0");
            await _page.GetByRole(AriaRole.Row, new() { Name = "CUST001_c10e04a5 14/05/2025" }).GetByRole(AriaRole.Button).ClickAsync();
            await _page.Locator("html").ClickAsync();
            await _page.GetByRole(AriaRole.Img).Nth(2).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "CANCEL" }).ClickAsync();
        }
    }
} 