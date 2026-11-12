using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LLG
{
    [Serializable]
    public class BagSlot
    {
        public string id = "";
        public int count;
        public bool Empty => string.IsNullOrEmpty(id) || count <= 0;
    }

    /// <summary>Everything about the current playthrough that gets saved.</summary>
    [Serializable]
    public class SaveData
    {
        public PlayerStats stats = new PlayerStats();
        public SkillSet skills = new SkillSet();
        public List<BagSlot> bag = new List<BagSlot>();
        public List<string> equipped = new List<string>();     // one item id per EquipSlot ("" = empty)
        public string pet = "";                                 // the pet following you
        public int fishCaught;
        public bool hasPosition;                                // where the player was when they last saved
        public float posX, posY;
        public int facing;
        public List<string> items = new List<string>();         // old saves only (moved into the bag on load)
        public int level = 1, xp;                               // your character level (no maximum)
        public string lessonCategory = "";                      // the chosen lesson topic ("" = all)
        public int lessonLevel = 1;                             // 1 Principiante ... 5 Experto
        public List<PlotState> plots = new List<PlotState>();   // your vegetable patch
        public int dungeonFloor;                                // highest dungeon floor beaten
    }

    /// <summary>The running game: the save data, the loaded language, inventory and helper actions.</summary>
    public static class GameState
    {
        public const int BagSize = 20, StackSize = 20;

        public static SaveData Data { get; private set; }
        public static LanguagePack Pack { get; private set; }

        /// <summary>Short pop-up messages ("+10 Fishing XP"). The HUD listens to this.</summary>
        public static event Action<string> Toast;
        /// <summary>Fires when the bag or equipment changes, so windows and the character can refresh.</summary>
        public static event Action InventoryChanged;

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
            if (!Data.stats.goldGiven)
            {
                Data.stats.gold += GameConfig.StartingGold;
                Data.stats.goldGiven = true;
            }
            Normalise();
            Data.skills.LeveledUp += (skill, level) => Say(skill + " → " + level);
            if (Data.level < 1) Data.level = 1;
            if (Data.lessonLevel < 1) Data.lessonLevel = 1;

            var profile = PlayerProfile.Current ?? PlayerProfile.Load();
            Pack = LanguageDatabase.Load(profile.languageCode);
        }

        /// <summary>Makes sure the bag and equipment lists have the right size, and upgrades old saves.</summary>
        static void Normalise()
        {
            while (Data.bag.Count < BagSize) Data.bag.Add(new BagSlot());
            int slots = Enum.GetValues(typeof(EquipSlot)).Length;
            while (Data.equipped.Count < slots) Data.equipped.Add("");
            if (Data.items != null && Data.items.Count > 0)
            {
                foreach (var id in Data.items) AddItem(id, 1, false);
                Data.items.Clear();
            }
            RecalculateStats();
        }

        public static void Save()
        {
            if (Data == null) return;
            try { File.WriteAllText(SavePath, JsonUtility.ToJson(Data, true)); }
            catch (Exception e) { Debug.LogWarning("Could not save: " + e.Message); }
        }

        public static void Say(string message) => Toast?.Invoke(message);

        static void Changed()
        {
            RecalculateStats();
            InventoryChanged?.Invoke();
        }

        // ------------------------------------------------------------ bag

        public static int Count(string id)
        {
            int n = 0;
            foreach (var s in Data.bag) if (s.id == id) n += s.count;
            return n;
        }

        /// <summary>True if the item is in the bag or being worn.</summary>
        public static bool Has(string id) => Count(id) > 0 || Data.equipped.Contains(id);

        /// <summary>Puts an item in the bag. Returns false if the bag is full.</summary>
        public static bool AddItem(string id, int count = 1, bool notify = true)
        {
            var item = ItemDatabase.Get(id);
            if (item == null) return false;
            for (int n = 0; n < count; n++)
            {
                BagSlot target = null;
                if (item.Stacks)
                    foreach (var s in Data.bag)
                        if (s.id == id && s.count < StackSize) { target = s; break; }
                if (target == null)
                    foreach (var s in Data.bag)
                        if (s.Empty) { target = s; s.id = id; s.count = 0; break; }
                if (target == null)
                {
                    if (notify) Say("¡La mochila está llena! (Your bag is full!)");
                    if (notify) Changed();
                    return false;
                }
                target.count++;
            }
            if (notify) Changed();
            return true;
        }

        public static void RemoveAt(int index, int count = 1)
        {
            var s = Data.bag[index];
            s.count -= count;
            if (s.count <= 0) { s.id = ""; s.count = 0; }
            Changed();
        }

        /// <summary>What happens when you click an item in the bag: eat, read, wear or call a pet.</summary>
        public static void UseBagSlot(int index)
        {
            var s = Data.bag[index];
            if (s.Empty) return;
            var item = ItemDatabase.Get(s.id);
            var stats = Data.stats;
            switch (item.kind)
            {
                case ItemKind.Food:
                    stats.Eat(item.power);
                    Say("¡Qué rico! " + Capital(item.spanish) + "  +" + item.power);
                    RemoveAt(index);
                    break;
                case ItemKind.Book:
                    stats.intelligence++;
                    stats.maxMana++;
                    stats.RestoreMana(1);
                    Say("Lees " + item.spanish + ". ¡Inteligencia +1!");
                    RemoveAt(index);
                    break;
                case ItemKind.Weapon:
                case ItemKind.Gear:
                    Equip(index);
                    break;
                case ItemKind.Pet:
                    Data.pet = Data.pet == item.id ? "" : item.id;
                    Say(Data.pet == "" ? "Tu mascota descansa." : "¡" + Capital(item.spanish) + " te sigue!");
                    Changed();
                    break;
                case ItemKind.Rod:
                    Say("Usa la caña en el estanque. (Use it at the pond.)");
                    break;
                case ItemKind.Seed:
                    Say("Plántalas en tu huerto, al lado de tu casa. (Plant them in your patch next to your house.)");
                    break;
                case ItemKind.Instrument:
                    Say("Toca música en la escuela de música. (Play it at the music school.)");
                    break;
                case ItemKind.Material:
                    Say("Véndelo en una tienda. (Sell it in a shop.)");
                    break;
            }
        }

        // ------------------------------------------------------------ equipment

        public static ItemDef Equipped(EquipSlot slot) => ItemDatabase.Get(Data.equipped[(int)slot]);

        /// <summary>Wears the item from this bag slot; whatever was in that equipment slot goes back in the bag.</summary>
        public static void Equip(int bagIndex)
        {
            var s = Data.bag[bagIndex];
            var item = ItemDatabase.Get(s.id);
            if (item == null || item.slot == EquipSlot.None) return;
            string old = Data.equipped[(int)item.slot];
            Data.equipped[(int)item.slot] = item.id;
            s.id = old;
            s.count = string.IsNullOrEmpty(old) ? 0 : 1;
            Say("Llevas " + item.spanish + ".");
            Changed();
        }

        public static void Unequip(EquipSlot slot)
        {
            string id = Data.equipped[(int)slot];
            if (string.IsNullOrEmpty(id)) return;
            foreach (var s in Data.bag)
            {
                if (!s.Empty) continue;
                s.id = id;
                s.count = 1;
                Data.equipped[(int)slot] = "";
                Changed();
                return;
            }
            Say("¡La mochila está llena! (Your bag is full!)");
        }

        public static ItemDef EquippedWeapon()
        {
            var item = Equipped(EquipSlot.Primary);
            return item != null && item.kind == ItemKind.Weapon ? item : null;
        }

        public static int Attack() => EquippedWeapon()?.power ?? 0;

        public static int Defense()
        {
            int total = 0;
            foreach (var id in Data.equipped) total += ItemDatabase.Get(id)?.defense ?? 0;
            return total;
        }

        static void RecalculateStats()
        {
            int bonus = 0;
            foreach (var id in Data.equipped) bonus += ItemDatabase.Get(id)?.mana ?? 0;
            var stats = Data.stats;
            stats.maxMana = 10 + stats.intelligence + bonus;
            stats.RestoreMana(0);
        }

        // ------------------------------------------------------------ rewards

        /// <summary>XP needed to go from this character level to the next. Levels never stop.</summary>
        public static int XpForLevel(int level) => Mathf.RoundToInt(80 * Mathf.Pow(level, 1.4f));

        /// <summary>Gives XP to a skill and to your character level.
        /// Each point of intelligence adds 5% bonus XP.</summary>
        public static void AddXp(SkillType skill, int amount)
        {
            amount = Mathf.Max(1, Mathf.RoundToInt(amount * (1f + 0.05f * Data.stats.intelligence)));
            Say("+" + amount + " XP");
            Data.skills.AddXp(skill, amount);
            Data.xp += amount;
            while (Data.xp >= XpForLevel(Data.level))
            {
                Data.xp -= XpForLevel(Data.level);
                Data.level++;
                Data.stats.maxHp += 2;
                Data.stats.Heal(Data.stats.maxHp);
                Say("¡Subes al nivel " + Data.level + "!  (Level up!)");
            }
        }

        /// <summary>Lexa: the magical money you earn by learning (activities, bosses).</summary>
        public static void AddCurrency(int amount)
        {
            Data.stats.AddCoins(amount);
            if (amount > 0) Say("+" + amount + " " + GameConfig.CurrencyName);
        }

        /// <summary>Gold (oro): everyday money from selling things and defeating animals.</summary>
        public static void AddGold(int amount)
        {
            Data.stats.AddGold(amount);
            if (amount > 0) Say("+" + amount + " " + GameConfig.GoldName);
        }

        /// <summary>Pets are bought with Lexa; everything else in the shops costs gold.</summary>
        public static bool PaidInLexa(ItemDef item) => item.kind == ItemKind.Pet;

        /// <summary>Swap Lexa for gold (10 Lexa = 20 gold).</summary>
        public static void ExchangeLexa()
        {
            if (Data.stats.coins < 10) { Say("Necesitas 10 " + GameConfig.CurrencyName + ". (You need 10 Lexa.)"); return; }
            Data.stats.AddCoins(-10);
            Data.stats.AddGold(20);
            Say("Cambias 10 " + GameConfig.CurrencyName + " por 20 " + GameConfig.GoldName + ".");
        }

        public static string Capital(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);
    }
}
