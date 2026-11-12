using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>The always-visible bar (HP, mana, hunger, currency), pop-up messages and the "E: ..." prompt.</summary>
    public class Hud : MonoBehaviour
    {
        Text hp, mana, hunger, coins, toast, prompt;
        GameObject promptBox;
        float toastTimer;

        public void Build(Transform canvas)
        {
            UIFactory.Panel("HudBar", canvas, new Vector2(-170, 160), new Vector2(292, 34));
            hp = Stat(canvas, 0, "Art/UI/hp_heart");
            mana = Stat(canvas, 1, "Art/UI/mana_drop");
            hunger = Stat(canvas, 2, "Art/Icons/food_bread");
            coins = Stat(canvas, 3, "Art/UI/coin");

            var help = UIFactory.Label("Help", canvas, new Vector2(170, 164), new Vector2(280, 20),
                "Move: WASD   Use: E   Bag: I", Color.white, TextAnchor.MiddleRight);
            UIFactory.Outline(help);

            toast = UIFactory.Label("Toast", canvas, new Vector2(0, 112), new Vector2(600, 60), "", Color.white);
            UIFactory.Outline(toast);

            promptBox = UIFactory.Panel("Prompt", canvas, new Vector2(0, -150), new Vector2(300, 34)).gameObject;
            prompt = UIFactory.Label("Text", promptBox.transform, new Vector2(0, 1), new Vector2(290, 24), "", UIFactory.Ink);
            promptBox.SetActive(false);

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

        /// <summary>Shows what the E key will do right now, or hides the prompt when given null.</summary>
        public void SetPrompt(string text)
        {
            promptBox.SetActive(text != null);
            if (text != null) prompt.text = "E: " + text;
        }

        void Update()
        {
            toastTimer -= Time.deltaTime;
            var color = toast.color;
            color.a = Mathf.Clamp01(toastTimer);
            toast.color = color;
        }
    }
}
