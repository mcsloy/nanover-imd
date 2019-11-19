using System;
using UnityEngine;

namespace Narupa.Frontend.UI
{
    public class DynamicMenu : MonoBehaviour
    {
        [SerializeField]
        private UiButton buttonPrefab;

        public void AddItem(string name, Sprite icon, Action callback)
        {
            var button = CreateButton();
            button.Text = name;
            button.Image = icon;
            button.OnClick += callback;
        }

        private UiButton CreateButton()
        {
            var button = Instantiate(buttonPrefab, transform);
            button.gameObject.SetActive(true);
            return button;
        }
    }
}