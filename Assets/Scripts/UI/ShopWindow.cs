using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// A shop: a grid of item cards (picture, Spanish name, price). Click a card to buy it.
    /// The "Vender" tab lists what is in your bag so you can sell it (wood, pelts, fish...).
    /// Some shops (the Armería) have sections, and some items need a character level:
    /// those cards are greyed out with a padlock and the level you need.
    /// The globe button shows the English names.
    /// </summary>
    public class ShopWindow : GameWindow
    {
        const int Columns = 4, MaxCards = 12;
        Transform cards, sectionRow;
        Text money;
        Image buyTab, sellTab;
        LocalizedText greeting, feedback;
        string[] itemIds;
        (string es, string en, string[] ids)[] sections;
        int section;
        bool selling;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -4), new Vector2(560, 334), "", "");
            greeting = UIFactory.LLabel("Greeting", W, new Vector2(0, 120), new Vector2(420, 20), "", "", UIFactory.Faded).GetComponent<LocalizedText>();
            buyTab = UIFactory.LBtn("BuyTab", W, new Vector2(-60, 98), new Vector2(110, 22), "Comprar", "Buy", UIFactory.Gold, () => SetTab(false)).GetComponent<Image>();
            sellTab = UIFactory.LBtn("SellTab", W, new Vector2(60, 98), new Vector2(110, 22), "Vender", "Sell", UIFactory.Faded, () => SetTab(true)).GetComponent<Image>();
            sectionRow = UIFactory.Rect("Sections", W, Vector2.zero, Vector2.zero);
            cards = UIFactory.Rect("Cards", W, Vector2.zero, Vector2.zero);
            feedback = UIFactory.LLabel("Feedback", W, new Vector2(-50, -148), new Vector2(400, 20), "", "", UIFactory.Ink, TextAnchor.MiddleLeft).GetComponent<LocalizedText>();
            UIFactory.Img("Coin", W, new Vector2(190, -148), new Vector2(16, 16), Color.white, UIFactory.LoadSprite("Art/UI/coin"));
            money = UIFactory.Label("Money", W, new Vector2(228, -148), new Vector2(60, 20), "", UIFactory.Ink, TextAnchor.MiddleLeft);
            GameState.InventoryChanged += Rebuild;
            GameState.Data.stats.Changed += Rebuild;
        }

        void OnDestroy()
        {
            GameState.InventoryChanged -= Rebuild;
            if (GameState.Data != null) GameState.Data.stats.Changed -= Rebuild;
        }

        /// <summary>Opens a shop. "shopSections" (optional) splits the goods into tabs.</summary>
        public void Show(string spanishName, string englishName, string[] ids, (string es, string en, string[] ids)[] shopSections = null)
        {
            sections = shopSections;
            section = 0;
            itemIds = sections != null ? sections[0].ids : ids;
            W.Find("Title").GetComponent<LocalizedText>().Set(spanishName, englishName);
            greeting.Set("¿Qué quieres comprar?", "What would you like to buy?");
            feedback.Set("Haz clic en algo para comprarlo.", "Click something to buy it.");
            selling = false;
            OpenWindow();
            Rebuild();
        }

        void SetTab(bool sell)
        {
            selling = sell;
            feedback.Set(sell ? "Haz clic en algo de tu mochila para venderlo." : "Haz clic en algo para comprarlo.",
                sell ? "Click something from your bag to sell it." : "Click something to buy it.");
            Rebuild();
        }

        void SetSection(int i)
        {
            section = i;
            itemIds = sections[i].ids;
            Rebuild();
        }

        void Rebuild()
        {
            if (!Open || itemIds == null) return;
            foreach (Transform child in cards) Destroy(child.gameObject);
            foreach (Transform child in sectionRow) Destroy(child.gameObject);
            money.text = GameState.Data.stats.coins.ToString();
            buyTab.color = selling ? UIFactory.Faded : UIFactory.Gold;
            sellTab.color = selling ? UIFactory.Gold : UIFactory.Faded;

            bool showSections = sections != null && !selling;
            if (showSections)
            {
                for (int i = 0; i < sections.Length; i++)
                {
                    int index = i;
                    UIFactory.LBtn("Section", sectionRow, new Vector2((i - (sections.Length - 1) / 2f) * 130, 72), new Vector2(124, 22),
                        sections[i].es, sections[i].en, i == section ? UIFactory.Purple : UIFactory.Teal, () => SetSection(index));
                }
            }

            var entries = new List<(ItemDef item, int count, int bagIndex)>();
            if (selling)
            {
                var bag = GameState.Data.bag;
                for (int i = 0; i < bag.Count; i++)
                    if (!bag[i].Empty) entries.Add((ItemDatabase.Get(bag[i].id), bag[i].count, i));
                if (entries.Count == 0)
                    UIFactory.LLabel("Empty", cards, new Vector2(0, 20), new Vector2(400, 20), "Tu mochila está vacía.", "Your bag is empty.", UIFactory.Faded);
            }
            else
            {
                foreach (var id in itemIds)
                {
                    var item = ItemDatabase.Get(id);
                    if (item != null) entries.Add((item, 0, -1));
                }
            }

            float top = showSections ? 40 : 66;
            int playerLevel = GameState.Data.level;
            for (int i = 0; i < entries.Count && i < (showSections ? MaxCards : 16); i++)
            {
                var (item, count, bagIndex) = entries[i];
                var pos = new Vector2(-198 + (i % Columns) * 132, top - (i / Columns) * 54);
                bool once = item.kind != ItemKind.Food && item.kind != ItemKind.Book && item.kind != ItemKind.Material;
                bool owned = !selling && once && GameState.Has(item.id);
                bool locked = !selling && !owned && item.level > playerLevel;
                int price = selling ? item.sellPrice : item.price;
                Color back = owned ? new Color32(200, 190, 170, 255) : locked ? new Color32(196, 188, 176, 255) : new Color32(252, 238, 206, 255);
                var card = UIFactory.Btn("Card", cards, pos, new Vector2(126, 50), "", back, () =>
                {
                    if (selling) Sell(bagIndex);
                    else if (locked) feedback.Set("Necesitas el nivel " + item.level + " para comprar esto.", "You need level " + item.level + " to buy this.");
                    else Buy(item);
                });
                card.interactable = !owned;
                var icon = UIFactory.Img("Icon", card.transform, new Vector2(-44, 7), new Vector2(24, 24), locked ? new Color(0.45f, 0.45f, 0.45f) : Color.white,
                    UIFactory.LoadSprite("Art/Icons/" + item.icon));
                if (locked) UIFactory.Img("Lock", card.transform, new Vector2(-36, 4), new Vector2(16, 16), Color.white, UIFactory.LoadSprite("Art/UI/lock"));
                string priceText = owned ? "Tuyo" : locked ? "Nv " + item.level : price.ToString();
                Color priceColor = owned ? UIFactory.Green : locked ? UIFactory.Red : (Color)new Color32(150, 100, 30, 255);
                UIFactory.Label("Price", card.transform, new Vector2(-44, -15), new Vector2(44, 14), priceText, priceColor);
                var name = UIFactory.LLabel("Name", card.transform, new Vector2(14, 6), new Vector2(84, 34), item.spanish, item.english,
                    locked ? UIFactory.Faded : UIFactory.Ink, TextAnchor.MiddleLeft);
                name.horizontalOverflow = HorizontalWrapMode.Wrap;
                name.verticalOverflow = VerticalWrapMode.Truncate;
                name.resizeTextForBestFit = true;      // long names shrink to fit the card
                name.resizeTextMinSize = 10;
                name.resizeTextMaxSize = 16;
                name.lineSpacing = 0.85f;
                if (locked)
                    UIFactory.Label("Cost", card.transform, new Vector2(30, -16), new Vector2(60, 14), price.ToString(), new Color32(150, 100, 30, 255), TextAnchor.MiddleRight);
                if (count > 1) UIFactory.Label("Count", card.transform, new Vector2(-30, 14), new Vector2(30, 14), "x" + count, UIFactory.Faded, TextAnchor.MiddleRight);
            }
            translate.Set(translate.English);     // new cards follow the current language
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
            GameState.AddXp(SkillType.Trading, 2);
            feedback.Set("¡Gracias! Compraste " + item.spanish + ".", "Thank you! You bought: " + item.english + ".");
            GameState.Save();
        }

        void Sell(int bagIndex)
        {
            var slot = GameState.Data.bag[bagIndex];
            var item = ItemDatabase.Get(slot.id);
            if (item == null) return;
            GameState.RemoveAt(bagIndex);
            GameState.Data.stats.AddCoins(item.sellPrice);
            if (GameState.Data.pet == item.id && !GameState.Has(item.id)) GameState.Data.pet = "";
            feedback.Set("Vendiste " + item.spanish + " por " + item.sellPrice + ".", "You sold: " + item.english + " for " + item.sellPrice + ".");
            GameState.Save();
        }
    }
}
