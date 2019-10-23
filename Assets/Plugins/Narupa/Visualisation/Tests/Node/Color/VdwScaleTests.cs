using Narupa.Core.Science;
using Narupa.Visualisation.Node.Color;
using Narupa.Visualisation.Node.Scale;
using Narupa.Visualisation.Property;
using NUnit.Framework;
using UnityEngine;

namespace Narupa.Visualisation.Tests.Node.Color
{
    public class ElementPaletteColorTests
    {
        private Element[] ValidElements = {Element.Carbon, Element.Hydrogen};
        
        [Test]
        public void NullPalette()
        {
            var node = new ElementPaletteColor();

            node.Elements.Value = ValidElements;
            node.Mapping.Value = null;

            node.Refresh();
            
            Assert.IsFalse(node.Colors.HasNonNullValue());
        }
        
        [Test]
        public void NullElements()
        {
            var node = new ElementPaletteColor();

            node.Mapping.Value = ScriptableObject.CreateInstance<ElementColorMapping>();
            node.Elements.UndefineValue();

            node.Refresh();
            
            Assert.IsFalse(node.Colors.HasNonNullValue());
        }
        
        [Test]
        public void EmptyElements()
        {
            var node = new ElementPaletteColor();

            node.Elements.Value = new Element[0];
            node.Mapping.Value = ScriptableObject.CreateInstance<ElementColorMapping>();

            node.Refresh();
            
            Assert.IsFalse(node.Colors.HasNonNullValue());
        }
        
        [Test]
        public void ValidInput()
        {
            var node = new ElementPaletteColor();

            node.Elements.Value = ValidElements;
            node.Mapping.Value = ScriptableObject.CreateInstance<ElementColorMapping>();

            node.Refresh();
            
            Assert.IsTrue(node.Colors.HasNonNullValue());
        }
        
        [Test]
        public void OnlyRefreshOnce()
        {
            var node = new ElementPaletteColor();
            var linked = new ColorArrayProperty()
            {
                LinkedProperty = node.Colors, 
                IsDirty = false
            };
            Assert.IsFalse(linked.IsDirty);

            node.Elements.Value = ValidElements;
            node.Mapping.Value = ScriptableObject.CreateInstance<ElementColorMapping>();

            node.Refresh();

            Assert.IsTrue(linked.IsDirty);
            linked.IsDirty = false;
            
            node.Refresh();
            
            Assert.IsFalse(linked.IsDirty);
        }
    }
}