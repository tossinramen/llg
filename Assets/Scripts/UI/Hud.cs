using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Everything drawn on top of the village: the stats bar (HP, mana, hunger, currency) top-left,
    /// the minimap top-right with the menu (Esc) and controls (page) buttons above it,
    /// pop-up messages, the "E: ..." prompt and the word sign shown when you left-click something.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        const float MapSize = 92, MapZoom = 1.4f;       // minimap diameter (canvas units) and zoom
        static readonly Vector2 MapCentre = new Vector2(266, 98);

        Text hp, mana, hunger, coins, toast, prompt, zone;
        GameObject promptBox;
        RectTransform canvasRect, labelBox, mapImage, mapArrow;
        LocalizedText labelWord, labelEnglish;
        TranslateToggle labelTranslate;
        Camera cam;
        Vector3 labelWorld;
        Transform player;
        CharacterSprite playerSprite;
        float toastTimer;

        public void Build(Transform canvas, Action openMenu, Action openControls)
        {
            canvasRect = (RectTransform)canvas;
            cam = Camera.main;

            UIFactory.Panel("HudBar", canvas, new Vector2(-170, 160), new Vector2(292, 34));
            hp = Stat(canvas, 0, "Art/UI/hp_heart");
            mana = Stat(canvas, 1, "Art/UI/mana_drop");
            hunger = Stat(canvas, 2, "Art/Icons/food_bread");
            coins = Stat(canvas, 3, "Art/UI/coin");

            // menu and controls buttons above the minimap
            UIFactory.Btn("MenuButton", canvas, new Vector2(248, 164), new Vector2(40, 22), "Esc", UIFactory.Purple, openMenu);
            var page = UIFactory.Btn("ControlsButton", canvas, new Vector2(286, 164), new Vector2(28, 22), "", UIFactory.Purple, openControls);
            UIFactory.Img("Page", page.transform, Vector2.zero, new Vector2(16, 16), Color.white, UIFactory.LoadSprite("Art/UI/page"));

            toast = UIFactory.Label("Toast", canvas, new Vector2(-40, 112), new Vector2(520, 60), "", Color.white);
            UIFactory.Outline(toast);

            promptBox = UIFactory.Panel("Prompt", canvas, new Vector2(0, -150), new Vector2(300, 34)).gameObject;
            prompt = UIFactory.Label("Text", promptBox.transform, new Vector2(0, 1), new Vector2(290, 24), "", UIFactory.Ink);
            promptBox.SetActive(false);

            // the word sign stays up until you click it (or read something else)
            var label = UIFactory.Panel("WordLabel", canvas, Vector2.zero, new Vector2(160, 62));
            label.gameObject.AddComponent<Button>().onClick.AddListener(HideWordLabel);
            labelBox = label.rectTransform;
            labelWord = UIFactory.LLabel("Word", labelBox, new Vector2(10, 9), new Vector2(150, 20), "", "", UIFactory.Ink).GetComponent<LocalizedText>();
            labelEnglish = UIFactory.LLabel("English", labelBox, new Vector2(10, -9), new Vector2(150, 20), "", "", UIFactory.Faded, TextAnchor.MiddleCenter, 16, false, true).GetComponent<LocalizedText>();
            UIFactory.TranslateButton(labelBox, Vector2.zero);
            labelTranslate = label.GetComponent<TranslateToggle>();
            labelBox.gameObject.SetActive(false);

            GameState.Toast += ShowToast;
            GameState.Data.stats.Changed += Refresh;
            Refresh();
        }

        Text Stat(Transform canvas, int index, string icon)
        {
            float x = -304 + index * 70;
            UIFactory.Img("Icon", canvas, new Vector2(x + 8, 160), new Vector2(16, 16), Color.white, UIFactory.LoadSprite(icon));
            return UIFactory.Label("Value", canvas, new Vector2(x + 44, 161), new Vector2(48, 20), "", UIFactory.Ink, TextAnchor.MiddleLeft);
        }

        /// <summary>The round map in the corner. Points of interest are (tile, icon) pairs.</summary>
        public void BuildMinimap(Transform canvas, VillageMap map, Transform playerTransform, CharacterSprite sprite,
            List<(Vector2Int tile, string icon)> places)
        {
            player = playerTransform;
            playerSprite = sprite;
            var mask = UIFactory.Img("Minimap", canvas, MapCentre, new Vector2(MapSize, MapSize), new Color32(70, 96, 60, 255), UIFactory.LoadSprite("Art/UI/minimap_mask"));
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var size = new Vector2(map.Width, map.Height) * 4 * MapZoom;
            mapImage = UIFactory.Img("Map", mask.transform, Vector2.zero, size, Color.white, UIFactory.LoadSprite("Art/UI/minimap")).rectTransform;
            foreach (var (tile, icon) in places)
            {
                var pos = new Vector2((tile.x + 0.5f) * 4 * MapZoom - size.x / 2, size.y / 2 - (tile.y + 0.5f) * 4 * MapZoom);
                bool dot = icon.EndsWith("map_dot");
                UIFactory.Img("Place", mapImage, pos, dot ? new Vector2(6, 6) : new Vector2(12, 12),
                    dot ? (Color)new Color32(255, 220, 90, 255) : Color.white, UIFactory.LoadSprite(icon));
            }
            mapArrow = UIFactory.Img("You", mask.transform, Vector2.zero, new Vector2(11, 11), Color.white, UIFactory.LoadSprite("Art/UI/map_arrow")).rectTransform;
            UIFactory.Img("Ring", canvas, MapCentre, new Vector2(MapSize + 8, MapSize + 8), Color.white, UIFactory.LoadSprite("Art/UI/minimap_ring"));
            zone = UIFactory.Label("Zone", canvas, MapCentre + new Vector2(0, -MapSize / 2 - 12), new Vector2(120, 18), GameConfig.TownName, Color.white);
            UIFactory.Outline(zone);
            UpdateMinimap();
        }

        void UpdateMinimap()
        {
            if (mapImage == null || player == null) return;
            var size = mapImage.sizeDelta;
            float px = player.position.x * 4 * MapZoom, py = -player.position.y * 4 * MapZoom;
            mapImage.anchoredPosition = new Vector2(size.x / 2 - px, py - size.y / 2);
            float[] angles = { 180, -90, 0, 90 };
            mapArrow.localRotation = Quaternion.Euler(0, 0, angles[playerSprite.Facing]);
        }

        void OnDestroy()
        {
            GameState.Toast -= ShowToast;
            if (GameState.Data != null) GameState.Data.stats.Changed -= Refresh;
        }

        void Refresh()
        {
            var s = GameState.Data.stats;
            hp.text = s.hp + "/" + s.maxHp;
            mana.text = s.mana + "/" + s.maxMana;
            hunger.text = s.hunger.ToString();
            coins.text = s.coins.ToString();
        }

        void ShowToast(string message)
        {
            // messages that arrive close together stack up instead of replacing each other
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
        /// The globe on the sign shows the English; clicking the sign closes it.</summary>
        public void ShowWordLabel(Vector3 world, string word, string english)
        {
            labelWord.Set(word, word);
            labelEnglish.Set("", english, true);
            int longest = Mathf.Max(word.Length, english.Length);
            float w = Mathf.Max(110, longest * 8 + 64);
            labelBox.sizeDelta = new Vector2(w, 62);
            ((RectTransform)labelBox.Find("Translate")).anchoredPosition = new Vector2(-w / 2 + 22, 0);
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
            // keep the whole label on screen, below the top bar and away from the minimap
            float halfW = labelBox.sizeDelta.x / 2, halfCanvas = canvasRect.rect.width / 2;
            local += new Vector2(0, 36);
            local.x = Mathf.Clamp(local.x, -halfCanvas + halfW + 4, halfCanvas - halfW - 4);
            local.y = Mathf.Clamp(local.y, -150, local.x + halfW > 210 ? 20 : 112);
            labelBox.anchoredPosition = local;
        }

        void Update()
        {
            toastTimer -= Time.deltaTime;
            var color = toast.color;
            color.a = Mathf.Clamp01(toastTimer);
            toast.color = color;

            if (labelBox.gameObject.activeSelf) PlaceLabel();
        }

        void LateUpdate() => UpdateMinimap();
    }
}
