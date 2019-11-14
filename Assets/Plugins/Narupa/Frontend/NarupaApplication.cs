using System.Linq;
using System.Runtime.CompilerServices;
using Narupa.Frontend.Controllers;
using UnityEngine;
using UnityEngine.Assertions;

namespace Plugins.Narupa.Frontend
{
    /// <summary>
    /// Represents resources that are shared across an application.
    /// </summary>
    [CreateAssetMenu(fileName = "Narupa Application", menuName = "Narupa Application")]
    public class NarupaApplication : ScriptableObject
    {
        public static NarupaApplication Instance;

        public static void SetApplication(NarupaApplication application)
        {
            Assert.IsNull(Instance);
            Assert.IsNotNull(application);
            Instance = application;
        }

        [SerializeField]
        private SteamVrControllerDefinition[] controllerDefinitions = new SteamVrControllerDefinition[0];

        public SteamVrControllerDefinition FindControllerDefinition(string name)
        {
            return controllerDefinitions.FirstOrDefault(c => c.ControllerId == name);
        }
    }
}