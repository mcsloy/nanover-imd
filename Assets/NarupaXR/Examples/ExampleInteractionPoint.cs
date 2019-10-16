// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using UnityEngine;

namespace NarupaXR.Examples
{
    /// <summary>
    /// Example component that applies interaction forces to an IMD simulation
    /// via the <see cref="ExampleImdClient" />.
    /// </summary>
    public sealed class ExampleInteractionPoint : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        ExampleImdClient imdClient;

        [SerializeField]
        private string streamId;

        [SerializeField]
        private uint atomIndex = 0;

        [SerializeField]
        private float force = 100f;
#pragma warning restore 0649

        private void Update()
        {
            imdClient.InteractiveSession.SetInteraction(streamId, 
                                                        transform.position, 
                                                        force, 
                                                        particles: atomIndex);
        }

        private void OnDisable()
        {
            imdClient.InteractiveSession.UnsetInteraction(streamId);
        }
    }
}