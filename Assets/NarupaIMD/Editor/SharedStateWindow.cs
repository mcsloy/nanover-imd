using System.Collections.Generic;
using Narupa.Session;
using UnityEditor;
using UnityEngine;

namespace NarupaIMD.Editor
{
    public class SharedStateWindow : EditorWindow
    {
        [MenuItem("Window/Shared State")]
        static void Init()
        {
            var window = (SharedStateWindow) GetWindow(typeof(SharedStateWindow));
            window.titleContent = new GUIContent("Shared State");
            window.Show();
        }

        private Dictionary<string, object> data = new Dictionary<string, object>
        {
            ["myfloat"] = 1.2f,
            ["mystring"] = "abc",
            ["myint"] = 12,
            ["mystruct"] = new Dictionary<string, object>
            {
                ["mybool"] = false
            },
            ["mylist"] = new List<object>
            {
                1.2f,
                "xyz"
            }
        };

        private NarupaMultiplayer multiplayer;

        private void OnGUI()
        {
            if (multiplayer == null)
                multiplayer = GameObject.FindObjectOfType<NarupaMultiplayer>();
            DrawObject(multiplayer.Session.SharedStateDictionary);
        }

        private void DrawObject(object value, string key = null)
        {
            switch (value)
            {
                case Dictionary<string, object> dict:
                    if (key != null)
                        EditorGUILayout.LabelField($"{key}:");
                    else
                        EditorGUILayout.LabelField($"{{dict}}:");
                    EditorGUI.indentLevel++;
                    foreach (var field in dict)
                    {
                        DrawObject(field.Value, field.Key);
                    }

                    EditorGUI.indentLevel--;
                    return;
                case List<object> list:
                    if (key != null)
                        EditorGUILayout.LabelField($"{key}:");
                    else
                        EditorGUILayout.LabelField($"{{list}}:");
                    EditorGUI.indentLevel++;
                    foreach (var field in list)
                    {
                        DrawObject(field);
                    }

                    EditorGUI.indentLevel--;
                    return;
                default:
                    if (key != null)
                        EditorGUILayout.LabelField($"{key}:", $"{value}");
                    else
                        EditorGUILayout.LabelField($"{value}");
                    return;
            }
        }
    }
}