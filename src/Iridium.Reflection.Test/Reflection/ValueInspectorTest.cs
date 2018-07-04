using NUnit.Framework;

namespace Iridium.Reflection.Test
{
    [TestFixture]
    public class ValueInspectorTest
    {
        [Test]
        public void SimpleAnonymous()
        {
            var obj = new {Field1 = 1, Field2 = 2};

            var inspector = obj.DeepFieldInspector();

            Assert.That(inspector.GetValue("Field1"), Is.EqualTo(1));
            Assert.That(inspector.GetValue("Field2"), Is.EqualTo(2));

            var inspectorField1 = obj.DeepFieldInspector().ForPath("Field1");
            var inspectorField2 = obj.DeepFieldInspector().ForPath("Field2");

            Assert.That(inspectorField1.GetValue(), Is.EqualTo(1));
            Assert.That(inspectorField2.GetValue(), Is.EqualTo(2));
            Assert.That(inspectorField1.HasValue);
            Assert.That(inspectorField2.HasValue);
        }


        [Test]
        public void NestedAnonymous()
        {
            var obj = new
            {
                Field1 = 1,
                FieldNull = (object)null,
                Obj1 = new
                {
                    Field2 = 2,
                    Obj2 = new
                    {
                        Field3 = 3
                    }
                }
            };

            var inspector = obj.DeepFieldInspector();

            Assert.That(inspector.GetValue("Field1"), Is.EqualTo(1));
            Assert.That(inspector.HasValue("Field1"), Is.True);
            Assert.That(inspector.ForPath("Field1").GetValue(), Is.EqualTo(1));
            Assert.That(inspector.ForPath("Field1").HasValue, Is.True);

            Assert.That(inspector.GetValue("FieldNull"), Is.Null);
            Assert.That(inspector.HasValue("FieldNull"), Is.True);

            Assert.That(inspector.GetValue("Obj1.Field2"), Is.EqualTo(2));
            Assert.That(inspector.GetValue("Obj1.Obj2.Field3"), Is.EqualTo(3));
            Assert.That(inspector.GetValue("Obj3.Obj2.Field2"), Is.Null);
            Assert.That(inspector.GetValue("Obj1.Obj3.Field2"), Is.Null);
            Assert.That(inspector.HasValue("Obj3.Obj2.Field2"), Is.False);
            Assert.That(inspector.HasValue("Obj1.Obj3.Field2"), Is.False);

            Assert.That(inspector.HasValue("Field1"), Is.True);
            Assert.That(inspector.HasValue("Field2"), Is.False);
            Assert.That(inspector.HasValue("Field3"), Is.False);
        }

        [Test]
        public void NestedConcrete()
        {
            var inspector = new NestedClass().DeepFieldInspector();

            Assert.That(inspector.GetValue("Field1"), Is.EqualTo(1));
            Assert.That(inspector.GetValue("Field2"), Is.EqualTo(2));
            Assert.That(inspector.GetValue("Method1"), Is.EqualTo(999));
            Assert.That(inspector.GetValue("Inner1.Field1"), Is.EqualTo(11));
            Assert.That(inspector.GetValue("Inner1.Field2"), Is.EqualTo(12));
            Assert.That(inspector.GetValue("Inner1.Field3"), Is.EqualTo(13));
            Assert.That(inspector.GetValue("Inner2.Field1"), Is.EqualTo(21));
            Assert.That(inspector.GetValue("Inner2.Field2"), Is.EqualTo(22));
            Assert.That(inspector.GetValue("Inner2.Field3"), Is.EqualTo(23));
            Assert.That(inspector.GetValue("Inner1.Field4"), Is.Null);
            Assert.That(inspector.GetValue("Inner2.Field4"), Is.Null);
       }

        [Test]
        public void NestedStatic()
        {
            var inspector = typeof(StaticNestedClass).DeepFieldInspector();

            Assert.That(inspector.GetValue("Field1"), Is.EqualTo(1));
            Assert.That(inspector.GetValue("Field2"), Is.EqualTo(2));
            Assert.That(inspector.GetValue("Inner1.Field1"), Is.EqualTo(11));
            Assert.That(inspector.GetValue("Inner1.Field2"), Is.EqualTo(12));
            Assert.That(inspector.GetValue("Inner1.Field3"), Is.EqualTo(13));
            Assert.That(inspector.GetValue("Inner2.Field1"), Is.EqualTo(21));
            Assert.That(inspector.GetValue("Inner2.Field2"), Is.EqualTo(22));
            Assert.That(inspector.GetValue("Inner2.Field3"), Is.EqualTo(23));
            Assert.That(inspector.GetValue("Inner1.Field4"), Is.Null);
            Assert.That(inspector.GetValue("Inner2.Field4"), Is.Null);
        }


        public class NestedClass
        {
            public int Field1 = 1;
            public int Field2 { get; } = 2;
            
            public int Method1(int i) => i;
            public int Method1() => 999;

            public class InnerClass1
            {
                public int Field1 = 11;
                public int Field2 { get; } = 12;
                public static int Field3 = 13;
            }

            public class InnerClass2
            {
                public int Field1 = 21;
                public int Field2 { get; } = 22;
                public static int Field3 = 23;
            }

            public InnerClass1 Inner1 = new InnerClass1();
            public static InnerClass2 Inner2 = new InnerClass2();
        }

        public static class StaticNestedClass
        {
            public static int Field1 = 1;
            public static int Field2 { get; } = 2;

            public class InnerClass1
            {
                public int Field1 = 11;
                public int Field2 { get; } = 12;
                public static int Field3 = 13;
            }

            public class InnerClass2
            {
                public int Field1 = 21;
                public int Field2 { get; } = 22;
                public static int Field3 = 23;
            }

            public static InnerClass1 Inner1 = new InnerClass1();
            public static InnerClass2 Inner2 = new InnerClass2();

        }

    }
}