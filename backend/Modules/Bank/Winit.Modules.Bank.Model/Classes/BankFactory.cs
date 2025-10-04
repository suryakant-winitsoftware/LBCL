using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Bank.Model.Interfaces;


namespace Winit.Modules.Bank.Model.Classes
{
    public class BankFactory:IFactory
    {
        private readonly string _className;

        public BankFactory(string className)
        {
            _className = className;
        }

        public object? CreateInstance()
        {
            switch (_className)
            {
                case BankModule.Bank:
                    return new Bank();
                
                default:
                    return null;
            }
        }
    }
}
