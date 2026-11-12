using System;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>Small helpers for building UI from code. Positions are measured from the screen centre
    /// on a 640x360 canvas (x: -320..320, y: -180..180).</summary>
    public static class UIFactory
    {
        static Font font;
        public static Font Font => font ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static Sprite LoadSprite(string path)
        {
            var sprite = Resources.Load<Sprite>(path);
            if (sprite == null) Debug.LogWarning("Missing sprite: Resources/" + path);
            return sprite;
        }

        public static RectTransform Rect(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static Image Img(string name, Transform parent, Vector2 pos, Vector2 size, Color color, Sprite sprite = null)
        {
            var img = Rect(name, parent, pos, size).gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static Text Label(string name, Transform parent, Vector2 pos, Vector2 size, string text,
            int fontSize, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var label = Rect(name, parent, pos, size).gameObject.AddComponent<Text>();
            label.font = Font;
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = anchor;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        public static Button Btn(string name, Transform parent, Vector2 pos, Vector2 size, string text,
            Color color, Action onClick, int fontSize = 14)
        {
            var img = Img(name, parent, pos, size, color);
            img.raycastTarget = true;
            var button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            button.onClick.AddListener(() => onClick());
            if (!string.IsNullOrEmpty(text))
                Label("Label", img.transform, Vector2.zero, size, text, fontSize, Color.white);
            return button;
        }

        public static InputField Input(string name, Transform parent, Vector2 pos, Vector2 size, string placeholder)
        {
            var bg = Img(name, parent, pos, size, Color.white);
            bg.raycastTarget = true;
            var input = bg.gameObject.AddComponent<InputField>();
            var inner = size - new Vector2(10, 2);
            var text = Label("Text", bg.transform, Vector2.zero, inner, "", 14, new Color32(40, 30, 50, 255), TextAnchor.MiddleLeft);
            text.supportRichText = false;
            var hint = Label("Placeholder", bg.transform, Vector2.zero, inner, placeholder, 14, new Color32(150, 150, 160, 255), TextAnchor.MiddleLeft);
            hint.fontStyle = FontStyle.Italic;
            input.targetGraphic = bg;
            input.textComponent = text;
            input.placeholder = hint;
            return input;
        }
    }
}
