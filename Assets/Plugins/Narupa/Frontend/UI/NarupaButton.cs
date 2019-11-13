using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Narupa.Frontend.UI
{
    public class NarupaButton : MonoBehaviour
    {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private TMP_Text text;

        [SerializeField]
        private Button button;
        
        public event Action OnClick
        {
            add
            {
                button.onClick.AddListener(() => value());
            }
            remove
            {
                button.onClick.RemoveListener(() => value());
            }
        }

        public Sprite Image
        {
            set
            {
                if (value == null)
                {
                    icon.enabled = false;
                }
                else
                {
                    icon.enabled = true;
                    icon.sprite = value;
                }
            }
        }

        public string Text
        {
            set => text.text = value;
        }
    }
}