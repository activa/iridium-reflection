﻿#region License
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
using System.Globalization;
using System.Linq;

namespace Iridium.Reflection
{
    public static class StringConverter
    {
        private class TypedStringConverter<T> : IStringConverter
        {
            private readonly IStringConverter<T> _converter;

            public TypedStringConverter(IStringConverter<T> converter)
            {
                _converter = converter;
            }

            public bool TryConvert(string s, Type targetType, out object value)
            {
                value = null;

                if (!typeof(T).Inspector().IsAssignableFrom(targetType))
                    return false;

                if (_converter.TryConvert(s, out var typedValue))
                {
                    value = typedValue;
                    return true;
                }

                return false;
            }
        }

        private static List<IStringConverter> _stringConverters;

        private static readonly object _staticLock = new object();
        private static string[] _dateFormats = new[] { "yyyyMMdd", "yyyy-MM-dd", "yyyy.MM.dd", "yyyy/MM/dd", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-ddTHH:mm:ss" };

        public static void UnregisterAllStringConverters()
        {
            lock (_staticLock)
            {
                _stringConverters = null;
            }
        }

        public static void UnregisterStringConverter(IStringConverter stringConverter)
        {
            lock (_staticLock)
            {
                _stringConverters?.Remove(stringConverter);
            }
        }

        public static void RegisterStringConverter(IStringConverter stringConverter)
        {
            lock (_staticLock)
            {
                _stringConverters = _stringConverters ?? new List<IStringConverter>();

                _stringConverters.Add(stringConverter);
            }
        }

        public static void RegisterStringConverter<T>(IStringConverter<T> stringConverter)
        {
            RegisterStringConverter(new TypedStringConverter<T>(stringConverter));
        }

        public static void RegisterDateFormats(params string[] dateFormats)
        {
            _dateFormats = dateFormats;
        }

        public static void RegisterDateFormat(string dateFormat, bool replace = false)
        {
            _dateFormats = replace ? new[] {dateFormat} : _dateFormats.Union(new[] {dateFormat}).ToArray();
        }

        public static T To<T>(this string stringValue, params string[] dateFormats)
        {
            return Convert<T>(stringValue, dateFormats);
        }

        public static object To(this string stringValue, Type targetType, params string[] dateFormats)
        {
            return Convert(stringValue, targetType, dateFormats);
        }

        public static T Convert<T>(this string stringValue, params string[] dateFormats)
        {
            return (T) Convert(stringValue, typeof (T), dateFormats);
        }

        public static object Convert(this string stringValue, Type targetType, params string[] dateFormats)
        {
            if (targetType == typeof(string))
                return stringValue;

            var targetTypeInspector = targetType.Inspector();

            if (string.IsNullOrWhiteSpace(stringValue))
                return targetTypeInspector.DefaultValue();

            object returnValue = null;
            
            targetType = targetTypeInspector.RealType;
            
            if (_stringConverters != null)
                if (_stringConverters.Any(converter => converter.TryConvert(stringValue, targetType, out returnValue)))
                    return returnValue;

            if (targetTypeInspector.IsEnum)
            {
                if (char.IsNumber(stringValue, 0))
                {
                    if (Int64.TryParse(stringValue, out var longValue))
                    {
                        returnValue = Enum.ToObject(targetType, longValue);

                        if (Enum.IsDefined(targetType, returnValue))
                            return returnValue;
                    }
                }
                else
                {
                    if (Enum.IsDefined(targetType, stringValue))
                        return Enum.Parse(targetType, stringValue, true);
                }

                return targetTypeInspector.DefaultValue();
            }
            else if (targetTypeInspector.Is(TypeFlags.SignedInteger))
            {
                if (!Int64.TryParse(stringValue, out var longValue))
                    returnValue = null;
                else
                    returnValue = longValue;
            }
            else if (targetTypeInspector.Is(TypeFlags.Single|TypeFlags.Double))
            {
                if (!Double.TryParse(stringValue.Replace(',', '.'), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var doubleValue))
                    returnValue = null;
                else
                    returnValue = doubleValue;
            }
            else if (targetTypeInspector.Is(TypeFlags.Decimal))
            {
                if (!Decimal.TryParse(stringValue.Replace(',', '.'), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var decimalValue))
                    returnValue = null;
                else
                    returnValue = decimalValue;
            }
            else if (targetTypeInspector.Is(TypeFlags.UnsignedInteger))
            {
                if (!UInt64.TryParse(stringValue, out var longValue))
                    returnValue = null;
                else
                    returnValue = longValue;
            }
            else if (targetType == typeof (DateTime))
            {
                if (dateFormats.Length == 0)
                    dateFormats = _dateFormats;

                if (!DateTime.TryParseExact(stringValue, dateFormats ?? _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out var dateTime))
                {
                    if (!DateTime.TryParse(stringValue, out dateTime))
                    {
                        double? seconds = Convert<double?>(stringValue);

                        if (seconds == null)
                            returnValue = null;
                        else
                            returnValue = new DateTime(1970, 1, 1).AddSeconds(seconds.Value);
                    }
                    else
                    {
                        returnValue = dateTime;
                    }
                }
                else
                    returnValue = dateTime;
            }
            else if (targetType == typeof (bool))
            {
                returnValue = (stringValue.ToUpper() == "TRUE" || stringValue == "1" || stringValue.ToUpper() == "Y" || stringValue.ToUpper() == "YES" || stringValue.ToUpper() == "T");
            }
            else if (targetType == typeof(char))
            {
                if (stringValue.Length == 1)
                    returnValue = stringValue[0];
                else
                    returnValue = null;
            }
            else
            {
                var implicitOperator = targetTypeInspector.ImplicitConversion(typeof(string));

                if (implicitOperator != null)
                    returnValue = implicitOperator(stringValue);
            }

            if (returnValue == null)
                return targetTypeInspector.DefaultValue();

            try
            {
                return System.Convert.ChangeType(returnValue, targetType, null);
            }
            catch
            {
                return targetTypeInspector.DefaultValue();
            }
        }
    }
}