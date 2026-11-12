using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLG
{
    [Serializable] public class SpellRank { public string id; public int rank; }
    [Serializable] public class EnchantEntry { public string item, ench; public int rank; }

    /// <summary>
    /// Spells (K) and enchantments (the altar in La Torre Rúnica). Both are paid for with Lexa.
    /// Spells are cast by building the Spanish word letter by letter - a wrong letter fizzles.
    /// </summary>
    public static class Magic
    {
        public class Spell
        {
            public string id, es, en, word, icon, effectEs, effectEn;
            public int level, mana;
            public int LearnCost => level * 20;
            public int UpgradeCost(int rank) => LearnCost * (rank + 1);
        }

        public const int MaxSpellRank = 5;

        public static readonly Spell[] Spells =
        {
            S("spark", "Chispa", "Spark", "chispa", 25, 3, "Fuego pequeño", "Small fire damage"),
            S("shield", "Escudo de luz", "Light Shield", "escudo", 30, 4, "Bloquea el próximo golpe", "Blocks the next hit"),
            S("lightning", "Rayo", "Lightning", "rayo", 35, 6, "Mucho daño a un enemigo", "Big damage to one enemy"),
            S("ice", "Hielo", "Ice", "hielo", 40, 5, "Congela al enemigo", "Freezes the enemy"),
            S("heal", "Curación", "Heal", "curar", 45, 5, "Recupera vida", "Restores HP"),
            S("quake", "Terremoto", "Earthquake", "terremoto", 55, 8, "Golpea a todos los enemigos", "Hits every enemy"),
            S("meteor", "Meteoro", "Meteor", "meteoro", 70, 12, "Daño enorme", "Huge damage (long cooldown)"),
        };

        static Spell S(string id, string es, string en, string word, int level, int mana, string effEs, string effEn) => new Spell
        {
            id = id, es = es, en = en, word = word, level = level, mana = mana, icon = "spell_" + (id == "lightning" ? "lightning" : id),
            effectEs = effEs, effectEn = effEn,
        };

        public static Spell GetSpell(string id) => Array.Find(Spells, s => s.id == id);

        public static int Rank(string spellId)
        {
            var r = GameState.Data.spells.Find(s => s.id == spellId);
            return r == null ? 0 : r.rank;
        }

        /// <summary>How strong a spell is at its current rank (damage or healing).</summary>
        public static int Power(Spell s)
        {
            int r = Mathf.Max(1, Rank(s.id));
            return s.id switch
            {
                "spark" => 10 + 6 * r, "lightning" => 25 + 10 * r, "heal" => 10 + 8 * r,
                "quake" => 20 + 8 * r, "meteor" => 50 + 20 * r, "ice" => 2 + r, _ => r,
            };
        }

        /// <summary>Learn a spell, or raise it one rank. Costs Lexa.</summary>
        public static bool Upgrade(Spell s)
        {
            int rank = Rank(s.id);
            if (GameState.Data.level < s.level) { GameState.Say("Necesitas el nivel " + s.level + "."); return false; }
            if (rank >= MaxSpellRank) { GameState.Say("Ya está al máximo."); return false; }
            int cost = rank == 0 ? s.LearnCost : s.UpgradeCost(rank);
            if (GameState.Data.stats.coins < cost) { GameState.Say("Necesitas " + GameState.Short(cost) + " " + GameConfig.CurrencyName + "."); return false; }
            GameState.Data.stats.AddCoins(-cost);
            var entry = GameState.Data.spells.Find(x => x.id == s.id);
            if (entry == null) GameState.Data.spells.Add(new SpellRank { id = s.id, rank = 1 });
            else entry.rank++;
            if (string.IsNullOrEmpty(GameState.Data.preparedSpell)) GameState.Data.preparedSpell = s.id;
            GameState.Say(rank == 0 ? "¡Aprendes " + s.es + "!" : s.es + " sube al rango " + (rank + 1) + ".");
            GameState.AddXp(SkillType.Magic, 20);
            GameState.Save();
            return true;
        }

        // ---------------------------------------------------------------- enchantments
        public class Enchant
        {
            public string id, es, en, icon, target, effectEs, effectEn;   // target: weapon, armor or any
        }

        public const int MaxEnchantRank = 3;
        public static readonly int[] EnchantCost = { 100, 400, 1000 };

        public static readonly Enchant[] Enchants =
        {
            E("sharp", "Afilado", "Sharpness", "ench_sharp", "weapon", "+2 de daño por rango", "+2 damage per rank"),
            E("fire", "Fuego", "Fire", "ench_fire", "weapon", "Quema: +1 de daño por rango", "Burns: +1 damage per rank"),
            E("ice", "Hielo", "Ice", "ench_ice", "weapon", "Ralentiza al enemigo", "Slows the enemy"),
            E("protect", "Protección", "Protection", "ench_protect", "armor", "+1 de defensa por rango", "+1 defence per rank"),
            E("vampire", "Vampiro", "Vampire", "ench_vampire", "weapon", "Recuperas vida al golpear", "Heal when you hit"),
            E("luck", "Suerte", "Luck", "ench_luck", "any", "+10% de Lexa por rango", "+10% Lexa per rank"),
        };

        static Enchant E(string id, string es, string en, string icon, string target, string effEs, string effEn) => new Enchant
        { id = id, es = es, en = en, icon = icon, target = target, effectEs = effEs, effectEn = effEn };

        public static Enchant GetEnchant(string id) => Array.Find(Enchants, e => e.id == id);

        public static EnchantEntry On(string itemId) => GameState.Data.enchants.Find(e => e.item == itemId);

        public static bool Fits(Enchant e, ItemDef item)
        {
            if (item == null) return false;
            if (e.target == "any") return item.kind == ItemKind.Weapon || item.kind == ItemKind.Gear;
            if (e.target == "weapon") return item.kind == ItemKind.Weapon;
            return item.kind == ItemKind.Gear;
        }

        /// <summary>Put an enchantment on an item (or raise its rank). One enchantment per item.</summary>
        public static bool Apply(Enchant e, ItemDef item)
        {
            if (!Fits(e, item)) { GameState.Say("Ese encanto no sirve para esto."); return false; }
            var current = On(item.id);
            int rank = current != null && current.ench == e.id ? current.rank : 0;
            if (rank >= MaxEnchantRank) { GameState.Say("Ya está al máximo."); return false; }
            int cost = EnchantCost[rank];
            if (GameState.Data.stats.coins < cost) { GameState.Say("Necesitas " + GameState.Short(cost) + " " + GameConfig.CurrencyName + "."); return false; }
            GameState.Data.stats.AddCoins(-cost);
            if (current == null) GameState.Data.enchants.Add(new EnchantEntry { item = item.id, ench = e.id, rank = 1 });
            else { current.ench = e.id; current.rank = rank + 1; }
            GameState.Say("¡" + GameState.Capital(item.spanish) + " brilla! " + e.es + " " + Roman(rank + 1));
            GameState.AddXp(SkillType.Magic, 10);
            GameState.Save();
            return true;
        }

        public static string Roman(int n) => n switch { 1 => "I", 2 => "II", 3 => "III", 4 => "IV", 5 => "V", _ => "" };

        /// <summary>Total rank of one enchantment across everything you are wearing.</summary>
        public static int Worn(string enchId)
        {
            int total = 0;
            foreach (var id in GameState.Data.equipped)
            {
                if (string.IsNullOrEmpty(id)) continue;
                var e = On(id);
                if (e != null && e.ench == enchId) total += e.rank;
            }
            return total;
        }

        /// <summary>Enchantment rank on the weapon in your hand.</summary>
        public static int OnWeapon(string enchId)
        {
            var w = GameState.EquippedWeapon();
            var e = w == null ? null : On(w.id);
            return e != null && e.ench == enchId ? e.rank : 0;
        }

        public static int WeaponBonus() => OnWeapon("sharp") * 2 + OnWeapon("fire");
    }
}
