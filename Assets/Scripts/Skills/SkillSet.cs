using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLG
{
    public enum SkillType
    {
        // language skills
        Vocabulary, Grammar, Reading, Listening, Spelling, Conversation,
        // life skills
        Cooking, Fishing, Farming, Trading, Crafting, Exploration, Magic, Archery, Combat, Woodcutting, Music,
    }

    [Serializable]
    public class SkillProgress
    {
        public SkillType type;
        public int level = 1;
        public int xp;
    }

    /// <summary>
    /// All of the player's skills. Each activity gives XP to one life skill and one language skill.
    /// Higher levels unlock harder word tiers (see WordTierFor).
    /// </summary>
    [Serializable]
    public class SkillSet
    {
        public const int MaxLevel = int.MaxValue;     // skills keep levelling forever
        public List<SkillProgress> skills = new List<SkillProgress>();

        public event Action<SkillType, int> LeveledUp;

        public SkillProgress Get(SkillType type)
        {
            foreach (var s in skills)
                if (s.type == type) return s;
            var created = new SkillProgress { type = type };
            skills.Add(created);
            return created;
        }

        public static int XpForNextLevel(int level) => Mathf.RoundToInt(40f * Mathf.Pow(level, 1.6f));

        public void AddXp(SkillType type, int amount)
        {
            var s = Get(type);
            s.xp += amount;
            while (s.level < MaxLevel && s.xp >= XpForNextLevel(s.level))
            {
                s.xp -= XpForNextLevel(s.level);
                s.level++;
                LeveledUp?.Invoke(type, s.level);
            }
        }

        /// <summary>Levels 1-9 use tier 1 words, 10-19 tier 2 ... 40+ tier 5.</summary>
        public int WordTierFor(SkillType type) => Mathf.Clamp(1 + Get(type).level / 10, 1, 5);
    }
}
