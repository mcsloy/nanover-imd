// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Text;
using Plugins.Narupa.Frontend;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR;

namespace Narupa.Frontend.Controllers
{
    /// <summary>
    /// Reacts to a change in the SteamVR device name and loads the appropriate prefab.
    /// </summary>
    public class SteamVrDynamicController : MonoBehaviour
    {
        [SerializeField]
        private SteamVR_Behaviour_Pose steamVrComponent;

        [SerializeField]
        private Transform prefabRoot;

        private VrControllerPrefab currentPrefab;

        [SerializeField]
        private VrController controller;

        private void Awake()
        {
            Assert.IsNotNull(steamVrComponent);
            Assert.IsNotNull(controller);
            Assert.IsNotNull(prefabRoot);
            steamVrComponent.onDeviceIndexChanged.AddListener(OnDeviceIndexChanged);
        }


        private void OnDeviceIndexChanged(SteamVR_Behaviour_Pose pose,
                                          SteamVR_Input_Sources source,
                                          int index)
        {
            if (Enum.IsDefined(typeof(SteamVR_TrackedObject.EIndex), index))
            {
                var eIndex = (SteamVR_TrackedObject.EIndex) index;
                var controllerType = GetOpenVrProperty(eIndex,
                                                       ETrackedDeviceProperty
                                                           .Prop_ControllerType_String);
                OnControllerTypeChanged(controllerType, source);
            }
        }

        private void OnControllerTypeChanged(string type, SteamVR_Input_Sources input)
        {
            if (currentPrefab != null)
                Destroy(currentPrefab);

            var definition = NarupaApplication.Instance.FindControllerDefinition(type);
            if (definition == null)
                Debug.LogWarning($"Unsupported controller type: {type}");
            var prefab = definition.GetPrefab(input);

            if (prefab == null)
            {
                Debug.LogWarning($"Controller type '{type}' is missing prefab for {input}");
                currentPrefab = null;
            }
            else
            {
                currentPrefab = Instantiate(prefab, prefabRoot ?? transform);
            }

            controller.ResetController(currentPrefab);
        }
        
        private static string GetOpenVrProperty(SteamVR_TrackedObject.EIndex index,
                                                ETrackedDeviceProperty property)
        {
            var system = OpenVR.System;
            var error = ETrackedPropertyError.TrackedProp_Success;
            var capacity =
                system.GetStringTrackedDeviceProperty((uint) index, property, null, 0, ref error);
            if (capacity <= 1)
            {
                Debug.LogError(
                    $"<b>[SteamVR]</b> Failed to get property {property} name for tracked object {index}");
                return null;
            }

            var buffer = new StringBuilder((int) capacity);
            system.GetStringTrackedDeviceProperty((uint) index, property, buffer, capacity,
                                                  ref error);

            return buffer.ToString();
        }
    }
}