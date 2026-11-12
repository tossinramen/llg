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
        public string category;   // food, animal, place ...
        public int tier;          // 1 (easiest) to 5
        public string icon;       // optional sprite under Resources/Art/Icons
        public int hunger;        // food only: how much hunger it restores
    }

    [Serializable]
    public class SentenceEntry
    {
        public string id;
        public string text;       // in the language being learned
        public string english;
        public int tier;

        /// <summary>The word tiles the player arranges in sentence-building activities.</summary>
        public string[] Tokens => text.Split(' ');
    }

    [Serializable]
    public class LanguagePack
    {
        public string language;
        public List<VocabEntry> words = new List<VocabEntry>();
        public List<SentenceEntry> sentences = new List<SentenceEntry>();

        public List<VocabEntry> Words(string category = null, int maxTier = 5)
        {
            return words.FindAll(w => w.tier <= maxTier && (category == null || w.category == category));
        }

        public List<SentenceEntry> Sentences(int maxTier = 5)
        {
            return sentences.FindAll(s => s.tier <= maxTier);
        }
    }

    /// <summary>
    /// Loads Resources/Languages/{code}/pack.json. Adding a language = adding a folder with
    /// the same file format; no code changes needed. See Docs/03_LANGUAGE_DATA.md.
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
