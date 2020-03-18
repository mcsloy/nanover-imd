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
        private TMP_Text trajectoryPortInput;
        [SerializeField]
        private TMP_Text imdPortInput;
        [SerializeField]
        private TMP_Text multiplayerPortInput;
    
        private void Start()
        {
            Assert.IsNotNull(application);
            Assert.IsNotNull(hostInputField);
            Assert.IsNotNull(trajectoryPortInput);
            Assert.IsNotNull(imdPortInput);
            Assert.IsNotNull(multiplayerPortInput);
        }

        /// <summary>
        /// Called from the UI button to tell the application to connect
        /// to remote services.
        /// </summary>
        public void ConnectToServer()
        {
            var trajectoryPort = trajectoryPortInput.text.Length > 0
                                     ? (int?) int.Parse(trajectoryPortInput.text)
                                     : null;
            var imdPort = imdPortInput.text.Length > 0
                              ? (int?) int.Parse(imdPortInput.text)
                              : null;
            var multiplayerPort = multiplayerPortInput.text.Length > 0
                                      ? (int?) int.Parse(multiplayerPortInput.text)
                                      : null;

            application.Connect(hostInputField.text, trajectoryPort, imdPort, multiplayerPort);
        }
    }
}
