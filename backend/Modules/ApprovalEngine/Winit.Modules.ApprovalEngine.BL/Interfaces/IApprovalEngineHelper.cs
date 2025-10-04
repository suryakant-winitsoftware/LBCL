using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.UIModels.Common;
namespace Winit.Modules.ApprovalEngine.BL.Interfaces;

public interface IApprovalEngineHelper
{
    Task<ApprovalApiResponse<ApprovalStatus>> CreateApprovalRequest(ApprovalRequestItem approvalRequestItem, IAllApprovalRequest allApprovalRequest);
    Task<bool> UpdateApprovalStatus(ApprovalStatusUpdate approvalStatusUpdate);
}
