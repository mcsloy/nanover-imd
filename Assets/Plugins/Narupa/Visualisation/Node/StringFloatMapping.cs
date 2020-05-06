using System;
using System.Collections.Generic;
using Narupa.Core;
using UnityEngine;

namespace Narupa.Visualisation.Node.Color
{
    /// <summary>
    /// Stores key-value pair of string to float, used for mappings such as atom name to numeric.
    /// </summary>
    [CreateAssetMenu(menuName = "Definition/String-Float Mapping")]
    public class StringFloatMapping : ScriptableObject, IMapping<string, float>
    {
#pragma warning disable 0649
        /// <summary>
        /// Default value used when a key is not found.
        /// </summary>
        [SerializeField]
        private float defaultValue;

        /// <summary>
        /// List of assignments, acting as a dictionary so Unity can serialize.
        /// </summary>
        [SerializeField]
        private List<StringFloatAssignment> dictionary;
#pragma warning restore 0649

        /// <summary>
        /// Get the value for the given key, returning a default if the
        /// key is not defined.
        /// </summary>
        public float Map(string name)
        {
            foreach (var item in dictionary)
                if (item.key.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return item.value;
            return defaultValue;
        }

        /// <summary>
        /// Key-value pair for atomic element to color mappings for serialisation in Unity
        /// </summary>
        [Serializable]
        public class StringFloatAssignment
        {
            public string key;
            public float value;
        }
    }
}