using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WinIT.RuleEngine.UI.Models;
using WinIT.RuleEngine.UI.Util;

namespace WinIT.RuleEngine.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        protected readonly WINITServices.Interfaces.RuleEngine.IRuleEngineService _ruleEngineService;
        public HomeController(ILogger<HomeController> logger, WINITServices.Interfaces.RuleEngine.IRuleEngineService ruleEngineService)
        {
            _logger = logger;
            _ruleEngineService = ruleEngineService;
        }

        public async Task<IActionResult> CreateRule()
        {
            await initialize();
            RuleViewModel model = TempData.ContainsKey("data") ? JsonConvert.DeserializeObject<RuleViewModel>(TempData.Peek("data").ToString()) : new RuleViewModel();
            TempData["data"] = JsonConvert.SerializeObject(model);
            return View(model);
        }
        private async Task initialize()
        {
            string targetNamespace = "WinIT.RuleEngine.UI.Models"; // Update with the desired target namespace
            //List<string> classProperties = ObjectScanner.GetClassProperties(targetNamespace);
            //ViewBag.Parameters = classProperties;
            List<string> classPropertiesWithTypes = ObjectScanner.GetClassPropertiesTypes(targetNamespace);
            ViewBag.ClassPropertiesWithTypes = classPropertiesWithTypes.Distinct().ToList();

            var users = await _ruleEngineService.RetrieveApproverAllAsync();
            ViewBag.Users = new SelectList(users, "Key", "Value");

            var enumData = (from WINITSharedObjects.Models.RuleEngine.ConditionOperator e in Enum.GetValues(typeof(WINITSharedObjects.Models.RuleEngine.ConditionOperator))
                            select new KeyValuePair<string, string>(
                                e.ToString(),
                                e.ToString()
                            )).ToDictionary(x => x.Key, x => x.Value);
            ViewBag.EnumList = enumData;
        }
        [HttpPost]
        public IActionResult CreateRule(RuleMaster ruleMaster)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var models = new RuleViewModel();
                    ruleMaster.Id = Guid.NewGuid().ToString();
                    models.ruleMaster = ruleMaster;
                    TempData["data"] = JsonConvert.SerializeObject(models);
                    return Ok(new { success = true, message = "RuleMaster created successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Invalid data." });
                }
            }
            catch (Exception ex) { }
            return Json(new { success = false, message = "Invalid data." });
        }
        [HttpPost]
        public IActionResult CreateRuleParameter(RuleParameter editRuleParameter)
        {
            if (ModelState.IsValid)
            {
                RuleViewModel model = TempData.ContainsKey("data") ? JsonConvert.DeserializeObject<RuleViewModel>(TempData.Peek("data").ToString()) : new RuleViewModel();
                editRuleParameter.Id = Guid.NewGuid().ToString();
                editRuleParameter.RuleId = model.ruleMaster.Id;
                model.ruleParameters.Add(editRuleParameter);
                TempData["data"] = JsonConvert.SerializeObject(model);
                return PartialView("_parameters", new List<RuleParameter> { editRuleParameter });
            }
            else
            {
                return Json(new { success = false, message = "Invalid data." });
            }
        }
        public IActionResult GetRuleParameterDropdownData()
        {
            RuleViewModel model = TempData.ContainsKey("data") ? JsonConvert.DeserializeObject<RuleViewModel>(TempData.Peek("data").ToString()) : new RuleViewModel();
            // Retrieve the data for the dropdown
            var dropdownData = model.ruleParameters;
            // Convert the data to a format suitable for JSON serialization
            var jsonData = dropdownData.Select(d => new { Value = d.Id, Text = d.Name });
            return Json(jsonData);
        }

        [HttpPost]
        public IActionResult CreateCondition(RuleCondition editRuleCondition)
        {
            // Perform validation and insert the conditions into the database
            if (ModelState.IsValid)
            {
                RuleViewModel model = TempData.ContainsKey("data") ? JsonConvert.DeserializeObject<RuleViewModel>(TempData.Peek("data").ToString()) : new RuleViewModel();
                editRuleCondition.Id = Guid.NewGuid().ToString();
                editRuleCondition.RuleId = model.ruleMaster.Id;
                model.ruleConditions.Add(editRuleCondition);
                TempData["data"] = JsonConvert.SerializeObject(model);
                // return Json(new { success = true });
                return PartialView("_conditions", new List<RuleCondition> { editRuleCondition });
            }
            else
            {
                return Json(new { success = false, message = "Invalid data." });
            }
        }
        [HttpPost]
        public IActionResult CreateApprovalHierarchy(ApprovalHierarchy editApprovalHierarchy)
        {
            if (ModelState.IsValid)
            {
                RuleViewModel model = TempData.ContainsKey("data") ? JsonConvert.DeserializeObject<RuleViewModel>(TempData.Peek("data").ToString()) : new RuleViewModel();
                // Map the view model to the entity model
                var entity = new ApprovalHierarchy
                {
                    id = Guid.NewGuid().ToString(),
                    RuleId = model.ruleMaster.Id,
                    ApproverId = editApprovalHierarchy.ApproverId,
                    Level = editApprovalHierarchy.Level,
                    NextApproverId = editApprovalHierarchy.NextApproverId
                };
                model.approvalHierarchies.Add(entity);
                TempData["data"] = JsonConvert.SerializeObject(model);
                return PartialView("_approvers", new List<ApprovalHierarchy> { entity });
            }
            else
            {
                return Json(new { success = false, message = "Invalid data." });
            }
        }
        [HttpPost]
        public IActionResult getSummery()
        {
            RuleViewModel model = TempData.ContainsKey("data") ? JsonConvert.DeserializeObject<RuleViewModel>(TempData.Peek("data").ToString()) : new RuleViewModel();
            TempData["data"] = JsonConvert.SerializeObject(model);
            return PartialView("_preview", model);
        }
        [HttpPost]
        public IActionResult InsertRuleAction(RuleAction editRuleAction)
        {
            if (ModelState.IsValid)
            {
                RuleViewModel model = TempData.ContainsKey("data") ? JsonConvert.DeserializeObject<RuleViewModel>(TempData.Peek("data").ToString()) : new RuleViewModel();
                editRuleAction.Id = Guid.NewGuid().ToString();
                editRuleAction.RuleId = model.ruleMaster.Id;
                model.ruleActions.Add(editRuleAction);
                TempData["data"] = JsonConvert.SerializeObject(model); ;
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Invalid data." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Confirm()
        {
            RuleViewModel model = TempData.ContainsKey("data") ? JsonConvert.DeserializeObject<RuleViewModel>(TempData.Peek("data").ToString()) : new RuleViewModel();
            try
            {
                if (model != null && model.ruleMaster.Id != "")
                {
                    var ruleid = await _ruleEngineService.InsertRuleMaster(new WINITSharedObjects.Models.RuleEngine.RuleMaster { Description = model.ruleMaster.Description, ModifiedOn = DateTime.Now, CreatedOn = DateTime.Now, Name = model.ruleMaster.Name });
                    if (ruleid > 0)
                    {
                        //model.ruleMaster.Id = ruleid.ToString();
                        foreach (var i in model.ruleParameters)
                        {
                            var parameterid = await _ruleEngineService.UpsertRuleParameter(new WINITSharedObjects.Models.RuleEngine.RuleParameter
                            {
                                DataType = i.DataType,
                                Description = i.Description,
                                Name = i.Name,
                                RuleId = ruleid
                            });
                            if (parameterid > 0)
                            {
                                foreach (var c in model.ruleConditions)
                                {
                                    if (c.ParameterId == i.Id)
                                    {
                                        //c.ParameterId = parameterid.ToString();
                                        var conditionid = await _ruleEngineService.UpsertCondition(new WINITSharedObjects.Models.RuleEngine.Condition
                                        {
                                            ParameterId = parameterid,
                                            RuleId = ruleid,
                                            Value = c.Value,
                                            Operator = c.Operator
                                        });
                                       // c.Id = conditionid.ToString();
                                    }
                                }
                               // i.Id = parameterid.ToString();
                            }
                        }
                        foreach (var a in model.ruleActions)
                        {
                            var actionid = await _ruleEngineService.UpsertRuleAction(new WINITSharedObjects.Models.RuleEngine.RuleAction
                            {
                                RuleId = ruleid,
                                ActionType =a.ActionType,
                                Template = a.Template
                            });
                        }
                        foreach (var i in model.approvalHierarchies.OrderBy(i => i.Level))
                        {
                            string nextapproverid = model.approvalHierarchies.Where(x => x.ApproverId != i.ApproverId && i.Level<x.Level)?.OrderBy(i => i.Level)?.FirstOrDefault()?.ApproverId??null;
                            var appid = await _ruleEngineService.UpsertApprovalHierarchy(
                                new WINITSharedObjects.Models.RuleEngine.ApprovalHierarchy { ApproverId = i.ApproverId, Level = i.Level, RuleId = ruleid, NextApproverId = nextapproverid }
                                );
                          //  i.id = appid.ToString();
                        }
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
            }
            await initialize();
            TempData["data"] = JsonConvert.SerializeObject(model); ; // Store the model for future reference if needed
            return View("Index", model);
        }

        public async Task<IActionResult> Index()
        {
            return View(await _ruleEngineService.RetrieveAllRuleAsync());
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
