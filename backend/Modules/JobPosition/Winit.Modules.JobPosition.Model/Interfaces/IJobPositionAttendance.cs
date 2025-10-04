using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
namespace Winit.Modules.JobPosition.Model.Interfaces;

public interface IJobPositionAttendance : IBaseModel
{
    public string OrgUID { get; set; }
    public string JobPositionUID { get; set; }
    public string EmpUID { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int NoOfDays { get; set; }
    public int NoOfHolidays { get; set; }
    public int NoOfWorkingDays { get; set; }
    public int DaysPresent { get; set; }
    public decimal AttendancePercentage
    {
        get
        {
            return NoOfWorkingDays == 0 ? 0 : Math.Round((decimal)DaysPresent * 100 / NoOfWorkingDays, 2);
        }
        private set { }
    }
    public DateTime LastUpdateDate { get; set; }
    public int TotalAssignedStores { get; set; }
    public int TotalVisitedStores { get; set; }
}
