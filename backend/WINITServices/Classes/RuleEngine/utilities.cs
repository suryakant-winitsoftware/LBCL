using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using WINITSharedObjects.Models.RuleEngine;

namespace WINITServices.Classes.RuleEngine
{
    public class utilities
    {
        public static Dictionary<string, object> GetKeyValuePairsFromObject(string jsonString)
        {
            JObject jsonObject = JObject.Parse(jsonString);
            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();

            foreach (var property in jsonObject)
            {
                var key = property.Key;
                var value = GetObjectValue(property.Value);
                keyValuePairs.Add(key, value);
            }

            return keyValuePairs;
        }

        private static object GetObjectValue(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return GetObjectProperties(token as JObject);
                case JTokenType.Array:
                    return GetArrayElements(token as JArray);
                case JTokenType.Integer:
                case JTokenType.Float:
                    return token.Value<decimal>();
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.String:
                    return token.Value<string>();
                case JTokenType.Null:
                    return null;
                default:
                    return null;
            }
        }

        private static object GetObjectProperties(JObject obj)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            foreach (var property in obj)
            {
                var key = property.Key;
                var value = GetObjectValue(property.Value);
                properties.Add(key, value);
            }

            return properties;
        }

        private static object GetArrayElements(JArray array)
        {
            List<object> elements = new List<object>();

            foreach (var item in array)
            {
                var value = GetObjectValue(item);
                elements.Add(value);
            }

            return elements;
        }

        public static T ConvertToGenericType<T>(string jsonString)
        {
            JToken token = JToken.Parse(jsonString);

            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                return token.Value<T>();
            }
            else if (token.Type == JTokenType.String)
            {
                if (typeof(T) == typeof(string))
                {
                    return token.Value<T>();
                }
                else if (decimal.TryParse(token.Value<string>(), out decimal number))
                {
                    return (T)Convert.ChangeType(number, typeof(T));
                }
            }

            throw new ArgumentException("Invalid JSON value kind or format. Expected 'Number'.");
        }
        public static bool EvaluateCondition(ConditionOperator op, object leftOperand, object rightOperand)
        {
            switch (op)
            {
                case ConditionOperator.Equals:
                    return leftOperand.Equals(rightOperand);
                case ConditionOperator.NotEquals:
                    return !leftOperand.Equals(rightOperand);
                case ConditionOperator.GreaterThan:
                    return Comparer<object>.Default.Compare(leftOperand, rightOperand) > 0;
                case ConditionOperator.LessThan:
                    return Comparer<object>.Default.Compare(leftOperand, rightOperand) < 0;
                case ConditionOperator.GreaterThanOrEqual:
                    return Comparer<object>.Default.Compare(leftOperand, rightOperand) >= 0;
                case ConditionOperator.LessThanOrEqual:
                    return Comparer<object>.Default.Compare(leftOperand, rightOperand) <= 0;
                default:
                    throw new ArgumentException("Invalid condition operator.");
            }
        }
    }
}
