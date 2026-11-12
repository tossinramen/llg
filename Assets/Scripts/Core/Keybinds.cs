using System;
using UnityEngine;

namespace LLG
{
    public enum GameAction { Up, Down, Left, Right, Use, Backpack, Profile, Jump, Crouch, Run }

    /// <summary>
    /// Which key does what. Players can change these in Options; they are remembered between sessions.
    /// The arrow keys always move as well, and Esc always opens the menu.
    /// </summary>
    public static class Keybinds
    {
        public static readonly GameAction[] All = (GameAction[])Enum.GetValues(typeof(GameAction));

        static readonly KeyCode[] Defaults =
        {
            KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.E, KeyCode.B, KeyCode.U,
            KeyCode.Space, KeyCode.LeftShift, KeyCode.LeftControl,
        };

        static readonly string[] Names =
        {
            "Arriba", "Abajo", "Izquierda", "Derecha", "Usar", "Mochila", "Perfil", "Saltar", "Agacharse", "Correr",
        };

        static readonly string[] EnglishNames =
        {
            "Up", "Down", "Left", "Right", "Use", "Backpack", "Profile", "Jump", "Crouch", "Run",
        };

        static KeyCode[] keys;

        /// <summary>Fires whenever a key is changed, so on-screen hints can update.</summary>
        public static event Action Changed;

        static KeyCode[] Keys
        {
            get
            {
                if (keys != null) return keys;
                keys = new KeyCode[All.Length];
                for (int i = 0; i < All.Length; i++)
                    keys[i] = (KeyCode)PlayerPrefs.GetInt("key_" + All[i], (int)Defaults[i]);
                return keys;
            }
        }

        public static KeyCode Get(GameAction action) => Keys[(int)action];

        /// <summary>The action's name in Spanish.</summary>
        public static string Name(GameAction action) => Names[(int)action];

        public static string EnglishName(GameAction action) => EnglishNames[(int)action];

        /// <summary>Changes a key. If another action already used it, the two swap.</summary>
        public static void Set(GameAction action, KeyCode key)
        {
            for (int i = 0; i < Keys.Length; i++)
                if (Keys[i] == key && i != (int)action)
                    Store((GameAction)i, Keys[(int)action]);
            Store(action, key);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static void ResetAll()
        {
            for (int i = 0; i < All.Length; i++) Store(All[i], Defaults[i]);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        static void Store(GameAction action, KeyCode key)
        {
            Keys[(int)action] = key;
            PlayerPrefs.SetInt("key_" + action, (int)key);
        }

        public static bool Held(GameAction action) => Input.GetKey(Get(action));

        public static bool Down(GameAction action) => Input.GetKeyDown(Get(action));

        /// <summary>A short readable name for a key, e.g. "Shift", "Ctrl", "Space".</summary>
        public static string KeyName(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.LeftShift: case KeyCode.RightShift: return "Shift";
                case KeyCode.LeftControl: case KeyCode.RightControl: return "Ctrl";
                case KeyCode.LeftAlt: case KeyCode.RightAlt: return "Alt";
                case KeyCode.Return: return "Enter";
                case KeyCode.UpArrow: return "Up arrow";
                case KeyCode.DownArrow: return "Down arrow";
                case KeyCode.LeftArrow: return "Left arrow";
                case KeyCode.RightArrow: return "Right arrow";
            }
            string name = key.ToString();
            if (name.StartsWith("Alpha")) return name.Substring(5);
            if (name.StartsWith("Keypad")) return "Num " + name.Substring(6);
            return name;
        }

        public static string KeyName(GameAction action) => KeyName(Get(action));
    }
}
