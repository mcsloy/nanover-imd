// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

namespace Narupa.Visualisation.Components.Color
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Color.NumericGradientColor" />
    public sealed class NumericGradientColor : VisualisationComponent<Node.Color.NumericGradientColor>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}