using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLG
{
    [Serializable]
    public class VocabEntry
    {
        public string id;         // stable key, e.g. "apple" (same across all languages)
        public string word;       // in the language being learned, e.g. "la manzana"
        public string english;
        public string category;   // food, animal, town ... (see CategoryEntry)
        public int tier;          // lesson level 1 (Principiante) to 5 (Experto)
        public string icon;       // optional picture under Resources/Art/Icons
        public int hunger;        // food only: how much hunger it restores
    }

    [Serializable]
    public class SentenceEntry
    {
        public string id;
        public string text;       // in the language being learned
        public string english;
        public int tier;
        public string category;

        /// <summary>The word tiles the player arranges in sentence-building activities.</summary>
        public string[] Tokens => text.Split(' ');
    }

    /// <summary>A lesson topic, e.g. "food" = "La comida" / "Food".</summary>
    [Serializable]
    public class CategoryEntry
    {
        public string id, name, english;
    }

    [Serializable]
    public class LanguagePack
    {
        public string language;
        public List<CategoryEntry> categories = new List<CategoryEntry>();
        public List<VocabEntry> words = new List<VocabEntry>();
        public List<SentenceEntry> sentences = new List<SentenceEntry>();

        public List<VocabEntry> Words(string category = null, int maxTier = 5)
        {
            return words.FindAll(w => w.tier <= maxTier && (string.IsNullOrEmpty(category) || w.category == category));
        }

        public List<SentenceEntry> Sentences(int maxTier = 5)
        {
            return sentences.FindAll(s => s.tier <= maxTier);
        }

        public CategoryEntry Category(string id) => categories.Find(c => c.id == id);
    }

    /// <summary>
    /// Loads Resources/Languages/{code}/pack.json. Adding a language = adding a folder with
    /// the same file format; no code changes needed. The Spanish pack is built by
    /// Tools/data/build_spanish_pack.py. See Docs/03_LANGUAGE_DATA.md.
    /// </summary>
    public static class LanguageDatabase
    {
        static readonly Dictionary<string, LanguagePack> Cache = new Dictionary<string, LanguagePack>();

        public static LanguagePack Load(string code)
        {
            if (Cache.TryGetValue(code, out var cached)) return cached;

            var asset = Resources.Load<TextAsset>("Languages/" + code + "/pack");
            if (asset == null)
            {
                Debug.LogError("No language pack found at Resources/Languages/" + code + "/pack.json");
                return new LanguagePack { language = code };
            }
            var pack = JsonUtility.FromJson<LanguagePack>(asset.text);
            Cache[code] = pack;
            return pack;
        }
    }
}
