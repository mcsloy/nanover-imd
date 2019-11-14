using NarupaXR;
using UnityEngine;

public class Autoconnect : MonoBehaviour
{
    [SerializeField]
    private NarupaXRPrototype prototype;

    private void Start()
    {
#if UNITY_EDITOR
        prototype.Connect("localhost", 54321, 54322, null);
#endif
    }
}