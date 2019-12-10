using System.Collections;
using System.Collections.Generic;
using NarupaIMD.Selection;
using NUnit.Framework;
using UnityEngine;

namespace NarupaIMD.Tests
{
    internal class TryParseColorTests
    {
        private static IEnumerable<(object value, Color color)> GetParameters()
        {
            yield return (new object[] { 1.0, 0.0, 0.0 }, Color.red);
            yield return (new object[] { 0.0, 1.0, 0.0 }, Color.green);
            yield return (new object[] { 0.0, 0.0, 1.0 }, Color.blue);
            yield return (new object[] { 1.0, 1.0, 1.0 }, Color.white);
        }
        
        [Test]
        public void TestList3([ValueSource(nameof(GetParameters))] (object value, Color color) parameter)
        {
            Assert.IsTrue(VisualiserFactory.TryParseColor(parameter.value,
                                                          out var color));
            Assert.AreEqual(parameter.color,  color);
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