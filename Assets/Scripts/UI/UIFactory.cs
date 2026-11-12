using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>Small helpers for building UI from code. Positions are measured from the screen centre
    /// on a 640x360 canvas (x: -320..320, y: -180..180). One art pixel = 2 canvas units.</summary>
    public static class UIFactory
    {
        public static readonly Color Ink = new Color32(40, 30, 50, 255);
        public static readonly Color Faded = new Color32(130, 110, 100, 255);
        public static readonly Color Green = new Color32(96, 196, 96, 255);
        public static readonly Color Teal = new Color32(64, 190, 178, 255);
        public static readonly Color Pink = new Color32(244, 120, 170, 255);
        public static readonly Color Purple = new Color32(156, 118, 236, 255);
        public static readonly Color Gold = new Color32(250, 196, 64, 255);
        public static readonly Color Blue = new Color32(100, 168, 244, 255);
        public static readonly Color Red = new Color32(236, 96, 96, 255);

        static Font titleFont, bodyFont;
        static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        static Font Builtin => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        /// <summary>Old-world blackletter pixel font for titles. Use sizes 12, 24, 36, 48 (multiples of 12).</summary>
        public static Font TitleFont => titleFont ??= Resources.Load<Font>("Fonts/Jacquard12-Regular") ?? Builtin;
        /// <summary>Readable 16-pixel font for everything else. Use sizes 16 or 32.</summary>
        public static Font BodyFont => bodyFont ??= Resources.Load<Font>("Fonts/DotGothic16-Regular") ?? Builtin;

        public static Sprite LoadSprite(string path)
        {
            if (Sprites.TryGetValue(path, out var cached) && cached != null) return cached;
            var sprite = Resources.Load<Sprite>(path);
            if (sprite == null) Debug.LogWarning("Missing sprite: Resources/" + path);
            Sprites[path] = sprite;
            return sprite;
        }

        /// <summary>A sprite whose corners stay sharp when stretched (for panels and buttons).</summary>
        public static Sprite Sliced(string path, int border)
        {
            string key = path + "#sliced";
            if (Sprites.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = Resources.Load<Texture2D>(path);
            if (tex == null) { Debug.LogWarning("Missing texture: Resources/" + path); return null; }
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16, 0,
                SpriteMeshType.FullRect, Vector4.one * border);
            Sprites[key] = sprite;
            return sprite;
        }

        public static Transform Canvas(string name, int order)
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 1;
            canvas.sortingOrder = 20000 + order;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(640, 360);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 32;
            return go.transform;
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

        /// <summary>Parchment panel with a wooden frame.</summary>
        public static Image Panel(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            var img = Img(name, parent, pos, size, Color.white, Sliced("Art/UI/panel", 6));
            img.type = Image.Type.Sliced;
            img.raycastTarget = true;      // clicks on a window should not fall through to the game
            return img;
        }

        /// <summary>Rounded box tinted any colour (used for buttons and colour swatches).</summary>
        public static Image Box(string name, Transform parent, Vector2 pos, Vector2 size, Color color)
        {
            var img = Img(name, parent, pos, size, color, Sliced("Art/UI/button", 5));
            img.type = Image.Type.Sliced;
            return img;
        }

        public static Text Label(string name, Transform parent, Vector2 pos, Vector2 size, string text, Color color,
            TextAnchor anchor = TextAnchor.MiddleCenter, int fontSize = 16, bool title = false)
        {
            var label = Rect(name, parent, pos, size).gameObject.AddComponent<Text>();
            label.font = title ? TitleFont : BodyFont;
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = anchor;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        public static void Outline(Graphic graphic, float distance = 1)
        {
            var outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = Ink;
            outline.effectDistance = new Vector2(distance, -distance);
        }

        public static Button Btn(string name, Transform parent, Vector2 pos, Vector2 size, string text, Color color, Action onClick)
        {
            var img = Box(name, parent, pos, size, color);
            img.raycastTarget = true;
            var button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            button.onClick.AddListener(() => onClick());
            if (!string.IsNullOrEmpty(text))
            {
                var label = Label("Label", img.transform, new Vector2(0, 1), size, text, Color.white);
                Outline(label);
            }
            return button;
        }

        static RectTransform Stretch(string name, Transform parent, Vector2 inset)
        {
            var rt = Rect(name, parent, Vector2.zero, Vector2.zero);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = inset;
            rt.offsetMax = -inset;
            return rt;
        }

        /// <summary>A horizontal 0-1 slider (used for volume).</summary>
        public static Slider Slider(string name, Transform parent, Vector2 pos, Vector2 size, Color fillColor)
        {
            var root = Rect(name, parent, pos, size);
            var back = Box("Background", root, Vector2.zero, size, Faded);
            back.raycastTarget = true;

            var fillArea = Stretch("Fill Area", root, new Vector2(3, 3));
            var fill = Box("Fill", fillArea, Vector2.zero, Vector2.zero, fillColor);
            fill.rectTransform.sizeDelta = Vector2.zero;

            var handleArea = Stretch("Handle Area", root, new Vector2(6, 0));
            var handle = Box("Handle", handleArea, Vector2.zero, new Vector2(14, 8), Color.white);

            var slider = root.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            return slider;
        }

        /// <summary>A label with Spanish and English versions; it shows Spanish until its window is translated.</summary>
        public static Text LLabel(string name, Transform parent, Vector2 pos, Vector2 size, string spanish, string english,
            Color color, TextAnchor anchor = TextAnchor.MiddleCenter, int fontSize = 16, bool title = false, bool englishOnly = false)
        {
            var label = Label(name, parent, pos, size, "", color, anchor, fontSize, title);
            label.gameObject.AddComponent<LocalizedText>().Set(spanish, english, englishOnly);
            return label;
        }

        /// <summary>A button whose label switches language with its window.</summary>
        public static Button LBtn(string name, Transform parent, Vector2 pos, Vector2 size, string spanish, string english, Color color, Action onClick)
        {
            var button = Btn(name, parent, pos, size, "", color, onClick);
            var label = LLabel("Label", button.transform, new Vector2(0, 1), size, spanish, english, Color.white);
            Outline(label);
            return button;
        }

        /// <summary>The globe button that translates everything in "window" to English and back.</summary>
        public static Button TranslateButton(Transform window, Vector2 pos)
        {
            var toggle = window.GetComponent<TranslateToggle>() ?? window.gameObject.AddComponent<TranslateToggle>();
            var button = Btn("Translate", window, pos, new Vector2(26, 24), "", Blue, toggle.Flip);
            Img("Globe", button.transform, Vector2.zero, new Vector2(16, 16), Color.white, LoadSprite("Art/UI/translate"));
            return button;
        }

        public static InputField Input(string name, Transform parent, Vector2 pos, Vector2 size, string placeholder)
        {
            var bg = Box(name, parent, pos, size, Color.white);
            bg.raycastTarget = true;
            var input = bg.gameObject.AddComponent<InputField>();
            var inner = size - new Vector2(14, 2);
            var text = Label("Text", bg.transform, Vector2.zero, inner, "", Ink, TextAnchor.MiddleLeft);
            text.supportRichText = false;
            var hint = Label("Placeholder", bg.transform, Vector2.zero, inner, placeholder, new Color32(170, 160, 170, 255), TextAnchor.MiddleLeft);
            input.targetGraphic = bg;
            input.textComponent = text;
            input.placeholder = hint;
            return input;
        }
    }
}
