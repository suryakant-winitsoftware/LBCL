using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;
using Winit.Modules.Promotion.Model.Classes;
using NPOI.SS.Formula.Functions;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Log = Serilog.Log;
using Microsoft.Extensions.Configuration;
using Winit.Shared.CommonUtilities.Common;

namespace WINITAPI.Controllers.Promotion
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PromotionController : WINITBaseController
    {
        private readonly Winit.Modules.Promotion.BL.Interfaces.IPromotionBL _promotionBL;
        ISchemeBranchBL _schemeBranchBL { get; }
        ISchemeBroadClassificationBL _broadClassificationBL { get; }
        ISchemeOrgBL _schemeOrgBL { get; }
        ISchemesBL _schemesBL { get; }
        bool IsApprovalsNeeded { get; }
        public PromotionController(IServiceProvider serviceProvider,
            Winit.Modules.Promotion.BL.Interfaces.IPromotionBL promotionBL,
            ISchemeBranchBL schemeBranchBL,
            ISchemeBroadClassificationBL broadClassificationBL, ISchemeOrgBL schemeOrgBL,
            ISchemesBL schemesBL, IConfiguration configuration) : base(serviceProvider)
        {
            _promotionBL = promotionBL;
            _schemeBranchBL = schemeBranchBL;
            _broadClassificationBL = broadClassificationBL;
            _schemeOrgBL = schemeOrgBL;
            _schemesBL = schemesBL;
            IsApprovalsNeeded = CommonFunctions.GetBooleanValue(configuration["IsApprovalsNeeded"]);
        }

        [HttpPost]
        [Route("GetPromotionDetails")]
        public async Task<ActionResult> GetPromotionDetails(PagingRequest pagingRequest)
        {
            try
            {
                if (pagingRequest == null)
                {
                    return BadRequest("Invalid request data");
                }
                if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                PagedResponse<Winit.Modules.Promotion.Model.Interfaces.IPromotion> PagedResponse = null;
                PagedResponse = await _promotionBL.GetPromotionDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
                if (PagedResponse == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponse);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Promotion");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CUDPromotionMaster")]
        public async Task<ActionResult> CUDPromotionMaster([FromBody] Winit.Modules.Promotion.Model.Classes.PromoMasterView promoMasterView)
        {
            try
            {
                // Log incoming request for debugging
                Log.Information("[DEBUG] CUDPromotionMaster request received");
                
                if (promoMasterView == null)
                {
                    Log.Error("PromoMasterView is null");
                    return CreateErrorResponse("Invalid request: PromoMasterView is null");
                }
                
                // Log the structure of the incoming data
                Log.Information($"[DEBUG] IsNew: {promoMasterView.IsNew}");
                Log.Information($"[DEBUG] PromotionView: {(promoMasterView.PromotionView != null ? "Present" : "NULL")}");
                
                if (promoMasterView.PromotionView != null)
                {
                    Log.Information($"[DEBUG] Promotion UID: {promoMasterView.PromotionView.UID}");
                    Log.Information($"[DEBUG] Promotion Code: {promoMasterView.PromotionView.Code}");
                    Log.Information($"[DEBUG] Promotion Name: {promoMasterView.PromotionView.Name}");
                    Log.Information($"[DEBUG] OrgUID: {promoMasterView.PromotionView.OrgUID}");
                    Log.Information($"[DEBUG] CompanyUID: {promoMasterView.PromotionView.CompanyUID}");
                    Log.Information($"[DEBUG] ActionType: {promoMasterView.PromotionView.ActionType}");
                }
                else
                {
                    Log.Error("PromotionView is null - this will cause NullReferenceException");
                    // Log what we received instead
                    Log.Information($"[DEBUG] PromoOrderViewList count: {promoMasterView.PromoOrderViewList?.Count ?? 0}");
                    Log.Information($"[DEBUG] PromoOfferViewList count: {promoMasterView.PromoOfferViewList?.Count ?? 0}");
                    Log.Information($"[DEBUG] PromoOrderItemViewList count: {promoMasterView.PromoOrderItemViewList?.Count ?? 0}");
                    Log.Information($"[DEBUG] SchemeOrgs count: {promoMasterView.SchemeOrgs?.Count ?? 0}");
                    Log.Information($"[DEBUG] SchemeBranches count: {promoMasterView.SchemeBranches?.Count ?? 0}");
                    
                    // Serialize the entire object to see its structure
                    var json = System.Text.Json.JsonSerializer.Serialize(promoMasterView);
                    Log.Information($"[DEBUG] Full PromoMasterView JSON: {json}");
                    
                    return CreateErrorResponse("Invalid request: PromotionView is required but was null");
                }
                
                // Ensure ActionType is set if missing
                if (promoMasterView.PromotionView.ActionType == null)
                {
                    promoMasterView.PromotionView.ActionType = Winit.Shared.Models.Enums.ActionType.Add;
                    Log.Information("[DEBUG] ActionType was null, setting to Add");
                }
                
                var returnValue = await _promotionBL.CUDPromotionMaster(promoMasterView);
                List<Task> tasks = new List<Task>();
                if (promoMasterView.SchemeBranches != null && promoMasterView.SchemeBranches.Any())
                {
                    returnValue += await _schemeBranchBL.CDBranches(promoMasterView.SchemeBranches, promoMasterView.PromotionView.UID);
                }
                if (promoMasterView.SchemeBroadClassifications != null && promoMasterView.SchemeBroadClassifications.Any())
                {
                    returnValue += await _broadClassificationBL.CDBroadClassification(promoMasterView.SchemeBroadClassifications, promoMasterView.PromotionView.UID);
                }
                if (promoMasterView.SchemeOrgs != null && promoMasterView.SchemeOrgs.Any())
                {
                    returnValue += await _schemeOrgBL.CDOrgs(promoMasterView.SchemeOrgs, promoMasterView.PromotionView.UID);
                }
                if (IsApprovalsNeeded && returnValue > 0)
                {
                    if (promoMasterView.IsNew)
                    {
                        bool isSuccess = await _schemesBL.CreateApproval(linkedItemType: promoMasterView.PromotionView.Type,
                        linkedItemUID: promoMasterView.PromotionView.UID, approvalRequestItem: promoMasterView.ApprovalRequestItem);
                        if (isSuccess)
                        {
                            int cnt = await _promotionBL.UpdatePromotion(promoMasterView.PromotionView);
                        }
                    }
                    else
                    {
                        bool isSuccess = await _schemesBL.UpdateApproval(approvalStatusUpdate: promoMasterView.ApprovalStatusUpdate);
                    }
                }
                
                Log.Information($"[SUCCESS] Promotion operation completed. Return value: {returnValue}");
                return (returnValue > 0) ? CreateOkApiResponse(returnValue) : throw new Exception("Operation Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create PromoMaster details");
                
                // Check if this is a unique constraint violation for promotion code
                if (ex.Message.Contains("uk_promotion_code") || 
                    ex.Message.Contains("duplicate key value violates unique constraint") ||
                    ex.Message.Contains("23505"))
                {
                    return CreateErrorResponse($"Promotion code '{promoMasterView?.PromotionView?.Code}' already exists. Please choose a different promotion code as all codes must be globally unique.");
                }
                
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateDMSPromotionByJsonData")]
        public async Task<ActionResult> CreateDMSPromotionByJsonData([FromBody] List<string> applicablePromotions)
        {
            try
            {
                int returnValue = await _promotionBL.CreateDMSPromotionByJsonData(applicablePromotions);
                return (returnValue > 0) ? CreateOkApiResponse(returnValue) : throw new Exception("Operation Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Promotion details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("CreateDMSPromotionByPromotionUID")]
        public async Task<ActionResult> CreateDMSPromotionByPromotionUID(string PromotionUID)
        {
            try
            {
                IEnumerable<Winit.Modules.Promotion.Model.Classes.DmsPromotion> DmsPromotionDetails = await _promotionBL.CreateDMSPromotionByPromotionUID(PromotionUID);
                if (DmsPromotionDetails != null)
                {
                    return CreateOkApiResponse(DmsPromotionDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve DmsPromotionDetails with UID: {@UID}", PromotionUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("CreateDMSPromotionByPromoUID")]
        public async Task<ActionResult> CreateDMSPromotionByPromoUID([FromQuery] string applicablePromotionUIDs,
            [FromBody] PromotionHeaderView promoHeaderView, [FromQuery] string promotionType, [FromQuery] string priority)
        {
            try
            {
                //List<Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion>> DmsPromotionDetails = await _promotionBL.CreateDMSPromotionByPromoUID(applicablePromotionUIDs, promotionType);
                string[] promotionsArray = applicablePromotionUIDs.Split(',');
                List<string> promotionsList = promotionsArray.ToList();
                Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion> DmsPromotionDetails = await _promotionBL.GetDMSPromotionByPromotionUIDs(promotionsList);
                string applicablePromotionUIDsFromDictionary = string.Join(",", DmsPromotionDetails.Keys);
                List<AppliedPromotionView> lstAppliedPromotion = _promotionBL.ApplyPromotion(applicablePromotionUIDsFromDictionary, promoHeaderView, DmsPromotionDetails, PromotionPriority.MinPriority);
                if (lstAppliedPromotion != null)
                {
                    return CreateOkApiResponse(lstAppliedPromotion);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve DmsPromotionDetails with UID: {@UID}", applicablePromotionUIDs);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpGet]
        [Route("GetPromotionDetailsByUID")]
        public async Task<ActionResult> GetPromotionDetailsByUID(string PromotionUID)
        {
            try
            {
                Winit.Modules.Promotion.Model.Classes.PromoMasterView DmsPromotionDetails = await _promotionBL.GetPromotionDetailsByUID(PromotionUID);
                if (DmsPromotionDetails != null)
                {
                    // Fetch additional related data
                    try 
                    {
                        // DmsPromotionDetails.SchemeBroadClassifications = await _broadClassificationBL.GetSchemeBroadClassificationByLinkedItemUID(PromotionUID);
                        DmsPromotionDetails.SchemeBranches = await _schemeBranchBL.GetSchemeBranchesByLinkedItemUID(PromotionUID);
                        DmsPromotionDetails.SchemeOrgs = await _schemeOrgBL.GetSchemeOrgByLinkedItemUID(PromotionUID);
                    }
                    catch (Exception relatedDataEx)
                    {
                        Log.Warning(relatedDataEx, "Failed to fetch some related data for promotion {PromotionUID}", PromotionUID);
                        // Continue without the related data
                    }
                    return CreateOkApiResponse(DmsPromotionDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve DmsPromotionDetails with UID: {@UID}", PromotionUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpGet]
        [Route("GetPromotionDetailsValidated")]
        public async Task<ActionResult> GetPromotionDetailsValidated(string PromotionUID, string OrgUID, string PromotionCode, string PriorityNo, bool isNew)
        {
            try
            {
                return CreateOkApiResponse(await _promotionBL.GetPromotionDetailsValidated(PromotionUID, OrgUID, PromotionCode, PriorityNo, isNew));
            }
            catch (Exception ex)
            {

                return CreateErrorResponse(ex.Message);

            }
        }
        [HttpDelete]
        [Route("DeletePromotionDetailsbyUID")]
        public async Task<ActionResult> DeletePromotionDetailsByUID(string PromotionUID)
        {
            try
            {
                return CreateOkApiResponse(await _promotionBL.DeletePromotionDetailsByUID(PromotionUID));
            }
            catch (Exception ex)
            {

                return CreateErrorResponse(ex.Message);

            }
        }
        [HttpDelete]
        [Route("DeletePromotionSlabByPromoOrderUID")]
        public async Task<ActionResult> DeletePromotionSlabByPromoOrderUID(string promoOrderUID)
        {
            try
            {
                return CreateOkApiResponse(await _promotionBL.DeletePromotionSlabByPromoOrderUID(promoOrderUID));
            }
            catch (Exception ex)
            {

                return CreateErrorResponse(ex.Message);

            }
        }
        [HttpPost]
        [Route("DeletePromoOrderItemsByUIDs")]
        public async Task<ActionResult> DeletePromoOrderItems([FromBody] List<string> UIDs)
        {
            try
            {
                return CreateOkApiResponse(await _promotionBL.DeletePromoOrderItems(UIDs));
            }
            catch (Exception ex)
            {

                return CreateErrorResponse(ex.Message);

            }
        }
        [HttpPut]
        [Route("ChangeEndDate")]
        public async Task<ActionResult> ChangeEndDate([FromBody] PromotionView promotionView)
        {
            try
            {
                int cnt = await _promotionBL.ChangeEndDate(promotionView);
                if (cnt > 0)
                {
                    if (promotionView.SchemeExtendHistory != null)
                    {
                        cnt += await _schemesBL.InsertSchemeExtendHistory(promotionView.SchemeExtendHistory);
                        await _schemesBL.UpdateSchemeCustomerMappingData(promotionView.SchemeExtendHistory.SchemeType, promotionView.SchemeExtendHistory.SchemeUid, promotionView.SchemeExtendHistory.NewDate.Value, promotionView.SchemeExtendHistory.ActionType == "Extend" ? true : false);
                    }
                }
                return CreateOkApiResponse(cnt);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex.Message);

            }
        }

        [HttpPut]
        [Route("ActivatePromotion")]
        public async Task<ActionResult> ActivatePromotion([FromBody] dynamic request)
        {
            try
            {
                string promotionUID = request?.PromotionUID?.ToString();
                if (string.IsNullOrWhiteSpace(promotionUID))
                {
                    return CreateErrorResponse("Promotion UID is required.");
                }

                // Get the promotion details first
                var promotionDetails = await _promotionBL.GetPromotionDetailsByUID(promotionUID);
                if (promotionDetails == null || promotionDetails.PromotionView == null)
                {
                    return CreateErrorResponse("Promotion not found.");
                }

                // Update the IsActive flag
                promotionDetails.PromotionView.IsActive = true;
                promotionDetails.PromotionView.ActionType = Winit.Shared.Models.Enums.ActionType.Update;
                promotionDetails.IsNew = false;

                // Save the changes
                int result = await _promotionBL.CUDPromotionMaster(promotionDetails);
                
                if (result > 0)
                {
                    // Return the updated promotion data
                    var updatedPromotion = await _promotionBL.GetPromotionDetailsByUID(promotionUID);
                    return CreateOkApiResponse(new { Success = true, Message = "Promotion activated successfully", Promotion = updatedPromotion?.PromotionView });
                }
                
                return CreateErrorResponse("Failed to activate promotion.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error activating promotion");
                return CreateErrorResponse($"An error occurred while activating promotion: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("DeactivatePromotion")]
        public async Task<ActionResult> DeactivatePromotion([FromBody] dynamic request)
        {
            try
            {
                string promotionUID = request?.PromotionUID?.ToString();
                if (string.IsNullOrWhiteSpace(promotionUID))
                {
                    return CreateErrorResponse("Promotion UID is required.");
                }

                // Get the promotion details first
                var promotionDetails = await _promotionBL.GetPromotionDetailsByUID(promotionUID);
                if (promotionDetails == null || promotionDetails.PromotionView == null)
                {
                    return CreateErrorResponse("Promotion not found.");
                }

                // Update the IsActive flag
                promotionDetails.PromotionView.IsActive = false;
                promotionDetails.PromotionView.ActionType = Winit.Shared.Models.Enums.ActionType.Update;
                promotionDetails.IsNew = false;

                // Save the changes
                int result = await _promotionBL.CUDPromotionMaster(promotionDetails);
                
                if (result > 0)
                {
                    // Return the updated promotion data
                    var updatedPromotion = await _promotionBL.GetPromotionDetailsByUID(promotionUID);
                    return CreateOkApiResponse(new { Success = true, Message = "Promotion deactivated successfully", Promotion = updatedPromotion?.PromotionView });
                }
                
                return CreateErrorResponse("Failed to deactivate promotion.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deactivating promotion");
                return CreateErrorResponse($"An error occurred while deactivating promotion: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("ValidatePromotionCode")]
        public async Task<ActionResult> ValidatePromotionCode(string code, string orgUID = null, string promotionUID = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    return CreateErrorResponse("Promotion code is required for validation.");
                }

                // For global validation, use a default org if none provided
                if (string.IsNullOrWhiteSpace(orgUID))
                {
                    orgUID = "DEFAULT"; // Use a default value since validation is global
                }
                
                // Call the validation method that checks for global uniqueness
                int validationResult = await _promotionBL.GetPromotionDetailsValidated(
                    promotionUID ?? string.Empty, 
                    orgUID, 
                    code, 
                    "0", // Priority not relevant for code validation - will be converted to int in DL
                    string.IsNullOrEmpty(promotionUID) // isNew = true if no promotionUID provided
                );

                // Create response based on validation result
                var response = new PromotionValidationResponse
                {
                    IsValid = validationResult == Winit.Shared.Models.Constants.Promotions.None,
                    ConflictType = validationResult,
                    Message = GetValidationMessage(validationResult)
                };

                return CreateOkApiResponse(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error validating promotion code: {Code}", code);
                return CreateErrorResponse($"Error validating promotion code: {ex.Message}");
            }
        }

        private string GetValidationMessage(int validationResult)
        {
            return validationResult switch
            {
                Winit.Shared.Models.Constants.Promotions.Code => "Promotion code already exists and must be unique globally.",
                Winit.Shared.Models.Constants.Promotions.Priority => "Promotion priority conflict detected.",
                Winit.Shared.Models.Constants.Promotions.Code_Priority => "Both promotion code and priority conflicts detected.",
                Winit.Shared.Models.Constants.Promotions.None => "Promotion code is valid and unique.",
                _ => "Unknown validation result."
            };
        }
    }

    public class PromotionValidationResponse
    {
        public bool IsValid { get; set; }
        public int ConflictType { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
