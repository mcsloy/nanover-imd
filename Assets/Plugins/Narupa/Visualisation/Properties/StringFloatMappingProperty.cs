using System;
using Narupa.Core;
using Narupa.Core.Science;
using Narupa.Visualisation.Node.Color;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Properties
{
    /// <summary>
    /// Serializable <see cref="Property" /> for a <see cref="StringFloatMapping" />
    /// value.
    /// </summary>
    [Serializable]
    public class StringFloatMappingProperty : InterfaceProperty<IMapping<string, float>>
    {
    }
}