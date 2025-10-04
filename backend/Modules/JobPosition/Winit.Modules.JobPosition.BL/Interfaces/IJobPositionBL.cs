using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JobPosition.BL.Interfaces
{
    public interface IJobPositionBL
    {
        Task<PagedResponse<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>> SelectAllJobPositionDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> GetJobPositionByUID(string UID);
        Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> GetJobPositionLocationTypeAndValueByUID(string UID);
        Task<int> CreateJobPosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPosition);
        Task<int> UpdateJobPosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPosition);
        Task<int> UpdateJobPosition1(Winit.Modules.JobPosition.Model.Classes.JobPositionApprovalDTO jobPositionApprovalDTO);
        Task<int> UpdateJobPositionLocationTypeAndValue(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPosition);
        Task<int> DeleteJobPosition(string UID);
        Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> SelectJobPositionByEmpUID(string empUID);
        Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance> GetJobPositionAttendanceByEmpUID(string jobPositionUID, string empUID);
        Task<int> UpdateJobPositionAttendance(Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance jobPositionDetails);
        Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance> GetTotalAssignedAndVisitedStores(string JobPositionUID);
    }
}
