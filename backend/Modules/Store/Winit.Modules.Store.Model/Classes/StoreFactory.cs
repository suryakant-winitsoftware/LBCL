using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreFactory : IFactory
    {
        private readonly string _className; 

        public StoreFactory(string className)
        {
            _className = className;
        }

        public object? CreateInstance()
        {
            switch (_className)
            {
                case StoreModule.Store:
                    return new Store();
                case StoreModule.StoreAdditionalInfo:
                    return new StoreAdditionalInfo();
                case StoreModule.StoreCredit:
                    return new StoreCredit();
                case StoreModule.StoreAttributes:
                    return new StoreAttributes();
                case StoreModule.StoreGroup:
                    return new StoreGroup();
                case StoreModule.StoreGroupType:
                    return new StoreGroupType();
                case StoreModule.StoreSpecialDay:
                    return new StoreSpecialDay();
                case StoreModule.StoreToGroupMapping:
                    return new StoreToGroupMapping();
                default:
                    return null; 
            }
        }
    }

}
