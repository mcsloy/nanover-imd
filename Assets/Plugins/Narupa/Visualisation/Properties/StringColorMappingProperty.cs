using System;
using Narupa.Core;
using Narupa.Core.Science;
using Narupa.Visualisation.Node.Color;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Properties
{
    /// <summary>
    /// Serializable <see cref="Property" /> for a <see cref="StringColorMapping" />
    /// value.
    /// </summary>
    [Serializable]
    public class StringColorMappingProperty : InterfaceProperty<IMapping<string, Color>>
    {
    }
}