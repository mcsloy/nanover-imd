using System;
using System.Collections.Generic;
using Narupa.Core.Science;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Color
{
    /// <summary>
    /// Defines a predetermined gradient.
    /// </summary>
    [CreateAssetMenu(menuName = "Definition/Gradient")]
    public class GradientDefinition : ScriptableObject
    {
#pragma warning disable 0649
        [SerializeField]
        private GradientProperty gradient;
#pragma warning disable 0649

        /// <summary>
        /// Gradient
        /// </summary>
        public Gradient Gradient => gradient;
    }
}