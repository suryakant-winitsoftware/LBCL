using Winit.Shared.Models.Common;
using Winit.Modules.ApprovalEngine.Model.Classes;

namespace WinIt.Pages.ApprovalRoleEngine
{
    public partial class ApprovalRuleMapping
    {
        
        public bool IsLoading = true;
        private List<ISelectionItem> ApprovalType = new List<ISelectionItem>();
        protected override async Task OnInitializedAsync()
        {
            
            try
            {
                await _ApprovalEngine.DropDownsForApprovalMapping();
                ApprovalType = typeof(Customer).GetProperties()
            .Select(p => (ISelectionItem)new SelectionItem
            {
                UID = p.Name,
                Label = $"Customer.{p.Name}"
            })
            .ToList();

            }
            catch (Exception ex)
            {
               
            }
            finally
            {
                IsLoading = false; 
            }
        }
        public async Task OnTypeSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                var matchedTypeCodes = _ApprovalEngine.RuleMap
                                       .Where(rule => rule.Type == selecetedValue?.UID).ToList();
                _ApprovalEngine.TypeCode= Winit.Shared.CommonUtilities.Common.CommonFunctions.ConvertToSelectionItems(matchedTypeCodes, new List<string> { "TypeCode", "TypeCode", "TypeCode" });
                _ApprovalEngine.ApprovalRuleMapping.Type = selecetedValue?.UID;
               

            }
        }
        public async Task OnTypeCodeSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                var matchedTypeCodes = _ApprovalEngine.RuleMap
                                      .Where(rule => rule.TypeCode == selecetedValue?.UID).ToList();
                _ApprovalEngine.RuleIDs=new List<ISelectionItem>();
                foreach (var item in matchedTypeCodes)
                {
                    ISelectionItem ruleIdItem = new SelectionItem
                    {
                        UID = item.RuleId.ToString(),   // UID and Label will be set to 'RuleId'
                        Label = item.RuleId.ToString(),
                        Code = item.RuleId.ToString(),  // Code will also be set to 'RuleId'
                    };

                    _ApprovalEngine.RuleIDs.Add(ruleIdItem);
                }
                _ApprovalEngine.ApprovalRuleMapping.TypeCode = selecetedValue?.UID;

            }
        }
        public async Task OnRuleIdSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                _ApprovalEngine.ApprovalRuleMapping.RuleId = int.Parse(selecetedValue?.UID);
            }
        }

        public async Task IntegrateRule()
        {
            try
            {
                
                if (await _ApprovalEngine.IntegrateRule())
                {
                    await _alertService.ShowSuccessAlert("Success", "Rule Integrated successfully");
                }
                else
                {
                    await _alertService.ShowSuccessAlert("Failed", "Rule Integrated failed");
                }
                
            }
            catch(Exception ex) { }
            finally
            {

            }

         
        }
    }
}
