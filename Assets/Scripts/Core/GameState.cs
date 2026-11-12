using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LLG
{
    /// <summary>Everything about the current playthrough that gets saved.</summary>
    [Serializable]
    public class SaveData
    {
        public PlayerStats stats = new PlayerStats();
        public SkillSet skills = new SkillSet();
        public List<string> items = new List<string>();    // ids of owned rods, weapons...
        public int fishCaught;
    }

    /// <summary>The running game: the save data, the loaded language, and helper actions.</summary>
    public static class GameState
    {
        public static SaveData Data { get; private set; }
        public static LanguagePack Pack { get; private set; }

        /// <summary>Short pop-up messages ("+10 Fishing XP"). The HUD listens to this.</summary>
        public static event Action<string> Toast;

        static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        public static void Load()
        {
            SaveData data = null;
            try
            {
                if (File.Exists(SavePath)) data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Could not read the save file, starting fresh: " + e.Message);
            }
            if (data == null)
            {
                data = new SaveData();
                data.stats.coins = GameConfig.StartingCurrency;
            }
            Data = data;
            Data.skills.LeveledUp += (skill, level) => Say("Level up! " + skill + " Lv " + level);

            var profile = PlayerProfile.Current ?? PlayerProfile.Load();
            Pack = LanguageDatabase.Load(profile.languageCode);
        }

        public static void Save()
        {
            if (Data == null) return;
            try { File.WriteAllText(SavePath, JsonUtility.ToJson(Data, true)); }
            catch (Exception e) { Debug.LogWarning("Could not save: " + e.Message); }
        }

        public static void Say(string message) => Toast?.Invoke(message);

        public static bool Has(string itemId) => Data.items.Contains(itemId);

        /// <summary>Gives skill XP. Each point of intelligence adds 5% bonus XP.</summary>
        public static void AddXp(SkillType skill, int amount)
        {
            amount = Mathf.RoundToInt(amount * (1f + 0.05f * Data.stats.intelligence));
            Say("+" + amount + " " + skill + " XP");
            Data.skills.AddXp(skill, amount);
        }

        public static void AddCurrency(int amount)
        {
            Data.stats.AddCoins(amount);
            if (amount > 0) Say("+" + amount + " " + GameConfig.CurrencyName);
        }

        /// <summary>Attack power of the best weapon owned (for combat later).</summary>
        public static int Attack()
        {
            int best = 0;
            foreach (var id in Data.items)
            {
                var item = ItemDatabase.Get(id);
                if (item != null && item.kind == ItemKind.Weapon) best = Mathf.Max(best, item.power);
            }
            return best;
        }
    }
}
