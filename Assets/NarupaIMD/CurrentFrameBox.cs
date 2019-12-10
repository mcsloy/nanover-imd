using Narupa.Frame;
using Narupa.Frame.Event;
using Narupa.Visualisation;
using UnityEngine;
using UnityEngine.Assertions;

namespace NarupaXR
{
    public class CurrentFrameBox : MonoBehaviour
    {
        [SerializeField]
        private SynchronisedFrameSource frameSource;

        [SerializeField]
        private BoxVisualiser boxVisualiser;

        private void Start()
        {
            Assert.IsNotNull(boxVisualiser);
            Assert.IsNotNull(frameSource);
            boxVisualiser.enabled = false;
            frameSource.FrameChanged += OnFrameChanged;
        }

        private void OnFrameChanged(IFrame frame, FrameChanges changes)
        {
            if (changes.GetIsChanged(StandardFrameProperties.BoxTransformation.Key))
            {
                var box = (frame as Frame).BoxVectors;
                if (box == null)
                {
                    boxVisualiser.enabled = false;
                }
                else
                {
                    boxVisualiser.enabled = true;
                    boxVisualiser.SetBox(box.Value);
                }
            }
        }
    }
}