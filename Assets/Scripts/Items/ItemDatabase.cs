using System.Collections.Generic;

namespace LLG
{
    public enum ItemKind { Rod, Weapon, Gear, Book, Food, Pet }

    /// <summary>The equipment slots on the character sheet (U). "None" = cannot be worn.</summary>
    public enum EquipSlot { None, Hat, Necklace, Cloak, Chest, Legs, Wrists, Primary, Secondary }

    public class ItemDef
    {
        public string id, spanish, english, icon;
        public int price;
        public ItemKind kind;
        public EquipSlot slot;
        public int power;      // weapon: attack. rod: extra seconds when fishing. food: hunger restored.
        public int defense;    // armour and shields
        public int mana;       // jewellery: extra max mana

        public bool Stacks => kind == ItemKind.Food;

        public ItemDef(string id, string spanish, string english, string icon, int price, ItemKind kind,
            EquipSlot slot = EquipSlot.None, int power = 0, int defense = 0, int mana = 0)
        {
            this.id = id; this.spanish = spanish; this.english = english; this.icon = icon; this.price = price;
            this.kind = kind; this.slot = slot; this.power = power; this.defense = defense; this.mana = mana;
        }
    }

    /// <summary>
    /// Everything that can be bought or found. To add an item: add a line here, add its id to a shop
    /// list below, and put a 16x16 picture in Assets/Resources/Art/Icons/.
    /// (Later these names should move into the language packs so each language has its own.)
    /// </summary>
    public static class ItemDatabase
    {
        static readonly ItemDef[] All =
        {
            // fishing
            new ItemDef("rod",       "la caña de pescar", "fishing rod", "item_rod",       30, ItemKind.Rod),
            new ItemDef("rod_fine",  "la caña mágica",    "magic rod",   "item_rod_fine", 120, ItemKind.Rod, power: 10),
            // weapons (Armería)
            new ItemDef("knife",     "el cuchillo",       "knife",       "item_knife",     40, ItemKind.Weapon, EquipSlot.Primary, power: 1),
            new ItemDef("dagger",    "la daga",           "dagger",      "item_dagger",    80, ItemKind.Weapon, EquipSlot.Primary, power: 2),
            new ItemDef("bow",       "el arco",           "bow",         "item_bow",      150, ItemKind.Weapon, EquipSlot.Primary, power: 3),
            new ItemDef("sword",     "la espada",         "sword",       "item_sword",    200, ItemKind.Weapon, EquipSlot.Primary, power: 5),
            new ItemDef("shield",    "el escudo",         "shield",      "gear_shield",   120, ItemKind.Gear, EquipSlot.Secondary, defense: 3),
            // clothes and jewellery (Tienda)
            new ItemDef("hat",       "el sombrero",       "hat",         "gear_hat",       45, ItemKind.Gear, EquipSlot.Hat, defense: 1),
            new ItemDef("necklace",  "el collar",         "necklace",    "gear_necklace",  90, ItemKind.Gear, EquipSlot.Necklace, mana: 3),
            new ItemDef("cloak",     "la capa",           "cloak",       "gear_cloak",     70, ItemKind.Gear, EquipSlot.Cloak, defense: 1),
            new ItemDef("vest",      "el chaleco",        "vest",        "gear_vest",      60, ItemKind.Gear, EquipSlot.Chest, defense: 2),
            new ItemDef("pants",     "los pantalones",    "trousers",    "gear_pants",     50, ItemKind.Gear, EquipSlot.Legs, defense: 1),
            new ItemDef("bracelet",  "la pulsera",        "bracelet",    "gear_bracelet",  55, ItemKind.Gear, EquipSlot.Wrists, mana: 2),
            new ItemDef("book",      "el libro",          "book",        "item_book",      60, ItemKind.Book, power: 1),
            // food
            new ItemDef("bread",     "el pan",            "bread",       "food_bread",      8, ItemKind.Food, power: 20),
            new ItemDef("croissant", "el cruasán",        "croissant",   "food_croissant", 12, ItemKind.Food, power: 30),
            new ItemDef("cake",      "el pastel",         "cake",        "food_cake",      20, ItemKind.Food, power: 50),
            new ItemDef("candy",     "el caramelo",       "candy",       "food_candy",      3, ItemKind.Food, power: 5),
            new ItemDef("lollipop",  "la piruleta",       "lollipop",    "food_lollipop",   5, ItemKind.Food, power: 8),
            new ItemDef("cookie",    "la galleta",        "cookie",      "food_cookie",     6, ItemKind.Food, power: 12),
            new ItemDef("chocolate", "el chocolate",      "chocolate",   "food_chocolate", 10, ItemKind.Food, power: 18),
            new ItemDef("fish",      "el pez",            "fish",        "food_fish",       0, ItemKind.Food, power: 30),
            new ItemDef("orange",    "la naranja",        "orange",      "food_orange",     0, ItemKind.Food, power: 10),
            // pets (Mascotas)
            new ItemDef("pet_dog",    "el perro",         "dog",         "pet_dog",       150, ItemKind.Pet),
            new ItemDef("pet_cat",    "el gato",          "cat",         "pet_cat",       150, ItemKind.Pet),
            new ItemDef("pet_rabbit", "el conejo",        "rabbit",      "pet_rabbit",    120, ItemKind.Pet),
            new ItemDef("pet_parrot", "el loro",          "parrot",      "pet_parrot",    250, ItemKind.Pet),
        };

        public static readonly string[] Store = { "rod", "rod_fine", "hat", "necklace", "cloak", "vest", "pants", "bracelet", "book" };
        public static readonly string[] Weapons = { "knife", "dagger", "bow", "sword", "shield" };
        public static readonly string[] Pets = { "pet_dog", "pet_cat", "pet_rabbit", "pet_parrot" };
        public static readonly string[] Bakery = { "bread", "croissant", "cake" };
        public static readonly string[] CandyShop = { "candy", "lollipop", "cookie", "chocolate" };

        static Dictionary<string, ItemDef> byId;

        public static ItemDef Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (byId == null)
            {
                byId = new Dictionary<string, ItemDef>();
                foreach (var item in All) byId[item.id] = item;
            }
            return byId.TryGetValue(id, out var found) ? found : null;
        }

        /// <summary>Spanish and English names of the equipment slots.</summary>
        public static readonly string[,] SlotNames =
        {
            { "", "" }, { "Cabeza", "Head" }, { "Cuello", "Neck" }, { "Espalda", "Back" }, { "Pecho", "Chest" },
            { "Piernas", "Legs" }, { "Muñecas", "Wrists" }, { "Mano principal", "Main hand" }, { "Mano secundaria", "Off hand" },
        };
    }
}
