using System;
using Narupa.Core.Math;
using Narupa.Frontend.Controllers;
using UnityEngine;
using Valve.VR;

namespace NarupaIMD.UI.Teleport
{
    public class TeleportMode : ControllerInputMode
    {
        [SerializeField]
        private int priority;
        
        [SerializeField]
        private SteamVR_ActionSet[] actionSets;

        public override int Priority => priority;
        
        [SerializeField]
        private SteamVR_Action_Vector2 angleAction;

        [SerializeField]
        private SteamVR_Action_Boolean teleportAction;
        
        [SerializeField]
        private Transform headTransform;

        [SerializeField]
        private Transform boxTransform;

        [SerializeField]
        private TeleportAim aim;

        [SerializeField]
        private Transform ghostHeadset;

        protected override void OnEnable()
        {
            base.OnEnable();
            angleAction.onChange += AngleChanged;
            teleportAction.onStateUp += DoTeleport;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            angleAction.onChange -= AngleChanged;
            teleportAction.onStateUp -= DoTeleport;
        }

        public override void OnModeStarted()
        {
            foreach(var set in actionSets)
                set.Activate();
        }
    
        public override void OnModeEnded()
        { 
            foreach(var set in actionSets)
                set.Deactivate();
        }

        private void Update()
        {
            var headPosition = headTransform.position;
            var forward = headTransform.forward;
            forward.y = 0;
            headPosition.y = 0;
            var headRotation = Quaternion.LookRotation(forward, Vector3.up);
            var headSpace = new UnitScaleTransformation(headPosition, headRotation);
            var destinationSpace = aim.Destination;
            var headToDestination = destinationSpace * headSpace.inverse;
            
            var newTransform = headToDestination * Transformation.FromTransformRelativeToWorld(headTransform);
            newTransform.CopyToTransformRelativeToWorld(ghostHeadset);
        }


        private void DoTeleport(SteamVR_Action_Boolean fromaction, SteamVR_Input_Sources fromsource)
        {
            var headPosition = headTransform.position;
            var forward = headTransform.forward;
            forward.y = 0;
            headPosition.y = 0;
            var headRotation = Quaternion.LookRotation(forward, Vector3.up);
            var headSpace = new UnitScaleTransformation(headPosition, headRotation);
            var destinationSpace = aim.Destination;
            var destinationToHead = headSpace * destinationSpace.inverse;
            var newTransform = destinationToHead * Transformation.FromTransformRelativeToWorld(boxTransform);
            newTransform.CopyToTransformRelativeToWorld(boxTransform);
            this.gameObject.SetActive(false);
        }

        private void AngleChanged(SteamVR_Action_Vector2 fromaction, SteamVR_Input_Sources fromsource, Vector2 axis, Vector2 delta)
        {
            if(axis.magnitude > 0.5f)
                aim.SetAngle(Mathf.Rad2Deg * Mathf.Atan2(axis.x, axis.y));
        }


        public override void SetupController(VrController controller, SteamVR_Input_Sources inputSource)
        {
        
        }

        public bool WouldBeActiveIfEnabled()
        {
            return Controllers.CurrentInputMode == this || Controllers.WouldBecomeCurrentMode(this);
        }
    }
}
