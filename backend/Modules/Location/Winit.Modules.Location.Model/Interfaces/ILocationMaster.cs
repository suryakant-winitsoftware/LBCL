using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface ILocationMaster
    {
        public string UID {  get; set; }
        public string LocationTypeName {  get; set; }
        public string Label {  get; set; }
        public int Level {  get; set; }
    }
}
