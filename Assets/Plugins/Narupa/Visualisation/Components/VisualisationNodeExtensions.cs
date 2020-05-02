using System.Collections.Generic;
using Narupa.Visualisation.Property;

namespace Narupa.Visualisation.Components
{
    public static class VisualisationNodeExtensions
    {
        public static IPropertyProvider AsProvider(this IVisualisationNode node)
        {
            if (node is IPropertyProvider provider)
                return provider;
            return new VisualisationNodeAsProvider(node);
        }
    }

    public class VisualisationNodeAsProvider : IPropertyProvider
    {
        private IVisualisationNode node;

        public VisualisationNodeAsProvider(IVisualisationNode node)
        {
            this.node = node;
        }

        /// <inheritdoc cref="IPropertyProvider.GetProperty" />
        public virtual IReadOnlyProperty GetProperty(string name)
        {
            return VisualisationUtility.GetPropertyField(node, name);
        }

        /// <inheritdoc cref="IPropertyProvider.GetProperties" />
        public virtual IEnumerable<(string name, IReadOnlyProperty property)> GetProperties()
        {
            return VisualisationUtility.GetAllPropertyFields(node);
        }
    }
}