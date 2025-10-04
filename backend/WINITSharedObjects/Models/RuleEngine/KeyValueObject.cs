using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.RuleEngine
{
    public class KeyValueObject<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public KeyValueObject()
        {

        }
        public KeyValueObject(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

}
