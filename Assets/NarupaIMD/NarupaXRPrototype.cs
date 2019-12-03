// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Plugins.Narupa.Frontend;
using UnityEngine;
using UnityEngine.Assertions;
using Text = TMPro.TextMeshProUGUI;

namespace NarupaXR
{
    /// <summary>
    /// The entry point to the application, and central location for accessing
    /// shared resources.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NarupaXRPrototype : MonoBehaviour
    {
        [SerializeField]
        private NarupaApplication application;

        private void Awake()
        {
            Assert.IsNotNull(application, "Narupa iMD script is missing reference to application");
            NarupaApplication.SetApplication(application);
        }
    }
}