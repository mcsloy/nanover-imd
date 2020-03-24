using System;
using System.Collections;
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
        private GameObject initialScene;

        [SerializeField]
        private NarupaCanvas canvas;

        private Stack<GameObject> sceneStack = new Stack<GameObject>();

        private void Start()
        {
            if(initialScene != null)
                GotoScene(initialScene);
        }

        private void LeaveScene(GameObject scene)
        {
            WorldSpaceCursorInput.ClearSelection();
            scene.SetActive(false);
        }

        private void EnterScene(GameObject scene)
        {
            scene.SetActive(true);
        }

        public void GotoScene(GameObject scene)
        {
            if (scene == currentScene)
                return;
            if (currentScene != null)
                LeaveScene(currentScene);
            currentScene = scene;
            canvas.enabled = currentScene != null;
            if (currentScene != null)
                EnterScene(currentScene);
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