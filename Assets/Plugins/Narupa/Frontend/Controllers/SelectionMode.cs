using System.Linq;
using Narupa.Frame;
using Narupa.Frontend.Controllers;
using Narupa.Visualisation;
using Narupa.Visualisation.Components.Adaptor;
using Narupa.Visualisation.Property;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR;

public class SelectionMode : MonoBehaviour
{
    [SerializeField]
    private ControllerManager controller;

    [SerializeField]
    private Transform simulationTransform;

    [SerializeField]
    private SynchronisedFrameSource frameSource;

    private void Awake()
    {
        Assert.IsNotNull(controller);
        Assert.IsNotNull(simulationTransform);
        Assert.IsNotNull(frameSource);

        controller.LeftController.ControllerReset += () =>
        {
            SetupController(controller.LeftController);
        };
        controller.RightController.ControllerReset += () =>
        {
            SetupController(controller.RightController);
        };
    }

    [SerializeField]
    private FrameAdaptor highlightVisualiser;

    [SerializeField]
    private FrameAdaptor selectionVisualiser;

    [SerializeField]
    private SteamVR_Action_Boolean toolAction;

    private void Start()
    {
        highlightVisualiser.Adaptor
                           .ParticleFilter
                           .LinkedProperty = highlighted.AsVisualisationProperty<int, Selection>();

        selectionVisualiser.Adaptor
                           .ParticleFilter
                           .LinkedProperty = selection.AsVisualisationProperty<int, Selection>();
    }

    private Selection highlighted = new Selection();

    private Selection selection = new Selection();

    private bool isAddingToSelection = false;

    public void Update()
    {
        if (controller.LeftController.IsControllerActive)
        {
            var pos = controller.LeftController.Cursor.Pose;
            var nearest = pos.HasValue ? GetNearestParticle(pos.Value.Position) : null;
            if (toolAction.GetStateDown(SteamVR_Input_Sources.LeftHand))
            {
                isAddingToSelection = !nearest.HasValue || !selection.Contains(nearest.Value);
            }

            if (nearest.HasValue)
            {
                highlighted.Set(nearest.Value);
                if (toolAction.GetState(SteamVR_Input_Sources.LeftHand))
                {
                    if (isAddingToSelection)
                        selection.Add(nearest.Value);
                    else
                        selection.Remove(nearest.Value);
                }
            }
            else
            {
                highlighted.Clear();
            }
        }
    }

    private int? GetNearestParticle(Vector3 position)
    {
        position = simulationTransform.InverseTransformPoint(position);

        var frame = frameSource.CurrentFrame;
        if (frame == null)
            return null;

        var bestSqrDistance = Mathf.Infinity;
        var bestParticleIndex = 0;

        for (var i = 0; i < frame.ParticlePositions.Length; ++i)
        {
            var particlePosition = frame.ParticlePositions[i];
            var sqrDistance = Vector3.SqrMagnitude(position - particlePosition);

            if (sqrDistance < bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                bestParticleIndex = i;
            }
        }

        var cutoff = Mathf.Pow(toolRadius / simulationTransform.localScale.x, 2f);

        return bestSqrDistance < cutoff ? (int?) bestParticleIndex : null;
    }

    [SerializeField]
    private float toolRadius;

    [SerializeField]
    private GameObject selectionGizmo;

    private void SetupController(VrController controller)
    {
        if (controller.IsControllerActive && selectionGizmo != null)
        {
            controller.Cursor.SetGizmo(Instantiate(selectionGizmo,
                                                   controller.Cursor.transform));
        }
    }
}