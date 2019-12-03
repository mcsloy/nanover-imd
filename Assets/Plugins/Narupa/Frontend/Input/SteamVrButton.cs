using System;
using Valve.VR;

namespace Narupa.Frontend.Input
{
    public class SteamVrButton : IButton
    {
        private SteamVR_Action_Boolean action;
        private SteamVR_Input_Sources inputSource;

        public SteamVrButton(SteamVR_Action_Boolean action, SteamVR_Input_Sources inputSource)
        {
            this.action = action;
            this.inputSource = inputSource;
            action.AddOnStateDownListener(OnStateDown, inputSource);
            action.AddOnStateUpListener(OnStateUp, inputSource);
        }

        private void OnStateDown(SteamVR_Action_Boolean fromAction,
                                 SteamVR_Input_Sources fromSource)
        {
            Pressed?.Invoke();
        }

        private void OnStateUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            Released?.Invoke();
        }

        public void ReleaseBinding()
        {
            action.RemoveOnStateDownListener(OnStateDown, inputSource);
            action.RemoveOnStateUpListener(OnStateUp, inputSource);
        }

        public bool IsPressed => action.GetState(inputSource);

        public event Action Pressed;

        public event Action Released;
    }
}