using System.Collections.Generic;

namespace LLG
{
    public enum ItemKind { Rod, Weapon, Gear, Book, Food, Pet, Material }

    /// <summary>The equipment slots on the character sheet (U). "None" = cannot be worn.</summary>
    public enum EquipSlot { None, Hat, Necklace, Cloak, Chest, Legs, Wrists, Primary, Secondary }

    public class ItemDef
    {
        public string id, spanish, english, icon;
        public int price;
        public ItemKind kind;
        public EquipSlot slot;
        public int power;      // weapon: damage. rod: extra seconds when fishing. food: hunger restored.
        public int defense;    // armour and shields
        public int mana;       // jewellery: extra max mana
        public int chop;       // how much a hit with it damages a tree (axes are best)
        public bool ranged;    // bows fire arrows
        public int sellPrice;  // what shops pay you for it

        public bool Stacks => kind == ItemKind.Food || kind == ItemKind.Material;

        public ItemDef(string id, string spanish, string english, string icon, int price, ItemKind kind,
            EquipSlot slot = EquipSlot.None, int power = 0, int defense = 0, int mana = 0, int chop = 1,
            bool ranged = false, int sell = -1)
        {
            this.id = id; this.spanish = spanish; this.english = english; this.icon = icon; this.price = price;
            this.kind = kind; this.slot = slot; this.power = power; this.defense = defense; this.mana = mana;
            this.chop = chop; this.ranged = ranged;
            sellPrice = sell >= 0 ? sell : System.Math.Max(1, price / 2);
        }
    }

    /// <summary>
    /// Everything that can be bought, found or sold. To add an item: add a line here, add its id to a
    /// shop list below, and put a 16x16 picture in Assets/Resources/Art/Icons/.
    /// </summary>
    public static class ItemDatabase
    {
        static readonly ItemDef[] All =
        {
            // ---- general store (Tienda)
            new ItemDef("rod",        "la caña de pescar", "fishing rod", "item_rod",       30, ItemKind.Rod),
            new ItemDef("rod_fine",   "la caña mágica",    "magic rod",   "item_rod_fine", 120, ItemKind.Rod, power: 10),
            new ItemDef("book",       "el libro",          "book",        "item_book",      60, ItemKind.Book, power: 1),
            new ItemDef("hat",        "el sombrero",       "hat",         "gear_hat",       45, ItemKind.Gear, EquipSlot.Hat, defense: 1),
            new ItemDef("necklace",   "el collar",         "necklace",    "gear_necklace",  90, ItemKind.Gear, EquipSlot.Necklace, mana: 3),
            new ItemDef("cloak",      "la capa",           "cloak",       "gear_cloak",     70, ItemKind.Gear, EquipSlot.Cloak, defense: 1),
            new ItemDef("vest",       "el chaleco",        "vest",        "gear_vest",      60, ItemKind.Gear, EquipSlot.Chest, defense: 2),
            new ItemDef("pants",      "los pantalones",    "trousers",    "gear_pants",     50, ItemKind.Gear, EquipSlot.Legs, defense: 1),
            new ItemDef("bracelet",   "la pulsera",        "bracelet",    "gear_bracelet",  55, ItemKind.Gear, EquipSlot.Wrists, mana: 2),
            new ItemDef("apple",      "la manzana",        "apple",       "food_apple",      5, ItemKind.Food, power: 10),
            new ItemDef("cheese",     "el queso",          "cheese",      "food_cheese",     9, ItemKind.Food, power: 15),
            new ItemDef("egg",        "el huevo",          "egg",         "food_egg",        6, ItemKind.Food, power: 12),

            // ---- weapons (Armería): knives, swords, axes, bows
            new ItemDef("knife",      "el cuchillo",       "knife",       "item_knife",      40, ItemKind.Weapon, EquipSlot.Primary, power: 2),
            new ItemDef("pocketknife","la navaja",         "pocketknife", "item_pocketknife",35, ItemKind.Weapon, EquipSlot.Primary, power: 2),
            new ItemDef("dagger",     "la daga",           "dagger",      "item_dagger",     80, ItemKind.Weapon, EquipSlot.Primary, power: 3),
            new ItemDef("shortsword", "la espada corta",   "short sword", "item_shortsword",110, ItemKind.Weapon, EquipSlot.Primary, power: 4),
            new ItemDef("sword",      "la espada",         "sword",       "item_sword",     200, ItemKind.Weapon, EquipSlot.Primary, power: 6),
            new ItemDef("sabre",      "el sable",          "sabre",       "item_sabre",     230, ItemKind.Weapon, EquipSlot.Primary, power: 6),
            new ItemDef("rapier",     "el estoque",        "rapier",      "item_rapier",    280, ItemKind.Weapon, EquipSlot.Primary, power: 7),
            new ItemDef("greatsword", "el mandoble",       "greatsword",  "item_greatsword",420, ItemKind.Weapon, EquipSlot.Primary, power: 10),
            new ItemDef("hatchet",    "el hacha de mano",  "hatchet",     "item_hatchet",    60, ItemKind.Weapon, EquipSlot.Primary, power: 3, chop: 2),
            new ItemDef("axe",        "el hacha",          "axe",         "item_axe",       120, ItemKind.Weapon, EquipSlot.Primary, power: 5, chop: 3),
            new ItemDef("battleaxe",  "el hacha de guerra","battle axe",  "item_battleaxe", 320, ItemKind.Weapon, EquipSlot.Primary, power: 8, chop: 3),
            new ItemDef("shortbow",   "el arco corto",     "short bow",   "item_shortbow",  100, ItemKind.Weapon, EquipSlot.Primary, power: 3, ranged: true),
            new ItemDef("bow",        "el arco",           "bow",         "item_bow",       150, ItemKind.Weapon, EquipSlot.Primary, power: 4, ranged: true),
            new ItemDef("longbow",    "el arco largo",     "longbow",     "item_longbow",   260, ItemKind.Weapon, EquipSlot.Primary, power: 6, ranged: true),
            new ItemDef("crossbow",   "la ballesta",       "crossbow",    "item_crossbow",  400, ItemKind.Weapon, EquipSlot.Primary, power: 9, ranged: true),

            // ---- armour (Herrería)
            new ItemDef("helmet",     "el casco",          "helmet",      "gear_helmet",     90, ItemKind.Gear, EquipSlot.Hat, defense: 2),
            new ItemDef("chainmail",  "la cota de malla",  "chainmail",   "gear_chainmail", 150, ItemKind.Gear, EquipSlot.Chest, defense: 3),
            new ItemDef("breastplate","el peto",           "breastplate", "gear_breastplate",260, ItemKind.Gear, EquipSlot.Chest, defense: 5),
            new ItemDef("greaves",    "las grebas",        "greaves",     "gear_greaves",   110, ItemKind.Gear, EquipSlot.Legs, defense: 2),
            new ItemDef("bracers",    "los brazales",      "bracers",     "gear_bracers",    70, ItemKind.Gear, EquipSlot.Wrists, defense: 1),
            new ItemDef("wood_shield","el escudo de madera","wooden shield","gear_wood_shield",60, ItemKind.Gear, EquipSlot.Secondary, defense: 2),
            new ItemDef("shield",     "el escudo",         "shield",      "gear_shield",    120, ItemKind.Gear, EquipSlot.Secondary, defense: 3),
            new ItemDef("iron_shield","el escudo de hierro","iron shield", "gear_iron_shield",220, ItemKind.Gear, EquipSlot.Secondary, defense: 5),

            // ---- bakery (Panadería)
            new ItemDef("bread",      "el pan",            "bread",       "food_bread",       8, ItemKind.Food, power: 20),
            new ItemDef("baguette",   "la barra de pan",   "baguette",    "food_baguette",   10, ItemKind.Food, power: 25),
            new ItemDef("croissant",  "el cruasán",        "croissant",   "food_croissant",  12, ItemKind.Food, power: 30),
            new ItemDef("muffin",     "la magdalena",      "muffin",      "food_muffin",      9, ItemKind.Food, power: 18),
            new ItemDef("churros",    "los churros",       "churros",     "food_churros",     7, ItemKind.Food, power: 15),
            new ItemDef("ensaimada",  "la ensaimada",      "ensaimada (sweet spiral bun)", "food_ensaimada", 14, ItemKind.Food, power: 28),
            new ItemDef("empanada",   "la empanada",       "empanada (filled pastry)", "food_empanada", 15, ItemKind.Food, power: 35),
            new ItemDef("tart",       "la tarta de manzana","apple tart", "food_tart",       18, ItemKind.Food, power: 40),
            new ItemDef("donut",      "la rosquilla",      "doughnut",    "food_donut",       8, ItemKind.Food, power: 16),
            new ItemDef("sponge",     "el bizcocho",       "sponge cake", "food_sponge",     16, ItemKind.Food, power: 32),
            new ItemDef("cake",       "el pastel",         "cake",        "food_cake",       20, ItemKind.Food, power: 50),

            // ---- candy shop (Dulcería)
            new ItemDef("candy",      "el caramelo",       "sweet",       "food_candy",       3, ItemKind.Food, power: 5),
            new ItemDef("lollipop",   "la piruleta",       "lollipop",    "food_lollipop",    5, ItemKind.Food, power: 8),
            new ItemDef("chocolate",  "el chocolate",      "chocolate",   "food_chocolate",  10, ItemKind.Food, power: 18),
            new ItemDef("cookie",     "la galleta",        "cookie",      "food_cookie",      6, ItemKind.Food, power: 12),
            new ItemDef("gum",        "el chicle",         "chewing gum", "food_gum",         2, ItemKind.Food, power: 2),
            new ItemDef("gummies",    "las gominolas",     "gummy sweets","food_gummies",     4, ItemKind.Food, power: 6),
            new ItemDef("turron",     "el turrón",         "turrón (nougat)", "food_turron", 12, ItemKind.Food, power: 20),
            new ItemDef("cotton_candy","el algodón de azúcar","candy floss","food_cotton_candy",6, ItemKind.Food, power: 8),
            new ItemDef("ice_cream",  "el helado",         "ice cream",   "food_ice_cream",   9, ItemKind.Food, power: 14),
            new ItemDef("marshmallow","la nube",           "marshmallow", "food_marshmallow", 4, ItemKind.Food, power: 6),
            new ItemDef("licorice",   "el regaliz",        "liquorice",   "food_licorice",    3, ItemKind.Food, power: 5),

            // ---- pets (Mascotas)
            new ItemDef("pet_dog",    "el perro",          "dog",         "pet_dog",        150, ItemKind.Pet),
            new ItemDef("pet_cat",    "el gato",           "cat",         "pet_cat",        150, ItemKind.Pet),
            new ItemDef("pet_rabbit", "el conejo",         "rabbit",      "pet_rabbit",     120, ItemKind.Pet),
            new ItemDef("pet_parrot", "el loro",           "parrot",      "pet_parrot",     250, ItemKind.Pet),
            new ItemDef("pet_turtle", "la tortuga",        "turtle",      "pet_turtle",     140, ItemKind.Pet),
            new ItemDef("pet_hamster","el hámster",        "hamster",     "pet_hamster",     90, ItemKind.Pet),

            // ---- found, not sold in shops
            new ItemDef("fish",       "el pez",            "fish",        "food_fish",        0, ItemKind.Food, power: 30, sell: 6),
            new ItemDef("orange",     "la naranja",        "orange",      "food_orange",      0, ItemKind.Food, power: 10, sell: 2),
            new ItemDef("wood",       "la madera",         "wood",        "item_wood",        0, ItemKind.Material, sell: 4),
            new ItemDef("pelt",       "la piel de lobo",   "wolf pelt",   "item_pelt",        0, ItemKind.Material, sell: 15),
        };

        public static readonly string[] Store = { "rod", "rod_fine", "book", "hat", "necklace", "cloak", "vest", "pants", "bracelet", "apple", "cheese", "egg" };
        public static readonly string[] Weapons = { "knife", "pocketknife", "dagger", "shortsword", "sword", "sabre", "rapier", "greatsword",
                                                    "hatchet", "axe", "battleaxe", "shortbow", "bow", "longbow", "crossbow" };
        public static readonly string[] Smithy = { "helmet", "chainmail", "breastplate", "greaves", "bracers", "wood_shield", "shield", "iron_shield" };
        public static readonly string[] Pets = { "pet_dog", "pet_cat", "pet_rabbit", "pet_parrot", "pet_turtle", "pet_hamster" };
        public static readonly string[] Bakery = { "bread", "baguette", "croissant", "muffin", "churros", "ensaimada", "empanada", "tart", "donut", "sponge", "cake" };
        public static readonly string[] CandyShop = { "candy", "lollipop", "chocolate", "cookie", "gum", "gummies", "turron", "cotton_candy", "ice_cream", "marshmallow", "licorice" };

        /// <summary>What each shop sells, by the shop id used for the rooms.</summary>
        public static string[] ForShop(string shop) => shop switch
        {
            "bakery" => Bakery, "candy" => CandyShop, "store" => Store, "weapons" => Weapons,
            "smithy" => Smithy, "pets" => Pets, _ => new string[0],
        };

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
