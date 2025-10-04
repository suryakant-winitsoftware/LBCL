using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Common.Model.Classes
{

    public static class DatamanagerGeneric<T>
    {
      
        // one dictionary shared by all callers that use the same T
        private static readonly ConcurrentDictionary<string, T> _data = new();

        // hide the constructor – the generic type itself plays the role of “the” instance


        /*-------------------------------------------------
         *  CRUD‑style API
         *------------------------------------------------*/

        /// <summary>Adds or overwrites a value.</summary>
        public static void Set(string key, T value)
            => _data[key] = value;

        /// <summary>Tries to get a value; returns true if found.</summary>
        public static bool TryGet(string key, out T value)
            => _data.TryGetValue(key, out value);

        /// <summary>Gets a value or the default of T (null for reference types).</summary>
        public static T? GetOrDefault(string key)
            => _data.TryGetValue(key, out var v) ? v : default;

        /// <summary>Removes a value; returns true if something was deleted.</summary>
        public static bool Remove(string key)
            => _data.TryRemove(key, out _);

        /// <summary>Clears all cached items for this T.</summary>
        public static void Clear()
            => _data.Clear();
    }
}
