using Narupa.Frontend.Controllers;
using NarupaIMD;
using UnityEngine;
using Valve.VR;

public class InteractionStrengthController : MonoBehaviour
{
    [SerializeField]
    private NarupaImdSimulation simulation;

    [SerializeField]
    private VrController controller;

    [SerializeField]
    private SteamVR_Action_Boolean increaseInteractionStrength;

    [SerializeField]
    private SteamVR_Action_Boolean decreaseInteractionStrength;

    [SerializeField]
    private float maximumInteractionStrength;

    [SerializeField]
    private float minimumInteractionStrength;

    [SerializeField]
    private float scaling;

    private void Update()
    {
        var change = 0f;
        if (increaseInteractionStrength.state)
            change++;
        if (decreaseInteractionStrength.state)
            change--;
        if (change != 0)
        {
            change = Mathf.Pow(scaling, change * Time.deltaTime);
            Scale = Mathf.Clamp(Scale * change,
                                minimumInteractionStrength,
                                maximumInteractionStrength);

            controller.PushNotification($"{(int) Scale}x");
        }
    }

    private float Scale
    {
        get => simulation.ManipulableParticles.ForceScale;
        set => simulation.ManipulableParticles.ForceScale = value;
    }
}