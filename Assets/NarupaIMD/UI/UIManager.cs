using Narupa.Frontend.Controllers;
using Narupa.Frontend.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace NarupaXR.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private NarupaCanvas startingScenePrefab;

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

        private void Start()
        {
            if (startingScenePrefab != null)
                GotoScene(startingScenePrefab);
        }

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

        public void CloseScene()
        {
            GotoScene(null);
        }
    }
}