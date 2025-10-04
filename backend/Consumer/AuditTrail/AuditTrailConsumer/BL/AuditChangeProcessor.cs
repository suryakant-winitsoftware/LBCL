using Newtonsoft.Json.Linq;
using Winit.Modules.AuditTrail.Model.Classes;

namespace AuditTrailConsumer.BL
{
    public class AuditChangeProcessor
    {
        public List<ChangeLog> ProcessDiff(JObject diff, List<ChangeLog>? changeLogs, string parentKey = "")
        {
            if (changeLogs == null)
            {
                changeLogs = new List<ChangeLog>();
            }
            foreach (var property in diff.Properties())
            {
                string currentKey = string.IsNullOrEmpty(parentKey)
                    ? property.Name
                    : $"{parentKey}.{property.Name}";

                var value = property.Value;

                if (value is JArray array)
                {
                    // Scalar Difference
                    if (array.Count == 2)
                    {
                        changeLogs.Add(new ChangeLog
                        {
                            Field = currentKey,
                            OldValue = array[0]?.ToString(),
                            NewValue = array[1]?.ToString()
                        });
                    }
                    else if (array.Count == 1)
                    {
                        // New value only: ["newValue"]
                        changeLogs.Add(new ChangeLog
                        {
                            Field = currentKey,
                            OldValue = null, // Old value is absent
                            NewValue = array[0]?.ToString()
                        });
                    }
                    else
                    {
                        // Handle complex nested structures in arrays (if needed)
                        for (int i = 0; i < array.Count; i++)
                        {
                            var arrayElement = array[i];
                            string arrayKey = $"{currentKey}[{i + 1}]";

                            if (arrayElement is JObject nestedObj)
                            {
                                // Process nested object in the array
                                ProcessDiff(nestedObj, changeLogs, arrayKey);
                            }
                            else
                            {
                                // Handle scalar values or other unexpected formats
                                changeLogs.Add(new ChangeLog
                                {
                                    Field = arrayKey,
                                    OldValue = null,
                                    NewValue = arrayElement?.ToString()
                                });
                            }
                        }
                    }
                }
                else if (value is JObject nestedObj)
                {
                    if (nestedObj.ContainsKey("_t") && nestedObj["_t"]?.ToString() == "a")
                    {
                        // Handle Array Changes
                        foreach (var arrayProperty in nestedObj.Properties())
                        {
                            if (arrayProperty.Name == "_t") continue;

                            string arrayKey = $"{currentKey}[{arrayProperty.Name}]";
                            if (arrayProperty.Value is JArray arrayDiff && arrayDiff.Count == 2)
                            {
                                // Direct array differences (e.g., ["oldValue", "newValue"])
                                changeLogs.Add(new ChangeLog
                                {
                                    Field = arrayKey,
                                    OldValue = arrayDiff[0]?.ToString(),
                                    NewValue = arrayDiff[1]?.ToString()
                                });
                            }
                            else if (arrayProperty.Value is JObject nestedArrayObj)
                            {
                                // Nested object in the array (e.g., {"LineNumber": ["oldValue", "newValue"]})
                                foreach (var nestedProperty in nestedArrayObj.Properties())
                                {
                                    string nestedArrayKey = $"{arrayKey}.{nestedProperty.Name}";
                                    var nestedArrayDiff = nestedProperty.Value as JArray;

                                    if (nestedArrayDiff != null && nestedArrayDiff.Count == 2)
                                    {
                                        changeLogs.Add(new ChangeLog
                                        {
                                            Field = nestedArrayKey,
                                            OldValue = nestedArrayDiff[0]?.ToString(),
                                            NewValue = nestedArrayDiff[1]?.ToString()
                                        });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Nested Object, Process Recursively
                        changeLogs.AddRange(ProcessDiff(nestedObj, changeLogs, currentKey));
                    }
                }
            }

            return changeLogs;
        }
        public List<ChangeLog> ExtractChanges(JObject diff, List<ChangeLog>? changes, string parentKey = "")
        {
            if (changes == null)
            {
                changes = new List<ChangeLog>();
            }
            foreach (var property in diff.Properties())
            {
                if (property.Name == "_t") continue;
                string currentKey = string.IsNullOrEmpty(parentKey) ? property.Name : $"{parentKey}.{property.Name}";
                var value = property.Value;

                if (value is JArray array)
                {
                    if (array.Count == 2 && array[0] is JObject oldObj && array[1] is JObject newObj)
                    {
                        // If the array contains two objects, compare them field by field
                        ExtractChangesForNestedObjects(oldObj, newObj, changes, currentKey);
                    }
                    else if (array.Count == 2)
                    {
                        // Standard diff: ["oldValue", "newValue"]
                        changes.Add(new ChangeLog
                        {
                            Field = currentKey,
                            OldValue = array[0]?.ToString(),
                            NewValue = array[1]?.ToString()
                        });
                    }
                    else if (array.Count == 1)
                    {
                        // New value only: ["newValue"]
                        changes.Add(new ChangeLog
                        {
                            Field = currentKey,
                            OldValue = null,
                            NewValue = array[0]?.ToString()
                        });
                    }
                    else
                    {
                        // Process nested arrays or objects inside the array
                        for (int i = 0; i < array.Count; i++)
                        {
                            var arrayElement = array[i];
                            string arrayKey = $"{currentKey}[{i}]";

                            if (arrayElement is JObject nestedObj)
                            {
                                ExtractChanges(nestedObj, changes, arrayKey); // Recursive call
                            }
                            else
                            {
                                changes.Add(new ChangeLog
                                {
                                    Field = arrayKey,
                                    OldValue = null,
                                    NewValue = arrayElement?.ToString()
                                });
                            }
                        }
                    }
                }
                else if (value is JObject nestedDiff)
                {
                    // Nested object: Process recursively
                    ExtractChanges(nestedDiff, changes, currentKey);
                }
                else if (value is JValue scalar)
                {
                    // Handle scalar values (e.g., primitive types like string, number, etc.)
                    changes.Add(new ChangeLog
                    {
                        Field = currentKey,
                        OldValue = null,
                        NewValue = scalar.ToString()
                    });
                }
            }
            return changes;
        }

        /// <summary>
        /// Extract changes field by field for nested objects.
        /// </summary>
        private void ExtractChangesForNestedObjects(JObject oldObj, JObject newObj, List<ChangeLog> changes, string parentKey)
        {
            // Get all unique property names from both objects
            var allProperties = new HashSet<string>(oldObj.Properties().Select(p => p.Name)
                .Union(newObj.Properties().Select(p => p.Name)));

            foreach (var property in allProperties)
            {
                string currentKey = $"{parentKey}.{property}";
                var oldValue = oldObj[property]?.ToString();
                var newValue = newObj[property]?.ToString();

                // Add the change for this specific field
                changes.Add(new ChangeLog
                {
                    Field = currentKey,
                    OldValue = oldValue,
                    NewValue = newValue
                });
            }
        }
    }
}
