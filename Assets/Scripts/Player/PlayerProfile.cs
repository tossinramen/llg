using System;
using System.IO;
using UnityEngine;

namespace LLG
{
    public enum Gender { Male, Female, NonBinary }

    [Serializable]
    public class CharacterAppearance
    {
        public int skin;
        public int hairStyle;
        public int hairColor;
        public int shirt;
        public int pants;
    }

    /// <summary>The colours and styles the character creator offers. Add entries to offer more.</summary>
    public static class AppearancePalette
    {
        public static readonly string[] HairStyles = { "Short", "Long", "Spiky", "Pigtails", "Buzz" };

        public static readonly Color32[] Skin =
        {
            new Color32(255, 224, 189, 255), new Color32(241, 194, 125, 255), new Color32(224, 172, 105, 255),
            new Color32(198, 134, 66, 255), new Color32(141, 85, 36, 255), new Color32(92, 56, 30, 255),
        };

        public static readonly Color32[] Hair =
        {
            new Color32(60, 40, 32, 255), new Color32(110, 70, 40, 255), new Color32(230, 190, 90, 255),
            new Color32(200, 80, 50, 255), new Color32(90, 90, 100, 255), new Color32(235, 235, 240, 255),
            new Color32(150, 90, 210, 255), new Color32(70, 150, 220, 255),
        };

        public static readonly Color32[] Shirt =
        {
            new Color32(210, 60, 60, 255), new Color32(70, 120, 220, 255), new Color32(70, 170, 90, 255),
            new Color32(240, 200, 70, 255), new Color32(240, 240, 240, 255), new Color32(150, 90, 200, 255),
            new Color32(240, 140, 60, 255), new Color32(60, 60, 70, 255),
        };

        public static readonly Color32[] Pants =
        {
            new Color32(60, 80, 150, 255), new Color32(110, 80, 55, 255), new Color32(70, 70, 80, 255),
            new Color32(200, 190, 160, 255), new Color32(60, 120, 90, 255), new Color32(130, 50, 60, 255),
        };
    }

    /// <summary>Everything chosen on the start screen. Saved as JSON between sessions.</summary>
    [Serializable]
    public class PlayerProfile
    {
        public string playerName = "";
        public Gender gender = Gender.NonBinary;
        public CharacterAppearance appearance = new CharacterAppearance();
        public string languageCode = "es";

        public static PlayerProfile Current { get; private set; }

        static string SavePath => Path.Combine(Application.persistentDataPath, "profile.json");

        public static PlayerProfile Load()
        {
            PlayerProfile profile = null;
            try
            {
                if (File.Exists(SavePath))
                    profile = JsonUtility.FromJson<PlayerProfile>(File.ReadAllText(SavePath));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Could not read saved profile, starting fresh: " + e.Message);
            }
            Current = profile ?? new PlayerProfile();
            return Current;
        }

        public void Save()
        {
            Current = this;
            File.WriteAllText(SavePath, JsonUtility.ToJson(this, true));
        }
    }
}
