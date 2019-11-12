using System.Collections;
using Narupa.Core.Science;
using NUnit.Framework;
using UnityEngine;

namespace Narupa.Frame.Tests
{
    public class FrameTests
    {
        [Test]
        public void ShallowCopy()
        {
            var originalFrame = new Frame()
            {
                ParticlePositions = new[] { Vector3.one, Vector3.left },
                ParticleElements = new[] { Element.Hydrogen, Element.Oxygen }
            };

            var clonedFrame = Frame.ShallowCopy(originalFrame);

            CollectionAssert.AreEqual(originalFrame.ParticlePositions,
                                      clonedFrame.ParticlePositions);
            CollectionAssert.AreEqual(originalFrame.ParticleElements, clonedFrame.ParticleElements);
        }

        [Test]
        public void SetFloatValue()
        {
            var frame = new Frame();
            frame.Data["id"] = 0.5f;
            Assert.AreEqual(0.5f, frame.Data["id"]);
        }

        [Test]
        public void SetStringValue()
        {
            var frame = new Frame();
            frame.Data["id"] = "test";
            Assert.AreEqual("test", frame.Data["id"]);
        }

        [Test]
        public void HasFloatValue()
        {
            var frame = new Frame();
            frame.Data["id"] = 0.5f;
            Assert.IsTrue(frame.Data.ContainsKey("id"));
        }

        [Test]
        public void Element_Provided()
        {
            var frame = new Frame()
            {
                ParticlePositions = new[] { Vector3.zero },
                ParticleElements = new[] { Element.Carbon }
            };
            Assert.AreEqual(Element.Carbon, frame.ParticleElements[0]);
        }

        [Test]
        public void Type_Provided()
        {
            var frame = new Frame()
            {
                ParticlePositions = new[] { Vector3.zero },
                ParticleTypes = new[] { "type" }
            };
            Assert.AreEqual("type", frame.ParticleTypes[0]);
        }

        [Test]
        public void ParticleNames_Assignment()
        {
            var frame = new Frame
            {
                ParticleNames = new[] {"abc", "def"}
            };
            CollectionAssert.AreEqual(new[] {"abc", "def"}, frame.ParticleNames);
        }
        
        [Test]
        public void ResidueNames_Assignment()
        {
            var frame = new Frame
            {
                ResidueNames = new[] {"abc", "def"}
            };
            CollectionAssert.AreEqual(new[] {"abc", "def"}, frame.ResidueNames);
        }
        
        [Test]
        public void ParticleResidues_Assignment()
        {
            var frame = new Frame
            {
                ParticleResidues = new[] {1, 3}
            };
            CollectionAssert.AreEqual(new[] {1, 3}, frame.ParticleResidues);
        }
        
        [Test]
        public void BondOrders_Assignment()
        {
            var frame = new Frame
            {
                BondOrders = new[] {1, 3}
            };
            CollectionAssert.AreEqual(new[] {1, 3}, frame.BondOrders);
        }
        
        [Test]
        public void Bonds_Assignment()
        {
            var frame = new Frame
            {
                BondPairs = new[] { new BondPair(0, 1), new BondPair(1,2)}  
            };
            CollectionAssert.AreEqual( new[] { new BondPair(0, 1), new BondPair(1,2)}  , frame.Bonds);
        }
    }
}