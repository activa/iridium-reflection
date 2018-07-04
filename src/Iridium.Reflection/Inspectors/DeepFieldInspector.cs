using System;
using System.Linq;
using System.Reflection;

namespace Iridium.Reflection
{
    public class DeepFieldInspector
    {
        private readonly object _obj;

        public DeepFieldInspector(object obj)
        {
            _obj = obj;
        }

        public MemberWithObjectInspector ForPath(string path) => GetMember(path, _obj);

        public object GetValue(string path)
        {
            return ForPath(path).GetValue();
        }

        public void SetValue(string path, object value)
        {
            ForPath(path).SetValue(value);
        }

        public T GetValue<T>(string path)
        {
            return ForPath(path).GetValue<T>();
        }

        public bool HasValue(string path)
        {
            return ForPath(path).HasValue;
        }

        public static bool HasValue(string path, object obj)
        {
            return new DeepFieldInspector(obj).ForPath(path).HasValue;
        }

        public static object GetValue(string path, object obj)
        {
            return new DeepFieldInspector(obj).ForPath(path).GetValue();
        }

        public static T GetValue<T>(string path, object obj)
        {
            return new DeepFieldInspector(obj).ForPath(path).GetValue<T>();
        }

        private static MemberWithObjectInspector GetMember(string path, object obj)
        {
            int dotIndex = path.IndexOf('.');

            string field = dotIndex > 0 ? path.Substring(0, dotIndex) : path;
            string subField = dotIndex > 0 ? path.Substring(dotIndex + 1) : null;

            if (obj is Type type)
                obj = null;
            else
                type = obj.GetType();

            var fieldInfo = type.Inspector().GetDeclaredMembers(field).FirstOrDefault(m => m is FieldInfo || m is PropertyInfo || (m is MethodInfo && ((MethodInfo)m).GetParameters().Length == 0));

            if (fieldInfo == null)
                return new MemberWithObjectInspector(null, obj);

            var fieldInspector = fieldInfo.Inspector();

            if (subField == null)
                return new MemberWithObjectInspector(fieldInspector, obj);

            return GetMember(subField, fieldInspector.GetValue(obj));
        }

        public Func<string, object> Getter() => GetValue;
        public Func<string, T> Getter<T>() => path => GetValue<T>(path);
        public Func<object> Getter(string path) => () => GetValue(path);
        public Func<T> Getter<T>(string path) => () => GetValue<T>(path);
    }
}