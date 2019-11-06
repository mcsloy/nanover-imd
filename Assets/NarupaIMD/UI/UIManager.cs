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
        private VrController controller;
        
        private void Awake()
        {
            Assert.IsNotNull(camera);
            Assert.IsNotNull(controller);
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
                currentScene = Instantiate(scenePrefab, this.transform);
                currentScene.gameObject.SetActive(true);
                currentScene.SetCamera(camera);
                currentScene.SetController(controller);
            }
        }
        
        public void CloseScene()
        {
            GotoScene(null);
        }
    }
}