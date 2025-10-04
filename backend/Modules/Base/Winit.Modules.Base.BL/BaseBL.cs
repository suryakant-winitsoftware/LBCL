using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Base.BL
{
    public class BaseBL
    {
        protected Model.RestBaseModel _model;
        public BaseBL(Model.RestBaseModel model)
        {
            _model = model;
        }
    }
}
