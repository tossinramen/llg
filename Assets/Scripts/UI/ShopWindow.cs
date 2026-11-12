using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// A list of items in a window. Used by the three shops and, without Buy buttons, as the backpack.
    /// Every item is shown by its name in the language being learned, with the English underneath.
    /// </summary>
    public class ShopWindow : MonoBehaviour
    {
        GameObject window;
        Transform rows;
        Text title, subtitle, feedback;
        string[] itemIds;
        bool backpack;
        VillageController village;

        public bool Open => window != null && window.activeSelf;

        public void Build(Transform canvas, VillageController owner)
        {
            village = owner;
            window = UIFactory.Panel("Shop", canvas, new Vector2(0, -8), new Vector2(520, 316)).gameObject;
            title = UIFactory.Label("Title", window.transform, new Vector2(0, 132), new Vector2(480, 20), "", UIFactory.Purple, TextAnchor.MiddleCenter, 16, true);
            subtitle = UIFactory.Label("Subtitle", window.transform, new Vector2(0, 110), new Vector2(480, 20), "", UIFactory.Faded);
            rows = UIFactory.Rect("Rows", window.transform, Vector2.zero, Vector2.zero);
            feedback = UIFactory.Label("Feedback", window.transform, new Vector2(-60, -130), new Vector2(360, 20), "", UIFactory.Ink, TextAnchor.MiddleLeft);
            UIFactory.Btn("Close", window.transform, new Vector2(196, -130), new Vector2(104, 26), "Salir (Esc)", UIFactory.Red, Close);
            window.SetActive(false);
        }

        public void Show(string spanishTitle, string englishTitle, string[] ids)
        {
            backpack = false;
            itemIds = ids;
            title.text = spanishTitle;
            subtitle.text = englishTitle;
            feedback.text = "";
            window.SetActive(true);
            Rebuild();
        }

        public void ShowBackpack()
        {
            backpack = true;
            var stats = GameState.Data.stats;
            title.text = "La mochila";
            subtitle.text = "Backpack";
            feedback.text = "ATK +" + GameState.Attack() + "   INT " + stats.intelligence + "   Fish " + GameState.Data.fishCaught;
            window.SetActive(true);
            Rebuild();
        }

        void Rebuild()
        {
            foreach (Transform child in rows) Destroy(child.gameObject);

            var ids = new List<string>(backpack ? GameState.Data.items.ToArray() : itemIds);
            if (ids.Count == 0)
                UIFactory.Label("Empty", rows, new Vector2(0, 20), new Vector2(480, 20), "Vacía. (Empty - visit the shops!)", UIFactory.Faded);

            for (int i = 0; i < ids.Count && i < 7; i++)
            {
                var item = ItemDatabase.Get(ids[i]);
                if (item == null) continue;
                float y = 80 - i * 30;
                UIFactory.Img("Icon", rows, new Vector2(-228, y), new Vector2(24, 24), Color.white, UIFactory.LoadSprite("Art/Icons/" + item.icon));
                UIFactory.Label("Name", rows, new Vector2(-128, y), new Vector2(160, 22), item.spanish, UIFactory.Ink, TextAnchor.MiddleLeft);
                UIFactory.Label("English", rows, new Vector2(22, y), new Vector2(110, 22), item.english, UIFactory.Faded, TextAnchor.MiddleLeft);
                if (backpack) continue;

                UIFactory.Img("Coin", rows, new Vector2(96, y), new Vector2(16, 16), Color.white, UIFactory.LoadSprite("Art/UI/coin"));
                UIFactory.Label("Price", rows, new Vector2(130, y), new Vector2(40, 22), item.price.ToString(), UIFactory.Ink, TextAnchor.MiddleLeft);

                bool owned = item.kind != ItemKind.Food && item.kind != ItemKind.Book && GameState.Has(item.id);
                var captured = item;
                var button = UIFactory.Btn("Buy", rows, new Vector2(202, y), new Vector2(92, 26), owned ? "Tuyo" : "Comprar",
                    owned ? UIFactory.Faded : UIFactory.Green, () => Buy(captured));
                button.interactable = !owned;
            }
        }

        void Buy(ItemDef item)
        {
            var stats = GameState.Data.stats;
            if (stats.coins < item.price)
            {
                feedback.text = "No tienes suficiente " + GameConfig.CurrencyName + ". (Not enough.)";
                return;
            }
            stats.AddCoins(-item.price);
            switch (item.kind)
            {
                case ItemKind.Food:
                    stats.Eat(item.power);
                    feedback.text = "¡Qué rico! (Yum!)  +" + item.power + " hunger";
                    break;
                case ItemKind.Book:
                    stats.intelligence++;
                    stats.maxMana++;
                    stats.RestoreMana(1);
                    feedback.text = "¡Más inteligente! (Smarter!)  INT " + stats.intelligence;
                    break;
                default:
                    GameState.Data.items.Add(item.id);
                    feedback.text = "¡Gracias! (Thank you!)  " + item.spanish;
                    break;
            }
            GameState.Save();
            Rebuild();
        }

        public void Close()
        {
            if (!Open) return;
            window.SetActive(false);
            village.NoteWindowClosed();
        }

        void Update()
        {
            if (Open && Input.GetKeyDown(KeyCode.Escape)) Close();
        }
    }
}
