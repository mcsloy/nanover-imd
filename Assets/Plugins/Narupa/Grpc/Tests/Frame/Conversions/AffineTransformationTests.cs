using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Math;
using Narupa.Grpc.Frame;
using Narupa.Protocol;
using Narupa.Testing;
using NUnit.Framework;
using UnityEngine;

namespace Narupa.Grpc.Tests.Frame.Conversions
{
    public class AffineTransformationTests
    {
        private static readonly int TestCount = 64;

        private static (ValueArray, AffineTransformation) Random9Floats()
        {
            var a = SpatialTestData.GetRandomPosition();
            var b = SpatialTestData.GetRandomPosition();
            var c = SpatialTestData.GetRandomPosition();
            var value = new ValueArray()
            {
                FloatValues = new FloatArray
                {
                    Values = { a.x, a.y, a.z, b.x, b.y, b.z, c.x, c.y, c.z }
                }
            };
            var transformation = new AffineTransformation(a, b, c, Vector3.zero);
            return (value, transformation);
        }
        
        private static (ValueArray, AffineTransformation) Random12Floats()
        {
            var a = SpatialTestData.GetRandomPosition();
            var b = SpatialTestData.GetRandomPosition();
            var c = SpatialTestData.GetRandomPosition();
            var p = SpatialTestData.GetRandomPosition();
            var value = new ValueArray()
            {
                FloatValues = new FloatArray
                {
                    Values = { a.x, a.y, a.z, b.x, b.y, b.z, c.x, c.y, c.z, p.x, p.y, p.z }
                }
            };
            var transformation = new AffineTransformation(a, b, c, p);
            return (value, transformation);
        }

        public static IEnumerable<(ValueArray, AffineTransformation)> Get9FloatParameters()
        {
            return RandomTestData.SeededRandom(Random9Floats, 4214).Take(TestCount);
        }
        
        public static IEnumerable<(ValueArray, AffineTransformation)> Get12FloatParameters()
        {
            return RandomTestData.SeededRandom(Random12Floats, 72354).Take(TestCount);
        }
        
        [Test]
        public void Identity9Floats()
        {
            var value = new ValueArray()
            {
                FloatValues = new FloatArray
                {
                    Values = { 1, 0, 0, 0, 1, 0, 0, 0, 1 }
                }
            };
            var transformation = value.ToAffineTransformation();
            Assert.AreEqual(AffineTransformation.identity, transformation);
        }
        
        [Test]
        public void Identity12Floats()
        {
            var value = new ValueArray()
            {
                FloatValues = new FloatArray
                {
                    Values = { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0}
                }
            };
            var transformation = value.ToAffineTransformation();
            Assert.AreEqual(AffineTransformation.identity, transformation);
        }
        
        [Test]
        public void Test9Floats([ValueSource(nameof(Get9FloatParameters))] (ValueArray, AffineTransformation) parameters)
        {
            var transformation = parameters.Item1.ToAffineTransformation();
            Assert.AreEqual(parameters.Item2, transformation);
        }
        
        [Test]
        public void Test12Floats([ValueSource(nameof(Get12FloatParameters))] (ValueArray, AffineTransformation) parameters)
        {
            var transformation = parameters.Item1.ToAffineTransformation();
            Assert.AreEqual(parameters.Item2, transformation);
        }
        
        [Test]
        public void ValueArray_WrongType()
        {
            var value = new ValueArray()
            {
                IndexValues = new IndexArray()
                {
                    Values = { 1, 0, 0, 0, 1, 0, 0, 0, 1 }
                }
            };
            Assert.Throws<ArgumentException>(() => value.ToAffineTransformation());
        }
        
        [Test]
        public void ValueArray_Null()
        {
            ValueArray value = null;
            Assert.Throws<ArgumentNullException>(() => value.ToAffineTransformation());
        }
        
        [Test]
        public void ValueArray_EmptyValueArray()
        {
            var value = new ValueArray
            {
                
            };
            Assert.Throws<ArgumentException>(() => value.ToAffineTransformation());
        }
        
        [Test]
        public void ValueArray_WrongArgumentCount()
        {
            var value = new ValueArray()
            {
                FloatValues = new FloatArray
                {
                    Values = { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0}
                }
            };
            Assert.Throws<ArgumentException>(() => value.ToAffineTransformation());
        }
        
        [Test]
        public void ValueArray_ZeroArgumentCount()
        {
            var value = new ValueArray()
            {
                FloatValues = new FloatArray
                {
                    Values = {}
                }
            };
            Assert.Throws<ArgumentException>(() => value.ToAffineTransformation());
        }

    }
}