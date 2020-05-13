using System;
using UnityEngine;

namespace Narupa.Frontend.XR
{
    public class Colocation
    {
        private static string ColocationKey = "narupa.colocation.enabled";

        public static bool IsEnabled()
        {
            return PlayerPrefs.HasKey(ColocationKey);
        }

        public static void SetEnabled(bool active)
        {
            var current = IsEnabled();
            if (current != active)
            {
                if (active)
                    PlayerPrefs.SetInt(ColocationKey, 1);
                else
                    PlayerPrefs.DeleteKey(ColocationKey);
                ColocationSettingChanged?.Invoke();
            }
        }

        public static event Action ColocationSettingChanged;
    }
}