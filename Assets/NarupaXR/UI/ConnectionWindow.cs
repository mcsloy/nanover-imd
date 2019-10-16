// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using UnityEngine;

using InputField = TMPro.TMP_InputField;

namespace NarupaXR.UI
{
    /// <summary>
    /// Manage UI for the prototype's connection window.
    /// </summary>
    public sealed class ConnectionWindow : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private NarupaXRPrototype narupaXR;
        [SerializeField]
        private InputField hostInputField;
        [SerializeField]
        private InputField trajectoryPortInput;
        [SerializeField]
        private InputField imdPortInput;
        [SerializeField]
        private InputField multiplayerPortInput;
#pragma warning restore 0649

        /// <summary>
        /// Called from the UI button to tell the application to connect
        /// to remote services.
        /// </summary>
        public void OnConnectButtonPressed()
        {
            var trajectoryPort = trajectoryPortInput.text.Length > 0
                               ? (int?) int.Parse(trajectoryPortInput.text)
                               : null;
            int? imdPort = imdPortInput.text.Length > 0
                         ? (int?) int.Parse(imdPortInput.text)
                         : null;
            int? multiplayerPort = multiplayerPortInput.text.Length > 0
                                 ? (int?) int.Parse(multiplayerPortInput.text)
                                 : null;

            narupaXR.Connect(hostInputField.text, trajectoryPort, imdPort, multiplayerPort);
        }
    }
}
