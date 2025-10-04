using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JobPosition.BL.Classes
{
    public class JobPositionBL : Winit.Modules.JobPosition.BL.Interfaces.IJobPositionBL
    {
        protected readonly DL.Interfaces.IJobPositionDL _JobPositionDL = null;
        public JobPositionBL(DL.Interfaces.IJobPositionDL jobPositionDL)
        {
            _JobPositionDL = jobPositionDL;
        }
        public async Task<PagedResponse<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition>> SelectAllJobPositionDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _JobPositionDL.SelectAllJobPositionDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> GetJobPositionByUID(string UID)
        {
            return await _JobPositionDL.GetJobPositionByUID(UID);
        }
        public async Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> GetJobPositionLocationTypeAndValueByUID(string UID)
        {
            return await _JobPositionDL.GetJobPositionLocationTypeAndValueByUID(UID);
        }
        public async Task<int> CreateJobPosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition jobPosition)
        {
            return await _JobPositionDL.CreateJobPosition(jobPosition);
        }
        public async Task<int> UpdateJobPosition(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPosition)
        {
            return await _JobPositionDL.UpdateJobPosition(JobPosition);
        }
        public async Task<int> UpdateJobPosition1(Winit.Modules.JobPosition.Model.Classes.JobPositionApprovalDTO jobPositionApprovalDTO)
        {
            return await _JobPositionDL.UpdateJobPosition1(jobPositionApprovalDTO);
        }
        public async Task<int> UpdateJobPositionLocationTypeAndValue(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPosition)
        {
            return await _JobPositionDL.UpdateJobLocationTypeAndValuePosition(JobPosition);
        }

        public async Task<int> DeleteJobPosition(string UID)
        {
            return await _JobPositionDL.DeleteJobPosition(UID);
        }
        public async Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> SelectJobPositionByEmpUID(string EmpUID)
        {
            return await _JobPositionDL.SelectJobPositionByEmpUID(EmpUID);
        }
        public async Task<IJobPositionAttendance> GetJobPositionAttendanceByEmpUID(string jobPositionUID, string empUID)
        {
            return await _JobPositionDL.GetJobPositionAttendanceByEmpUID(jobPositionUID, empUID);
        }

        public async Task<int> UpdateJobPositionAttendance(IJobPositionAttendance jobPositionDetails)
        {
            return await _JobPositionDL.UpdateJobPositionAttendance(jobPositionDetails);
        }
        
        public async Task<Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance> GetTotalAssignedAndVisitedStores(string JobPositionUID)
        {
            return await _JobPositionDL.GetTotalAssignedAndVisitedStores(JobPositionUID);
        }
    }
}
