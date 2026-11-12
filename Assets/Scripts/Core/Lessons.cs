using System.Collections.Generic;
using UnityEngine;

namespace LLG
{
    /// <summary>
    /// The lesson you have chosen (on the lesson board in the plaza, or with L): a topic such as
    /// "La comida", and a level from 1 (Principiante) to 5 (Experto). The activities (archery,
    /// fishing, El intruso) use these words, and harder levels give more XP.
    /// </summary>
    public static class Lessons
    {
        public static readonly string[] LevelNames = { "Principiante", "Básico", "Intermedio", "Avanzado", "Experto" };
        public static readonly string[] LevelEnglish = { "Beginner", "Elementary", "Intermediate", "Advanced", "Expert" };
        public static readonly float[] XpMultiplier = { 1f, 1.5f, 2f, 3f, 4f };

        public static int Level => Mathf.Clamp(GameState.Data.lessonLevel, 1, 5);

        /// <summary>The chosen topic id, or "" for every topic.</summary>
        public static string Category => GameState.Data.lessonCategory ?? "";

        public static float Multiplier => XpMultiplier[Level - 1];

        public static int Xp(int baseXp) => Mathf.RoundToInt(baseXp * Multiplier);

        public static string CategoryName(bool english = false)
        {
            if (string.IsNullOrEmpty(Category)) return english ? "All topics" : "Todos los temas";
            var c = GameState.Pack.Category(Category);
            return c == null ? Category : english ? c.english : c.name;
        }

        /// <summary>Words for the current lesson: this level first, then easier ones if there are too few.</summary>
        public static List<VocabEntry> Words(int atLeast = 4)
        {
            var all = GameState.Pack.words;
            var list = all.FindAll(w => w.tier == Level && Matches(w.category));
            if (list.Count < atLeast) list = all.FindAll(w => w.tier <= Level && Matches(w.category));
            if (list.Count < atLeast) list = all.FindAll(w => w.tier <= Level);
            if (list.Count < atLeast) list = new List<VocabEntry>(all);
            return list;
        }

        public static List<SentenceEntry> Sentences()
        {
            var all = GameState.Pack.sentences;
            var list = all.FindAll(s => s.tier == Level && Matches(s.category));
            if (list.Count == 0) list = all.FindAll(s => s.tier <= Level && Matches(s.category));
            if (list.Count == 0) list = all.FindAll(s => s.tier == Level);
            if (list.Count == 0) list = new List<SentenceEntry>(all);
            return list;
        }

        static bool Matches(string category) => string.IsNullOrEmpty(Category) || category == Category;
    }
}
