using UnityEngine;

namespace NarupaIMD.State
{
    public class ApplicationState : MonoBehaviour
    {
        [SerializeField]
        private ApplicationStateManager application;
        
        public ApplicationStateManager Application => application;
    }
}