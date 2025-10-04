using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.UIState.Interfaces;

namespace Winit.Modules.Common.UIState.Classes
{
    public class StateManagerService : IStateManagerService
    {
        private readonly IEnumerable<IClearableState> _states;

        public StateManagerService(IEnumerable<IClearableState> states)
        {
            _states = states;
        }

        public void ClearAllStates()
        {
            foreach (var state in _states)
            {
                state.Clear();
            }
        }

        public void ClearState<T>() where T : IClearableState
        {
            var state = _states.OfType<T>().FirstOrDefault();
            state?.Clear();
        }
    }

}
