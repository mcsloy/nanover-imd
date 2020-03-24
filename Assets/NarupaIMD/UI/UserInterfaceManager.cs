using System.Collections.Generic;
using System.Linq;
using Narupa.Frontend.UI;
using UnityEngine;

namespace NarupaIMD.UI
{
    public class UserInterfaceManager : MonoBehaviour
    {
        private GameObject currentScenePrefab;
        
        [SerializeField]
        private GameObject currentScene;

        [SerializeField]
        private GameObject initialScene;

        [SerializeField]
        private NarupaCanvas canvas;

        private Stack<GameObject> sceneStack = new Stack<GameObject>();

        private void Start()
        {
            if (initialScene != null)
                GotoScene(initialScene);
        }

        private void LeaveScene(GameObject scene)
        {
            WorldSpaceCursorInput.ClearSelection();
            Destroy(scene);
        }

        private GameObject EnterScene(GameObject scene)
        {
            if (scene != null)
            {
                var newScene = Instantiate(scene, transform);
                newScene.SetActive(true);
                return newScene;
            }

            return null;
        }

        public void GotoScene(GameObject scene)
        {
            if (currentScene != null)
                LeaveScene(currentScene);
            currentScene = EnterScene(scene);
            if (currentScene != null)
                currentScenePrefab = scene;
            else
                currentScenePrefab = null;
            canvas.enabled = currentScene != null;
        }

        public void GotoSceneAndAddToStack(GameObject newScene)
        {
            var previousScenePrefab = currentScenePrefab;
            GotoScene(newScene);
            if (newScene != null && previousScenePrefab != null)
                sceneStack.Push(previousScenePrefab);
        }

        public void GoBack()
        {
            if (sceneStack.Any())
            {
                GotoScene(sceneStack.Pop());
                sceneStack.Clear();
            }
        }

        public void CloseScene()
        {
            sceneStack.Clear();
            GotoScene(null);
        }
    }
}