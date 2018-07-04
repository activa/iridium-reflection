using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Iridium.Reflection.Test
{
    [TestFixture]
    public class TypeFlagsTest
    {
        private TypeFlags[] _allTests;

        [OneTimeSetUp]
        public void Init()
        {
            //var modifiers = new[] {TypeFlags.Array, };
            var modifiers = new[] { TypeFlags.CanBeNull, TypeFlags.ElementCanBeNull, TypeFlags.Nullable, TypeFlags.ElementNullable, TypeFlags.ValueType, TypeFlags.ElementValueType, TypeFlags.Array };

            var types = Enumerable.Range(0, 19).Select(i => (TypeFlags) (1<<i)).ToArray();

            var allTests = new List<TypeFlags>();

            foreach (var type in types)
            {
                allTests.Add(type);

                foreach (var flag in modifiers.AllCombinations())
                {
                    allTests.Add(type | flag);
                    allTests.Add(flag);
                }
                
            }

            _allTests = allTests.ToArray();
        }

        [TestCaseSource(nameof(TypeTestCaseSource))]
        public void Integers(Type type, TypeFlags[] validFlags)
        {
            foreach (var flag in validFlags)
            {
                Assert.That(type.Inspector().Is(flag),Is.True,$"{type.Name} has {type.Inspector().TypeFlags} but didn't match {flag}");
            }

            foreach (var flag in _allTests)
            {
                if (validFlags.Contains(flag))
                    continue;

                Assert.That(type.Inspector().Is(flag), Is.False, $"{type.Name} has {type.Inspector().TypeFlags} and matched {flag} but it shouldn't");
            }
            
            
        }

        public static IEnumerable<TestCaseData> TypeTestCaseSource()
        {
            var primitives = new []
            {
                new { Type = typeof(int), Flags = new[] {TypeFlags.Int32, TypeFlags.Integer32, TypeFlags.Integer, TypeFlags.SignedInteger, TypeFlags.Numeric, TypeFlags.Primitive}},
                new { Type = typeof(uint), Flags = new[] {TypeFlags.UInt32, TypeFlags.Integer32, TypeFlags.Integer, TypeFlags.UnsignedInteger, TypeFlags.Numeric, TypeFlags.Primitive }},
                new { Type = typeof(byte), Flags = new[] {TypeFlags.Byte, TypeFlags.Integer8, TypeFlags.Integer, TypeFlags.UnsignedInteger, TypeFlags.Numeric, TypeFlags.Primitive }},
                new { Type = typeof(char), Flags = new[] {TypeFlags.Char, TypeFlags.Integer16, TypeFlags.Integer, TypeFlags.SignedInteger, TypeFlags.Numeric, TypeFlags.Primitive }},
                new { Type = typeof(sbyte), Flags = new[] {TypeFlags.SByte, TypeFlags.Integer8, TypeFlags.Integer, TypeFlags.SignedInteger, TypeFlags.Numeric, TypeFlags.Primitive }},
            };

            List<TypeFlags> validFlags = new List<TypeFlags>();

            foreach (var primitive in primitives)
            {
                validFlags.Clear();
                

                validFlags.AddRange(primitive.Flags);
                validFlags.Add(TypeFlags.ValueType);
                validFlags.AddRange(primitive.Flags.Select(f => f | TypeFlags.ValueType));

                yield return new TestCaseData(primitive.Type, validFlags.ToArray());

                // nullable

                foreach (var combo in new[] { TypeFlags.Nullable, TypeFlags.CanBeNull, TypeFlags.ValueType }.AllCombinations())
                {
                    validFlags.Add(combo);
                    validFlags.AddRange(primitive.Flags.Select(flag => flag | combo));
                }


                yield return new TestCaseData(typeof(Nullable<>).MakeGenericType(primitive.Type), validFlags.ToArray());

                // array

                validFlags.Clear();

                foreach (var flag in primitive.Flags)
                {
                    validFlags.Add(flag | TypeFlags.Array);

                    validFlags.Add(flag | TypeFlags.Array | TypeFlags.CanBeNull);
                    validFlags.Add(flag | TypeFlags.Array | TypeFlags.ElementValueType);
                    validFlags.Add(flag | TypeFlags.Array | TypeFlags.CanBeNull | TypeFlags.ElementValueType);

                    validFlags.AddRange(new[] { TypeFlags.Array, TypeFlags.CanBeNull, TypeFlags.ElementValueType }.AllCombinations());
                }

                yield return new TestCaseData(primitive.Type.MakeArrayType(), validFlags.ToArray());
            }

            // string

            validFlags.Clear();
            validFlags.AddRange(new[] {TypeFlags.CanBeNull, TypeFlags.String, }.AllCombinations());

            yield return new TestCaseData(typeof(string),validFlags.ToArray());


        }

    }
}