using NarupaXR;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace NarupaIMD.UI.Scene
{
    public class ManualConnect : MonoBehaviour
    {
        [SerializeField]
        private NarupaXRPrototype application;
        [SerializeField]
        private TMP_Text hostInputField;
        [SerializeField]
        private TMP_Text portInput;
    
        private void Start()
        {
            Assert.IsNotNull(application);
            Assert.IsNotNull(hostInputField);
            Assert.IsNotNull(portInput);
        }

        /// <summary>
        /// Called from the UI button to tell the application to connect
        /// to remote services.
        /// </summary>
        public void ConnectToServer()
        {
            var trajectoryPort = portInput.text.Length > 0
                                     ? (int?) int.Parse(portInput.text)
                                     : null;
            var imdPort = portInput.text.Length > 0
                              ? (int?) int.Parse(portInput.text)
                              : null;
            var multiplayerPort = portInput.text.Length > 0
                                      ? (int?) int.Parse(portInput.text)
                                      : null;

            application.Connect(hostInputField.text, trajectoryPort, imdPort, multiplayerPort);
        }
    }
}
