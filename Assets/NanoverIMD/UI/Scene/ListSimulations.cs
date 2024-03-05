using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Essd;
using Nanover.Core.Async;
using Nanover.Frontend.UI;
using UnityEngine;
using UnityEngine.Events;

namespace NanoverImd.UI.Scene
{
    public class ListSimulations : MonoBehaviour
    {
        private List<string> simulations = new List<string>();

        [SerializeField]
        private Sprite localServerIcon;

        [SerializeField]
        private Sprite remoteServerIcon;

        [SerializeField]
        private NanoverImdApplication application;

        [SerializeField]
        private DynamicMenu menu;

        private void OnEnable()
        {
            Refresh();
        }

        [SerializeField]
        private UnityEvent startSearch;

        [SerializeField]
        private UnityEvent endSearch;

        public void Refresh()
        {
            GetListing();

            async void GetListing()
            {
                startSearch?.Invoke();
                var listing = await application.Simulation.Trajectory.GetSimulationListing();

                simulations.Clear();
                simulations.AddRange(listing);

                endSearch?.Invoke();

                RefreshHubs();
            }
        }

        public void RefreshHubs()
        {
            menu.ClearChildren();
            for (int i = 0; i < simulations.Count; ++i)
            {
                var index = i;

                menu.AddItem(simulations[i], 
                             localServerIcon, 
                             () => application.Simulation.Trajectory.SetSimulationIndex(index));
            }
        }
    }
}