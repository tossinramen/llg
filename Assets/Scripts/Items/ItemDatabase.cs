using System.Collections.Generic;

namespace LLG
{
    public enum ItemKind { Rod, Weapon, Book, Food }

    public class ItemDef
    {
        public string id, spanish, english, icon;
        public int price;
        public ItemKind kind;
        public int power;      // weapon: attack. rod: extra seconds when fishing. food: hunger restored.

        public ItemDef(string id, string spanish, string english, string icon, int price, ItemKind kind, int power)
        {
            this.id = id; this.spanish = spanish; this.english = english; this.icon = icon;
            this.price = price; this.kind = kind; this.power = power;
        }
    }

    /// <summary>
    /// Everything that can be bought. To add an item: add a line here, add its id to a shop list
    /// below, and put a 16x16 picture in Assets/Resources/Art/Icons/.
    /// (Later these names should move into the language packs so each language has its own.)
    /// </summary>
    public static class ItemDatabase
    {
        static readonly ItemDef[] All =
        {
            new ItemDef("rod",       "la caña de pescar", "fishing rod", "item_rod",      30, ItemKind.Rod, 0),
            new ItemDef("rod_fine",  "la caña mágica",    "magic rod",   "item_rod_fine", 120, ItemKind.Rod, 10),
            new ItemDef("knife",     "el cuchillo",       "knife",       "item_knife",    40, ItemKind.Weapon, 1),
            new ItemDef("dagger",    "la daga",           "dagger",      "item_dagger",   80, ItemKind.Weapon, 2),
            new ItemDef("bow",       "el arco",           "bow",         "item_bow",      150, ItemKind.Weapon, 3),
            new ItemDef("sword",     "la espada",         "sword",       "item_sword",    200, ItemKind.Weapon, 5),
            new ItemDef("book",      "el libro",          "book",        "item_book",     60, ItemKind.Book, 1),

            new ItemDef("bread",     "el pan",            "bread",       "food_bread",     8, ItemKind.Food, 20),
            new ItemDef("croissant", "el cruasán",        "croissant",   "food_croissant", 12, ItemKind.Food, 30),
            new ItemDef("cake",      "el pastel",         "cake",        "food_cake",      20, ItemKind.Food, 50),

            new ItemDef("candy",     "el caramelo",       "candy",       "food_candy",      3, ItemKind.Food, 5),
            new ItemDef("lollipop",  "la piruleta",       "lollipop",    "food_lollipop",   5, ItemKind.Food, 8),
            new ItemDef("cookie",    "la galleta",        "cookie",      "food_cookie",     6, ItemKind.Food, 12),
            new ItemDef("chocolate", "el chocolate",      "chocolate",   "food_chocolate", 10, ItemKind.Food, 18),
        };

        public static readonly string[] Store = { "rod", "rod_fine", "knife", "dagger", "bow", "sword", "book" };
        public static readonly string[] Bakery = { "bread", "croissant", "cake" };
        public static readonly string[] CandyShop = { "candy", "lollipop", "cookie", "chocolate" };

        static Dictionary<string, ItemDef> byId;

        public static ItemDef Get(string id)
        {
            if (byId == null)
            {
                byId = new Dictionary<string, ItemDef>();
                foreach (var item in All) byId[item.id] = item;
            }
            return byId.TryGetValue(id, out var found) ? found : null;
        }
    }
}
