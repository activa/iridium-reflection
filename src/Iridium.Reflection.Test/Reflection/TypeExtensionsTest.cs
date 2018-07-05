using System;
using NUnit.Framework;

namespace Iridium.Reflection.Test
{
    [TestFixture]
    public class TypeExtensionsTest
    {
        [Test]
        public void IsNullable()
        {
            Assert.IsFalse(typeof(object).Inspector().IsNullable);
            Assert.IsFalse(typeof(int).Inspector().IsNullable);
            Assert.IsTrue(typeof(int?).Inspector().IsNullable);
            Assert.IsFalse(typeof(string).Inspector().IsNullable);
            Assert.IsFalse(typeof(DateTime).Inspector().IsNullable);
        }

        [Test]
        public void CanBeNull()
        {
            Assert.IsTrue(typeof(object).Inspector().CanBeNull);
            Assert.IsFalse(typeof(int).Inspector().CanBeNull);
            Assert.IsTrue(typeof(int?).Inspector().CanBeNull);
            Assert.IsTrue(typeof(string).Inspector().CanBeNull);
            Assert.IsFalse(typeof(DateTime).Inspector().CanBeNull);
        }

        [Test]
        public void DefaultValue()
        {
            Assert.IsNull(typeof(object).Inspector().DefaultValue());
            Assert.AreEqual(0, typeof(int).Inspector().DefaultValue());
            Assert.IsNull(typeof(int?).Inspector().DefaultValue());
            Assert.IsNull(typeof(string).Inspector().DefaultValue());
            Assert.AreEqual(new DateTime(), typeof(DateTime).Inspector().DefaultValue());
        }

        [Test]
        public void GetRealType()
        {
            Assert.AreEqual(typeof(object), typeof(object).Inspector().RealType);
            Assert.AreEqual(typeof(int), typeof(int?).Inspector().RealType);
            Assert.AreEqual(typeof(string), typeof(string).Inspector().RealType);
        }

        private class Test1Attribute  : Attribute
        {
            
        }

        [Test1]
        private class TestClass1
        {
            
        }

        private class TestClass2
        {

        }

        [Test]
        public void GetAttribute()
        {
            Assert.IsInstanceOf<Test1Attribute>(typeof(TestClass1).Inspector().GetAttribute<Test1Attribute>(false));
            Assert.IsNull(typeof(TestClass2).Inspector().GetAttribute<Test1Attribute>(false));
        }

        private class Class1
        {
            private int _n;

            public Class1(int n)
            {
                _n = n;
            }

            public int Value => _n;

            public static implicit operator int(Class1 c) { return c.Value; }
            public static implicit operator string(Class1 c) { return c.Value.ToString(); }
            public static implicit operator Class1(int i) { return new Class1(i);}
            public static implicit operator Class1(string s) { return new Class1(s.To<int>());}

        }

        private class Class2 : Class1
        {
            public Class2(int n) : base(n)
            {
            }


        }


        [Test]
        public void ImplicitConversions()
        {
            Func<object, object> conv;

            conv = typeof(Class1).Inspector().ImplicitConversion(typeof(int));

            Assert.That(conv, Is.Not.Null);
            Assert.That(conv(123), Is.InstanceOf<Class1>() );
            Assert.That(((Class1)conv(123)).Value, Is.EqualTo(123));

            conv = typeof(Class1).Inspector().ImplicitConversion(typeof(string));

            Assert.That(conv, Is.Not.Null);
            Assert.That(conv("123"), Is.InstanceOf<Class1>() );
            Assert.That(((Class1)conv("123")).Value, Is.EqualTo(123));

            conv = typeof(Class1).Inspector().ImplicitConversion(typeof(double));

            Assert.That(conv, Is.Null);

            conv = typeof(int).Inspector().ImplicitConversion(typeof(Class1));

            Assert.That(conv, Is.Not.Null);
            Assert.That(conv(new Class1(123)), Is.InstanceOf<int>() );
            Assert.That((int)conv(new Class1(123)), Is.EqualTo(123));

            conv = typeof(string).Inspector().ImplicitConversion(typeof(Class1));

            Assert.That(conv, Is.Not.Null);
            Assert.That(conv(new Class1(123)), Is.InstanceOf<string>() );
            Assert.That((string)conv(new Class1(123)), Is.EqualTo("123"));

            conv = typeof(double).Inspector().ImplicitConversion(typeof(Class1));

            Assert.That(conv, Is.Null);

            conv = typeof(string).Inspector().ImplicitConversion(typeof(Class2));

            Assert.That(conv, Is.Not.Null);
            Assert.That(conv(new Class2(123)), Is.InstanceOf<string>() );
            Assert.That((string)conv(new Class2(123)), Is.EqualTo("123"));

//            conv = typeof(decimal).Inspector().ImplicitConversion(typeof(Class1));
//
//            Assert.That(conv, Is.Not.Null);
//            Assert.That(conv(new Class1(123)), Is.InstanceOf<decimal>() );
//            Assert.That((decimal)conv(new Class1(123)), Is.EqualTo(123m));
        }
    }
}