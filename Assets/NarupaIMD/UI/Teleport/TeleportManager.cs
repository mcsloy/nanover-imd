using UnityEngine;

namespace NarupaIMD.UI.Teleport
{
    /// <summary>
    /// Projects a parabola along the forward vector towards the ground.
    /// </summary>
    [ExecuteInEditMode]
    public class TeleportManager : MonoBehaviour
    {
        [SerializeField]
        private Transform controller;
        
        [SerializeField]
        private LineRenderer renderer;

        [SerializeField]
        private GameObject destination;

        [SerializeField]
        private float speed;

        [SerializeField]
        private float timestep;
        
        // Update is called once per frame
        void Update()
        {
            var vel = controller.transform.forward * speed;
            var y0 = controller.transform.position.y;
            var tmax = 0.5f * (vel.y + Mathf.Sqrt(vel.y * vel.y + 4 * y0));
            var steps = (int) Mathf.Ceil(tmax / timestep) + 1;
            var positions = new Vector3[steps + 1];
            for (var i = 0; i <= steps; i++)
            {
                var t = tmax * i / steps;
                positions[i] = controller.transform.position + vel * t - t * t * Vector3.up;
            }

            renderer.positionCount = positions.Length;
            renderer.SetPositions(positions);

            var target = controller.transform.position + vel * tmax - tmax * tmax * Vector3.up;
            target.y = 0;
            destination.transform.position = target;
        }
    }
}
