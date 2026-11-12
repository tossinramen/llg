using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// A shop. Items are listed by their Spanish name; the globe button shows the English.
    /// Bought things go into your bag (B). Clothes, weapons and pets can only be bought once.
    /// </summary>
    public class ShopWindow : GameWindow
    {
        Transform rows;
        Text money;
        LocalizedText greeting, feedback;
        string[] itemIds;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -8), new Vector2(520, 320), "", "");
            greeting = UIFactory.LLabel("Greeting", W, new Vector2(0, 112), new Vector2(460, 20), "", "", UIFactory.Faded).GetComponent<LocalizedText>();
            rows = UIFactory.Rect("Rows", W, Vector2.zero, Vector2.zero);
            feedback = UIFactory.LLabel("Feedback", W, new Vector2(-40, -136), new Vector2(380, 20), "", "", UIFactory.Ink, TextAnchor.MiddleLeft).GetComponent<LocalizedText>();
            UIFactory.Img("Coin", W, new Vector2(176, -136), new Vector2(16, 16), Color.white, UIFactory.LoadSprite("Art/UI/coin"));
            money = UIFactory.Label("Money", W, new Vector2(214, -136), new Vector2(60, 20), "", UIFactory.Ink, TextAnchor.MiddleLeft);
            GameState.InventoryChanged += Rebuild;
            GameState.Data.stats.Changed += Rebuild;
        }

        void OnDestroy()
        {
            GameState.InventoryChanged -= Rebuild;
            if (GameState.Data != null) GameState.Data.stats.Changed -= Rebuild;
        }

        public void Show(string spanishName, string englishName, string[] ids)
        {
            itemIds = ids;
            W.Find("Title").GetComponent<LocalizedText>().Set(spanishName, englishName);
            greeting.Set("¡Bienvenido! ¿Qué quieres comprar?", "Welcome! What would you like to buy?");
            feedback.Set("", "");
            OpenWindow();
            Rebuild();
        }

        void Rebuild()
        {
            if (!Open || itemIds == null) return;
            foreach (Transform child in rows) Destroy(child.gameObject);
            money.text = GameState.Data.stats.coins.ToString();

            for (int i = 0; i < itemIds.Length && i < 9; i++)
            {
                var item = ItemDatabase.Get(itemIds[i]);
                if (item == null) continue;
                float y = 84 - i * 24;
                UIFactory.Img("Icon", rows, new Vector2(-226, y), new Vector2(20, 20), Color.white, UIFactory.LoadSprite("Art/Icons/" + item.icon));
                UIFactory.Label("Name", rows, new Vector2(-124, y), new Vector2(170, 20), item.spanish, UIFactory.Ink, TextAnchor.MiddleLeft);
                UIFactory.LLabel("English", rows, new Vector2(30, y), new Vector2(130, 20), "", item.english, UIFactory.Faded, TextAnchor.MiddleLeft, 16, false, true);
                UIFactory.Img("Coin", rows, new Vector2(100, y), new Vector2(14, 14), Color.white, UIFactory.LoadSprite("Art/UI/coin"));
                UIFactory.Label("Price", rows, new Vector2(132, y), new Vector2(40, 20), item.price.ToString(), UIFactory.Ink, TextAnchor.MiddleLeft);

                bool once = item.kind != ItemKind.Food && item.kind != ItemKind.Book;
                bool owned = once && GameState.Has(item.id);
                var captured = item;
                var button = UIFactory.LBtn("Buy", rows, new Vector2(204, y), new Vector2(88, 22), owned ? "Tuyo" : "Comprar", owned ? "Owned" : "Buy",
                    owned ? UIFactory.Faded : UIFactory.Green, () => Buy(captured));
                button.interactable = !owned;
            }
            translate.Set(translate.English);     // new rows follow the current language
        }

        void Buy(ItemDef item)
        {
            var stats = GameState.Data.stats;
            if (stats.coins < item.price)
            {
                feedback.Set("No tienes suficiente " + GameConfig.CurrencyName + ".", "You don't have enough " + GameConfig.CurrencyName + ".");
                return;
            }
            if (!GameState.AddItem(item.id))
            {
                feedback.Set("Tu mochila está llena.", "Your bag is full.");
                return;
            }
            stats.AddCoins(-item.price);
            if (item.kind == ItemKind.Pet && string.IsNullOrEmpty(GameState.Data.pet)) GameState.Data.pet = item.id;
            feedback.Set("¡Gracias! Compraste " + item.spanish + ".", "Thank you! You bought: " + item.english + ".");
            GameState.Save();
        }
    }
}
