using System.Collections.Generic;
using System.Runtime.Serialization;
using Narupa.Grpc.Interactive;
using Narupa.Grpc.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Narupa.Grpc.Tests
{
    public class SerializationTests
    {

        public static IEnumerable<object> Primitives()
        {
            yield return "abc";
            yield return 1;
            yield return 1.2f;
            yield return true;
            yield return false;
        }

        public static IEnumerable<object> Containers()
        {
            yield return new List<object>
            {
                "abc",
                2,
                4.5f,
                false
            };
            yield return new Dictionary<string, object>
            {
                ["abc"] = 4,
                ["def"] = "xyz",
                ["hgi"] = false
            };
            yield return new List<object>
            {
                "xyz",
                new List<object>
                {
                    4,
                    true
                },
                new Dictionary<string, object>()
                {
                    ["abc"] = 5f,
                    ["def"] = false
                }
            };
            
            yield return new Dictionary<string, object>
            {
                ["xyz"] = 3,
                ["nmo"] = new List<object>
                {
                    true,
                    "fgi"
                },
                ["pqr"] = new Dictionary<string, object>()
                {
                    ["abc"] = 5f,
                    ["def"] = false
                }
            };
        }
        
        [Test]
        public void SerializePrimitive([ValueSource(nameof(Primitives))] object value)
        {
            Assert.AreEqual(value, Serialization.Serialization.ToDataStructure(value));
        }
        
        [Test]
        public void DeserializePrimitive([ValueSource(nameof(Primitives))] object value)
        {
            Assert.AreEqual(value, Serialization.Serialization.FromDataStructure(value));
        }
        
        [Test]
        public void SerializeContainer([ValueSource(nameof(Containers))] object value)
        {
            Assert.AreEqual(value, Serialization.Serialization.ToDataStructure(value));
        }

        [Test]
        public void SerializeClass()
        {
            var test = new TestClass
            {
                PublicInt = 24,
            };
            
            using (var writer = new CSharpObjectWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, test);
                Assert.AreEqual(new Dictionary<string, object>()
                                {
                                    ["PublicInt"] = 24,
                                    ["CustomName"] = 15,
                                    ["Nested"] = new Dictionary<string, object>()
                                    {
                                        ["NestedField"] = "abc"
                                    }
                                }, writer.Object);
            }
        }
        
        [Test]
        public void DeserializeClass()
        {
            var data = new Dictionary<string, object>()
            {
                ["PublicInt"] = 259,
                ["CustomName"] = 15,
                ["Nested"] = new Dictionary<string, object>()
                {
                    ["NestedField"] = "abc"
                }
            };
            
            using (var reader = new CSharpObjectReader(data))
            {
                var serializer = new JsonSerializer();
                var deserialized = serializer.Deserialize<TestClass>(reader);
                Assert.AreEqual(259, deserialized.PublicInt);
            }
        }

        [DataContract]
        public class TestClass
        {
            [DataMember]
            public int PublicInt;

            [DataMember(Name = "CustomName")]
            protected float ProtectedFloat = 15;

            [DataMember]
            public NestedClass Nested = new NestedClass();

            [DataContract]
            public class NestedClass
            {
                [DataMember]
                private string NestedField = "abc";
            }
        }
    }
}