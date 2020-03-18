using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NarupaIMD.UI
{
    public class UserInterfaceManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject currentScene;

        private Stack<GameObject> sceneStack = new Stack<GameObject>();

        public void GotoScene(GameObject newScene)
        {
            if (newScene == currentScene)
                return;
            currentScene?.SetActive(false);
            currentScene = newScene;
            newScene?.SetActive(true);
        }

        public void GotoSceneAndAddToStack(GameObject newScene)
        {
            GotoScene(newScene);
            sceneStack.Push(currentScene);
        }

        public void GoBack()
        {
            if (sceneStack.Any())
            {
                GotoScene(sceneStack.Pop());
            }
        }

        public void CloseScene()
        {
            sceneStack.Clear();
            GotoScene(null);
        }
    }
}