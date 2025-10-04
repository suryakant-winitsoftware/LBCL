using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.SKU.Model.Interfaces
{
    public interface ISKUGroupTypeView 
    {
      public int Id { get; set; }
      public string UID { get; set; }
      public string OrgUID { get; set; }
      public string Code { get; set; }
     public string Name { get; set; }
     public string ParentUID { get; set; }
     public int ItemLevel { get; set; }
    }
}
