using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace LLG
{
    /// <summary>
    /// Plays the recorded voice for a piece of text. The recordings are made ahead of time by
    /// Tools/audio/generate_speech.py (using the voices built into Windows) and stored in
    /// Resources/Audio/Speech/es and /en, named after a short code made from the text.
    /// </summary>
    public static class Speech
    {
        static readonly Dictionary<string, AudioClip> Cache = new Dictionary<string, AudioClip>();

        /// <summary>The file name for a text: the first 12 characters of its MD5 hash.
        /// generate_speech.py makes the same name, so the two always match.</summary>
        public static string Key(string text)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(text.Trim()));
                var sb = new StringBuilder();
                for (int i = 0; i < 6; i++) sb.Append(bytes[i].ToString("x2"));
                return sb.ToString();
            }
        }

        static AudioClip Clip(string text, string language)
        {
            if (string.IsNullOrEmpty(text)) return null;
            string path = "Audio/Speech/" + language + "/" + Key(text);
            if (!Cache.TryGetValue(path, out var clip))
            {
                clip = Resources.Load<AudioClip>(path);
                Cache[path] = clip;
            }
            return clip;
        }

        public static bool Has(string text, string language) => Clip(text, language) != null;

        /// <summary>Speaks the text. language: "es" (the language being learned) or "en".</summary>
        public static void Play(string text, string language)
        {
            var clip = Clip(text, language);
            if (clip != null) AudioManager.PlayVoice(clip);
            else GameState.Say(language == "en" ? "(No recording for this line yet.)"
                                                : "(Sin audio: instala la voz española y ejecuta generate_speech.py)");
        }
    }
}
