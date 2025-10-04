using System.Data;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JobPosition.DL.Interfaces
{
    public interface IJobPositionDL
    {
        Task<PagedResponse<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>> SelectAllJobPositionDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> GetJobPositionByUID(string UID, IDbConnection? connection = null, IDbTransaction? transaction = null);

        Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> GetJobPositionLocationTypeAndValueByUID(string UID, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> CreateJobPosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition jobPosition, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> UpdateJobPosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition jobPosition, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> UpdateJobPosition1(Winit.Modules.JobPosition.Model.Classes.JobPositionApprovalDTO jobPositionApprovalDTO, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> UpdateJobLocationTypeAndValuePosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition jobPosition, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> DeleteJobPosition(string UID);
        Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> SelectJobPositionByEmpUID(string EmpUID);
        Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance> GetJobPositionAttendanceByEmpUID(string jobPositionUID, string empUID);
        Task<int> UpdateJobPositionAttendance(Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance jobPositionDetails);
        Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance> GetTotalAssignedAndVisitedStores(string JobPositionUID);
    }
}
