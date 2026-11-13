using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Runs the world of Valdeluz: the village, the farm, the river and wolf forest, and the inside of
    /// the shops and your house. It draws everything, creates the people, animals and monsters, and
    /// handles the controls:
    ///   E (use)        talk, go through doors, fish... whatever you are facing
    ///   B / U / L      bag / profile / lessons
    ///   Left click     read: shows the Spanish name of whatever you click (the menu board opens the shop)
    ///   Right click    attack an animal or chop a tree (the cursor becomes a fist); on a door: go in
    ///   Mouse wheel    zoom in and out
    ///   Esc            menu
    /// Like the start screen, everything is created from code when the game runs.
    /// </summary>
    public class VillageController : MonoBehaviour
    {
        class Villager
        {
            public string Name, Role, RoleEnglish;
            public List<Line> Lines;
            public CharacterSprite Sprite;
        }

        public static VillageController Instance { get; private set; }
        public VillageMap Map { get; private set; }
        public List<Mob> Mobs { get; } = new List<Mob>();
        public Vector2 PlayerPosition => player.transform.position;
        public bool PlayerInVillage => area == null || !area.interior;

        PlayerController player;
        CharacterSprite playerSprite;
        PetFollower pet;
        string petShown = "";
        Camera cam;
        Hud hud;
        DialogueBox dialogue;
        ShopWindow shop;
        BagWindow bag;
        ProfileWindow profile;
        ControlsWindow controls;
        LessonsWindow lessons;
        MapWindow mapWindow;
        MusicGame music;
        DungeonWindow dungeon;
        BattleWindow battle;
        WateringWindow watering;
        Farm farm;
        IntrusoGame intruso;
        ArcheryGame archery;
        FishingGame fishing;
        GameMenu menu;
        OptionsWindow options;
        Trees trees;
        Image fade;
        readonly SpriteRenderer[] curtains = new SpriteRenderer[4];
        VillageLayout layout;
        Dictionary<char, string[]> words;
        List<ClickableObject> clickable;
        readonly Dictionary<Vector2Int, Portal> portals = new Dictionary<Vector2Int, Portal>();
        readonly Dictionary<char, Villager> villagers = new Dictionary<char, Villager>();
        readonly HashSet<string> wordsRead = new HashSet<string>();
        readonly Dictionary<Vector2Int, float> fruitTimers = new Dictionary<Vector2Int, float>();
        Area area;
        Vector2 spawnPoint;
        Portal pendingPortal;
        int closedFrame = -1;
        float hungerTimer, manaTimer, attackCooldown, autosaveTimer, fadeTimer = -1, portalCooldown, eggTimer;
        bool fistCursor;
        bool shieldUp;                                   // Escudo de luz: the next bite is blocked
        bool sitting;                                    // on a bench
        Vector2 standSpot;
        float invulnerableUntil;                         // a moment after a bite when nothing else can hurt you
        readonly Dictionary<string, float> spellReady = new Dictionary<string, float>();
        SpellbookWindow spellbook;
        ChatBox chat;
        MiningGame mining;
        MineRocks rocks;
        CastWindow cast;
        EnchantWindow enchant;
        Texture2D cursorNormal, cursorFist;

        /// <summary>True while a window that needs your full attention is open (talking, shopping,
        /// a minigame, the menu). The bag, profile, lessons and controls windows let you keep walking.</summary>
        public bool Busy => dialogue.Open || shop.Open || archery.Open || fishing.Open || intruso.Open || menu.Open
                            || music.Open || watering.Open || dungeon.Open || battle.Open || cast.Open || enchant.Open || chat.Typing || mining.Open
                            || fadeTimer >= 0 || closedFrame == Time.frameCount;

        /// <summary>Windows call this when they close, so the key that closed them does not also trigger something.</summary>
        public void NoteWindowClosed() => closedFrame = Time.frameCount;

        void Awake()
        {
            Instance = this;
            GameState.Load();
            Map = VillageMap.Load();
            layout = VillageLayout.Load();
            if (!string.IsNullOrEmpty(layout.walkable)) Map.WalkableLetters = layout.walkable;
            words = layout.WordTable();
            if (layout.portals != null)
                foreach (var p in layout.portals) portals[new Vector2Int(p.x, p.y)] = p;

            cam = Camera.main;
            cam.orthographic = true;
            cam.orthographicSize = 5.625f;
            cam.backgroundColor = Color.black;
            cursorNormal = Resources.Load<Texture2D>("Art/Cursor/cursor");
            cursorFist = Resources.Load<Texture2D>("Art/Cursor/cursor_fist");

            var world = new GameObject("World").transform;
            world.SetParent(transform, false);
            clickable = layout.Spawn(world);
            trees = gameObject.AddComponent<Trees>();
            trees.Build(this, clickable);
            rocks = gameObject.AddComponent<MineRocks>();
            rocks.Build(clickable);

            SpawnPlayer();
            SpawnPeople();
            SpawnCreatures(world);

            var canvas = UIFactory.Canvas("VillageCanvas", 0);
            canvas.SetParent(transform, false);
            hud = gameObject.AddComponent<Hud>();
            hud.Build(canvas, OpenMenu, () => controls.Toggle());
            var village = layout.areas != null && layout.areas.Length > 0 ? layout.areas[0] : new Area { w = 112, h = 72 };
            var places = MapPlaces();
            hud.BuildMinimap(canvas, Map, village.w, village.h, player.transform, playerSprite, places, () => mapWindow.Toggle());
            dialogue = Add<DialogueBox>(); dialogue.Build(canvas, this);
            shop = Add<ShopWindow>(); shop.Build(canvas, this);
            bag = Add<BagWindow>(); bag.Build(canvas, this);
            profile = Add<ProfileWindow>(); profile.Build(canvas, this);
            controls = Add<ControlsWindow>(); controls.Build(canvas, this);
            lessons = Add<LessonsWindow>(); lessons.Build(canvas, this);
            mapWindow = Add<MapWindow>(); mapWindow.Build(canvas, this, Map, village.w, village.h, player.transform, playerSprite, places);
            intruso = Add<IntrusoGame>(); intruso.Build(canvas, this);
            archery = Add<ArcheryGame>(); archery.Build(canvas, this);
            fishing = Add<FishingGame>(); fishing.Build(canvas, this);
            mining = Add<MiningGame>(); mining.Build(canvas, this);
            music = Add<MusicGame>(); music.Build(canvas, this);
            battle = Add<BattleWindow>(); battle.Build(canvas, this);
            dungeon = Add<DungeonWindow>(); dungeon.Build(canvas, this, battle);
            watering = Add<WateringWindow>(); watering.Build(canvas, this);
            cast = Add<CastWindow>(); cast.Build(canvas, this);
            chat = Add<ChatBox>(); chat.Build(canvas, this, player.transform);
            spellbook = Add<SpellbookWindow>(); spellbook.Build(canvas, this, cast);
            enchant = Add<EnchantWindow>(); enchant.Build(canvas, this);
            farm = Add<Farm>(); farm.Build(Map, world, watering);
            options = Add<OptionsWindow>(); options.Build(canvas);
            menu = Add<GameMenu>(); menu.Build(canvas, this, options);
            fade = UIFactory.Img("Fade", canvas, Vector2.zero, new Vector2(2000, 2000), new Color(0, 0, 0, 0));

            GameState.InventoryChanged += RefreshPlayerLook;
            RefreshPlayerLook();
            UpdateArea(true);
            FollowPlayer();
            string playerName = PlayerProfile.Current.playerName;
            dialogue.Show("", "¡Hola, " + playerName + "! Bienvenido a " + GameConfig.TownName + ".",
                "Hello, " + playerName + "! Welcome to " + GameConfig.TownName + ".");
        }

        T Add<T>() where T : Component => gameObject.AddComponent<T>();

        void OnDestroy()
        {
            GameState.InventoryChanged -= RefreshPlayerLook;
            if (cursorNormal != null) Cursor.SetCursor(cursorNormal, new Vector2(1, 1), CursorMode.Auto);
        }

        // ------------------------------------------------------------ setup

        void SpawnPlayer()
        {
            var start = Map.Find('P');
            var tile = start.Count > 0 ? start[0] : new Vector2Int(Map.Width / 2, Map.Height / 2);
            Map.Set(tile, ':');
            spawnPoint = VillageMap.Center(tile);

            var go = new GameObject("Player");
            go.transform.SetParent(transform, false);
            go.transform.position = spawnPoint;
            playerSprite = go.AddComponent<CharacterSprite>();
            playerSprite.Build(PlayerProfile.Current.appearance, CharacterLayers.FromEquipment());
            player = go.AddComponent<PlayerController>();
            player.Init(this, playerSprite);

            // continue from where you were when you last saved
            var data = GameState.Data;
            var saved = new Vector2(data.posX, data.posY);
            if (data.hasPosition && player.Free(saved))
            {
                go.transform.position = saved;
                playerSprite.Face(Mathf.Clamp(data.facing, 0, 3));
            }
        }

        /// <summary>Hat/cloak you wear appear on your character, and your chosen pet follows you.</summary>
        void RefreshPlayerLook()
        {
            if (playerSprite == null) return;
            playerSprite.SetAccessories(CharacterLayers.FromEquipment());
            if (hud != null) hud.RefreshPortrait();
            string wanted = GameState.Has(GameState.Data.pet) ? GameState.Data.pet : "";
            if (wanted == petShown) return;
            if (pet != null) Destroy(pet.gameObject);
            pet = string.IsNullOrEmpty(wanted) ? null : PetFollower.Spawn(transform, player.transform, wanted);
            petShown = wanted;
        }

        void SpawnCreatures(Transform parent)
        {
            if (layout.spawns == null) return;
            foreach (var s in layout.spawns)
            {
                var at = VillageMap.Center(new Vector2Int(s.x, s.y));
                if (s.kind == "wolf" || s.kind == "wilddog") Mobs.Add(Mob.Spawn(this, parent, s.kind, at));
                else Wanderer.Spawn(this, parent, s.kind, at, s.room ? 1.5f : s.kind == "chicken" ? 1.8f : 3f);
            }
        }

        static CharacterAppearance Look(int skin, int hairStyle, int hairColor, int shirt, int pants)
        {
            return new CharacterAppearance { skin = skin, hairStyle = hairStyle, hairColor = hairColor, shirt = shirt, pants = pants };
        }

        static Accessory Acc(string kind, byte r, byte g, byte b) => new Accessory(kind, new Color32(r, g, b, 255));

        static Line L(string who, string es, string en) => new Line(who, es, en);

        void SpawnPeople()
        {
            string money = GameConfig.CurrencyName;
            AddVillager('1', "Abuela Rosa", "la abuela", "Grandmother", Look(2, 5, 5, 7, 2), new[] { Acc("shawl", 176, 40, 50), Acc("skirt", 52, 46, 54) },
                L("Abuela Rosa", "¡Hola, cariño! Bienvenido a Valdeluz.", "Hello, dear! Welcome to Valdeluz."),
                L("Abuela Rosa", "Puedes entrar en las tiendas. ¡Prueba la panadería!", "You can go inside the shops. Try the bakery!"),
                L("Abuela Rosa", "Si tienes hambre, come algo de tu mochila.", "If you are hungry, eat something from your bag."));
            AddVillager('2', "Mateo", "el pescador", "Fisherman", Look(3, 0, 0, 1, 1), new[] { Acc("hat", 200, 168, 108), Acc("beard", 74, 50, 36) },
                L("Mateo", "Buenos días. Soy Mateo, el pescador.", "Good morning. I am Mateo, the fisherman."),
                L("Mateo", "Necesitas una caña de pescar para pescar.", "You need a fishing rod to fish."),
                L("Mateo", "Compra una en la tienda de pesca de Marina, al norte del estanque.", "Buy one at Marina's fishing shop, north of the pond."),
                L("Mateo", "Cuando el corcho se hunda, ¡tira rápido!", "When the bobber sinks, strike fast!"));
            AddVillager('3', "Luna", "la niña", "Girl", Look(0, 3, 3, 4, 0), new[] { Acc("skirt", 196, 60, 76) },
                L("Luna", "¡Me encantan los caramelos!", "I love sweets!"),
                L("Luna", "¿Has visto los gatos del pueblo? ¡Son muy simpáticos!", "Have you seen the village cats? They are very friendly!"));
            AddVillager('4', "Don Felipe", "el arquero", "Archer", Look(4, 4, 4, 2, 1), new[] { Acc("cloak", 60, 100, 64), Acc("beard", 172, 172, 178), Acc("hat", 50, 46, 52) },
                L("Don Felipe", "Soy Don Felipe, el maestro de tiro con arco.", "I am Don Felipe, the archery master."),
                L("Don Felipe", "Dispara a la palabra correcta.", "Shoot the correct word."));
            AddVillager('5', "Sabia", "la sabia", "Wise Woman", Look(1, 1, 5, 5, 5), new[] { Acc("cloak", 90, 50, 140), Acc("skirt", 110, 70, 160) },
                L("Sabia", "Las runas de la fuente guardan el poder de las palabras.", "The fountain's runes hold the power of words."),
                L("Sabia", "Mira el tablón de lecciones para elegir qué aprender.", "Look at the lesson board to choose what to learn."),
                L("Sabia", "Aprende palabras y ganarás " + money + ".", "Learn words and you will earn " + money + "."));
            AddVillager('%', "Tomás", "el leñador", "Woodcutter", Look(3, 0, 3, 0, 1), new[] { Acc("beard", 150, 70, 40) },
                L("Tomás", "Soy Tomás, el leñador. Corto madera en el bosque.", "I'm Tomás, the woodcutter. I cut wood in the forest."),
                L("Tomás", "Con un hacha cortas los árboles mucho más rápido.", "With an axe you cut trees much faster."),
                L("Tomás", "¡Cuidado! Hay lobos al otro lado del río.", "Careful! There are wolves across the river."),
                L("Tomás", "¿Jugamos a El intruso? Busca la palabra que no pertenece.", "Shall we play The Odd One Out? Find the word that doesn't belong."));
            AddVillager('&', "Juana", "la granjera", "Farmer", Look(1, 1, 2, 3, 3), new[] { Acc("hat", 200, 168, 108), Acc("skirt", 110, 80, 60) },
                L("Juana", "Esta es mi granja. Cultivo trigo, coles y tomates.", "This is my farm. I grow wheat, cabbages and tomatoes."),
                L("Juana", "Las gallinas ponen huevos. ¡Busca en el gallinero!", "The hens lay eggs. Look in the chicken coop!"));
            // shopkeepers, inside their shops
            AddVillager('6', "Carmen", "la panadera", "Baker", Look(1, 5, 1, 4, 3), new[] { Acc("apron", 240, 236, 226) },
                L("Carmen", "¡Buenos días! Tenemos pan recién hecho.", "Good morning! We have freshly baked bread."));
            AddVillager('7', "Paco", "el dulcero", "Sweet Seller", Look(2, 0, 2, 0, 0), new[] { Acc("apron", 240, 170, 200) },
                L("Paco", "¡Hola! ¿Te gustan los dulces?", "Hi! Do you like sweets?"));
            AddVillager('8', "Doña Elena", "la tendera", "Shopkeeper", Look(0, 1, 4, 5, 5), new[] { Acc("shawl", 70, 110, 170), Acc("skirt", 60, 54, 70) },
                L("Doña Elena", "Bienvenido a la tienda. Tenemos de todo.", "Welcome to the shop. We have a bit of everything."));
            AddVillager('9', "Rodrigo", "el armero", "Weaponsmith", Look(3, 4, 0, 7, 2), new[] { Acc("apron", 120, 80, 50), Acc("beard", 50, 36, 30) },
                L("Rodrigo", "Aquí tienes las mejores armas del pueblo.", "Here are the best weapons in the village."));
            AddVillager('@', "Bruno", "el herrero", "Blacksmith", Look(4, 4, 0, 7, 1), new[] { Acc("apron", 90, 60, 40), Acc("beard", 40, 30, 26) },
                L("Bruno", "Soy el herrero. Hago armaduras de hierro.", "I am the blacksmith. I make iron armour."));
            AddVillager('$', "Lucía", "la cuidadora", "Animal Keeper", Look(2, 3, 1, 2, 1), new Accessory[0],
                L("Lucía", "¡Hola! Todos estos animales buscan un hogar.", "Hello! All these animals are looking for a home."));
            AddVillager('*', "Don Ramón", "el semillero", "Seed Seller", Look(4, 4, 5, 2, 1), new[] { Acc("hat", 200, 168, 108), Acc("apron", 110, 150, 70) },
                L("Don Ramón", "Planta las semillas en tu huerto y riégalas.", "Plant the seeds in your patch and water them."),
                L("Don Ramón", "La aceituna tarda mucho, pero se vende muy cara.", "Olives take a long time, but they sell for a lot."));
            AddVillager('/', "Marta", "la vendedora de música", "Music Seller", Look(1, 1, 6, 5, 2), new[] { Acc("skirt", 90, 60, 120) },
                L("Marta", "¿Buscas un instrumento? ¡Tenemos de todo!", "Looking for an instrument? We have everything!"));
            AddVillager('!', "Profesora Isabel", "la profesora de música", "Music Teacher", Look(0, 5, 1, 1, 2), new[] { Acc("shawl", 60, 80, 150), Acc("skirt", 50, 50, 70) },
                L("Profesora Isabel", "¡Bienvenido a la escuela de música!", "Welcome to the music school!"),
                L("Profesora Isabel", "Forma el verso con las palabras y la música sonará.", "Build the verse with the words and the music will play."));
            AddVillager('ĥ', "Don Ernesto", "el minero", "Miner", Look(3, 4, 4, 7, 2), new[] { Acc("hat", 230, 190, 60), Acc("beard", 90, 80, 76) },
                L("Don Ernesto", "¡Buenas! Esta es la mina. Dentro hay rocas, minerales y gemas.", "Hello! This is the mine. Inside there are rocks, ores and gems."),
                L("Don Ernesto", "Para picar, deletrea las palabras. ¡Y a veces se encuentran reliquias antiguas!", "To mine, spell the words. And sometimes you find ancient relics!"),
                L("Don Ernesto", "¿Quieres un pico?", "Want a pickaxe?"));
            AddVillager('§', "Marina", "la pescadera", "Fishmonger", Look(2, 1, 1, 1, 0), new[] { Acc("hat", 150, 160, 106), Acc("apron", 70, 120, 160) },
                L("Marina", "¡Hola! Tengo cañas, cebos y pescado fresco.", "Hello! I have rods, bait and fresh fish."),
                L("Marina", "Con mejor caña y mejor cebo, pescas peces más raros.", "With a better rod and better bait you catch rarer fish."));
            AddVillager('Ø', "Doña Runa", "la encantadora", "Enchantress", Look(1, 1, 7, 5, 2), new[] { Acc("cloak", 50, 70, 140), Acc("hat", 40, 50, 110), Acc("skirt", 60, 60, 120) },
                L("Doña Runa", "Bienvenido a la torre rúnica.", "Welcome to the Rune Tower."),
                L("Doña Runa", "Con Lexa, las runas encantan tus armas y armaduras.", "With Lexa, the runes enchant your weapons and armour."));
        }

        void AddVillager(char marker, string villagerName, string role, string roleEnglish, CharacterAppearance look,
            Accessory[] worn, params Line[] lines)
        {
            var tiles = Map.Find(marker);
            if (tiles.Count == 0) return;
            var go = new GameObject(villagerName);
            go.transform.SetParent(transform, false);
            go.transform.position = VillageMap.Center(tiles[0]) + new Vector2(0, -0.2f);
            var sprite = go.AddComponent<CharacterSprite>();
            sprite.Build(look, new List<Accessory>(worn));
            villagers[marker] = new Villager { Name = villagerName, Role = role, RoleEnglish = roleEnglish, Lines = new List<Line>(lines), Sprite = sprite };
        }

        /// <summary>Icons on the minimap: shops, the pond, the archery range, the fountain, wolves, villagers.</summary>
        List<(Vector2Int, string)> MapPlaces()
        {
            var icons = new Dictionary<char, string>
            {
                { 'B', "Art/Icons/food_bread" }, { 'C', "Art/Icons/food_candy" }, { 'S', "Art/Icons/item_rod" },
                { 'W', "Art/Icons/item_sword" }, { 'M', "Art/Icons/pet_dog" }, { 'H', "Art/UI/hp_heart" },
                { '^', "Art/Icons/gear_helmet" }, { 'n', "Art/UI/page" }, { 'o', "Art/Icons/food_egg" },
                { 'Ð', "Art/Bosses/guardian" }, { ']', "Art/Icons/seed_tomato" }, { ')', "Art/Icons/item_guitar" }, { '\'', "Art/Icons/item_flute" }, { 'Þ', "Art/Icons/spell_lightning" }, { 'Œ', "Art/Icons/item_rod" }, { 'Ŭ', "Art/Icons/item_pickaxe" },
            };
            var list = new List<(Vector2Int, string)>();
            foreach (var kv in icons)
            {
                var found = Map.Find(kv.Key);
                if (found.Count > 0 && found[0].x < 112) list.Add((found[0], kv.Value));
            }
            var fountain = Map.Find('R');
            if (fountain.Count > 0) list.Add((fountain[0] + new Vector2Int(1, 0), "Art/UI/mana_drop"));
            var targets = Map.Find('A');
            if (targets.Count > 0) list.Add((targets[targets.Count / 2] + new Vector2Int(0, 2), "Art/UI/target"));
            var water = Map.Find('F');
            if (water.Count > 0) list.Add((water[water.Count / 2], "Art/Icons/food_fish"));
            if (layout.spawns != null)
                foreach (var s in layout.spawns)
                    if (s.kind == "wolf") list.Add((new Vector2Int(s.x, s.y), "Art/Mobs/wolf_0"));
            foreach (var marker in villagers.Keys)
            {
                var t = Map.Find(marker)[0];
                if (t.x < 112) list.Add((t, "Art/UI/map_dot"));
            }
            return list;
        }

        // ------------------------------------------------------------ every frame

        void Update()
        {
            var stats = GameState.Data.stats;

            hungerTimer += Time.deltaTime * (player.Running ? 2f : 1f);     // running makes you hungry faster
            if (hungerTimer >= 8f) { hungerTimer = 0; stats.GetHungrier(1); }
            manaTimer += Time.deltaTime;
            if (manaTimer >= 20f) { manaTimer = 0; stats.RestoreMana(1); }
            attackCooldown -= Time.deltaTime;
            portalCooldown -= Time.deltaTime;
            eggTimer -= Time.deltaTime;
            autosaveTimer += Time.unscaledDeltaTime;
            if (autosaveTimer >= 30f) { autosaveTimer = 0; SaveProgress(); }
            UpdateFade();
            UpdateArea(false);

            if (Busy)
            {
                hud.SetPrompt(null);
                SetFist(false);
                return;
            }

            if (stats.IsDown)
            {
                Faint();
                return;
            }

            // zoom with the mouse wheel
            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0 && !PointerOverUI()) cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * 0.6f, 3f, 9f);

            // stepping onto a doormat takes you outside
            var feetTile = VillageMap.TileAt(player.transform.position);
            if (Map.At(feetTile) == 'X' && portals.TryGetValue(feetTile, out var exit) && portalCooldown <= 0)
            {
                Teleport(exit);
                return;
            }

            Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
            bool overUI = PointerOverUI();
            SetFist(!overUI && (MobAt(mouse) != null || TreeAt(mouse) != null));

            char facing = Map.At(player.FacingTile);
            hud.SetPrompt(sitting ? "Levantarse" : Describe(facing));

            if (Input.GetKeyDown(KeyCode.Escape))
                OpenMenu();
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                chat.Open();
            else if (Keybinds.Down(GameAction.Use))
                Interact(facing, player.FacingTile);
            else if (Keybinds.Down(GameAction.Backpack) && !bag.Open)
                bag.Toggle();
            else if (Keybinds.Down(GameAction.Profile) && !profile.Open)
                profile.Toggle();
            else if (Keybinds.Down(GameAction.Lessons) && !lessons.Open)
                lessons.Toggle();
            else if (Keybinds.Down(GameAction.Map) && !mapWindow.Open)
                mapWindow.Toggle();
            else if (Keybinds.Down(GameAction.Spells) && !spellbook.Open)
                spellbook.Toggle();
            else if (Input.GetMouseButtonDown(1) && !overUI)
                RightClick(mouse);
            else if (Input.GetMouseButtonDown(0) && !overUI)
                Read(mouse);
        }

        void OpenMenu()
        {
            if (Busy) return;
            hud.HideWordLabel();
            bag.Close();
            profile.Close();
            controls.Close();
            lessons.Close();
            mapWindow.Close();
            closedFrame = -1;
            menu.Show();
        }

        void SetFist(bool on)
        {
            if (on == fistCursor) return;
            fistCursor = on;
            var tex = on ? cursorFist : cursorNormal;
            if (tex != null) Cursor.SetCursor(tex, on ? new Vector2(12, 12) : new Vector2(1, 1), CursorMode.Auto);
        }

        static bool PointerOverUI() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        void LateUpdate() => FollowPlayer();

        /// <summary>The camera follows you but never shows past the edge of the area you are in.</summary>
        void FollowPlayer()
        {
            float halfH = cam.orthographicSize, halfW = halfH * cam.aspect;
            var a = area ?? new Area { w = Map.Width, h = Map.Height };
            float x0 = a.x, x1 = a.x + a.w, y0 = -(a.y + a.h), y1 = -a.y;
            var p = player.transform.position;
            float x = x1 - x0 <= halfW * 2 ? (x0 + x1) / 2 : Mathf.Clamp(p.x, x0 + halfW, x1 - halfW);
            float y = y1 - y0 <= halfH * 2 ? (y0 + y1) / 2 : Mathf.Clamp(p.y + 0.8f, y0 + halfH, y1 - halfH);
            cam.transform.position = new Vector3(x, y, -10);
        }

        void UpdateArea(bool force)
        {
            if (layout.areas == null) return;
            var tile = VillageMap.TileAt(player.transform.position);
            if (!force && area != null && area.Contains(tile)) return;
            foreach (var a in layout.areas)
            {
                if (!a.Contains(tile)) continue;
                area = a;
                hud?.SetArea(a);
                PlaceCurtains(a);
                return;
            }
        }

        /// <summary>Inside a building, black curtains hide everything around the room.</summary>
        void PlaceCurtains(Area a)
        {
            for (int i = 0; i < 4; i++)
            {
                if (curtains[i] == null)
                {
                    curtains[i] = new GameObject("Curtain").AddComponent<SpriteRenderer>();
                    curtains[i].transform.SetParent(transform, false);
                    curtains[i].sprite = WorldFx.Square;
                    curtains[i].color = Color.black;
                    curtains[i].sortingOrder = 29990;
                }
                curtains[i].enabled = a.interior;
            }
            if (!a.interior) return;
            const float big = 60;
            float x0 = a.x, x1 = a.x + a.w, y0 = -(a.y + a.h), y1 = -a.y, cx = (x0 + x1) / 2, cy = (y0 + y1) / 2;
            SetCurtain(curtains[0], new Vector2(x0 - big / 2, cy), new Vector2(big, big * 2));
            SetCurtain(curtains[1], new Vector2(x1 + big / 2, cy), new Vector2(big, big * 2));
            SetCurtain(curtains[2], new Vector2(cx, y1 + big / 2), new Vector2(a.w, big));
            SetCurtain(curtains[3], new Vector2(cx, y0 - big / 2), new Vector2(a.w, big));
        }

        static void SetCurtain(SpriteRenderer r, Vector2 pos, Vector2 size)
        {
            r.transform.position = pos;
            r.transform.localScale = new Vector3(size.x, size.y, 1);
        }

        // ------------------------------------------------------------ doors

        void Teleport(Portal p)
        {
            pendingPortal = p;
            fadeTimer = 0;
            portalCooldown = 0.8f;
            hud.HideWordLabel();
        }

        void UpdateFade()
        {
            if (fadeTimer < 0) return;
            fadeTimer += Time.unscaledDeltaTime;
            const float half = 0.18f;
            if (pendingPortal != null && fadeTimer >= half)
            {
                player.transform.position = VillageMap.Center(new Vector2Int(pendingPortal.tx, pendingPortal.ty));
                playerSprite.Face(pendingPortal.face);
                if (pet != null) pet.transform.position = player.transform.position + new Vector3(-0.8f, 0.45f, 0);
                pendingPortal = null;
                UpdateArea(true);
                FollowPlayer();
            }
            fade.color = new Color(0, 0, 0, fadeTimer < half ? fadeTimer / half : Mathf.Clamp01(2 - fadeTimer / half));
            if (fadeTimer >= half * 2)
            {
                fadeTimer = -1;
                fade.color = new Color(0, 0, 0, 0);
            }
        }

        void Faint()
        {
            var stats = GameState.Data.stats;
            int lost = stats.gold / 10;
            stats.AddGold(-lost);
            stats.Heal(stats.maxHp / 2);
            player.transform.position = spawnPoint;
            foreach (var m in Mobs) m.Calm();
            UpdateArea(true);
            dialogue.Show("", "Te desmayaste... Despiertas en la plaza. (-" + lost + " " + GameConfig.GoldName + ")",
                "You fainted and woke up in the town square.");
            GameState.Save();
        }

        // ------------------------------------------------------------ E: use

        string Describe(char what)
        {
            switch (what)
            {
                case 'B': return "Entrar en la panadería";
                case 'C': return "Entrar en la dulcería";
                case 'S': return "Entrar en la tienda";
                case 'W': return "Entrar en la armería";
                case '^': return "Entrar en la herrería";
                case 'M': return "Entrar en la tienda de mascotas";
                case 'H': return "Entrar en tu casa";
                case ']': return "Entrar en la semillería";
                case ')': return "Entrar en la tienda de música";
                case '\'': return "Entrar en la escuela de música";
                case 'ñ': return "Tocar el piano";
                case 'Ð': return "Entrar en la Ciudadela del Sol";
                case 'Þ': return "Entrar en la torre rúnica";
                case 'Œ': return "Entrar en la tienda de pesca";
                case 'Ŭ': return "Entrar en la mina";
                case 'ŕ': case 'Ř': case 'ř': return rocks.IsBroken(player.FacingTile, what) ? null : "Picar " + Mine.RockEs[(int)Mine.KindOf(what)];
                case 'ĉ': return "Mirar la vagoneta";
                case 'e': return "Sentarse en el banco";
                case 'ø': return "Encantar (el altar rúnico)";
                case 'ç': return "Abrir la puerta";
                case 'Ç': return "Cerrar la puerta";
                case '+': return farm.Describe(player.FacingTile);
                case 'D': return "Llamar a la puerta";
                case 'I': case '<': case '>': return "Abrir la puerta";
                case 'A': return "Tiro con arco";
                case 'F': case 'V': return "Pescar";
                case 'R': return "Tocar la fuente rúnica";
                case 'K': case 'Q': return "Comprar";
                case 'J': return "Dormir";
                case 'n': return "Mirar las lecciones";
                case 'o': return "Buscar huevos";
                case 'g': return "Leer el letrero";
            }
            return villagers.TryGetValue(what, out var villager) ? "Hablar con " + villager.Name : null;
        }

        /// <summary>Opens the shop of the room you are in (after the shopkeeper says hello).</summary>
        void OpenRoomShop(bool greet)
        {
            if (area == null || string.IsNullOrEmpty(area.shop)) return;
            void open() => shop.Show(GameState.Capital(area.es), area.en, ItemDatabase.ForShop(area.shop), ItemDatabase.Sections(area.shop));
            if (greet && !string.IsNullOrEmpty(area.keeper) && villagers.TryGetValue(area.keeper[0], out var keeper))
            {
                keeper.Sprite.Face(0);
                dialogue.Show(keeper.Lines, open);
            }
            else open();
        }

        /// <summary>Does whatever the given map letter stands for. Public so tools/tests can trigger it.</summary>
        public void Interact(char what, Vector2Int tile)
        {
            if (sitting) { StandUp(); return; }
            if (portals.TryGetValue(tile, out var door) && "BCSWM^H])'ÞŒŬ".IndexOf(what) >= 0)
            {
                Teleport(door);
                return;
            }
            switch (what)
            {
                case 'D': dialogue.Show("", "No hay nadie en casa.", "Nobody is home."); break;
                case 'I': dialogue.Show("", "La iglesia está cerrada hoy.", "The church is closed today."); break;
                case '<': dialogue.Show("", "El ayuntamiento está cerrado. Vuelve mañana.", "The town hall is closed. Come back tomorrow."); break;
                case '>': dialogue.Show("", "La taberna abre por la noche.", "The tavern opens at night."); break;
                case 'A': archery.Begin(); break;
                case 'F':
                case 'V':
                    if (Fishery.HasRod()) fishing.Begin(what == 'V');
                    else dialogue.Show("", "Necesitas una caña de pescar. Cómprala en la tienda de pesca, junto al estanque.",
                        "You need a fishing rod. Buy one at the fishing shop by the pond.");
                    break;
                case 'R':
                    var stats = GameState.Data.stats;
                    stats.RestoreMana(stats.maxMana);
                    dialogue.Show("", "Las runas brillan. Tu maná vuelve.", "The runes glow. Your mana returns.");
                    break;
                case 'K': OpenRoomShop(true); break;
                case 'Q': OpenRoomShop(false); break;
                case 'J':
                    var s = GameState.Data.stats;
                    s.Heal(s.maxHp);
                    s.RestoreMana(s.maxMana);
                    SaveProgress();
                    dialogue.Show("", "Duermes un rato... ¡Te sientes como nuevo!", "You sleep for a while... You feel brand new!");
                    break;
                case 'G': dialogue.Show("", "La chimenea calienta la casa.", "The fireplace warms the house."); break;
                case 'E': dialogue.Show("", "¡Qué calor hace aquí!", "It's so hot in here!"); break;
                case 'n': lessons.Toggle(); break;
                case '+': farm.Use(tile); break;
                case 'ñ': music.Begin(true); break;
                case 'Ð': dungeon.Toggle(); break;
                case 'ø': enchant.Begin(); break;
                case 'e': SitOn(tile); break;
                case 'ŕ': case 'Ř': case 'ř': MineRock(what, tile); break;
                case 'ĉ': dialogue.Show("", "La vagoneta está llena de piedras.", "The mine cart is full of stones."); break;
                case 'ç':
                case 'Ç': ToggleGate(tile); break;
                case 'g': dialogue.Show("", "Al este: el bosque. ¡Cuidado con los lobos!", "East: the forest. Beware of the wolves!"); break;
                case 'l': dialogue.Show("", "El agua del pozo está muy fría.", "The water in the well is very cold."); break;
                case 'y': dialogue.Show("", "El molino muele el trigo para hacer harina.", "The windmill grinds wheat into flour."); break;
                case '{': dialogue.Show("", "Las uvas todavía no están maduras.", "The grapes aren't ripe yet."); break;
                case 'o':
                    if (eggTimer > 0) GameState.Say("No hay más huevos. Vuelve más tarde.");
                    else if (GameState.AddItem("egg")) { eggTimer = 60; GameState.Say("¡Un huevo! Está en tu mochila."); }
                    break;
                default:
                    if (villagers.TryGetValue(what, out var villager))
                    {
                        villager.Sprite.Face((playerSprite.Facing + 2) % 4);     // turn to face the player
                        if (what == '4') dialogue.Show(villager.Lines, archery.Begin);
                        else if (what == '%') dialogue.Show(villager.Lines, StartIntruso);
                        else if (what == '!') dialogue.Show(villager.Lines, () => music.Begin(false));
                        else if (what == 'ĥ') dialogue.Show(villager.Lines, () => shop.Show("La mina", "The Mine", ItemDatabase.MineShop, null));
                        else if (what == 'Ø') dialogue.Show(villager.Lines, enchant.Begin);
                        else if (area != null && area.interior && area.keeper == what.ToString()) OpenRoomShop(true);
                        else dialogue.Show(villager.Lines);
                    }
                    break;
            }
        }

        public void StartIntruso() => intruso.Begin();

        public bool Sitting => sitting;

        /// <summary>E on a rock in the mine: needs a pickaxe strong enough for it.</summary>
        void MineRock(char what, Vector2Int tile)
        {
            var kind = Mine.KindOf(what);
            var pick = Mine.BestPick();
            if (rocks.IsBroken(tile, what)) { GameState.Say("Ya está picada. Vuelve en un rato. (Already mined - it comes back soon.)"); return; }
            if (pick == null)
            {
                dialogue.Show("", "Necesitas un pico. Don Ernesto los vende en la entrada de la mina.",
                    "You need a pickaxe. Don Ernesto sells them at the mine entrance.");
                return;
            }
            if (pick.tier < Mine.TierNeeded(kind))
            {
                string need = ItemDatabase.Get(Mine.Picks[Mine.TierNeeded(kind) - 1]).spanish;
                dialogue.Show("", "Esta roca es muy dura. Necesitas " + need + ".", "This rock is too hard. You need a better pickaxe.");
                return;
            }
            mining.Begin(kind, tile, t => rocks.Break(t, what));
        }

        /// <summary>Sit down on a bench (benches are two tiles wide): E or any movement key gets you up.</summary>
        void SitOn(Vector2Int tile)
        {
            var left = Map.At(tile + Vector2Int.left) == 'e' ? tile + Vector2Int.left : tile;
            var center = VillageMap.Center(left) + new Vector2(0.5f, 0);
            standSpot = player.transform.position;
            player.transform.position = center + new Vector2(0, -0.42f);     // just in front of the seat, so you sit on it
            playerSprite.Face(0);
            playerSprite.Sitting = true;
            sitting = true;
            GameState.Data.stats.Heal(1);
            GameState.Say("Te sientas en el banco. ¡Qué tranquilo!");
        }

        public void StandUp()
        {
            if (!sitting) return;
            sitting = false;
            playerSprite.Sitting = false;
            player.transform.position = standSpot;
        }

        /// <summary>Opens or closes a fence gate (like a Minecraft fence gate). It won't close on you.</summary>
        void ToggleGate(Vector2Int tile)
        {
            bool open = Map.At(tile) == 'ç';
            if (!open && VillageMap.TileAt(player.transform.position) == tile) return;
            Map.Set(tile, open ? 'Ç' : 'ç');
            foreach (var c in clickable)
            {
                if (c.Tile != tile || (c.letter != 'ç' && c.letter != 'Ç')) continue;
                c.letter = open ? 'Ç' : 'ç';
                c.renderer.sprite = UIFactory.LoadSprite("Art/World/Objects/" + (open ? "gate_open" : "gate_closed"));
            }
            GameState.Say(open ? "Abres la puerta." : "Cierras la puerta.");
        }

        /// <summary>Shows a one-line message from someone (or from the game when "who" is empty).</summary>
        public void SayLine(string who, string spanish, string english) => dialogue.Show(who, spanish, english);

        // ------------------------------------------------------------ left click: read

        /// <summary>Shows the Spanish name of whatever is at this world position (people first, then the
        /// object drawn on top there, then the ground). Reading a word for the first time gives Reading XP.
        /// Clicking a shop's menu board opens the shop.</summary>
        public void Read(Vector2 world)
        {
            string word = null, english = null, speak = null;
            Vector3 anchor = world;

            var feet = (Vector2)player.transform.position;
            if (Mathf.Abs(world.x - feet.x) < 0.5f && world.y > feet.y - 0.2f && world.y < feet.y + 1.9f)
            {
                word = PlayerProfile.Current.playerName;
                english = "You";
                anchor = feet + new Vector2(0, 2f);
            }
            if (word == null)
            {
                foreach (var v in villagers.Values)
                {
                    var p = (Vector2)v.Sprite.transform.position;
                    if (Mathf.Abs(world.x - p.x) < 0.5f && world.y > p.y - 0.2f && world.y < p.y + 1.9f)
                    {
                        word = v.Name + " - " + v.Role;
                        english = v.RoleEnglish;
                        speak = v.Role;
                        anchor = p + new Vector2(0, 2f);
                        break;
                    }
                }
            }
            var mob = word == null ? MobAt(world) : null;
            if (mob != null)
            {
                word = mob.spanish;
                english = mob.english;
                anchor = mob.Position + new Vector2(0, 1f);
            }
            if (word == null)
            {
                // cats, dogs, chickens, the pet-shop animals and your own pet
                string kind = null;
                Vector2 at = Vector2.zero;
                foreach (var a in Wanderer.All)
                    if (a.Covers(world)) { kind = a.kind; at = a.transform.position; break; }
                var pet = PetFollower.Current;
                if (kind == null && pet != null && pet.isActiveAndEnabled && Vector2.Distance(world, pet.transform.position) < 0.6f)
                {
                    kind = pet.petId;
                    at = pet.transform.position;
                }
                if (kind != null)
                {
                    var item = ItemDatabase.Get(kind);
                    word = kind == "chicken" ? "la gallina" : item != null ? item.spanish : kind;
                    english = kind == "chicken" ? "hen" : item != null ? item.english : kind;
                    if (pet != null && kind == pet.petId && at == (Vector2)pet.transform.position) english += " (your pet)";
                    anchor = at + new Vector2(0, 0.9f);
                }
            }
            if (word == null)
            {
                char letter = ObjectLetterAt(world);
                if (letter == 'Q')
                {
                    OpenRoomShop(false);
                    return;
                }
                if (letter == '\0') letter = Map.At(VillageMap.TileAt(world));
                if (words.TryGetValue(letter, out var w))
                {
                    word = w[0];
                    english = w[1];
                }
            }

            if (word == null) return;
            hud.ShowWordLabel(anchor, word, english, speak);
            if (wordsRead.Add(word)) GameState.AddXp(SkillType.Reading, 2);
        }

        /// <summary>The letter of the object drawn on top at this spot (so clicking a roof reads the building).</summary>
        char ObjectLetterAt(Vector2 world) => ObjectAt(world)?.letter ?? '\0';

        ClickableObject ObjectAt(Vector2 world)
        {
            int best = int.MinValue;
            ClickableObject found = null;
            foreach (var c in clickable)
            {
                if (!c.renderer.enabled) continue;
                var b = c.renderer.bounds;
                float trim = b.extents.x * 0.15f;     // ignore the see-through edges of the picture
                bool inside = world.x > b.min.x + trim && world.x < b.max.x - trim && world.y > b.min.y && world.y < b.max.y;
                if (inside && c.renderer.sortingOrder > best)
                {
                    best = c.renderer.sortingOrder;
                    found = c;
                }
            }
            return found;
        }

        Mob MobAt(Vector2 world)
        {
            foreach (var m in Mobs)
                if (m.Covers(world)) return m;
            return null;
        }

        ClickableObject TreeAt(Vector2 world)
        {
            var o = ObjectAt(world);
            return o != null && "ZOY}".IndexOf(o.letter) >= 0 ? o : null;
        }

        // ------------------------------------------------------------ right click: attack / chop

        void RightClick(Vector2 mouse)
        {
            var weapon = GameState.EquippedWeapon();
            float reach = weapon != null && weapon.ranged ? 8f : 1.8f;
            var feet = (Vector2)player.transform.position;
            var body = feet + new Vector2(0, 0.5f);

            var mob = MobAt(mouse);
            if (mob != null)
            {
                var dir = mob.Position - body;
                playerSprite.FaceTowards(dir);
                if (dir.magnitude <= reach) Attack(dir, mob, null);
                else GameState.Say("Está muy lejos. (Too far away.)");
                return;
            }
            var tree = TreeAt(mouse);
            if (tree != null)
            {
                var dir = tree.basePos + new Vector2(0, 0.4f) - body;
                playerSprite.FaceTowards(dir);
                if (dir.magnitude <= 2.4f) Attack(dir, null, tree);
                else GameState.Say("Acércate al árbol. (Get closer to the tree.)");
                return;
            }
            var tile = player.FacingTile;
            char c = Map.At(tile);
            if (portals.ContainsKey(tile) || "DI<>".IndexOf(c) >= 0)
            {
                Interact(c, tile);       // right-clicking a door opens it
                return;
            }
            Attack(PlayerController.FacingVectors[playerSprite.Facing], null, null);
        }

        /// <summary>Swings the equipped weapon (or punches) along "dir". Bows shoot an arrow instead.
        /// Hits the given mob or tree, or whatever is in front of you.</summary>
        public void Attack(Vector2 dir, Mob target, ClickableObject tree)
        {
            if (attackCooldown > 0) return;
            var weapon = GameState.EquippedWeapon();
            attackCooldown = weapon != null && weapon.ranged ? 0.6f : 0.4f;
            int facing = playerSprite.Facing;
            playerSprite.LockFacing(0.25f);     // walking can't turn you away mid-punch
            int order = Mathf.RoundToInt(-player.transform.position.y * 100) + (facing == 2 ? -1 : 1);
            var feet = (Vector2)player.transform.position;
            int damage = (weapon != null ? weapon.power : 1) + Magic.WeaponBonus();     // Afilado / Fuego runes

            if (weapon != null && weapon.ranged)
            {
                ArrowShot.Spawn(this, feet, dir, damage, UIFactory.LoadSprite("Art/Icons/" + weapon.projectile), order);
                return;
            }

            // where the hand is for each direction (down, right, up, left)
            // where the fist appears: just outside the body on the side you face (down, right, up = above
            // your head, left), always drawn in front of you
            Vector2[] fistStart = { new Vector2(0.28f, 0.78f), new Vector2(0.55f, 0.85f), new Vector2(0.2f, 1.95f), new Vector2(-0.55f, 0.85f) };
            var hand = feet + (weapon == null ? fistStart[facing] : new Vector2(0, 0.9f));
            if (weapon == null)
                SwingEffect.Spawn(hand, facing, UIFactory.LoadSprite("Art/Icons/item_fist"), AppearancePalette.Skin[PlayerProfile.Current.appearance.skin], true,
                    Mathf.RoundToInt(-player.transform.position.y * 100) + 5, player.transform);
            else
                SwingEffect.Spawn(hand, facing, UIFactory.LoadSprite("Art/Icons/" + weapon.icon), Color.white, false, order, player.transform);

            if (target == null && tree == null)
            {
                // whatever is just in front of you
                var front = feet + new Vector2(0, 0.4f) + dir.normalized * 0.9f;
                foreach (var m in Mobs)
                    if (!m.Dead && Vector2.Distance(m.Position, front) < 0.9f) { target = m; break; }
            }
            if (target != null)
            {
                HitMob(target, damage, feet);
                return;
            }
            var tile = tree != null ? tree.Tile : player.FacingTile;
            Hit(tile, weapon != null ? weapon.chop : 1);
        }

        /// <summary>Something got hit by a swing, punch or arrow: trees get chopped, people complain.</summary>
        public void Hit(Vector2Int tile, int strength)
        {
            char what = Map.At(tile);
            if (villagers.ContainsKey(what))
            {
                GameState.Say("No puedes atacar a la gente.  (You can't attack people.)");
                return;
            }
            if (trees.Chop(tile, strength))
            {
                // orange trees sometimes drop an orange too
                if (what == 'Z' && Random.value < 0.25f) DropItem("orange", VillageMap.Center(tile) + new Vector2(0.5f, -0.5f));
                return;
            }
            if (what == 'A') GameState.Say("¡Diana!");
        }

        public void DropItem(string id, Vector2 at) => Pickup.Spawn(this, id, at);

        /// <summary>Your weapon (or arrow) hits an animal: Hielo runes slow it, Vampiro runes heal you.</summary>
        public void HitMob(Mob mob, int damage, Vector2 from)
        {
            mob.TakeHit(damage, from);
            int ice = Magic.OnWeapon("ice");
            if (ice > 0) mob.Slow(1.5f * ice);
            int vampire = Magic.OnWeapon("vampire");
            if (vampire > 0) GameState.Data.stats.Heal(vampire);
        }

        // ------------------------------------------------------------ spells outside battle

        /// <summary>Casts a spell from the spellbook: spell its word letter by letter, then it goes off.</summary>
        public void CastInWorld(Magic.Spell s)
        {
            var stats = GameState.Data.stats;
            if (spellReady.TryGetValue(s.id, out float ready) && Time.time < ready)
            {
                GameState.Say(s.es + ": espera " + Mathf.CeilToInt(ready - Time.time) + " s.");
                return;
            }
            if (stats.mana < s.mana) { GameState.Say("No tienes maná. (Not enough mana.)"); return; }
            bool needsTarget = s.id != "heal" && s.id != "shield";
            if (needsTarget && NearestMob(s.id == "quake" ? 6 : 8) == null)
            {
                GameState.Say("No hay enemigos cerca. (No enemies nearby.)");
                return;
            }
            cast.Begin(s, () =>
            {
                if (!stats.TrySpendMana(s.mana)) return;
                spellReady[s.id] = Time.time + (s.id == "meteor" ? 60f : 3f);
                CastEffect(s);
                GameState.AddXp(SkillType.Magic, 5);
            });
        }

        Mob NearestMob(float range)
        {
            Mob best = null;
            float bestDist = range;
            foreach (var m in Mobs)
            {
                if (m.Dead) continue;
                float d = Vector2.Distance(m.Position, PlayerPosition);
                if (d < bestDist) { best = m; bestDist = d; }
            }
            return best;
        }

        void CastEffect(Magic.Spell s)
        {
            int power = Magic.Power(s);
            var me = PlayerPosition;
            int order = Mathf.RoundToInt(-me.y * 100) + 10;
            switch (s.id)
            {
                case "heal":
                    GameState.Data.stats.Heal(power);
                    WorldFx.Burst(me + new Vector2(0, 0.8f), new Color(0.4f, 0.9f, 0.5f), 14, order);
                    GameState.Say("+" + power + " HP");
                    break;
                case "shield":
                    shieldUp = true;
                    WorldFx.Burst(me + new Vector2(0, 0.8f), new Color(1f, 0.95f, 0.6f), 14, order);
                    GameState.Say("Un escudo de luz te protege. (A light shield protects you.)");
                    break;
                case "quake":
                    foreach (var m in Mobs)
                        if (!m.Dead && Vector2.Distance(m.Position, me) < 6)
                        {
                            m.TakeHit(power, me);
                            WorldFx.Burst(m.Position, new Color(0.6f, 0.45f, 0.3f), 8, order);
                        }
                    break;
                case "ice":
                    var frozen = NearestMob(8);
                    if (frozen != null) { frozen.Freeze(power); WorldFx.Burst(frozen.Position, new Color(0.7f, 0.9f, 1f), 12, order); }
                    break;
                default:      // chispa, rayo, meteoro
                    var target = NearestMob(8);
                    if (target == null) break;
                    var color = s.id == "spark" ? new Color(1f, 0.6f, 0.2f) : s.id == "lightning" ? new Color(1f, 0.95f, 0.4f) : new Color(0.9f, 0.3f, 0.2f);
                    WorldFx.Burst(target.Position + new Vector2(0, 0.4f), color, s.id == "meteor" ? 24 : 12, order);
                    target.TakeHit(power, me);
                    break;
            }
        }

        /// <summary>A monster bit you. Armour (defence) softens the blow; Escudo de luz blocks it.</summary>
        public void DamagePlayer(int raw, Vector2 from)
        {
            if (Time.time < invulnerableUntil) return;     // a pack can't all bite you in the same instant
            invulnerableUntil = Time.time + 0.5f;
            if (sitting) StandUp();
            if (shieldUp)
            {
                shieldUp = false;
                GameState.Say("¡El escudo bloquea el golpe! (The shield blocks the hit!)");
                return;
            }
            int dmg = Mathf.Max(1, raw - GameState.Defense() / 2);
            GameState.Data.stats.Damage(dmg);
            playerSprite.Flash();
            GameState.Say("-" + dmg + " HP");
        }

        // ------------------------------------------------------------ saving

        /// <summary>Saves stats, items, skills and where the player is standing.</summary>
        public void SaveProgress()
        {
            var data = GameState.Data;
            if (data == null || player == null) return;
            data.hasPosition = true;
            data.posX = player.transform.position.x;
            data.posY = player.transform.position.y;
            data.facing = playerSprite.Facing;
            GameState.Save();
        }

        /// <summary>"Exit Game" in the menu: save everything and go back to the title screen.</summary>
        public void ExitToTitle()
        {
            SaveProgress();
            Time.timeScale = 1;
            Instance = null;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.orthographicSize = 5.625f;
            foreach (var leftover in FindObjectsOfType<SwingEffect>()) Destroy(leftover.gameObject);
            foreach (var leftover in FindObjectsOfType<ArrowShot>()) Destroy(leftover.gameObject);
            foreach (var leftover in FindObjectsOfType<Pickup>()) Destroy(leftover.gameObject);
            Destroy(gameObject);
            new GameObject("StartScreen").AddComponent<StartScreenController>();
        }

        void OnApplicationQuit() => SaveProgress();
    }
}
