using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>The backpack (B): 20 slots. Click an item to eat it, read it, wear it or call your pet.</summary>
    public class BagWindow : GameWindow
    {
        const int Columns = 4;
        readonly List<ItemSlotView> slots = new List<ItemSlotView>();
        LocalizedText itemName, itemEnglish, hint;
        Text money;

        protected override bool ToggleKeyPressed => Keybinds.Down(GameAction.Backpack);

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(226, -8), new Vector2(180, 324), "Mochila", "Bag");
            for (int i = 0; i < GameState.BagSize; i++)
            {
                int index = i;
                var pos = new Vector2(-57 + (i % Columns) * 38, 98 - (i / Columns) * 38);
                var view = ItemSlotView.Create(W, pos, 34, () => GameState.UseBagSlot(index));
                view.hover.Entered += () => Describe(index);
                view.hover.Exited += () => Describe(-1);
                slots.Add(view);
            }
            itemName = UIFactory.LLabel("ItemName", W, new Vector2(0, -98), new Vector2(170, 20), "", "", UIFactory.Ink).GetComponent<LocalizedText>();
            itemEnglish = UIFactory.LLabel("ItemEnglish", W, new Vector2(0, -116), new Vector2(170, 20), "", "", UIFactory.Faded, TextAnchor.MiddleCenter, 16, false, true).GetComponent<LocalizedText>();
            hint = UIFactory.LLabel("Hint", W, new Vector2(0, -116), new Vector2(170, 20), "", "", UIFactory.Teal).GetComponent<LocalizedText>();
            var purse = UIFactory.Box("Purse", W, new Vector2(0, -138), new Vector2(120, 24), new Color32(232, 214, 176, 255));
            UIFactory.Img("Lexa", purse.transform, new Vector2(-42, 1), new Vector2(14, 14), Color.white, UIFactory.LoadSprite("Art/UI/lexa"));
            money = UIFactory.Label("Money", purse.transform, new Vector2(10, 1), new Vector2(80, 16), "", UIFactory.Ink, TextAnchor.MiddleLeft);
            GameState.InventoryChanged += Refresh;
            GameState.Data.stats.Changed += Refresh;
        }

        void OnDestroy()
        {
            GameState.InventoryChanged -= Refresh;
            if (GameState.Data != null) GameState.Data.stats.Changed -= Refresh;
        }

        public void Toggle()
        {
            if (Open) { Close(); return; }
            OpenWindow();
            Describe(-1);
            Refresh();
        }

        void Refresh()
        {
            if (!Open) return;
            var bag = GameState.Data.bag;
            for (int i = 0; i < slots.Count; i++)
            {
                var item = ItemDatabase.Get(bag[i].id);
                var tint = item != null && item.kind == ItemKind.Pet && GameState.Data.pet == item.id ? new Color(1f, 1f, 0.7f) : Color.white;
                slots[i].Show(item == null ? null : UIFactory.LoadSprite("Art/Icons/" + item.icon), bag[i].count, tint);
            }
            money.text = GameState.Data.stats.coins + " " + GameConfig.CurrencyName;
        }

        void Describe(int index)
        {
            var item = index >= 0 ? ItemDatabase.Get(GameState.Data.bag[index].id) : null;
            if (item == null)
            {
                itemName.Set("Elige un objeto", "Choose an item");
                itemEnglish.Set("", "", true);
                hint.Set("", "");
                return;
            }
            itemName.Set(GameState.Capital(item.spanish), GameState.Capital(item.spanish), false);
            itemEnglish.Set("", GameState.Capital(item.english), true);
            string es, en;
            switch (item.kind)
            {
                case ItemKind.Food: es = "Clic: comer (+" + item.power + ")"; en = ""; break;
                case ItemKind.Book: es = "Clic: leer (+1 INT)"; en = ""; break;
                case ItemKind.Pet: es = GameState.Data.pet == item.id ? "Clic: descansar" : "Clic: llamar"; en = ""; break;
                case ItemKind.Rod: es = "Úsala en el estanque"; en = ""; break;
                case ItemKind.Seed: es = "Plántalas en el huerto"; en = ""; break;
                case ItemKind.Instrument: es = "Tócalo en la escuela"; en = ""; break;
                case ItemKind.Material: es = "Para vender"; en = ""; break;
                default: es = "Clic: equipar"; en = ""; break;
            }
            hint.Set(es, "", false);
            // the hint and the English line share a spot: show the English one when translated
            hint.gameObject.SetActive(!translate.English);
        }

        protected override void Update()
        {
            base.Update();
            if (Open && hint != null) hint.gameObject.SetActive(!translate.English);
        }
    }
}
