using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LLG
{
    public enum ItemKind { Rod, Weapon, Gear, Book, Food, Pet, Material, Instrument, Seed, Bait, Tackle }

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
        public int level;      // character level needed to buy it (0 = none)
        public string projectile;  // ranged weapons: what flies (item_arrow, item_stone, item_bullet)
        public int tier;           // rods: the rarest kind of fish they can land (1 common ... 5 legendary)

        public bool Stacks => kind == ItemKind.Food || kind == ItemKind.Material || kind == ItemKind.Seed || kind == ItemKind.Bait;

        public ItemDef(string id, string spanish, string english, string icon, int price, ItemKind kind,
            EquipSlot slot = EquipSlot.None, int power = 0, int defense = 0, int mana = 0, int chop = 1,
            bool ranged = false, int sell = -1, int level = 0, string projectile = "item_arrow", int tier = 0)
        {
            this.id = id; this.spanish = spanish; this.english = english; this.icon = icon; this.price = price;
            this.kind = kind; this.slot = slot; this.power = power; this.defense = defense; this.mana = mana;
            this.chop = chop; this.ranged = ranged; this.level = level; this.projectile = projectile; this.tier = tier;
            sellPrice = sell >= 0 ? sell : System.Math.Max(1, price / 2);
        }
    }

    /// <summary>
    /// Everything that can be bought, found or sold. To add an item: add a line here, add its id to a
    /// shop list below, and put a 16x16 picture in Assets/Resources/Art/Icons/.
    /// </summary>
    public static class ItemDatabase
    {
        static readonly List<ItemDef> All = new List<ItemDef>
        {
            // ---- general store (Tienda)
            // ---- fishing shop (La tienda de pesca). Rods: power = extra seconds when reeling, tier = rarest fish.
            new ItemDef("rod",        "la caña de pescar", "fishing rod", "item_rod",       30, ItemKind.Rod, tier: 1),
            new ItemDef("rod_bamboo", "la caña de bambú",  "bamboo rod",  "item_rod_bamboo", 90, ItemKind.Rod, power: 3, level: 3, tier: 2),
            new ItemDef("rod_carbon", "la caña de carbono","carbon rod",  "item_rod_carbon",260, ItemKind.Rod, power: 6, level: 12, tier: 3),
            new ItemDef("rod_fine",   "la caña mágica",    "magic rod",   "item_rod_fine",  600, ItemKind.Rod, power: 10, level: 25, tier: 4),
            new ItemDef("rod_gold",   "la caña dorada",    "golden rod",  "item_rod_gold", 1500, ItemKind.Rod, power: 14, level: 40, tier: 5),
            new ItemDef("net",        "la red de pesca",   "fishing net", "item_net",       180, ItemKind.Tackle, level: 8),
            new ItemDef("hat_fishing","el sombrero de pescador", "fishing hat", "gear_fishing_hat", 120, ItemKind.Gear, EquipSlot.Hat, defense: 1, level: 5),
            new ItemDef("bait_bread", "las migas de pan",  "bread crumbs","bait_bread",       2, ItemKind.Bait),
            new ItemDef("bait_worm",  "la lombriz",        "worm",        "bait_worm",        4, ItemKind.Bait),
            new ItemDef("bait_shrimp","la gamba",          "shrimp",      "bait_shrimp",      9, ItemKind.Bait, level: 10),
            new ItemDef("bait_squid", "el calamar",        "squid",       "bait_squid",      20, ItemKind.Bait, level: 20),
            // fish (caught; the shop also sells some). price = shop price, sell = what shops pay you
            new ItemDef("fish_boot",     "la bota vieja",   "old boot",    "fish_boot",       0, ItemKind.Material, sell: 1),
            new ItemDef("fish_sardine",  "la sardina",      "sardine",     "fish_sardine",   10, ItemKind.Food, power: 15, sell: 5),
            new ItemDef("fish_anchovy",  "el boquerón",     "anchovy",     "fish_anchovy",   10, ItemKind.Food, power: 12, sell: 5),
            new ItemDef("fish_carp",     "la carpa",        "carp",        "fish_carp",      16, ItemKind.Food, power: 22, sell: 8),
            new ItemDef("fish_trout",    "la trucha",       "trout",       "fish_trout",     20, ItemKind.Food, power: 25, sell: 10),
            new ItemDef("fish_perch",    "la perca",        "perch",       "fish_perch",     28, ItemKind.Food, power: 28, sell: 14),
            new ItemDef("fish_seabass",  "la lubina",       "sea bass",    "fish_seabass",   32, ItemKind.Food, power: 30, sell: 16),
            new ItemDef("fish_salmon",   "el salmón",       "salmon",      "fish_salmon",    40, ItemKind.Food, power: 35, sell: 20),
            new ItemDef("fish_bream",    "la dorada",       "gilt-head bream", "fish_bream", 36, ItemKind.Food, power: 32, sell: 18),
            new ItemDef("fish_crab",     "el cangrejo",     "crab",        "fish_crab",      60, ItemKind.Food, power: 30, sell: 30),
            new ItemDef("fish_cod",      "el bacalao",      "cod",         "fish_cod",       64, ItemKind.Food, power: 40, sell: 32),
            new ItemDef("fish_tuna",     "el atún",         "tuna",        "fish_tuna",      90, ItemKind.Food, power: 50, sell: 45),
            new ItemDef("fish_eel",      "la anguila",      "eel",         "fish_eel",      140, ItemKind.Food, power: 45, sell: 70),
            new ItemDef("fish_octopus",  "el pulpo",        "octopus",     "fish_octopus",  160, ItemKind.Food, power: 55, sell: 80),
            new ItemDef("fish_swordfish","el pez espada",   "swordfish",   "fish_swordfish",180, ItemKind.Food, power: 60, sell: 90),
            new ItemDef("fish_koi",      "la carpa koi",    "koi carp",    "fish_koi",      400, ItemKind.Food, power: 40, sell: 200),
            new ItemDef("fish_rune",     "el pez rúnico",   "rune fish",   "fish_rune",     600, ItemKind.Food, power: 80, sell: 300),
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

            // ---- weapons (Armería). Three trees, each locked behind a character level.
            // blades
            new ItemDef("stick",       "el palo",              "stick",            "item_stick",         25, ItemKind.Weapon, EquipSlot.Primary, power: 2,  level: 5),
            new ItemDef("kitchen_knife","el cuchillo de cocina","kitchen knife",   "item_kitchen_knife", 60, ItemKind.Weapon, EquipSlot.Primary, power: 3,  level: 10),
            new ItemDef("dagger",      "la daga",              "dagger",           "item_dagger",       150, ItemKind.Weapon, EquipSlot.Primary, power: 4,  level: 15),
            new ItemDef("shortsword",  "la espada corta",      "short sword",      "item_shortsword",   300, ItemKind.Weapon, EquipSlot.Primary, power: 6,  level: 20),
            new ItemDef("sword",       "la espada",            "sword",            "item_sword",        550, ItemKind.Weapon, EquipSlot.Primary, power: 8,  level: 25),
            new ItemDef("sabre",       "el sable",             "sabre",            "item_sabre",        900, ItemKind.Weapon, EquipSlot.Primary, power: 10, level: 30),
            new ItemDef("rapier",      "el estoque",           "rapier",           "item_rapier",      1400, ItemKind.Weapon, EquipSlot.Primary, power: 12, level: 35),
            new ItemDef("toledo",      "la espada toledana",   "Toledo sword",     "item_toledo",      2200, ItemKind.Weapon, EquipSlot.Primary, power: 15, level: 40),
            new ItemDef("greatsword",  "el mandoble",          "greatsword",       "item_greatsword",  3200, ItemKind.Weapon, EquipSlot.Primary, power: 18, level: 45),
            new ItemDef("tizona",      "la Tizona",            "Tizona (El Cid's sword)", "item_tizona", 5000, ItemKind.Weapon, EquipSlot.Primary, power: 22, level: 50),
            new ItemDef("colada",      "la Colada",            "Colada (El Cid's second sword)", "item_colada", 8000, ItemKind.Weapon, EquipSlot.Primary, power: 27, level: 60),
            // axes (also chop trees)
            new ItemDef("hatchet",     "el hacha de mano",     "hatchet",          "item_hatchet",      120, ItemKind.Weapon, EquipSlot.Primary, power: 4,  chop: 2, level: 10),
            new ItemDef("axe",         "el hacha",             "axe",              "item_axe",          450, ItemKind.Weapon, EquipSlot.Primary, power: 7,  chop: 3, level: 20),
            new ItemDef("battleaxe",   "el hacha de guerra",   "battle axe",       "item_battleaxe",   1300, ItemKind.Weapon, EquipSlot.Primary, power: 12, chop: 3, level: 35),
            new ItemDef("doubleaxe",   "el hacha doble",       "double axe",       "item_doubleaxe",   4000, ItemKind.Weapon, EquipSlot.Primary, power: 20, chop: 5, level: 50),
            // ranged: bows -> crossbows -> medieval guns -> real guns
            new ItemDef("sling",       "la honda",             "sling",            "item_sling",         50, ItemKind.Weapon, EquipSlot.Primary, power: 2,  ranged: true, level: 5,  projectile: "item_stone"),
            new ItemDef("shortbow",    "el arco corto",        "short bow",        "item_shortbow",     160, ItemKind.Weapon, EquipSlot.Primary, power: 3,  ranged: true, level: 10),
            new ItemDef("bow",         "el arco",              "bow",              "item_bow",          300, ItemKind.Weapon, EquipSlot.Primary, power: 5,  ranged: true, level: 15),
            new ItemDef("longbow",     "el arco largo",        "longbow",          "item_longbow",      700, ItemKind.Weapon, EquipSlot.Primary, power: 8,  ranged: true, level: 25),
            new ItemDef("crossbow",    "la ballesta",          "crossbow",         "item_crossbow",    1100, ItemKind.Weapon, EquipSlot.Primary, power: 11, ranged: true, level: 30),
            new ItemDef("heavycrossbow","la ballesta pesada",  "heavy crossbow",   "item_heavycrossbow",2000, ItemKind.Weapon, EquipSlot.Primary, power: 14, ranged: true, level: 40),
            new ItemDef("arquebus",    "el arcabuz",           "arquebus",         "item_arquebus",    3500, ItemKind.Weapon, EquipSlot.Primary, power: 18, ranged: true, level: 50, projectile: "item_bullet"),
            new ItemDef("flintlock",   "la pistola de chispa", "flintlock pistol", "item_flintlock",   5500, ItemKind.Weapon, EquipSlot.Primary, power: 22, ranged: true, level: 60, projectile: "item_bullet"),
            new ItemDef("musket",      "el mosquete",          "musket",           "item_musket",      8000, ItemKind.Weapon, EquipSlot.Primary, power: 27, ranged: true, level: 70, projectile: "item_bullet"),
            new ItemDef("revolver",    "el revólver",          "revolver",         "item_revolver",   12000, ItemKind.Weapon, EquipSlot.Primary, power: 32, ranged: true, level: 80, projectile: "item_bullet"),
            new ItemDef("rifle",       "el rifle",             "rifle",            "item_rifle",      18000, ItemKind.Weapon, EquipSlot.Primary, power: 40, ranged: true, level: 90, projectile: "item_bullet"),
            // older weapons, kept so existing saves still work (no longer sold)
            new ItemDef("knife",       "el cuchillo",          "knife",            "item_knife",         40, ItemKind.Weapon, EquipSlot.Primary, power: 2),
            new ItemDef("pocketknife", "la navaja",            "pocketknife",      "item_pocketknife",   35, ItemKind.Weapon, EquipSlot.Primary, power: 2),

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

            // ---- instruments (Tienda de música). Better instruments give more time in the music game.
            new ItemDef("tambourine", "la pandereta",      "tambourine",  "item_tambourine", 80, ItemKind.Instrument, power: 2),
            new ItemDef("flute",      "la flauta",         "flute",       "item_flute",     120, ItemKind.Instrument, power: 3),
            new ItemDef("guitar",     "la guitarra",       "guitar",      "item_guitar",    200, ItemKind.Instrument, power: 4),
            new ItemDef("clarinet",   "el clarinete",      "clarinet",    "item_clarinet",  300, ItemKind.Instrument, power: 5),
            new ItemDef("trumpet",    "la trompeta",       "trumpet",     "item_trumpet",   380, ItemKind.Instrument, power: 6),
            new ItemDef("violin",     "el violín",         "violin",      "item_violin",    450, ItemKind.Instrument, power: 7),
        };

        /// <summary>Crops: id, Spanish, English, seed price, sell price, seconds to grow.</summary>
        public static readonly (string id, string es, string en, int seed, int sell, float grow)[] Crops =
        {
            ("wheat", "el trigo", "wheat", 5, 9, 40), ("lettuce", "la lechuga", "lettuce", 8, 15, 50),
            ("tomato", "el tomate", "tomato", 12, 24, 90), ("carrot", "la zanahoria", "carrot", 15, 30, 100),
            ("strawberry", "la fresa", "strawberry", 25, 55, 180), ("grape", "la uva", "grape", 40, 90, 200),
            ("olive", "la aceituna", "olive", 60, 140, 300),
        };

        /// <summary>Armour tiers (Herrería): key, Spanish word, English word, level needed, base price, base defence.</summary>
        public static readonly (string key, string es, string en, int level, int price, int def)[] Tiers =
        {
            ("cloth", "tela", "Cloth", 1, 20, 1), ("leather", "cuero", "Leather", 5, 60, 2), ("mail", "malla", "Chainmail", 15, 180, 3),
            ("iron", "hierro", "Iron", 25, 450, 5), ("toledo", "Toledo", "Toledo steel", 40, 1200, 8), ("rune", "rúnico", "Rune", 60, 3500, 12),
        };

        // Spanish names for each piece in each tier: head, chest, legs, wrists, shield
        static readonly string[,] TierNames =
        {
            { "la capucha de tela", "la túnica de tela", "los pantalones de tela", "las muñequeras de tela", "el escudo de madera" },
            { "el gorro de cuero", "el chaleco de cuero", "los pantalones de cuero", "los brazales de cuero", "el escudo de cuero" },
            { "la capucha de malla", "la cota de malla", "las calzas de malla", "los brazales de malla", "el escudo reforzado" },
            { "el casco de hierro", "la coraza de hierro", "las grebas de hierro", "los brazales de hierro", "el escudo de hierro" },
            { "el yelmo de Toledo", "la coraza de Toledo", "las grebas de Toledo", "los brazales de Toledo", "el escudo de Toledo" },
            { "el yelmo rúnico", "la coraza rúnica", "las grebas rúnicas", "los brazales rúnicos", "el escudo rúnico" },
        };
        static readonly string[] PieceEnglish = { "helmet", "chest armour", "leg armour", "bracers", "shield" };
        static readonly string[] PieceKey = { "head", "chest", "legs", "wrists", "shield" };
        static readonly EquipSlot[] PieceSlot = { EquipSlot.Hat, EquipSlot.Chest, EquipSlot.Legs, EquipSlot.Wrists, EquipSlot.Secondary };
        static readonly float[] PieceDefence = { 1f, 2f, 1.5f, 0.6f, 1.5f };
        static readonly float[] PiecePrice = { 1f, 2f, 1.5f, 0.8f, 1.5f };

        static ItemDatabase()
        {
            for (int t = 0; t < Tiers.Length; t++)
            {
                var tier = Tiers[t];
                for (int p = 0; p < 5; p++)
                {
                    All.Add(new ItemDef("armor_" + tier.key + "_" + PieceKey[p], TierNames[t, p], tier.en + " " + PieceEnglish[p],
                        "armor_" + tier.key + "_" + PieceKey[p], Mathf.RoundToInt(tier.price * PiecePrice[p]), ItemKind.Gear, PieceSlot[p],
                        defense: Mathf.Max(1, Mathf.RoundToInt(tier.def * PieceDefence[p])), level: tier.level));
                }
            }
            foreach (var c in Crops)
            {
                All.Add(new ItemDef("seed_" + c.id, "las semillas de " + c.es.Substring(c.es.IndexOf(' ') + 1), c.en + " seeds",
                    "seed_" + c.id, c.seed, ItemKind.Seed, sell: Mathf.Max(1, c.seed / 2)));
                All.Add(new ItemDef("crop_" + c.id, c.es, c.en, "crop_" + c.id, 0, ItemKind.Food, power: 4 + c.seed / 5, sell: c.sell));
            }
        }

        public static string[] TierItems(int tier)
        {
            var ids = new string[5];
            for (int p = 0; p < 5; p++) ids[p] = "armor_" + Tiers[tier].key + "_" + PieceKey[p];
            return ids;
        }

        public static string[] Seeds()
        {
            var ids = new string[Crops.Length];
            for (int i = 0; i < Crops.Length; i++) ids[i] = "seed_" + Crops[i].id;
            return ids;
        }

        public static readonly string[] Music = { "tambourine", "flute", "guitar", "clarinet", "trumpet", "violin" };

        public static readonly string[] Store = { "book", "hat", "necklace", "cloak", "vest", "pants", "bracelet", "apple", "cheese", "egg" };
        public static readonly string[] Blades = { "stick", "kitchen_knife", "dagger", "shortsword", "sword", "sabre", "rapier", "toledo", "greatsword", "tizona", "colada" };
        public static readonly string[] Axes = { "hatchet", "axe", "battleaxe", "doubleaxe" };
        public static readonly string[] Ranged = { "sling", "shortbow", "bow", "longbow", "crossbow", "heavycrossbow", "arquebus", "flintlock", "musket", "revolver", "rifle" };
        public static readonly string[] Weapons = Blades;

        /// <summary>Shops with several sections (the Armería): Spanish name, English name, items.</summary>
        public static (string es, string en, string[] ids)[] Sections(string shop)
        {
            if (shop == "weapons")
                return new[] { ("Espadas", "Blades", Blades), ("Hachas", "Axes", Axes), ("A distancia", "Ranged", Ranged) };
            if (shop == "fishing")
                return new[] { ("Cañas", "Rods", FishingRods), ("Cebos y equipo", "Bait & gear", FishingGear), ("Pescado", "Fish", FishingFish) };
            if (shop == "smithy")
            {
                var list = new (string, string, string[])[Tiers.Length];
                for (int t = 0; t < Tiers.Length; t++)
                    list[t] = (GameState.Capital(Tiers[t].es), Tiers[t].en, TierItems(t));
                return list;
            }
            return null;
        }
        public static readonly string[] FishingRods = { "rod", "rod_bamboo", "rod_carbon", "rod_fine", "rod_gold" };
        public static readonly string[] FishingGear = { "bait_bread", "bait_worm", "bait_shrimp", "bait_squid", "net", "hat_fishing" };
        public static readonly string[] FishingFish = { "fish_sardine", "fish_anchovy", "fish_carp", "fish_trout", "fish_seabass", "fish_salmon" };
        public static readonly string[] Smithy = { "helmet", "chainmail", "breastplate", "greaves", "bracers", "wood_shield", "shield", "iron_shield" };
        public static readonly string[] Pets = { "pet_dog", "pet_cat", "pet_rabbit", "pet_parrot", "pet_turtle", "pet_hamster" };
        public static readonly string[] Bakery = { "bread", "baguette", "croissant", "muffin", "churros", "ensaimada", "empanada", "tart", "donut", "sponge", "cake" };
        public static readonly string[] CandyShop = { "candy", "lollipop", "chocolate", "cookie", "gum", "gummies", "turron", "cotton_candy", "ice_cream", "marshmallow", "licorice" };

        /// <summary>What each shop sells, by the shop id used for the rooms.</summary>
        public static string[] ForShop(string shop) => shop switch
        {
            "bakery" => Bakery, "candy" => CandyShop, "store" => Store, "weapons" => Weapons,
            "smithy" => TierItems(0), "pets" => Pets, "seeds" => Seeds(), "music" => Music, "fishing" => FishingRods, _ => new string[0],
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

        // ------------------------------------------------------------ the price list for the game guide

        /// <summary>Every shop's items with prices, as Markdown tables (written into Docs/GAME_GUIDE.md).</summary>
        public static string PriceTable()
        {
            var sb = new StringBuilder();
            void Table(string title, IEnumerable<string> ids)
            {
                sb.AppendLine("**" + title + "**").AppendLine();
                sb.AppendLine("| Item | English | Price | Sells for | Level | What it does |");
                sb.AppendLine("|---|---|---|---|---|---|");
                foreach (var id in ids)
                {
                    var i = Get(id);
                    if (i == null) continue;
                    sb.AppendLine("| " + i.spanish + " | " + i.english + " | " + (i.price > 0 ? i.price.ToString() : "—") + " | " + i.sellPrice
                        + " | " + (i.level > 0 ? i.level.ToString() : "—") + " | " + Effect(i) + " |");
                }
                sb.AppendLine();
            }
            Table("Panadería (bakery) — Carmen", Bakery);
            Table("Dulcería (candy shop) — Paco", CandyShop);
            Table("Tienda (general store) — Doña Elena", Store);
            foreach (var s in Sections("weapons")) Table("Armería (weapons) — Rodrigo: " + s.en, s.ids);
            foreach (var s in Sections("smithy")) Table("Herrería (armour) — Bruno: " + s.en, s.ids);
            Table("Tienda de mascotas (pets) — Lucía", Pets);
            Table("Semillería (seeds) — Don Ramón", Seeds());
            Table("Tienda de música (instruments) — Marta", Music);
            foreach (var s in Sections("fishing")) Table("Tienda de pesca (fishing) — Marina: " + s.en, s.ids);
            var found = new List<string>();
            foreach (var f in Fishery.All) found.Add(f.id);
            found.AddRange(new[] { "fish", "orange", "wood", "pelt" });
            foreach (var c in Crops) found.Add("crop_" + c.id);
            Table("Found, caught or grown (sell them in any shop; fish marked with a price are also sold by Marina)", found);
            return sb.ToString();
        }

        static string Effect(ItemDef i)
        {
            var f = Fishery.Get(i.id);
            if (f != null) return Fishery.RarityEn[(int)f.rarity] + (f.water == "any" ? "" : ", " + f.water) + (i.kind == ItemKind.Food ? "; eat: +" + i.power + " hunger" : "");
            switch (i.kind)
            {
                case ItemKind.Food: return "+" + i.power + " hunger";
                case ItemKind.Book: return "+1 intelligence (+1 mana, +5% XP)";
                case ItemKind.Weapon: return i.power + " damage" + (i.chop > 1 ? ", chops ×" + i.chop : "") + (i.ranged ? ", ranged" : "");
                case ItemKind.Gear:
                    return (i.defense > 0 ? "+" + i.defense + " defence" : "") + (i.mana > 0 ? "+" + i.mana + " mana" : "")
                        + (i.id == "hat_fishing" ? "; fish bite 40% sooner, more rare fish" : "");
                case ItemKind.Rod: return "lands up to " + Fishery.RarityEn[i.tier] + " fish" + (i.power > 0 ? ", +" + i.power + " s to reel" : "");
                case ItemKind.Tackle: return "25% chance of a second fish";
                case ItemKind.Bait:
                    return i.id switch
                    {
                        "bait_bread" => "fish bite sooner; more common fish", "bait_worm" => "more uncommon fish, less junk",
                        "bait_shrimp" => "×2 rare fish", _ => "×2.5 epic, ×3 legendary fish",
                    };
                case ItemKind.Pet: return "follows you";
                case ItemKind.Instrument: return "+" + i.power + " s in the music game";
                case ItemKind.Seed:
                    foreach (var c in Crops) if ("seed_" + c.id == i.id) return "grows in " + c.grow + " s into " + c.en + " (sells " + c.sell + ")";
                    return "";
                case ItemKind.Material: return "sell it";
            }
            return "";
        }

        /// <summary>Spanish and English names of the equipment slots.</summary>
        public static readonly string[,] SlotNames =
        {
            { "", "" }, { "Cabeza", "Head" }, { "Cuello", "Neck" }, { "Espalda", "Back" }, { "Pecho", "Chest" },
            { "Piernas", "Legs" }, { "Muñecas", "Wrists" }, { "Mano principal", "Main hand" }, { "Mano secundaria", "Off hand" },
        };
    }
}
