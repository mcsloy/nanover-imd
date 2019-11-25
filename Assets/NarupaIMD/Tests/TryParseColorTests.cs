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
            Assert.IsTrue(VisualiserFactory.TryParseColor(new object[] { 1.0, 0.0, 0.0 },
                                                          out var color));

            Assert.AreEqual(Color.red, color);

            Assert.IsTrue(VisualiserFactory.TryParseColor(new object[] { 0.0, 1.0, 0.0 },
                                                          out color));

            Assert.AreEqual(Color.green, color);

            Assert.IsTrue(VisualiserFactory.TryParseColor(new object[] { 0.0, 0.0, 1.0 },
                                                          out color));

            Assert.AreEqual(Color.blue, color);

            Assert.IsTrue(VisualiserFactory.TryParseColor(new object[] { 1.0, 1.0, 1.0 },
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