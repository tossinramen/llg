using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Put on a window: remembers whether that window is currently showing English.
    /// The globe button (UIFactory.TranslateButton) flips it.
    /// </summary>
    public class TranslateToggle : MonoBehaviour
    {
        public bool English { get; private set; }

        public void Set(bool english)
        {
            English = english;
            foreach (var text in GetComponentsInChildren<LocalizedText>(true)) text.Apply();
        }

        public void Flip() => Set(!English);
    }

    /// <summary>
    /// A text with a Spanish and an English version. It shows Spanish until the window's
    /// translate button is pressed. "English only" texts are hidden until then
    /// (used for the English line under dialogue and word signs).
    /// </summary>
    public class LocalizedText : MonoBehaviour
    {
        string spanish = "", english = "";
        bool englishOnly;
        Text text;

        public void Set(string es, string en, bool onlyWhenTranslated = false)
        {
            spanish = es ?? "";
            english = en ?? "";
            englishOnly = onlyWhenTranslated;
            Apply();
        }

        public void Apply()
        {
            if (text == null) text = GetComponent<Text>();
            if (text == null) return;
            var toggle = GetComponentInParent<TranslateToggle>(true);
            bool showEnglish = toggle != null && toggle.English;
            text.text = englishOnly ? (showEnglish ? english : "") : (showEnglish && english.Length > 0 ? english : spanish);
        }
    }
}
