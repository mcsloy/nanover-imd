using NarupaIMD.Selection;
using NUnit.Framework;
using UnityEngine;

namespace NarupaIMD.Tests
{
    internal class TryParseColorTests
    {
        [Test]
        public void TestList3()
        {
            Assert.IsTrue(VisualiserFactory.TryParseColor(new int[] {255, 0, 0},
                                                          out var color));
            
            Assert.AreEqual(Color.red, color);
            
            Assert.IsTrue(VisualiserFactory.TryParseColor(new int[] {0, 255, 0},
                                                          out color));
            
            Assert.AreEqual(Color.green, color);
            
            Assert.IsTrue(VisualiserFactory.TryParseColor(new int[] {0, 0, 255},
                                                          out color));
            
            Assert.AreEqual(Color.blue, color);
            
            Assert.IsTrue(VisualiserFactory.TryParseColor(new int[] {255, 255, 255},
                                                          out color));
            
            Assert.AreEqual(Color.white, color);
        }
        
        [Test]
        public void TestName()
        {
            Assert.IsTrue(VisualiserFactory.TryParseColor("red",
                                                          out var color));
            
            Assert.AreEqual(Color.red, color);
        }
    }
}