using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Everything drawn on top of the world:
    ///  * top-left: an MMO-style unit frame - your portrait, name and level, HP / mana / hunger bars
    ///    (the HP bar shakes and drains with a trail when you are hurt) and your Lexa
    ///  * top-right: the minimap with the menu (Esc) and controls (page) buttons above it
    ///  * bottom: the XP bar for your character level
    ///  * pop-up messages, the "E: ..." prompt and the word sign shown when you left-click something
    /// </summary>
    public class Hud : MonoBehaviour
    {
        const float MapSize = 92, MapZoom = 1.4f;
        static readonly Vector2 MapCentre = new Vector2(266, 98);
        static readonly Vector2 FramePos = new Vector2(-228, 150);
        const float BarWidth = 104, BarX = 22;

        Text prompt, zone, nameLevel, hpText, manaText, hungerText, coins, gold, xpText, levelText;
        GameObject promptBox, minimap;
        RectTransform canvasRect, labelBox, mapImage, mapArrow, frame, hpFill, hpTrail, manaFill, hungerFill, xpFill;
        Image hpFlash;
        LocalizedText labelWord, labelEnglish;
        TranslateToggle labelTranslate;
        CharacterPreview portrait;
        Camera cam;
        Vector3 labelWorld;
        Transform player;
        CharacterSprite playerSprite;
        string labelSpeakEs, labelSpeakEn;
        float shakeTimer, trailHp, hurtTimer, xpShown, xpTarget, badgePulse;
        int shownLevel = -1;
        RectTransform badge;
        const float XpX = 18, XpY = -160, XpWidth = 400;
        Image hurt;
        Transform toastRoot;
        readonly List<Toast> toasts = new List<Toast>();
        const float ToastLife = 2.6f;

        class Toast { public RectTransform rect; public CanvasGroup group; public float age; }
        int lastHp = -1;

        public void Build(Transform canvas, Action openMenu, Action openControls)
        {
            canvasRect = (RectTransform)canvas;
            cam = Camera.main;

            // the red glow round the screen edges when you get hurt (drawn under every other piece of UI)
            hurt = UIFactory.Img("Hurt", canvas, Vector2.zero, Vector2.zero, new Color(0.85f, 0.05f, 0.05f, 0), Vignette());
            hurt.rectTransform.anchorMin = Vector2.zero;
            hurt.rectTransform.anchorMax = Vector2.one;
            hurt.rectTransform.sizeDelta = Vector2.zero;
            hurt.raycastTarget = false;
            BuildUnitFrame(canvas);
            BuildXpBar(canvas);

            UIFactory.Btn("MenuButton", canvas, new Vector2(248, 164), new Vector2(40, 22), "Esc", UIFactory.Purple, openMenu);
            var page = UIFactory.Btn("ControlsButton", canvas, new Vector2(286, 164), new Vector2(28, 22), "", UIFactory.Purple, openControls);
            UIFactory.Img("Page", page.transform, Vector2.zero, new Vector2(16, 16), Color.white, UIFactory.LoadSprite("Art/UI/page"));

            toastRoot = UIFactory.Rect("Toasts", canvas, new Vector2(0, 150), Vector2.zero);     // top middle, clear of speech bubbles

            promptBox = UIFactory.Panel("Prompt", canvas, new Vector2(0, -124), new Vector2(320, 34)).gameObject;
            prompt = UIFactory.Label("Text", promptBox.transform, new Vector2(0, 1), new Vector2(310, 24), "", UIFactory.Ink);
            promptBox.SetActive(false);

            // the word sign stays up until you click it (or read something else)
            var label = UIFactory.Panel("WordLabel", canvas, Vector2.zero, new Vector2(120, 44));
            label.pixelsPerUnitMultiplier = 2;          // a thin frame: these signs are small
            label.gameObject.AddComponent<Button>().onClick.AddListener(HideWordLabel);
            labelBox = label.rectTransform;
            labelWord = UIFactory.LLabel("Word", labelBox, new Vector2(0, 6), new Vector2(100, 16), "", "", UIFactory.Ink).GetComponent<LocalizedText>();
            labelEnglish = UIFactory.LLabel("English", labelBox, new Vector2(0, -9), new Vector2(100, 13), "", "", UIFactory.Faded, TextAnchor.MiddleCenter, 12, false, true).GetComponent<LocalizedText>();
            UIFactory.TranslateButton(labelBox, Vector2.zero, 20);
            labelTranslate = label.GetComponent<TranslateToggle>();
            UIFactory.ListenButton(labelBox, Vector2.zero, () => labelTranslate.English ? labelSpeakEn : labelSpeakEs, "es", 20);
            labelBox.gameObject.SetActive(false);

            GameState.Toast += ShowToast;
            Refresh();
        }

        // ------------------------------------------------------------ unit frame

        void BuildUnitFrame(Transform canvas)
        {
            var frameImg = UIFactory.Panel("UnitFrame", canvas, FramePos, new Vector2(176, 56));
            frameImg.raycastTarget = false;
            frameImg.pixelsPerUnitMultiplier = 2;          // thinner wooden border
            frame = frameImg.rectTransform;

            // round portrait: the character's head inside a gold ring
            var mask = UIFactory.Img("Portrait", frame, new Vector2(-60, 0), new Vector2(38, 38), new Color32(170, 210, 236, 255),
                UIFactory.LoadSprite("Art/UI/minimap_mask"));
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var holder = UIFactory.Rect("Head", mask.transform, new Vector2(0, -20), new Vector2(61, 81));
            portrait = holder.gameObject.AddComponent<CharacterPreview>();
            portrait.Build(new Vector2(61, 81));
            hpFlash = UIFactory.Img("Hurt", mask.transform, Vector2.zero, new Vector2(38, 38), new Color(1, 0, 0, 0));
            UIFactory.Img("Ring", frame, new Vector2(-60, 0), new Vector2(44, 44), Color.white, UIFactory.LoadSprite("Art/UI/minimap_ring"));

            // name plate in its own small box, then HP, mana and hunger bars with space between them
            UIFactory.Box("NamePlate", frame, new Vector2(BarX, 11), new Vector2(BarWidth + 4, 17), new Color32(70, 52, 46, 255));
            nameLevel = UIFactory.Label("Name", frame, new Vector2(BarX, 11), new Vector2(BarWidth - 6, 16), "", Color.white);
            nameLevel.resizeTextMaxSize = 12;
            hpTrail = Bar("HpTrail", -5, 12, new Color32(250, 230, 150, 255), out hpFill, new Color32(206, 52, 52, 255));
            hpText = UIFactory.Label("Hp", frame, new Vector2(BarX, -5), new Vector2(BarWidth, 13), "", Color.white);
            hpText.resizeTextMaxSize = 12;
            UIFactory.Outline(hpText);
            Bar("Mana", -15, 4, Color.clear, out manaFill, new Color32(64, 120, 220, 255));
            manaText = UIFactory.Label("ManaText", frame, Vector2.zero, Vector2.zero, "", Color.white);
            manaText.gameObject.SetActive(false);
            Bar("Hunger", -20, 2, Color.clear, out hungerFill, new Color32(226, 150, 50, 255));
            hungerText = UIFactory.Label("HungerText", frame, Vector2.zero, Vector2.zero, "", UIFactory.Ink);
            hungerText.gameObject.SetActive(false);

            // Lexa, in a small box under the portrait
            gold = Purse(canvas, FramePos + new Vector2(-38, -41), "Art/UI/coin");
            coins = Purse(canvas, FramePos + new Vector2(-38, -65), "Art/UI/lexa");
        }

        /// <summary>A small money box under the portrait: icon + amount.</summary>
        Text Purse(Transform canvas, Vector2 pos, string icon)
        {
            // the amount uses the font's full pixel size (16) so it stays sharp, never shrunk
            var img = UIFactory.Panel("Purse", canvas, pos, new Vector2(100, 22));
            img.pixelsPerUnitMultiplier = 2;
            img.raycastTarget = false;
            UIFactory.Img("Icon", img.rectTransform, new Vector2(-38, 0), new Vector2(12, 12), Color.white, UIFactory.LoadSprite(icon));
            return UIFactory.Label("Amount", img.rectTransform, new Vector2(8, 1), new Vector2(76, 18), "", UIFactory.Ink, TextAnchor.MiddleCenter, 16);
        }

        /// <summary>A bar in the unit frame: dark background, an optional trail and a coloured fill.</summary>
        RectTransform Bar(string name, float y, float height, Color trailColor, out RectTransform fill, Color fillColor)
        {
            var back = UIFactory.Img(name + "Back", frame, new Vector2(BarX, y), new Vector2(BarWidth + 4, height + 2), new Color32(40, 30, 34, 255));
            RectTransform trail = null;
            if (trailColor.a > 0)
            {
                trail = UIFactory.Img(name + "Trail", back.transform, Vector2.zero, new Vector2(BarWidth, height), trailColor).rectTransform;
                LeftAlign(trail, height);
            }
            fill = UIFactory.Img(name + "Fill", back.transform, Vector2.zero, new Vector2(BarWidth, height), fillColor).rectTransform;
            LeftAlign(fill, height);
            var shine = UIFactory.Img("Shine", fill, Vector2.zero, new Vector2(0, 1), new Color(1, 1, 1, 0.35f)).rectTransform;
            shine.anchorMin = new Vector2(0, 1);
            shine.anchorMax = new Vector2(1, 1);
            shine.pivot = new Vector2(0.5f, 1);
            shine.sizeDelta = new Vector2(0, 2);
            shine.anchoredPosition = Vector2.zero;
            return trail;
        }

        static void LeftAlign(RectTransform rt, float height)
        {
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = new Vector2(-BarWidth / 2, 0);
            rt.sizeDelta = new Vector2(BarWidth, height);
        }

        public void RefreshPortrait()
        {
            portrait.Show(PlayerProfile.Current.appearance, CharacterLayers.FromEquipment());
        }

        void BuildXpBar(Transform canvas)
        {
            // a wooden-framed bar near the bottom, with a round level badge on its left
            var frameImg = UIFactory.Panel("XpFrame", canvas, new Vector2(XpX, XpY), new Vector2(XpWidth + 16, 22));
            frameImg.pixelsPerUnitMultiplier = 2;
            frameImg.raycastTarget = false;
            var back = UIFactory.Img("XpBack", frameImg.transform, Vector2.zero, new Vector2(XpWidth, 10), new Color32(36, 26, 42, 255));
            xpFill = UIFactory.Img("XpFill", back.transform, new Vector2(-XpWidth / 2, 0), new Vector2(0, 10), new Color32(136, 84, 220, 255)).rectTransform;
            xpFill.pivot = new Vector2(0, 0.5f);
            var shine = UIFactory.Img("Shine", xpFill, Vector2.zero, Vector2.zero, new Color32(200, 164, 255, 255)).rectTransform;
            shine.anchorMin = new Vector2(0, 1);
            shine.anchorMax = new Vector2(1, 1);
            shine.pivot = new Vector2(0.5f, 1);
            shine.sizeDelta = new Vector2(0, 3);
            shine.anchoredPosition = new Vector2(0, -1);
            for (int i = 1; i < 10; i++)
                UIFactory.Img("Tick", back.transform, new Vector2(-XpWidth / 2 + i * XpWidth / 10, 0), new Vector2(1, 10), new Color(0, 0, 0, 0.3f));
            xpText = UIFactory.Label("XpText", frameImg.transform, new Vector2(0, 1), new Vector2(XpWidth - 8, 16), "", Color.white, TextAnchor.MiddleCenter, 12);
            UIFactory.Outline(xpText);
            var badgePos = new Vector2(XpX - XpWidth / 2 - 26, XpY);
            var disc = UIFactory.Img("LevelDisc", canvas, badgePos, new Vector2(28, 28), new Color32(104, 62, 176, 255), UIFactory.LoadSprite("Art/UI/minimap_mask"));
            disc.raycastTarget = false;
            badge = UIFactory.Img("LevelRing", canvas, badgePos, new Vector2(34, 34), Color.white, UIFactory.LoadSprite("Art/UI/minimap_ring")).rectTransform;
            disc.transform.SetParent(badge, true);
            levelText = UIFactory.Label("Level", badge, new Vector2(0, 1), new Vector2(26, 16), "", Color.white, TextAnchor.MiddleCenter, 12);
            UIFactory.Outline(levelText);
        }

        // ------------------------------------------------------------ minimap

        /// <summary>The round map in the corner. Points of interest are (tile, icon) pairs.</summary>
        public void BuildMinimap(Transform canvas, VillageMap map, int width, int height, Transform playerTransform, CharacterSprite sprite,
            List<(Vector2Int tile, string icon)> places, Action openMap)
        {
            player = playerTransform;
            playerSprite = sprite;
            var root = UIFactory.Rect("MinimapRoot", canvas, MapCentre, new Vector2(MapSize + 8, MapSize + 8));
            minimap = root.gameObject;
            var mask = UIFactory.Img("Minimap", root, Vector2.zero, new Vector2(MapSize, MapSize), new Color32(70, 96, 60, 255), UIFactory.LoadSprite("Art/UI/minimap_mask"));
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            mask.raycastTarget = true;
            mask.gameObject.AddComponent<Button>().onClick.AddListener(() => openMap());     // click for the big map
            var size = new Vector2(width, height) * 4 * MapZoom;
            mapImage = UIFactory.Img("Map", mask.transform, Vector2.zero, size, Color.white, UIFactory.LoadSprite("Art/UI/minimap")).rectTransform;
            foreach (var (tile, icon) in places)
            {
                var pos = new Vector2((tile.x + 0.5f) * 4 * MapZoom - size.x / 2, size.y / 2 - (tile.y + 0.5f) * 4 * MapZoom);
                bool dot = icon.EndsWith("map_dot");
                bool danger = icon.Contains("wolf");
                UIFactory.Img("Place", mapImage, pos, dot ? new Vector2(6, 6) : new Vector2(12, 12),
                    dot ? (Color)new Color32(255, 220, 90, 255) : danger ? new Color(1, 0.6f, 0.6f) : Color.white, UIFactory.LoadSprite(icon));
            }
            mapArrow = UIFactory.Img("You", mask.transform, Vector2.zero, new Vector2(11, 11), Color.white, UIFactory.LoadSprite("Art/UI/map_arrow")).rectTransform;
            UIFactory.Img("Ring", root, Vector2.zero, new Vector2(MapSize + 8, MapSize + 8), Color.white, UIFactory.LoadSprite("Art/UI/minimap_ring"));
            zone = UIFactory.Label("Zone", canvas, MapCentre + new Vector2(0, -MapSize / 2 - 12), new Vector2(180, 18), GameConfig.TownName, Color.white);
            UIFactory.Outline(zone);
        }

        /// <summary>Called when you go in or out of a building: inside, the minimap hides and the name changes.</summary>
        public void SetArea(Area area)
        {
            if (minimap != null) minimap.SetActive(!area.interior);
            if (zone != null)
            {
                zone.text = area.es;
                ((RectTransform)zone.transform).anchoredPosition = area.interior ? new Vector2(250, 140) : MapCentre + new Vector2(0, -MapSize / 2 - 12);
            }
        }

        void UpdateMinimap()
        {
            if (mapImage == null || player == null || !minimap.activeSelf) return;
            var size = mapImage.sizeDelta;
            float px = player.position.x * 4 * MapZoom, py = -player.position.y * 4 * MapZoom;
            mapImage.anchoredPosition = new Vector2(size.x / 2 - px, py - size.y / 2);
            float[] angles = { 180, -90, 0, 90 };
            mapArrow.localRotation = Quaternion.Euler(0, 0, angles[playerSprite.Facing]);
        }

        // ------------------------------------------------------------ every frame

        void OnDestroy() => GameState.Toast -= ShowToast;

        void Refresh()
        {
            var data = GameState.Data;
            var s = data.stats;
            if (lastHp >= 0 && s.hp < lastHp)
            {
                shakeTimer = 0.35f;         // hurt: shake the frame, flash the portrait, redden the screen edges
                hurtTimer = 0.6f;
            }
            if (lastHp < 0 || s.hp > trailHp) trailHp = s.hp;
            lastHp = s.hp;

            nameLevel.text = PlayerProfile.Current.playerName + "  ·  Nv " + data.level;
            hpFill.sizeDelta = new Vector2(BarWidth * s.hp / Mathf.Max(1f, s.maxHp), hpFill.sizeDelta.y);
            hpText.text = s.hp + "/" + s.maxHp;
            manaFill.sizeDelta = new Vector2(BarWidth * s.mana / Mathf.Max(1f, s.maxMana), manaFill.sizeDelta.y);
            manaText.text = s.mana + " / " + s.maxMana;
            hungerFill.sizeDelta = new Vector2(BarWidth * s.hunger / Mathf.Max(1f, s.maxHunger), hungerFill.sizeDelta.y);
            hungerText.text = "";
            coins.text = GameState.Short(s.coins) + " " + GameConfig.CurrencyName;
            gold.text = GameState.Short(s.gold) + " " + GameConfig.GoldName;

            int need = GameState.XpForLevel(data.level);
            xpTarget = XpWidth * Mathf.Clamp01(data.xp / (float)need);
            if (shownLevel != data.level)
            {
                if (shownLevel >= 0) { badgePulse = 0.8f; xpShown = 0; }      // level up: the badge pops
                else xpShown = xpTarget;
                shownLevel = data.level;
            }
            levelText.text = data.level.ToString();
            xpText.text = "Nivel " + data.level + "   ·   " + GameState.Short(data.xp) + " / " + GameState.Short(need) + " XP";
        }

        /// <summary>A message pops up in a small dark tag, coloured by what it is: green for
        /// victories and good news, gold for gold, blue for Lexa, red for damage.</summary>
        void ShowToast(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            var (color, icon) = ToastStyle(message);
            float width = Mathf.Clamp(message.Length * 8 + (icon != null ? 44 : 26), 70, 560);
            var box = UIFactory.Box("Toast", toastRoot, Vector2.zero, new Vector2(width, 22), new Color32(30, 24, 38, 230));
            box.raycastTarget = false;
            var border = box.gameObject.AddComponent<Outline>();
            border.effectColor = new Color(color.r * 0.7f, color.g * 0.7f, color.b * 0.7f, 0.9f);
            border.effectDistance = new Vector2(1, -1);
            UIFactory.Img("Accent", box.transform, new Vector2(-width / 2 + 4, 0), new Vector2(3, 14), color);
            float textX = 4;
            if (icon != null)
            {
                UIFactory.Img("Icon", box.transform, new Vector2(-width / 2 + 16, 0), new Vector2(12, 12), Color.white, UIFactory.LoadSprite(icon));
                textX = 10;
            }
            var label = UIFactory.Label("Text", box.transform, new Vector2(textX, 1), new Vector2(width - (icon != null ? 34 : 16), 18), message, color, TextAnchor.MiddleCenter, 16);
            UIFactory.Outline(label);
            var group = box.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            toasts.Insert(0, new Toast { rect = box.rectTransform, group = group });
            while (toasts.Count > 3)
            {
                Destroy(toasts[toasts.Count - 1].rect.gameObject);
                toasts.RemoveAt(toasts.Count - 1);
            }
        }

        static (Color, string) ToastStyle(string m)
        {
            Color red = new Color32(255, 112, 100, 255), gold = new Color32(255, 214, 92, 255), blue = new Color32(140, 206, 255, 255),
                  green = new Color32(126, 232, 120, 255), cream = new Color32(250, 240, 222, 255);
            if (m.Contains("HP") && m.StartsWith("-")) return (red, "Art/UI/hp_heart");
            if (m.StartsWith("+") && m.Contains(GameConfig.GoldName)) return (gold, "Art/UI/coin");
            if (m.StartsWith("+") && m.Contains(GameConfig.CurrencyName)) return (blue, "Art/UI/lexa");
            if (m.StartsWith("+") && m.Contains("HP")) return (green, "Art/UI/hp_heart");
            if (m.Contains("nivel")) return (gold, null);
            if (m.StartsWith("+") || m.StartsWith("¡") || m.Contains("mochila")) return (green, null);
            if (m.StartsWith("No ") || m.StartsWith("Necesitas")) return (new Color32(255, 170, 120, 255), null);
            return (cream, null);
        }

        void UpdateToasts()
        {
            for (int i = toasts.Count - 1; i >= 0; i--)
            {
                var t = toasts[i];
                t.age += Time.unscaledDeltaTime;
                if (t.age >= ToastLife)
                {
                    Destroy(t.rect.gameObject);
                    toasts.RemoveAt(i);
                    continue;
                }
                float pop = Mathf.Clamp01(t.age / 0.12f);
                t.rect.localScale = Vector3.one * Mathf.Lerp(0.7f, 1f, 1 - (1 - pop) * (1 - pop));
                t.group.alpha = Mathf.Clamp01((ToastLife - t.age) / 0.5f);
                var target = new Vector2(0, -i * 24 + Mathf.Min(t.age, 0.3f) * 10);
                t.rect.anchoredPosition = Vector2.Lerp(t.rect.anchoredPosition, target, Mathf.Clamp01(Time.unscaledDeltaTime * 14));
            }
        }

        /// <summary>A soft frame that is clear in the middle and solid at the edges (tinted red when hurt).</summary>
        static Sprite Vignette()
        {
            const int w = 128, h = 72;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float dx = (x + 0.5f) / w * 2 - 1, dy = (y + 0.5f) / h * 2 - 1;
                    float r = Mathf.Sqrt(dx * dx * 0.85f + dy * dy);
                    float a = Mathf.Clamp01((r - 0.6f) / 0.55f);
                    px[y * w + x] = new Color(1, 1, 1, a * a);
                }
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100);
        }

        /// <summary>Shows what the use key will do right now, or hides the prompt when given null.</summary>
        public void SetPrompt(string text)
        {
            promptBox.SetActive(text != null);
            if (text != null) prompt.text = Keybinds.KeyName(GameAction.Use) + ": " + text;
        }

        /// <summary>A little sign above something in the world with its Spanish name.
        /// The globe shows the English, the speaker reads it aloud; clicking the sign closes it.</summary>
        public void ShowWordLabel(Vector3 world, string word, string english, string speakSpanish = null)
        {
            labelWord.Set(word, word);
            labelEnglish.Set("", english, true);
            labelSpeakEs = speakSpanish ?? word;
            labelSpeakEn = english;
            int longest = Mathf.Max(word.Length, english.Length);
            float w = Mathf.Max(110, longest * 8 + 66);
            labelBox.sizeDelta = new Vector2(w, 44);
            ((RectTransform)labelBox.Find("Word")).sizeDelta = new Vector2(w - 60, 16);
            ((RectTransform)labelBox.Find("English")).sizeDelta = new Vector2(w - 60, 13);
            ((RectTransform)labelBox.Find("Translate")).anchoredPosition = new Vector2(-w / 2 + 17, 0);
            ((RectTransform)labelBox.Find("Listen_es")).anchoredPosition = new Vector2(w / 2 - 17, 0);
            labelWorld = world;
            labelBox.gameObject.SetActive(true);
            labelBox.SetAsLastSibling();
            labelTranslate.Set(false);
            PlaceLabel();
        }

        public void HideWordLabel() => labelBox.gameObject.SetActive(false);

        void PlaceLabel()
        {
            var screen = cam.WorldToScreenPoint(labelWorld);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, cam, out var local)) return;
            float halfW = labelBox.sizeDelta.x / 2, halfCanvas = canvasRect.rect.width / 2;
            local += new Vector2(0, 28);
            local.x = Mathf.Clamp(local.x, -halfCanvas + halfW + 4, halfCanvas - halfW - 4);
            local.y = Mathf.Clamp(local.y, -130, local.x + halfW > 210 ? 20 : 104);
            labelBox.anchoredPosition = local;
        }

        void Update()
        {
            UpdateToasts();
            Refresh();
            xpShown = Mathf.MoveTowards(xpShown, xpTarget, Time.unscaledDeltaTime * Mathf.Max(60, Mathf.Abs(xpTarget - xpShown) * 4));
            xpFill.sizeDelta = new Vector2(xpShown, 10);
            badgePulse -= Time.unscaledDeltaTime;
            badge.localScale = Vector3.one * (badgePulse > 0 ? 1 + 0.25f * Mathf.Sin(badgePulse / 0.8f * Mathf.PI) : 1);
            var s = GameState.Data.stats;
            // red screen edges: a flash when hit, and a slow pulse while your HP is low
            hurtTimer -= Time.unscaledDeltaTime;
            float low = s.hp > 0 && s.hp <= s.maxHp / 4 ? 0.22f + 0.12f * Mathf.Sin(Time.unscaledTime * 5) : 0;
            hurt.color = new Color(0.85f, 0.05f, 0.05f, Mathf.Max(low, 0.8f * Mathf.Clamp01(hurtTimer / 0.6f)));
            trailHp = Mathf.MoveTowards(trailHp, s.hp, Time.deltaTime * Mathf.Max(4, s.maxHp * 0.5f));
            hpTrail.sizeDelta = new Vector2(BarWidth * trailHp / Mathf.Max(1f, s.maxHp), hpTrail.sizeDelta.y);
            if (shakeTimer > 0)
            {
                shakeTimer -= Time.unscaledDeltaTime;
                frame.anchoredPosition = FramePos + UnityEngine.Random.insideUnitCircle * 3f * (shakeTimer / 0.35f);
                hpFlash.color = new Color(1, 0, 0, 0.5f * shakeTimer / 0.35f);
                if (shakeTimer <= 0) frame.anchoredPosition = FramePos;
            }

            if (labelBox.gameObject.activeSelf) PlaceLabel();
        }

        void LateUpdate() => UpdateMinimap();
    }
}
