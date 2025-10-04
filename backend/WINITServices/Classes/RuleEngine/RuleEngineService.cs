using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WINITServices.Classes.Email;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Models;
using WINITSharedObjects.Models.RuleEngine;

namespace WINITServices.Classes.RuleEngine
{
    public class RuleEngineService : RuleEngineBaseService
    {
        protected EmailMessaging _eml;
        protected const string qname = "RuleEngineQ";
        public RuleEngineService(WINITRepository.Interfaces.RuleEngine.IRuleEngineRepository ruleEngineRepository, EmailMessaging eml) : base(ruleEngineRepository)
        {
            _eml = eml;
        }
        public override async Task<RuleBO> GetRule(int ruleId)
        {
            RuleBO obj = new RuleBO();
            try
            {
                obj.Rule = await _ruleEngineRepository.RetrieveRuleAsync(ruleId);
                obj.Conditions = await _ruleEngineRepository.RetrieveConditionsAsync(ruleId);
                obj.Actions = await _ruleEngineRepository.RetrieveActionsAsync(ruleId);
            }
            catch (Exception ex) { }
            return obj;
        }
        public async override Task<int> CreateRequest(int ruleId, Dictionary<string, object> parameters)
        {
            int requestid = 0;
            try
            {
                //Log Request In DB
                var approvalRequest = new ApprovalRequest
                {
                    RuleId = ruleId,
                    RequesterId = parameters["RequesterId"]?.ToString(),
                    Status = "Pending",
                    CreatedOn = DateTime.Now,
                    ModifiedOn = DateTime.Now,
                    ModifiedBy = parameters["RequesterId"]?.ToString(),
                    Remarks = parameters["Remarks"]?.ToString()
                };

                requestid = await _ruleEngineRepository.InsertApprovalRequestAsync(approvalRequest);
                if (requestid > 0)
                {
                    // Evaluate the conditions and determine the appropriate action
                    List<Condition> conditions = await _ruleEngineRepository.RetrieveConditionsAsync(ruleId);
                    List<WINITSharedObjects.Models.RuleEngine.Action> actions = await _ruleEngineRepository.RetrieveActionsAsync(ruleId);
                    if (EvaluateConditions(conditions, parameters))
                    {
                        //update Approval_Request_Approver
                        var approvalHierarchy = await _ruleEngineRepository.RetrieveApprovalHierarchyAsync(ruleId);
                        if (approvalHierarchy != null)
                        {
                            var li = approvalHierarchy.Select(i => i.ApproverId).Distinct().ToList();
                            li.ForEach(async i =>
                            {
                                var approvalStatus = new ApprovalStatus
                                {
                                    ApprovalRequestId = requestid,
                                    ApproverId = i,
                                    Status = "Pending",
                                    CreatedOn = DateTime.Now,
                                    ActionId = actions != null && actions.Count > 0 ? actions[0].Id : 0,
                                    Remarks = "Pending approval"
                                };
                                await _ruleEngineRepository.InsertApprovalStatusAsync(approvalStatus);
                                await _ruleEngineRepository.InsertApproverAsync(requestid, i);
                            });
                            //update Approval_Request_Action
                            if (actions != null)
                            {
                                actions.ForEach(async i =>
                                {
                                    await _ruleEngineRepository.InsertApprovalRequestActionAsync(requestid, i.Id);
                                });
                            }
                            else
                                requestid = -3;//action not mapped
                        }
                        else
                            requestid = -2;//Hierarchy not mapped
                    }
                    else
                        requestid = -1;//EvaluateConditions Failed
                }
            }
            catch (Exception ex) { }
            return requestid;
        }


        protected bool EvaluateConditions(List<Condition> conditions, Dictionary<string, object> parameters)
        {
            foreach (var condition in conditions)
            {
                try
                {
                    var op = (ConditionOperator)Enum.Parse(typeof(ConditionOperator), condition.Operator);
                    object leftOperand = getOprand(condition.ParameterName, condition.DataType, parameters);
                    object rightOperand = getOprand(condition.Value, condition.DataType, parameters);
                    var result = utilities.EvaluateCondition(op, leftOperand, rightOperand);
                    if (!result)
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }

            }
            return true;
        }
        protected object getOprand(string ParameterName, string datatype, Dictionary<string, object> parameters)
        {
            object Operand = null;
            var pms = ParameterName.ToString().Split('.');
            if (pms.Length > 1)
            {
                if (parameters.TryGetValue(pms[0], out object parentObj))
                {
                    var dic = utilities.GetKeyValuePairsFromObject(parentObj.ToString());
                    if (dic.ContainsKey(pms[1]))
                        Operand = Convert.ChangeType(dic.GetValueOrDefault(pms[1]), Type.GetType(datatype));
                    else
                        throw new ArgumentException($"Input parameter '{pms[1]}' is missing.");
                }
                else
                {
                    throw new ArgumentException($"Input parameter '{pms[0]}' is missing.");
                }
            }
            else
            {
                try
                {
                    Operand = Convert.ChangeType(ParameterName, Type.GetType(datatype));
                }
                catch { }
                if (Operand == null)
                {
                    if (!parameters.TryGetValue(ParameterName, out var parameterValue))
                    {
                        throw new ArgumentException($"Input parameter '{ParameterName}' is missing.");
                    }
                    Type targetType = Type.GetType(datatype);
                    MethodInfo method = typeof(utilities).GetMethod("ConvertToGenericType");
                    MethodInfo genericMethod = method.MakeGenericMethod(targetType);

                    Operand = genericMethod.Invoke(null, new object[] { parameterValue.ToString() });

                }

            }

            return Operand;
        }

        protected void doAction(List<WINITSharedObjects.Models.RuleEngine.Action> actions, int requestId, ApprovalHierarchy approvalHierarchy, string status)
        {
            //do Action according to actions object
            actions.ForEach(i =>
            {

                if (approvalHierarchy != null)
                {
                    if (i.ActionType == ActionTypeConstants.Email)
                    {
                        string emailid = "";
                        if (status != "Rejected")
                            emailid = approvalHierarchy.Email;
                        else
                        {
                            var u = _ruleEngineRepository.RetrieveRequesterDetailsByRequestIdAsync(requestId);
                            if (u != null)
                            {
                                emailid = u.Result.Email;
                            }
                        }
                        try
                        {
                            Task.Run(async () =>
                            await _eml.sendEmailasync(new EmailMessage
                            {
                                to = new List<string> { emailid },
                                subject = "Winit Rule Engine Request - " + status,
                                message = i.EmailTemplate
                            }));
                        }
                        catch (Exception ex) { }

                    }
                    else if (i.ActionType == ActionTypeConstants.Notification) { }
                    //else if (i.ActionType == ActionTypeConstants.Discount) { }
                    // Update the rule execution history in the database
                    //Task.Run(async () =>
                    //{
                    //    var approvalStatusn = new ApprovalStatus
                    //    {
                    //        ApprovalRequestId = requestId,
                    //        ApproverId = approvalHierarchy?.ApproverId,
                    //        Status = status,
                    //        CreatedOn = DateTime.Now
                    //    };

                    //    await _ruleEngineRepository.UpdateApprovalStatusAsync(approvalStatusn);
                    //});
                }
                else if (status == "Rejected" && i.ActionType == ActionTypeConstants.Email)
                {
                    try
                    {
                        var u = _ruleEngineRepository.RetrieveRequesterDetailsByRequestIdAsync(requestId);
                        if (u != null)
                        {

                            Task.Run(async () =>
                                        await _eml.sendEmailasync(new EmailMessage
                                        {
                                            to = new List<string> { u.Result.Email },
                                            subject = "Winit Rule Engine Request - " + status,
                                            message = i.EmailTemplate
                                        }));
                        }

                    }
                    catch (Exception ex) { }
                }

            });


        }
        public override bool HandleMessageReceivedAsync(MessageData messageData)
        {
            if (messageData.requestId > 0)
            {
                _ = PerformAction(messageData.requestId, messageData.status);
            }
            return true;
        }
        private async Task<int> PerformAction(int requestid, string status)
        {
            int res = 0;
            try
            {

                List<WINITSharedObjects.Models.RuleEngine.Action> action = await _ruleEngineRepository.RetrieveActionsByRequestIdAsync(requestid);
                List<ApprovalHierarchy> approvalHierarchy = await _ruleEngineRepository.RetrieveApprovalHierarchyByRequestIdAsync(requestid);
                // Perform the appropriate action
                if (approvalHierarchy != null && approvalHierarchy.Count > 0)
                {
                        doAction(action, requestid, approvalHierarchy?[0], status);
               
                }
               
            }
            catch (Exception ex)
            {
                res = 0;
            }
           
            return res;
        }
        public override async Task<int> ApproveRejectRequest(int requestid, string status)
        {
            int res = 0;
            try
            {

                List<WINITSharedObjects.Models.RuleEngine.Action> action = await _ruleEngineRepository.RetrieveActionsByRequestIdAsync(requestid);
                List<ApprovalHierarchy> approvalHierarchy = await _ruleEngineRepository.RetrieveApprovalHierarchyByRequestIdAsync(requestid);
                // Perform the appropriate action
                if (approvalHierarchy != null && approvalHierarchy.Count > 0)
                {

                    var approvalStatusn = new ApprovalStatus
                    {
                        ApprovalRequestId = requestid,
                        ApproverId = approvalHierarchy?[0].ApproverId,
                        Status = status.ToString(),
                        CreatedOn = DateTime.Now,
                        Remarks = status.ToString()
                    };
                    res = await _ruleEngineRepository.UpdateApprovalStatusAsync(approvalStatusn);

                }

            }
            catch (Exception ex)
            {
                res = 0;
            }
            return res;
        }

        public override async Task<int> InsertRuleMaster(RuleMaster ruleMaster)
        {
            return await _ruleEngineRepository.InsertRuleMaster(ruleMaster);
        }

        public override async Task<int> UpsertRuleParameter(RuleParameter ruleParameter)
        {
            return await _ruleEngineRepository.UpsertRuleParameter(ruleParameter);
        }

        public override async Task<int> UpsertCondition(Condition condition)
        {
            return await _ruleEngineRepository.UpsertCondition(condition);
        }

        public override async Task<int> UpsertRuleAction(RuleAction ruleAction)
        {
            return await _ruleEngineRepository.UpsertRuleAction(ruleAction);
        }

        public override async Task<int> UpsertApprovalHierarchy(ApprovalHierarchy approvalHierarchy)
        {
            return await _ruleEngineRepository.UpsertApprovalHierarchy(approvalHierarchy);
        }

        public override async Task<List<KeyValueObject<string, string>>> RetrieveApproverAllAsync()
        {
            return await _ruleEngineRepository.RetrieveApproverAllAsync();
        }

        public override async Task<List<Rule>> RetrieveAllRuleAsync()
        {
            return await _ruleEngineRepository.RetrieveAllRuleAsync();
        }
    }
}
