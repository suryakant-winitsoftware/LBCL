using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Interfaces;

namespace Winit.Modules.Role.Model.Classes
{
    public class MenuMasterHierarchyView : IMenuMasterHierarchyView
    {
        public bool IsTestUserLoad { get; set; }
        public bool IsLoad { get; set; }
        public event EventHandler DataChanged;
        private List<MenuHierarchy> moduleMasterHierarchies;
        public Action<Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView> OnMenuAddigned {  get; set; }
        public List<MenuHierarchy> ModuleMasterHierarchies
        {
            get
            {
                return moduleMasterHierarchies;
            }
            set
            {
                if(value != null)
                {
                    moduleMasterHierarchies = value;
                    OnMenuAddigned.Invoke(this);
                }
            }
        }
    }
}
