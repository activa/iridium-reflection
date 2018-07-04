using System;

namespace Iridium.Reflection
{
    public class MemberWithObjectInspector
    {
        private readonly MemberInspector _memberInspector;
        private readonly object _obj;

        public MemberWithObjectInspector(MemberInspector inspector, object obj)
        {
            _obj = obj;
            _memberInspector = inspector;
        }

        public Type DeclaringType => _memberInspector.DeclaringType;

        public bool CanRead => _memberInspector != null && _memberInspector.CanRead;
        public bool CanWrite => _memberInspector != null && _memberInspector.CanWrite;
        public bool HasValue => _memberInspector != null;
        public bool IsStatic => _memberInspector != null && _memberInspector.IsStatic;

        public object GetValue() => _memberInspector?.GetValue(_obj);
        public T GetValue<T>() => GetValue().Convert<T>();
        public void SetValue(object value) => _memberInspector?.SetValue(_obj, value);
    }
}