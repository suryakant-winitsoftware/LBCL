using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Setting.Model.Classes
{
    public class SettingFactory:IFactory
    {
        private readonly string _ClassName;
        public SettingFactory(String className) 
        { 
            _ClassName = className;
        }
        public object? CreateInstance()
        {
            switch(_ClassName)
            {
                case SettingModule.Setting:
                    return new Setting();
                default: return null;
            }
        }
    }
}
