using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
namespace WinIT.RuleEngine.UI.Util
{


    public static class ObjectScanner
    {
        public static List<string> GetClassProperties(string targetNamespace)
        {
            List<string> classProperties = new List<string>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type.Namespace != null && type.Namespace.StartsWith(targetNamespace))
                    {
                        PropertyInfo[] properties = type.GetProperties();

                        foreach (PropertyInfo property in properties)
                        {
                            classProperties.Add($"{type.Name}.{property.Name}");
                        }
                    }
                }
            }

            return classProperties;
        }
        public static List<string> GetClassPropertiesTypes(string targetNamespace)
        {
            List<string> classPropertiesWithTypes = new List<string>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type.Namespace != null && type.Namespace.StartsWith(targetNamespace))
                    {
                        PropertyInfo[] properties = type.GetProperties();

                        foreach (PropertyInfo property in properties)
                        {
                            string propertyType = property.PropertyType.Name;
                            string propertyDescription = $"{propertyType}";
                            classPropertiesWithTypes.Add(propertyDescription);
                        }
                    }
                }
            }

            return classPropertiesWithTypes;
        }
    }

}
