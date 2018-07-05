using System;

namespace Iridium.Reflection
{
    public interface IDynamicObject
    {
        bool IsArray { get; }  // single value
        bool IsValue { get; }  // list
        bool IsObject { get; } // key/value pairs

        bool TryGetValue(out object value, out Type type);
        bool TryGetValue(string key, out object value, out Type type);
        bool TryGetValue(int index, out object value, out Type type);
    }
}