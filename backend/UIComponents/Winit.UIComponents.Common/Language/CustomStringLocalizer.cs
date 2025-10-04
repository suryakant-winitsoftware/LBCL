using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Resources;
using System.Collections;
namespace Winit.UIComponents.Common.Language
{
    public class CustomStringLocalizer<T> : IStringLocalizer<T>
    {
        private readonly ResourceManager _resourceManager;
        private readonly CultureInfo _culture;

        public CustomStringLocalizer(ResourceManager resourceManager, CultureInfo culture)
        {
            _resourceManager = resourceManager;
            _culture = culture;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var value = _resourceManager.GetString(name, _culture);
                return new LocalizedString(name, value ?? GetStringNotFound(name), value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var format = _resourceManager.GetString(name, _culture);
                var value = string.Format(_culture, format ?? GetStringNotFound(name), arguments);
                return new LocalizedString(name, value, format == null);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }
        //public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        //{
        //    var resourceSet = _resourceManager.GetResourceSet(_culture, true, includeParentCultures);
        //    return resourceSet.Cast<DictionaryEntry>()
        //        .Select(entry => new LocalizedString((string)entry.Key, (string)entry.Value))
        //        .ToList();
        //}

        private string GetStringNotFound(string name)
        {
            // Log missing resource name if necessary
            return $"[{name}]";
        }
    }
}


