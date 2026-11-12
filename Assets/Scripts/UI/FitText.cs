using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Keeps a text inside its box: whenever the words change (or the box is resized), the font size
    /// steps down from its normal size until the text fits, so nothing ever spills over a border.
    /// Added to every label by UIFactory.Label. The normal size is Text.resizeTextMaxSize.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class FitText : MonoBehaviour
    {
        const int MinSize = 8;
        Text text;
        RectTransform rect;
        string fittedText;
        Vector2 fittedSize;
        int fittedMax;

        void Awake()
        {
            text = GetComponent<Text>();
            rect = (RectTransform)transform;
        }

        void LateUpdate()
        {
            if (text.text == fittedText && rect.rect.size == fittedSize && text.resizeTextMaxSize == fittedMax) return;
            Fit();
        }

        public void Fit()
        {
            fittedText = text.text;
            fittedSize = rect.rect.size;
            fittedMax = text.resizeTextMaxSize;
            if (fittedSize.x <= 0 || fittedSize.y <= 0 || string.IsNullOrEmpty(fittedText)) { text.fontSize = fittedMax; return; }
            for (int size = fittedMax; size >= MinSize; size--)
            {
                text.fontSize = size;
                // preferredHeight wraps the text at the box's width, so it tells us whether all lines fit
                if (text.preferredHeight <= fittedSize.y + 2 && LongestWord() <= fittedSize.x + 2) return;
            }
            text.fontSize = MinSize;
        }

        /// <summary>The width of the longest single word (a word can't wrap, so it must fit on its own).</summary>
        float LongestWord()
        {
            float widest = 0;
            var settings = text.GetGenerationSettings(new Vector2(10000, 10000));
            var gen = text.cachedTextGeneratorForLayout;
            foreach (var word in fittedText.Split(' ', '\n'))
            {
                if (word.Length == 0) continue;
                widest = Mathf.Max(widest, gen.GetPreferredWidth(word, settings) / text.pixelsPerUnit);
            }
            return widest;
        }
    }
}
