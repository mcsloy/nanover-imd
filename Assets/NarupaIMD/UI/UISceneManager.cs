using System;
using Narupa.Frontend.Controllers;
using Narupa.Frontend.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace NarupaXR.UI
{
    public class UISceneManager : MonoBehaviour
    {
        [SerializeField]
        private Camera camera;

        [SerializeField]
        private CursorProvider cursorProvider;

        private void Awake()
        {
            Assert.IsNotNull(camera);
            Assert.IsNotNull(cursorProvider);
        }

        private NarupaCanvas currentScene;

        public void GotoScene(NarupaCanvas scenePrefab)
        {
            if (currentScene != null)
                Destroy(currentScene.gameObject);
            if (scenePrefab != null)
            {
                currentScene = Instantiate(scenePrefab, transform);
                currentScene.SetCamera(camera);
                currentScene.SetCursor(cursorProvider);
                currentScene.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            CloseScene();
        }

        public void CloseScene()
        {
            GotoScene(null);
        }
    }
}