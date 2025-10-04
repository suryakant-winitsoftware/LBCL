using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Base.BL.Helper.Classes
{
    public class FilterHelper : IFilterHelper
    {
        public async Task<bool> MatchFilter<T>(T item, FilterCriteria filterCriteria)
        {
            //var propertyValue = item.GetPropertyValue<T>(filterCriteria.Name);

            var method = typeof(ObjectExtensions)
            .GetMethod("GetPropertyValue")
            .MakeGenericMethod(filterCriteria.DataType);

            var propertyValue = method.Invoke(item, new object[] { item, filterCriteria.Name });

            if (propertyValue != null)
            {
                // Handle filtering based on filter type (Equal, NotEqual, Like)
                switch (filterCriteria.Type)
                {
                    case FilterType.Equal:
                        return CompareValues(propertyValue, filterCriteria.Value);
                    case FilterType.NotEqual:
                        return !CompareValues(propertyValue, filterCriteria.Value);
                    case FilterType.Like when propertyValue is string stringValue:
                        var filterValue = filterCriteria.Value?.ToString();
                        return filterValue != null && stringValue.Contains(filterValue, StringComparison.OrdinalIgnoreCase);
                    case FilterType.In:
                        return Contains(propertyValue, filterCriteria.Value);
                }
            }

            return false; // Property not found or value is null
        }

        private bool Contains(object propertyValue, object filterValue)
        {
            if (propertyValue == null) return false;

            if (propertyValue is IEnumerable enumerable)
            {
                return enumerable.Cast<object>().Contains(filterValue);
            }

            return false;
        }

        private bool CompareValues(object propertyValue, object filterValue)
        {
            if (propertyValue.GetType() == filterValue.GetType())
            {
                return propertyValue.Equals(filterValue);
            }
            else if (propertyValue is bool && filterValue is string filterStringBoolean)
            {
                if (bool.TryParse(filterStringBoolean, out bool parsedFilterValue))
                {
                    return (bool)propertyValue == parsedFilterValue;
                }
            }
            else if (propertyValue is decimal && filterValue is string filterStringDecimal)
            {
                if (decimal.TryParse(filterStringDecimal, out decimal parsedFilterValue))
                {
                    return (decimal)propertyValue == parsedFilterValue;
                }
            }
            else if (propertyValue is int && filterValue is string filterStringInt)
            {
                if (int.TryParse(filterStringInt, out int parsedFilterValue))
                {
                    return (int)propertyValue == parsedFilterValue;
                }
            }
            else if (propertyValue is Int64 && filterValue is string filterStringInt64)
            {
                if (Int64.TryParse(filterStringInt64, out Int64 parsedFilterValue))
                {
                    return (Int64)propertyValue == parsedFilterValue;
                }
            }
            else if (propertyValue is float && filterValue is string filterStringfloat)
            {
                if (float.TryParse(filterStringfloat, out float parsedFilterValue))
                {
                    return (float)propertyValue == parsedFilterValue;
                }
            }
            else if (propertyValue is DateTime && filterValue is string filterStringDateTime)
            {
                if (DateTime.TryParse(filterStringDateTime, out DateTime parsedFilterValue))
                {
                    return (DateTime)propertyValue == parsedFilterValue;
                }
            }
            // Handle other types or return false for unhandled comparisons
            return false;
        }
        /// <summary>
        /// Filter based on filterCriteriaList and filterMode
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="filterCriteriaList"></param>
        /// <param name="filterMode"></param>
        /// <returns></returns>
        public virtual async Task<List<T>> ApplyFilter<T>(List<T> sourceList, List<FilterCriteria> filterCriteriaList, FilterMode filterMode)
        {
            List<T> filteredList = new List<T>();

            foreach (var item in sourceList)
            {
                bool anyCriteriaMatched = false;

                foreach (var filterCriteria in filterCriteriaList)
                {
                    bool criteriaMatched = await MatchFilter(item, filterCriteria);

                    if (filterMode == FilterMode.And)
                    {
                        // If AND mode and any criteria doesn't match, break
                        if (!criteriaMatched)
                        {
                            anyCriteriaMatched = false;
                            break;
                        }
                        else
                        {
                            anyCriteriaMatched = true; // Set the flag if criteria matches
                        }
                    }
                    else if (filterMode == FilterMode.Or)
                    {
                        // If OR mode and any criteria matches, set the flag and break
                        if (criteriaMatched)
                        {
                            anyCriteriaMatched = true;
                            break;
                        }
                    }
                }

                if (anyCriteriaMatched || filterMode == FilterMode.And && filterCriteriaList.Count == 0)
                {
                    filteredList.Add(item);
                }
            }

            return filteredList;
        }
        /// <summary>
        /// Search based on searchString & properties to Search
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="searchString"></param>
        /// <param name="propertiesToSearch"></param>
        /// <returns></returns>
        public virtual async Task<List<T>> ApplySearch<T>(List<T> sourceList, string searchString, List<string> propertiesToSearch)
        {
            // Initialize the filtered result as an empty list
            List<T> searchedList = new List<T>();

            // Perform a case-insensitive search on specified properties
            foreach (var item in sourceList)
            {
                // Check if any of the specified properties contain the search string
                if (propertiesToSearch.Any(propName =>
                {
                    var propertyValue = item.GetPropertyValue<string>(propName);
                    return propertyValue != null &&
                        propertyValue.ToString().IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
                }))
                {
                    searchedList.Add(item);
                }
            }

            return searchedList;
        }

        /// <summary>
        /// Filter From List
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="items"></param>
        /// <param name="attributeListSelector"></param>
        /// <param name="filterCriteria"></param>
        /// <returns></returns>
        public async Task<List<T1>> FilterFromList<T1, U>(
            IEnumerable<T1> items,
            Func<T1, IEnumerable<U>> attributeListSelector,
            List<(Func<U, string> fieldSelector, string fieldSelectorValue)> filterCriteria)
        {
            if (items == null || filterCriteria == null || filterCriteria.Count == 0)
            {
                return null;
            }
            else
            {
                Func<T1, bool> filterPredicate = item =>
                {
                    var attributeList = attributeListSelector(item);
                    if (attributeList != null)
                    {
                        foreach (var attribute in attributeList)
                        {
                            if (attribute == null)
                            {
                                continue;
                            }

                            bool criteriaMatch = true; // Initialize to true for "AND" logic

                            foreach (var criteria in filterCriteria)
                            {
                                var fieldSelector = criteria.fieldSelector;
                                var fieldSelectorValue = criteria.fieldSelectorValue;
                                string fieldValue = fieldSelector(attribute);

                                if (fieldValue != fieldSelectorValue)
                                {
                                    criteriaMatch = false;
                                    break; // Exit the loop if any criterion fails
                                }
                            }

                            if (criteriaMatch)
                            {
                                return true; // Exit and return true if all criteria match
                            }
                        }
                    }
                    return false;
                };

                return items.Where(filterPredicate).ToList();
            }
        }
        /// <summary>
        /// Filter from dictionary
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="items"></param>
        /// <param name="attributeListSelector"></param>
        /// <param name="filterCriteria"></param>
        /// <param name="key">Optinal field. If null system will check for all data. If passed system will check for specific keys only</param>
        /// <returns></returns>
        public async Task<List<T1>> FilterFromDictionary<T1, U>(
            IEnumerable<T1> items,
            Func<T1, Dictionary<string, U>> attributeListSelector,
            List<(Func<U, string> fieldSelector, string fieldSelectorValue)> filterCriteria,
            string key = null)
        {
            if (items == null || filterCriteria == null || filterCriteria.Count == 0)
            {
                return null;
            }
            else
            {
                Func<T1, bool> filterPredicate = item =>
                {
                    var attributeDictionary = attributeListSelector(item);
                    if (attributeDictionary != null)
                    {
                        // Filter the dictionary based on the specified key or all keys
                        var filteredAttributes = key != null
                            ? attributeDictionary.Where(kvp => kvp.Key == key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                            : attributeDictionary;
                        foreach (var kvp in filteredAttributes)
                        {
                            U value = kvp.Value;

                            bool allCriteriaMatch = true;

                            foreach (var criteria in filterCriteria)
                            {
                                var fieldSelector = criteria.fieldSelector;
                                var fieldSelectorValue = criteria.fieldSelectorValue;
                                string fieldValue = fieldSelector(value);

                                if (fieldValue == null || !fieldValue.Equals(fieldSelectorValue))
                                {
                                    allCriteriaMatch = false;
                                    break; // Exit the loop as soon as one condition fails
                                }
                            }

                            if (allCriteriaMatch)
                            {
                                return true; // Return true if all criteria match
                            }
                        }
                    }
                    return false;
                };

                return items.Where(filterPredicate).ToList();
            }
        }
    }
}
