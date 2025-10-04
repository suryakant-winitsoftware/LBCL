using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Contact.Model.Classes
{
    public class ContactFactory:IFactory
    {
        public readonly string _classname;
        public ContactFactory(string classname)
        {
            _classname = classname;
        }
        public object? CreateInstance()
        {
            switch (_classname)
            {
                case ContactModule.Contact: return new Contact();
                default: return null;
            }
        }
    }
}
