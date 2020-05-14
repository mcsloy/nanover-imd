using System;
using System.Text.RegularExpressions;
using NarupaIMD.Selection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NarupaIMD.UI
{
    public class PlayerName : MonoBehaviour
    {
        private static string NameKey = "narupa.player.name";

        [SerializeField]
        private TMP_Text text;
        
        public static string GetPlayerName()
        {
            if (PlayerPrefs.HasKey(NameKey))
            {
                var value = PlayerPrefs.GetString(NameKey);
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            return Convert(Environment.UserName);
        }
        
        private static Regex usernameTextRegex = new Regex("[a-zA-Z]+");

        private static string Convert(string username)
        {
            var matches = usernameTextRegex.Matches(username);
            if (matches.Count > 0)
            {
                var name = matches[0].Value;
                if (name.Length > 7)
                    name = name.Substring(0, 7);
                return name;
            }

            return "User";
        }
        
        public static void GetPlayerName(string name)
        {
            PlayerPrefs.SetString(NameKey, name);
            PlayerNameChanged?.Invoke();
        }

        public void SetPlayerName(string name)
        {
            PlayerPrefs.SetString(NameKey, name);
        }

        public static event Action PlayerNameChanged;

        private void Awake()
        {
            PlayerNameChanged += UpdateUserText;
            UpdateUserText();
        }

        private void UpdateUserText()
        {
            if (text != null)
                text.text = GetPlayerName();
        }
    }
}