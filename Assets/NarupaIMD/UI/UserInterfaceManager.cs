using System.Collections.Generic;
using System.Linq;
using Narupa.Frontend.UI;
using UnityEngine;

namespace NarupaIMD.UI
{
    public class UserInterfaceManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject currentScene;

        [SerializeField]
        private NarupaCanvas canvas;

        private Stack<GameObject> sceneStack = new Stack<GameObject>();

        public void GotoScene(GameObject newScene)
        {
            if (newScene == currentScene)
                return;
            if (currentScene != null)
                currentScene.SetActive(false);
            currentScene = newScene;
            canvas.enabled = newScene != null;
            if (newScene != null)
                newScene.SetActive(true);
        }

        public void GotoSceneAndAddToStack(GameObject newScene)
        {
            sceneStack.Push(currentScene);
            GotoScene(newScene);
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