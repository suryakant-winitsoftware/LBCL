using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Classes;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IAsmDivisionMapping : IBaseModel
    {
        public IAsmDivisionMapping DeepCopy()
        {
            return new AsmDivisionMapping
            {
                DivisionUID = this.DivisionUID,
                DivisionName = this.DivisionName,
                AsmEmpUID = this.AsmEmpUID,
                AsmEmpName = this.AsmEmpName,
                StoreName = this.StoreName,
            };
        }
        public string LinkedItemType { get; set; }
        public string LinkedItemUID { get; set; }
        public string DivisionUID { get; set; }
        public string DivisionName { get; set; }
        public string AsmEmpUID { get; set; }
        public string StoreName { get; set; }
        public string AsmEmpName { get; set; }
    }
}
