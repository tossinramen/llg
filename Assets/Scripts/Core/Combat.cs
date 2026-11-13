using UnityEngine;

namespace LLG
{
    /// <summary>
    /// The RPG maths, all in one place (and written up in Docs/GAME_GUIDE.md):
    ///   - you hit harder as you level, and harder still against monsters below your level;
    ///   - monsters above your level hit you harder and take less damage;
    ///   - armour blocks a percentage that shrinks against higher-level monsters;
    ///   - critical hits double damage;
    ///   - XP is best from monsters near your level and fades to almost nothing for much weaker ones;
    ///   - gold grows with the monster's level.
    /// </summary>
    public static class Combat
    {
        public static int PlayerLevel => GameState.Data.level;

        /// <summary>How a level gap changes damage: +5% per level above the target, -5% per level below
        /// (never less than 30% or more than 250%).</summary>
        public static float LevelFactor(int attacker, int defender) => Mathf.Clamp(1 + 0.05f * (attacker - defender), 0.3f, 2.5f);

        /// <summary>Your attack before the level gap: weapon damage (fists = 1) plus rune bonuses,
        /// +4% per character level, +1% per Combat (melee) or Archery (ranged) skill level.</summary>
        public static float AttackPower()
        {
            var weapon = GameState.EquippedWeapon();
            int basePower = (weapon?.power ?? 1) + Magic.WeaponBonus();
            var skill = weapon != null && weapon.ranged ? SkillType.Archery : SkillType.Combat;
            return basePower * (1 + 0.04f * (PlayerLevel - 1)) * (1 + 0.01f * (GameState.Data.skills.Get(skill).level - 1));
        }

        /// <summary>Chance of a critical hit (double damage): 5%, +3% per Suerte rune rank worn, +0.2% per Combat level (max 40%).</summary>
        public static float CritChance() =>
            Mathf.Min(0.4f, 0.05f + 0.03f * Magic.Worn("luck") + 0.002f * GameState.Data.skills.Get(SkillType.Combat).level);

        /// <summary>One hit from you on a monster of this level.</summary>
        public static (int damage, bool crit) PlayerHit(int mobLevel)
        {
            bool crit = Random.value < CritChance();
            float dmg = AttackPower() * LevelFactor(PlayerLevel, mobLevel) * Random.Range(0.9f, 1.1f) * (crit ? 2 : 1);
            return (Mathf.Max(1, Mathf.RoundToInt(dmg)), crit);
        }

        /// <summary>A spell's damage on a monster: the spell's power, +3% per character level, changed by the level gap.</summary>
        public static int SpellHit(int power, int mobLevel) =>
            Mathf.Max(1, Mathf.RoundToInt(power * (1 + 0.03f * (PlayerLevel - 1)) * LevelFactor(PlayerLevel, mobLevel)));

        /// <summary>The share of a monster's hit your armour blocks: defence / (defence + 20 + 4 x monster level), at most 75%.</summary>
        public static float Mitigation(int mobLevel)
        {
            float def = GameState.Defense();
            return Mathf.Min(0.75f, def / (def + 20 + 4 * mobLevel));
        }

        /// <summary>One bite from a monster: its base bite, +12% per monster level, changed by the level gap, less your armour.</summary>
        public static int MobHit(int baseDamage, int mobLevel)
        {
            float raw = baseDamage * (1 + 0.12f * (mobLevel - 1)) * LevelFactor(mobLevel, PlayerLevel);
            return Mathf.Max(1, Mathf.RoundToInt(raw * (1 - Mitigation(mobLevel))));
        }

        /// <summary>A monster's health: its base health, +30% per level.</summary>
        public static int MobHp(int baseHp, int mobLevel) => Mathf.RoundToInt(baseHp * (1 + 0.3f * (mobLevel - 1)));

        /// <summary>XP for a kill: base +15% per monster level; full XP within 2 levels below you, -15% per level
        /// under that (never less than 10%); +10% per level the monster is above you (up to +50%).</summary>
        public static int XpReward(int baseXp, int mobLevel)
        {
            int gap = PlayerLevel - mobLevel;
            float factor = gap > 2 ? Mathf.Max(0.1f, 1 - 0.15f * (gap - 2)) : gap < 0 ? Mathf.Min(1.5f, 1 + 0.1f * -gap) : 1;
            return Mathf.Max(1, Mathf.RoundToInt(baseXp * (1 + 0.15f * (mobLevel - 1)) * factor));
        }

        /// <summary>Gold for a kill: the monster's base drop, +20% per monster level.</summary>
        public static int GoldReward(int low, int high, int mobLevel) =>
            Mathf.RoundToInt(Random.Range(low, high + 1) * (1 + 0.2f * (mobLevel - 1)));

        /// <summary>How dangerous a monster is compared with you, as a colour for its name (like other RPGs):
        /// grey = trivial, green = easy, yellow = even, orange = hard, red = deadly.</summary>
        public static Color Danger(int mobLevel)
        {
            int gap = mobLevel - PlayerLevel;
            if (gap >= 5) return new Color32(240, 70, 60, 255);
            if (gap >= 3) return new Color32(240, 150, 50, 255);
            if (gap >= -2) return new Color32(240, 220, 80, 255);
            if (gap >= -7) return new Color32(110, 210, 100, 255);
            return new Color32(170, 170, 176, 255);
        }
    }
}
