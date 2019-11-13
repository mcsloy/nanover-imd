using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Narupa.Frontend.UI
{
    /// <summary>
    /// Script for arranging children radially around a point
    /// </summary>
    public class RadialLayoutGroup : LayoutGroup
    {
        [SerializeField]
        private GameObject radialPrefab;

        [SerializeField]
        private float angularOffset = 0;

        private void ArrangeChildrenRadially()
        {
            var childCount = transform.childCount;
            var da = 360f / childCount;
            for (var i = 0; i < childCount; i++)
            {
                var angle = angularOffset + i * da;
                var d = Vector2.Scale(new Vector2(Mathf.Sin(Mathf.Deg2Rad * angle),
                                                  Mathf.Cos(Mathf.Deg2Rad * angle)),
                                      GetComponent<RectTransform>().sizeDelta * 0.5f);
                Debug.Log(GetComponent<RectTransform>().sizeDelta);
                transform.GetChild(i).transform.localPosition = d;
            }
        }

        public override void CalculateLayoutInputVertical()
        {
            
        }

        public override void SetLayoutHorizontal()
        {
        }

        public override void SetLayoutVertical()
        {
            ArrangeChildrenRadially();
        }
    }
}