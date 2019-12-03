using System;
using Narupa.Frontend.XR;
using UnityEngine;

namespace NarupaIMD.State
{
    public class CalibratedSpaceWidget : MonoBehaviour
    {
        public PhysicallyCalibratedSpace CalibratedSpace { get; } = new PhysicallyCalibratedSpace();

        private void Update()
        {
            CalibratedSpace.CalibrateFromLighthouses();
        }
    }
}