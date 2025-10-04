using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WINITMobile.TopBarButtonClasses
{
    public class Service
    {
        public int Count { get; private set; }

        public string Name { get; set; }

        public event Action<int> CountChanged;

        public event Action<string> StringChanged;
        public event Action<string> StringChangedForButton1;
        public event Action<string> StringChangedForButton2;
        public event Action<string> StringChangedForButton3;

        public void SetCount(int newCount)
        {
            Count = newCount;
            CountChanged?.Invoke(Count);
        }
        public void SetName(string Name)
        {
            this.Name = Name;
            StringChanged?.Invoke(Name);
        }
        public void OnStringChanged()
        {
            StringChanged?.Invoke(Name);
        }

        public void OnStringChangedForButton1(string Name)
        {
          this.Name=Name;
            StringChangedForButton1?.Invoke(Name);
        }
        public void OnStringChangedForButton2(string Name)
        {
            StringChangedForButton2?.Invoke(Name);
        }
        public void OnStringChangedForButton3(string Name)
        {
            StringChangedForButton3?.Invoke(Name);
        }
    }
}
