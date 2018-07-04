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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Iridium.Reflection
{
    [Flags]
    public enum BindingFlags
    {
        FlattenHierarchy = 64,
        Public = 16,
        NonPublic = 32,
        Static = 8,
        Instance = 4,
        DeclaredOnly = 2,
        Default = 0
    }

    public class SmartBinder
    {
        private enum ParameterCompareType
        {
            Exact,
            Assignable,
            Implicit
        }

        private class ParameterComparer : IEqualityComparer<Type>
        {
            private readonly ParameterCompareType _compareType;

            public ParameterComparer(ParameterCompareType compareType)
            {
                _compareType = compareType;
            }

            public bool Equals(Type x, Type y)
            {
                switch (_compareType)
                {
                    case ParameterCompareType.Exact:
                        return x == y;
                    case ParameterCompareType.Assignable:
                        return y.GetTypeInfo().IsAssignableFrom(x.GetTypeInfo());
                    case ParameterCompareType.Implicit:
                        return y.Inspector().GetMethod("op_Implicit", new[] { x }) != null;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public int GetHashCode(Type obj)
            {
                return obj.GetHashCode();
            }
        }

        public static bool MatchParameters(Type[] parameterTypes, ParameterInfo[] parameters)
        {
            var compareTypes = new[] { ParameterCompareType.Exact, ParameterCompareType.Assignable, ParameterCompareType.Implicit };

            return compareTypes.Any(compareType => MatchParameters(parameterTypes, parameters, compareType));
        }

        private static bool MatchParameters(Type[] parameterTypes, ParameterInfo[] parameters, ParameterCompareType compareType)
        {
            if (parameterTypes.Length != parameters.Length)
                return false;

            return parameterTypes.Length == 0 || parameterTypes.SequenceEqual(parameters.Select(p => p.ParameterType), new ParameterComparer(compareType));
        }

        public static T SelectBestMethod<T>(IEnumerable<T> methods, Type[] parameterTypes, BindingFlags bindingFlags = BindingFlags.Default) where T:MethodBase
        {
            var compareTypes = new[] { ParameterCompareType.Exact, ParameterCompareType.Assignable, ParameterCompareType.Implicit };

            return compareTypes
					.Select(compareType => methods.FirstOrDefault(m => MatchParameters(parameterTypes, m.GetParameters(), compareType) && m.Inspector().MatchBindingFlags(bindingFlags) ))
                    .FirstOrDefault(match => match != null);
        }

        private static object[] ConvertParameters(object[] parameters, ParameterInfo[] parameterTypes)
        {
            var newParameters = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                newParameters[i] = parameters[i].Convert(parameterTypes[i].ParameterType);
            }

            return newParameters;
        }

        public static object Invoke(MethodBase method, object[] parameters)
        {
            object[] p = ConvertParameters(parameters, method.GetParameters());

            if (method is ConstructorInfo constructorInfo)
                return constructorInfo.Invoke(p);

            return method.Invoke(null, p);
        }

        public static object Invoke(MethodBase method, object target, object[] parameters)
        {
            return method.Invoke(target, ConvertParameters(parameters, method.GetParameters()));
        }
    }
}
