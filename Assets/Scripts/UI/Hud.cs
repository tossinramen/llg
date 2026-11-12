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
        static readonly Vector2 FramePos = new Vector2(-206, 142);
        const float BarWidth = 120;

        Text toast, prompt, zone, nameLevel, hpText, manaText, hungerText, coins, xpText, levelText;
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
        float toastTimer, shakeTimer, trailHp;
        int lastHp = -1;

        public void Build(Transform canvas, Action openMenu, Action openControls)
        {
            canvasRect = (RectTransform)canvas;
            cam = Camera.main;
            BuildUnitFrame(canvas);
            BuildXpBar(canvas);

            UIFactory.Btn("MenuButton", canvas, new Vector2(248, 164), new Vector2(40, 22), "Esc", UIFactory.Purple, openMenu);
            var page = UIFactory.Btn("ControlsButton", canvas, new Vector2(286, 164), new Vector2(28, 22), "", UIFactory.Purple, openControls);
            UIFactory.Img("Page", page.transform, Vector2.zero, new Vector2(16, 16), Color.white, UIFactory.LoadSprite("Art/UI/page"));

            toast = UIFactory.Label("Toast", canvas, new Vector2(-20, 96), new Vector2(480, 60), "", Color.white);
            UIFactory.Outline(toast);

            promptBox = UIFactory.Panel("Prompt", canvas, new Vector2(0, -140), new Vector2(320, 34)).gameObject;
            prompt = UIFactory.Label("Text", promptBox.transform, new Vector2(0, 1), new Vector2(310, 24), "", UIFactory.Ink);
            promptBox.SetActive(false);

            // the word sign stays up until you click it (or read something else)
            var label = UIFactory.Panel("WordLabel", canvas, Vector2.zero, new Vector2(160, 62));
            label.gameObject.AddComponent<Button>().onClick.AddListener(HideWordLabel);
            labelBox = label.rectTransform;
            labelWord = UIFactory.LLabel("Word", labelBox, new Vector2(0, 9), new Vector2(150, 20), "", "", UIFactory.Ink).GetComponent<LocalizedText>();
            labelEnglish = UIFactory.LLabel("English", labelBox, new Vector2(0, -9), new Vector2(150, 20), "", "", UIFactory.Faded, TextAnchor.MiddleCenter, 16, false, true).GetComponent<LocalizedText>();
            UIFactory.TranslateButton(labelBox, Vector2.zero);
            labelTranslate = label.GetComponent<TranslateToggle>();
            UIFactory.ListenButton(labelBox, Vector2.zero, () => labelTranslate.English ? labelSpeakEn : labelSpeakEs, "es");
            labelBox.gameObject.SetActive(false);

            GameState.Toast += ShowToast;
            Refresh();
        }

        // ------------------------------------------------------------ unit frame

        void BuildUnitFrame(Transform canvas)
        {
            frame = UIFactory.Panel("UnitFrame", canvas, FramePos, new Vector2(216, 72)).rectTransform;
            frame.GetComponent<Image>().raycastTarget = false;

            // round portrait: the character's head inside a gold ring
            var mask = UIFactory.Img("Portrait", frame, new Vector2(-76, 0), new Vector2(50, 50), new Color32(170, 210, 236, 255),
                UIFactory.LoadSprite("Art/UI/minimap_mask"));
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var holder = UIFactory.Rect("Head", mask.transform, new Vector2(0, -26), new Vector2(81, 108));
            portrait = holder.gameObject.AddComponent<CharacterPreview>();
            portrait.Build(new Vector2(81, 108));
            hpFlash = UIFactory.Img("Hurt", mask.transform, Vector2.zero, new Vector2(50, 50), new Color(1, 0, 0, 0));
            UIFactory.Img("Ring", frame, new Vector2(-76, 0), new Vector2(58, 58), Color.white, UIFactory.LoadSprite("Art/UI/minimap_ring"));

            nameLevel = UIFactory.Label("Name", frame, new Vector2(18, 20), new Vector2(130, 16), "", UIFactory.Ink, TextAnchor.MiddleLeft);
            hpTrail = Bar("HpTrail", 3, 14, new Color32(250, 230, 150, 255), out hpFill, new Color32(206, 52, 52, 255));
            hpText = UIFactory.Label("Hp", frame, new Vector2(16, 3), new Vector2(BarWidth, 14), "", Color.white);
            UIFactory.Outline(hpText);
            Bar("Mana", -13, 12, Color.clear, out manaFill, new Color32(64, 120, 220, 255));
            manaText = UIFactory.Label("ManaText", frame, new Vector2(16, -13), new Vector2(BarWidth, 12), "", Color.white);
            UIFactory.Outline(manaText);
            Bar("Hunger", -26, 6, Color.clear, out hungerFill, new Color32(226, 150, 50, 255));
            hungerText = UIFactory.Label("HungerText", frame, new Vector2(98, -26), new Vector2(40, 12), "", UIFactory.Ink, TextAnchor.MiddleRight);

            // currency beside the frame
            var purse = UIFactory.Panel("Purse", canvas, FramePos + new Vector2(152, 20), new Vector2(84, 30)).rectTransform;
            UIFactory.Img("Coin", purse, new Vector2(-24, 0), new Vector2(16, 16), Color.white, UIFactory.LoadSprite("Art/UI/coin"));
            coins = UIFactory.Label("Coins", purse, new Vector2(18, 1), new Vector2(48, 20), "", UIFactory.Ink, TextAnchor.MiddleLeft);
        }

        /// <summary>A bar in the unit frame: dark background, an optional trail and a coloured fill.</summary>
        RectTransform Bar(string name, float y, float height, Color trailColor, out RectTransform fill, Color fillColor)
        {
            var back = UIFactory.Img(name + "Back", frame, new Vector2(16, y), new Vector2(BarWidth + 4, height + 4), new Color32(40, 30, 34, 255));
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
            var back = UIFactory.Img("XpBack", canvas, new Vector2(0, -173), new Vector2(604, 10), new Color32(30, 22, 30, 230));
            xpFill = UIFactory.Img("XpFill", back.transform, Vector2.zero, new Vector2(600, 6), new Color32(170, 110, 230, 255)).rectTransform;
            xpFill.pivot = new Vector2(0, 0.5f);
            xpFill.anchoredPosition = new Vector2(-300, 0);
            for (int i = 1; i < 10; i++)
                UIFactory.Img("Tick", back.transform, new Vector2(-300 + i * 60, 0), new Vector2(1, 8), new Color(0, 0, 0, 0.6f));
            levelText = UIFactory.Label("Level", canvas, new Vector2(-286, -160), new Vector2(60, 16), "", Color.white, TextAnchor.MiddleLeft);
            UIFactory.Outline(levelText);
            xpText = UIFactory.Label("XpText", canvas, new Vector2(240, -160), new Vector2(140, 16), "", Color.white, TextAnchor.MiddleRight);
            UIFactory.Outline(xpText);
        }

        // ------------------------------------------------------------ minimap

        /// <summary>The round map in the corner. Points of interest are (tile, icon) pairs.</summary>
        public void BuildMinimap(Transform canvas, VillageMap map, int width, int height, Transform playerTransform, CharacterSprite sprite,
            List<(Vector2Int tile, string icon)> places)
        {
            player = playerTransform;
            playerSprite = sprite;
            var root = UIFactory.Rect("MinimapRoot", canvas, MapCentre, new Vector2(MapSize + 8, MapSize + 8));
            minimap = root.gameObject;
            var mask = UIFactory.Img("Minimap", root, Vector2.zero, new Vector2(MapSize, MapSize), new Color32(70, 96, 60, 255), UIFactory.LoadSprite("Art/UI/minimap_mask"));
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = true;
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
                shakeTimer = 0.35f;         // hurt: shake the frame and flash the portrait
            }
            if (lastHp < 0 || s.hp > trailHp) trailHp = s.hp;
            lastHp = s.hp;

            nameLevel.text = PlayerProfile.Current.playerName + "   Nv " + data.level;
            hpFill.sizeDelta = new Vector2(BarWidth * s.hp / Mathf.Max(1f, s.maxHp), hpFill.sizeDelta.y);
            hpText.text = s.hp + " / " + s.maxHp;
            manaFill.sizeDelta = new Vector2(BarWidth * s.mana / Mathf.Max(1f, s.maxMana), manaFill.sizeDelta.y);
            manaText.text = s.mana + " / " + s.maxMana;
            hungerFill.sizeDelta = new Vector2(BarWidth * s.hunger / Mathf.Max(1f, s.maxHunger), hungerFill.sizeDelta.y);
            hungerText.text = "";
            coins.text = s.coins.ToString();

            int need = GameState.XpForLevel(data.level);
            xpFill.sizeDelta = new Vector2(600f * data.xp / need, 6);
            levelText.text = "Nv " + data.level;
            xpText.text = data.xp + " / " + need + " XP";
        }

        void ShowToast(string message)
        {
            toast.text = toastTimer > 1.5f ? toast.text + "\n" + message : message;
            toastTimer = 2.5f;
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
            float w = Mathf.Max(130, longest * 8 + 90);
            labelBox.sizeDelta = new Vector2(w, 62);
            ((RectTransform)labelBox.Find("Translate")).anchoredPosition = new Vector2(-w / 2 + 24, 0);
            ((RectTransform)labelBox.Find("Listen_es")).anchoredPosition = new Vector2(w / 2 - 24, 0);
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
            local += new Vector2(0, 36);
            local.x = Mathf.Clamp(local.x, -halfCanvas + halfW + 4, halfCanvas - halfW - 4);
            local.y = Mathf.Clamp(local.y, -130, local.x + halfW > 210 ? 20 : 104);
            labelBox.anchoredPosition = local;
        }

        void Update()
        {
            toastTimer -= Time.deltaTime;
            var color = toast.color;
            color.a = Mathf.Clamp01(toastTimer);
            toast.color = color;

            Refresh();
            var s = GameState.Data.stats;
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
