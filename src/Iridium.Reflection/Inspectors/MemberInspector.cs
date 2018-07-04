#region License
//=============================================================================
// Iridium-Reflection - Portable .NET Productivity Library 
//
// Copyright (c) 2008-2018 Philippe Leybaert
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//=============================================================================
#endregion

using System;
using System.Linq;
using System.Reflection;

namespace Iridium.Reflection
{
    public class MemberInspector
    {
        public Type Type { get; }
        public MemberInfo MemberInfo { get; }

        public MemberInspector(MemberInfo memberInfo)
        {
            MemberInfo = memberInfo;

            Type = Switch(f => f.FieldType, p => p.PropertyType, m => m.ReturnType, c => c.DeclaringType);
        }

        private T Switch<T>(Func<FieldInfo, T> field = null, Func<PropertyInfo, T> prop = null, Func<MethodInfo, T> method = null, Func<ConstructorInfo, T> constructor = null, T defaultValue = default(T))
        {
            switch (MemberInfo)
            {
                case ConstructorInfo constructorInfo:
                    return constructor == null ? defaultValue : constructor(constructorInfo);
                case FieldInfo fieldInfo:
                    return field == null ? defaultValue : field(fieldInfo);
                case PropertyInfo propertyInfo:
                    return prop == null ? defaultValue : prop(propertyInfo);
                case MethodInfo methodInfo:
                    return method == null ? defaultValue : method(methodInfo);

            }

            return defaultValue;
        }

        public Type DeclaringType => MemberInfo.DeclaringType;
        public string Name => Switch(f => f.Name, p => p.Name, m => m.Name, c => c.Name);
        public bool IsField => MemberInfo is FieldInfo;
        public bool IsProperty => MemberInfo is PropertyInfo;
        public bool IsMethod => MemberInfo is MethodBase;

        public bool HasAttribute(Type attributeType, bool inherit = false)
        {
            return MemberInfo.IsDefined(attributeType, inherit);
        }

        public bool HasAttribute<T>(bool inherit = false) where T : Attribute
        {
            return MemberInfo.IsDefined(typeof(T), inherit);
        }

        public Attribute[] GetAttributes(Type attributeType, bool inherit = false)
        {
            return (Attribute[]) MemberInfo.GetCustomAttributes(attributeType, inherit);
        }

        public Attribute GetAttribute(Type attributeType, bool inherit = false)
        {
            return GetAttributes(attributeType, inherit).FirstOrDefault();
        }

        public T GetAttribute<T>(bool inherit = false) where T : Attribute
        {
            return (T) GetAttribute(typeof (T), inherit);
        }

        public T[] GetAttributes<T>(bool inherit = false) where T : Attribute
        {
            return (T[])GetAttributes(typeof(T), inherit);
        }

        public bool CanRead => Switch(f => true, p => p.CanRead);
        public bool CanWrite => Switch(f => !f.IsInitOnly, p => p.CanWrite);

        public bool IsStatic => Switch(
            field: f => f.IsStatic, 
            prop: p => p.CanRead && p.GetMethod.IsStatic || p.CanWrite && p.SetMethod.IsStatic, 
            method: m => m.IsStatic,
            constructor: c => c.IsStatic
        );

        public bool IsPublic => Switch(
            field: f => f.IsPublic,
            prop: p => p.GetMethod.IsPublic,
            method: m => m.IsPublic,
            constructor: c => c.IsPublic
        );

        public bool IsWritePublic
        {
            get
            {
                if (MemberInfo is PropertyInfo)
                {
                    var propertyInfo = ((PropertyInfo) MemberInfo);

                    return propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsPublic;
                }

                throw new InvalidOperationException(nameof(IsWritePublic));
            }
            
        }


        internal bool MatchBindingFlags(BindingFlags flags)
        {
            if (flags == BindingFlags.Default)
                return true;

            if (IsStatic && (flags & BindingFlags.Static) == 0)
                return false;

            if (!IsStatic && (flags & BindingFlags.Instance) == 0)
                return false;

            if (IsPublic && (flags & BindingFlags.Public) == 0)
                return false;

            if (!IsPublic && (flags & BindingFlags.NonPublic) == 0)
                return false;

            return true;
        }

        public object GetValue(object instance)
        {
            return Switch(
                f => f.GetValue(instance), 
                p => p.GetValue(instance), 
                m => m.GetParameters().Length == 0 ? m.Invoke(instance,new object[0]) : null
                );
        }

        public T GetValue<T>(object instance) => GetValue(instance).Convert<T>();

        public void SetValue(object instance, object value)
        {
            if (MemberInfo is PropertyInfo)
                ((PropertyInfo)MemberInfo).SetValue(instance, value);
            else if (MemberInfo is FieldInfo)
                ((FieldInfo)MemberInfo).SetValue(instance, value);
            else
                throw new InvalidOperationException();
        }

        public Action<object, object> Setter() => SetValue;
        public Action<object> Setter(object target) => value => SetValue(target, value);
        public Action<object, T> Setter<T>() => (target, value) => SetValue(target, value);
        public Action<T> Setter<T>(object target) => value => SetValue(target, value);
        public Func<object, object> Getter() => GetValue;
        public Func<object, T> Getter<T>() => target => GetValue<T>(target);
        public Func<object> Getter(object target) => () => GetValue(target);
        public Func<T> Getter<T>(object target) => () => GetValue<T>(target);

        public MemberWithObjectInspector WithObject(object obj) => new MemberWithObjectInspector(this, obj);
    }
}