using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// The always-visible bar (HP, mana, hunger, currency), pop-up messages, the "E: ..." prompt
    /// and the word label that appears when you left-click something.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        Text hp, mana, hunger, coins, toast, prompt, labelWord, labelEnglish, help, closeMark;
        GameObject promptBox;
        RectTransform canvasRect, labelBox;
        Camera cam;
        Vector3 labelWorld;
        float toastTimer;

        public void Build(Transform canvas)
        {
            canvasRect = (RectTransform)canvas;
            cam = Camera.main;

            UIFactory.Panel("HudBar", canvas, new Vector2(-170, 160), new Vector2(292, 34));
            hp = Stat(canvas, 0, "Art/UI/hp_heart");
            mana = Stat(canvas, 1, "Art/UI/mana_drop");
            hunger = Stat(canvas, 2, "Art/Icons/food_bread");
            coins = Stat(canvas, 3, "Art/UI/coin");

            help = UIFactory.Label("Help", canvas, new Vector2(160, 158), new Vector2(300, 40), "", Color.white, TextAnchor.MiddleRight);
            UIFactory.Outline(help);
            RefreshHelp();
            Keybinds.Changed += RefreshHelp;

            toast = UIFactory.Label("Toast", canvas, new Vector2(0, 112), new Vector2(600, 60), "", Color.white);
            UIFactory.Outline(toast);

            promptBox = UIFactory.Panel("Prompt", canvas, new Vector2(0, -150), new Vector2(300, 34)).gameObject;
            prompt = UIFactory.Label("Text", promptBox.transform, new Vector2(0, 1), new Vector2(290, 24), "", UIFactory.Ink);
            promptBox.SetActive(false);

            // the word sign stays up until you click it (or read something else)
            var label = UIFactory.Panel("WordLabel", canvas, Vector2.zero, new Vector2(160, 62));
            label.gameObject.AddComponent<Button>().onClick.AddListener(HideWordLabel);
            labelBox = label.rectTransform;
            closeMark = UIFactory.Label("Close", labelBox, Vector2.zero, new Vector2(12, 12), "x", UIFactory.Faded);
            labelWord = UIFactory.Label("Word", labelBox, new Vector2(0, 9), new Vector2(150, 20), "", UIFactory.Ink);
            labelEnglish = UIFactory.Label("English", labelBox, new Vector2(0, -9), new Vector2(150, 20), "", UIFactory.Faded);
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

        void RefreshHelp()
        {
            string move = Keybinds.KeyName(GameAction.Up) + Keybinds.KeyName(GameAction.Left)
                + Keybinds.KeyName(GameAction.Down) + Keybinds.KeyName(GameAction.Right);
            if (move.Length > 4) move = "Move keys";
            help.text = move + " move   " + Keybinds.KeyName(GameAction.Use) + " use   "
                + Keybinds.KeyName(GameAction.Backpack) + " bag   Esc menu\n"
                + "Left click read   Right click attack";
        }

        void OnDestroy()
        {
            Keybinds.Changed -= RefreshHelp;
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

        /// <summary>Shows what the E key will do right now, or hides the prompt when given null.</summary>
        public void SetPrompt(string text)
        {
            promptBox.SetActive(text != null);
            if (text != null) prompt.text = "E: " + text;
        }

        /// <summary>A little sign above something in the world, showing its name.</summary>
        public void ShowWordLabel(Vector3 world, string word, string english)
        {
            labelWord.text = word;
            labelEnglish.text = english;
            int longest = Mathf.Max(word.Length, english.Length);
            labelBox.sizeDelta = new Vector2(Mathf.Max(80, longest * 8 + 52), 62);
            ((RectTransform)closeMark.transform).anchoredPosition = new Vector2(labelBox.sizeDelta.x / 2 - 12, 17);
            labelWorld = world;
            labelBox.gameObject.SetActive(true);
            labelBox.SetAsLastSibling();
            PlaceLabel();
        }

        public void HideWordLabel() => labelBox.gameObject.SetActive(false);

        public bool WordLabelOpen => labelBox.gameObject.activeSelf;

        void PlaceLabel()
        {
            var screen = cam.WorldToScreenPoint(labelWorld);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, cam, out var local)) return;
            // keep the whole label on screen and below the top bar
            float halfW = labelBox.sizeDelta.x / 2, halfCanvas = canvasRect.rect.width / 2;
            local += new Vector2(0, 36);
            local.x = Mathf.Clamp(local.x, -halfCanvas + halfW + 4, halfCanvas - halfW - 4);
            local.y = Mathf.Clamp(local.y, -150, 112);
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
    }
}
