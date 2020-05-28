using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Narupa.Visualisation
{
    public partial class VisualiserFactory
    {
        /// <summary>
        /// Get a prefab of a predefined visualiser with the given name.
        /// </summary>
        private static Dictionary<string, object> GetPredefinedVisualiser(string name)
        {
            var text = Resources.Load<TextAsset>($"{PrefabPath}/{name}");
            return Deserialize(text.text) as Dictionary<string, object>;
        }

        private static object Deserialize(string json)
        {
            return ToObject(JToken.Parse(json));
        }

        private static object ToObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return token.Children<JProperty>()
                                .ToDictionary(prop => prop.Name,
                                              prop => ToObject(prop.Value));

                case JTokenType.Array:
                    return token.Select(ToObject).ToList();

                default:
                    return ((JValue) token).Value;
            }
        }
        
        /// <summary>
        /// Path in the resources folder(s) where visualiser presets exist.
        /// </summary>
        private const string PrefabPath = "Visualiser/Preset";
    }
}