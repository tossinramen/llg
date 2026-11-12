using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LLG
{
    /// <summary>
    /// Runs the village of Valdeluz: draws the ground and every building/tree/prop (sorted so you can
    /// walk behind them), creates the player, villagers and pet, and handles the controls:
    ///   E (use)      talk, enter a shop, fish... whatever you are facing
    ///   B / U        bag / profile (character sheet and skills)
    ///   Left click   read: shows the Spanish name of whatever you click on
    ///   Right click  attack with your weapon (or punch); on a door it opens it
    ///   Esc          menu
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

        /// <summary>The Spanish (and English) name of every kind of map tile and object, for reading.
        /// Spanish only capitalises the first word ("La fuente rúnica"); English uses Title Case.</summary>
        static readonly Dictionary<char, string[]> Words = new Dictionary<char, string[]>
        {
            { '.', new[] { "La hierba", "Grass" } },          { ',', new[] { "El camino", "Road" } },
            { ':', new[] { "La plaza", "Town Square" } },      { ';', new[] { "El campo de tiro", "Archery Range" } },
            { '=', new[] { "El muelle", "Dock" } },            { 'F', new[] { "El estanque", "Pond" } },
            { 'R', new[] { "La fuente rúnica", "Rune Fountain" } }, { 'A', new[] { "La diana", "Target" } },
            { 'N', new[] { "La valla", "Fence" } },            { 'Z', new[] { "El naranjo", "Orange Tree" } },
            { 'O', new[] { "El olivo", "Olive Tree" } },        { 'Y', new[] { "El ciprés", "Cypress" } },
            { 'u', new[] { "El arbusto", "Bush" } },           { 'f', new[] { "La farola", "Street Lamp" } },
            { 'p', new[] { "La maceta", "Flower Pot" } },      { 'b', new[] { "Los barriles", "Barrels" } },
            { 'e', new[] { "El banco", "Bench" } },            { 'x', new[] { "El puesto de fruta", "Fruit Stall" } },
            { 'c', new[] { "El carro", "Cart" } },             { 'k', new[] { "La panadería", "Bakery" } },
            { 'q', new[] { "La dulcería", "Candy Shop" } },    { 's', new[] { "La tienda", "General Store" } },
            { 'w', new[] { "La armería", "Weapon Shop" } },    { 'm', new[] { "La tienda de mascotas", "Pet Shop" } },
            { 'h', new[] { "Tu casa", "Your House" } },        { 'd', new[] { "La casa", "House" } },
            { 'i', new[] { "La iglesia", "Church" } },
            { 'B', new[] { "La puerta", "Door" } }, { 'C', new[] { "La puerta", "Door" } }, { 'S', new[] { "La puerta", "Door" } },
            { 'W', new[] { "La puerta", "Door" } }, { 'M', new[] { "La puerta", "Door" } }, { 'H', new[] { "La puerta", "Door" } },
            { 'D', new[] { "La puerta", "Door" } }, { 'I', new[] { "La puerta", "Door" } },
        };
        const string Doors = "BCSWMHDI";

        public static VillageController Instance { get; private set; }
        public VillageMap Map { get; private set; }

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
        ArcheryGame archery;
        FishingGame fishing;
        GameMenu menu;
        OptionsWindow options;
        List<ClickableObject> clickable;
        readonly Dictionary<char, Villager> villagers = new Dictionary<char, Villager>();
        readonly HashSet<string> wordsRead = new HashSet<string>();
        readonly HashSet<Vector2Int> picked = new HashSet<Vector2Int>();   // trees/bushes already shaken for fruit
        Vector2 spawnPoint;
        int closedFrame = -1;
        float hungerTimer, manaTimer, attackCooldown, autosaveTimer;

        /// <summary>True while a window that needs your full attention is open (talking, shopping,
        /// a minigame, the menu). The bag, profile and controls windows let you keep walking.</summary>
        public bool Busy => dialogue.Open || shop.Open || archery.Open || fishing.Open || menu.Open
                            || closedFrame == Time.frameCount;

        /// <summary>Windows call this when they close, so the key that closed them does not also trigger something.</summary>
        public void NoteWindowClosed() => closedFrame = Time.frameCount;

        void Awake()
        {
            Instance = this;
            GameState.Load();
            Map = VillageMap.Load();

            cam = Camera.main;
            cam.orthographic = true;
            cam.orthographicSize = 5.625f;       // 180 art pixels tall at 16 pixels per tile

            var ground = new GameObject("Ground").AddComponent<SpriteRenderer>();
            ground.transform.SetParent(transform, false);
            ground.sprite = UIFactory.LoadSprite("Art/World/village_ground");
            ground.sortingOrder = -30000;
            ground.transform.position = new Vector3(Map.Width / 2f, -Map.Height / 2f, 0);
            var objects = new GameObject("Objects").transform;
            objects.SetParent(transform, false);
            clickable = VillageLayout.Load().Spawn(objects);

            SpawnPlayer();
            SpawnVillagers();
            RefreshPlayerLook();

            var canvas = UIFactory.Canvas("VillageCanvas", 0);
            canvas.SetParent(transform, false);
            hud = gameObject.AddComponent<Hud>();
            hud.Build(canvas, OpenMenu, () => controls.Toggle());
            hud.BuildMinimap(canvas, Map, player.transform, playerSprite, MapPlaces());
            dialogue = gameObject.AddComponent<DialogueBox>();
            dialogue.Build(canvas, this);
            shop = gameObject.AddComponent<ShopWindow>();
            shop.Build(canvas, this);
            bag = gameObject.AddComponent<BagWindow>();
            bag.Build(canvas, this);
            profile = gameObject.AddComponent<ProfileWindow>();
            profile.Build(canvas, this);
            controls = gameObject.AddComponent<ControlsWindow>();
            controls.Build(canvas, this);
            archery = gameObject.AddComponent<ArcheryGame>();
            archery.Build(canvas, this);
            fishing = gameObject.AddComponent<FishingGame>();
            fishing.Build(canvas, this);
            options = gameObject.AddComponent<OptionsWindow>();
            options.Build(canvas);
            menu = gameObject.AddComponent<GameMenu>();
            menu.Build(canvas, this, options);

            GameState.InventoryChanged += RefreshPlayerLook;
            FollowPlayer();
            string playerName = PlayerProfile.Current.playerName;
            dialogue.Show("", "¡Hola, " + playerName + "! Bienvenido a " + GameConfig.TownName + ".",
                "Hello, " + playerName + "! Welcome to " + GameConfig.TownName + ".");
        }

        void OnDestroy() => GameState.InventoryChanged -= RefreshPlayerLook;

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
            string wanted = GameState.Has(GameState.Data.pet) ? GameState.Data.pet : "";
            if (wanted == petShown) return;
            if (pet != null) Destroy(pet.gameObject);
            pet = string.IsNullOrEmpty(wanted) ? null : PetFollower.Spawn(transform, player.transform, wanted);
            petShown = wanted;
        }

        static CharacterAppearance Look(int skin, int hairStyle, int hairColor, int shirt, int pants)
        {
            return new CharacterAppearance { skin = skin, hairStyle = hairStyle, hairColor = hairColor, shirt = shirt, pants = pants };
        }

        static Accessory Acc(string kind, byte r, byte g, byte b) => new Accessory(kind, new Color32(r, g, b, 255));

        void SpawnVillagers()
        {
            string money = GameConfig.CurrencyName;
            AddVillager('1', "Abuela Rosa", "la abuela", "Grandmother", Look(2, 5, 5, 7, 2),
                new[] { Acc("shawl", 176, 40, 50), Acc("skirt", 52, 46, 54) },
                new Line("Abuela Rosa", "¡Hola, cariño! Bienvenido a Valdeluz.", "Hello, dear! Welcome to Valdeluz."),
                new Line("Abuela Rosa", "El pan de la panadería es delicioso.", "The bread from the bakery is delicious."),
                new Line("Abuela Rosa", "Si tienes hambre, come algo de tu mochila.", "If you are hungry, eat something from your bag."));
            AddVillager('2', "Mateo", "el pescador", "Fisherman", Look(3, 0, 0, 1, 1),
                new[] { Acc("hat", 200, 168, 108), Acc("beard", 74, 50, 36) },
                new Line("Mateo", "Buenos días. Soy Mateo, el pescador.", "Good morning. I am Mateo, the fisherman."),
                new Line("Mateo", "Necesitas una caña de pescar para pescar.", "You need a fishing rod to fish."),
                new Line("Mateo", "Compra una en la tienda.", "Buy one at the general store."));
            AddVillager('3', "Luna", "la niña", "Girl", Look(0, 3, 3, 4, 0),
                new[] { Acc("skirt", 196, 60, 76) },
                new Line("Luna", "¡Me encantan los caramelos!", "I love candy!"),
                new Line("Luna", "La dulcería es rosa. ¡Es mi color favorito!", "The candy shop is pink. It is my favourite colour!"),
                new Line("Luna", "¿Tienes una mascota? ¡Yo quiero un gato!", "Do you have a pet? I want a cat!"));
            AddVillager('4', "Don Felipe", "el arquero", "Archer", Look(4, 4, 4, 2, 1),
                new[] { Acc("cloak", 60, 100, 64), Acc("beard", 172, 172, 178), Acc("hat", 50, 46, 52) },
                new Line("Don Felipe", "Soy Don Felipe, el maestro de tiro con arco.", "I am Don Felipe, the archery master."),
                new Line("Don Felipe", "Dispara a la palabra correcta.", "Shoot the correct word."));
            AddVillager('5', "Sabia", "la sabia", "Wise Woman", Look(1, 1, 5, 5, 5),
                new[] { Acc("cloak", 90, 50, 140), Acc("skirt", 110, 70, 160) },
                new Line("Sabia", "Las runas de la fuente guardan el poder de las palabras.", "The fountain's runes hold the power of words."),
                new Line("Sabia", "Toca la fuente y tu maná volverá.", "Touch the fountain and your mana will return."),
                new Line("Sabia", "Aprende palabras y ganarás " + money + ".", "Learn words and you will earn " + money + "."));
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

        /// <summary>Icons on the minimap: shops, the pond, the archery range, the fountain, and villagers.</summary>
        List<(Vector2Int, string)> MapPlaces()
        {
            var icons = new Dictionary<char, string>
            {
                { 'B', "Art/Icons/food_bread" }, { 'C', "Art/Icons/food_candy" }, { 'S', "Art/Icons/item_rod" },
                { 'W', "Art/Icons/item_sword" }, { 'M', "Art/Icons/pet_dog" }, { 'H', "Art/UI/hp_heart" },
            };
            var list = new List<(Vector2Int, string)>();
            foreach (var kv in icons)
            {
                var found = Map.Find(kv.Key);
                if (found.Count > 0) list.Add((found[0], kv.Value));
            }
            var fountain = Map.Find('R');
            if (fountain.Count > 0) list.Add((fountain[0] + new Vector2Int(1, 0), "Art/UI/mana_drop"));
            var targets = Map.Find('A');
            if (targets.Count > 0) list.Add((targets[targets.Count / 2] + new Vector2Int(0, 2), "Art/UI/target"));
            var water = Map.Find('F');
            if (water.Count > 0) list.Add((water[water.Count / 2], "Art/Icons/food_fish"));
            foreach (var marker in villagers.Keys)
                list.Add((Map.Find(marker)[0], "Art/UI/map_dot"));
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
            autosaveTimer += Time.unscaledDeltaTime;
            if (autosaveTimer >= 30f) { autosaveTimer = 0; SaveProgress(); }

            if (Busy)
            {
                hud.SetPrompt(null);
                return;
            }

            if (stats.IsDown)
            {
                Faint();
                return;
            }

            char facing = Map.At(player.FacingTile);
            hud.SetPrompt(Describe(facing));

            if (Input.GetKeyDown(KeyCode.Escape))
                OpenMenu();
            else if (Keybinds.Down(GameAction.Use) || Input.GetKeyDown(KeyCode.Return))
                Interact(facing);
            else if (Keybinds.Down(GameAction.Backpack) && !bag.Open)
                bag.Toggle();
            else if (Keybinds.Down(GameAction.Profile) && !profile.Open)
                profile.Toggle();
            else if (Input.GetMouseButtonDown(1) && !PointerOverUI())
                RightClick();
            else if (Input.GetMouseButtonDown(0) && !PointerOverUI())
                Read(cam.ScreenToWorldPoint(Input.mousePosition));
        }

        void OpenMenu()
        {
            if (Busy) return;
            hud.HideWordLabel();
            bag.Close();
            profile.Close();
            controls.Close();
            closedFrame = -1;
            menu.Show();
        }

        static bool PointerOverUI() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        void LateUpdate() => FollowPlayer();

        void FollowPlayer()
        {
            float halfH = cam.orthographicSize, halfW = halfH * cam.aspect;
            var p = player.transform.position;
            float x = Map.Width <= halfW * 2 ? Map.Width / 2f : Mathf.Clamp(p.x, halfW, Map.Width - halfW);
            float y = Map.Height <= halfH * 2 ? -Map.Height / 2f : Mathf.Clamp(p.y + 0.8f, -Map.Height + halfH, -halfH);
            cam.transform.position = new Vector3(x, y, -10);
        }

        void Faint()
        {
            var stats = GameState.Data.stats;
            int lost = stats.coins / 10;
            stats.AddCoins(-lost);
            stats.Heal(stats.maxHp / 2);
            player.transform.position = spawnPoint;
            dialogue.Show("", "Te desmayaste... Despiertas en la plaza. (-" + lost + " " + GameConfig.CurrencyName + ")",
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
                case 'M': return "Entrar en la tienda de mascotas";
                case 'H': return "Tu casa";
                case 'D': return "Llamar a la puerta";
                case 'I': return "La iglesia";
                case 'A': return "Tiro con arco";
                case 'F': return "Pescar";
                case 'R': return "Tocar la fuente rúnica";
            }
            return villagers.TryGetValue(what, out var villager) ? "Hablar con " + villager.Name : null;
        }

        /// <summary>Does whatever the given map letter stands for. Public so tools/tests can trigger it.</summary>
        public void Interact(char what)
        {
            switch (what)
            {
                case 'B': shop.Show("Panadería", "Bakery", ItemDatabase.Bakery); break;
                case 'C': shop.Show("Dulcería", "Candy Shop", ItemDatabase.CandyShop); break;
                case 'S': shop.Show("Tienda", "General Store", ItemDatabase.Store); break;
                case 'W': shop.Show("Armería", "Weapon Shop", ItemDatabase.Weapons); break;
                case 'M': shop.Show("Tienda de mascotas", "Pet Shop", ItemDatabase.Pets); break;
                case 'H':
                    dialogue.Show("", "Esta es tu casa. Pronto podrás decorarla.", "This is your house. Soon you will be able to decorate it.");
                    break;
                case 'D':
                    dialogue.Show("", "No hay nadie en casa.", "Nobody is home.");
                    break;
                case 'I':
                    dialogue.Show("", "La iglesia está cerrada hoy.", "The church is closed today.");
                    break;
                case 'A':
                    archery.Begin();
                    break;
                case 'F':
                    if (GameState.Has("rod") || GameState.Has("rod_fine")) fishing.Begin();
                    else dialogue.Show("", "Necesitas una caña de pescar. Cómprala en la tienda.", "You need a fishing rod. Buy one at the general store.");
                    break;
                case 'R':
                    var stats = GameState.Data.stats;
                    stats.RestoreMana(stats.maxMana);
                    dialogue.Show("", "Las runas brillan. Tu maná vuelve.", "The runes glow. Your mana returns.");
                    break;
                default:
                    if (villagers.TryGetValue(what, out var villager))
                    {
                        villager.Sprite.Face((playerSprite.Facing + 2) % 4);     // turn to face the player
                        if (what == '4') dialogue.Show(villager.Lines, archery.Begin);
                        else dialogue.Show(villager.Lines);
                    }
                    break;
            }
        }

        // ------------------------------------------------------------ left click: read

        /// <summary>Shows the Spanish name of whatever is at this world position (people first, then the
        /// object drawn on top there, then the ground). Reading a word for the first time gives Reading XP.</summary>
        public void Read(Vector2 world)
        {
            string word = null, english = null;
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
                        anchor = p + new Vector2(0, 2f);
                        break;
                    }
                }
            }

            if (word == null)
            {
                // the object drawn on top at this spot (so clicking a roof reads the building)
                int best = int.MinValue;
                char letter = '\0';
                foreach (var c in clickable)
                {
                    var b = c.renderer.bounds;
                    float trim = b.extents.x * 0.15f;     // ignore the see-through edges of the picture
                    bool inside = world.x > b.min.x + trim && world.x < b.max.x - trim && world.y > b.min.y && world.y < b.max.y;
                    if (inside && c.renderer.sortingOrder > best)
                    {
                        best = c.renderer.sortingOrder;
                        letter = c.letter;
                    }
                }
                if (letter == '\0') letter = Map.At(VillageMap.TileAt(world));
                if (Words.TryGetValue(letter, out var w))
                {
                    word = w[0];
                    english = w[1];
                }
            }

            if (word == null) return;
            hud.ShowWordLabel(anchor, word, english);
            if (wordsRead.Add(word)) GameState.AddXp(SkillType.Reading, 2);
        }

        // ------------------------------------------------------------ right click: attack

        void RightClick()
        {
            var tile = player.FacingTile;
            if (Doors.IndexOf(Map.At(tile)) >= 0)
            {
                Interact(Map.At(tile));      // right-clicking a door opens it
                return;
            }
            Attack();
        }

        /// <summary>Swings the equipped weapon (or punches). The bow fires an arrow instead.</summary>
        public void Attack()
        {
            if (attackCooldown > 0) return;
            attackCooldown = 0.35f;

            var weapon = GameState.EquippedWeapon();
            int facing = playerSprite.Facing;
            int order = Mathf.RoundToInt(-player.transform.position.y * 100) + (facing == 2 ? -1 : 1);
            var feet = (Vector2)player.transform.position;

            if (weapon != null && weapon.id == "bow")
            {
                ArrowShot.Spawn(this, feet, facing, UIFactory.LoadSprite("Art/Icons/item_arrow"), order);
                return;
            }

            var hand = feet + new Vector2(0, 0.9f);
            if (weapon == null)
            {
                var skin = AppearancePalette.Skin[PlayerProfile.Current.appearance.skin];
                SwingEffect.Spawn(hand, facing, UIFactory.LoadSprite("Art/Icons/item_fist"), skin, true, order);
            }
            else
            {
                SwingEffect.Spawn(hand, facing, UIFactory.LoadSprite("Art/Icons/" + weapon.icon), Color.white, false, order);
            }
            Hit(player.FacingTile);
        }

        /// <summary>Something got hit by a swing, punch or arrow.</summary>
        public void Hit(Vector2Int tile)
        {
            char what = Map.At(tile);
            if (villagers.TryGetValue(what, out var villager))
            {
                villager.Sprite.Face((playerSprite.Facing + 2) % 4);
                GameState.Say(villager.Name + ": ¡Ay! ¡Oye!");
                return;
            }

            var stats = GameState.Data.stats;
            bool fresh = !picked.Contains(tile) && Random.value < 0.35f;
            switch (what)
            {
                case 'Z':
                    if (fresh && GameState.AddItem("orange")) { picked.Add(tile); GameState.Say("¡Una naranja! Está en tu mochila."); }
                    break;
                case 'O':
                    if (fresh) { picked.Add(tile); stats.Eat(5); GameState.Say("¡Unas aceitunas!  +5"); }
                    break;
                case 'u':
                    if (fresh) { picked.Add(tile); stats.Eat(5); GameState.Say("¡Unas moras!  +5"); }
                    break;
                case 'A':
                    GameState.Say("¡Diana!");
                    break;
            }
        }

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
            foreach (var leftover in FindObjectsOfType<SwingEffect>()) Destroy(leftover.gameObject);
            foreach (var leftover in FindObjectsOfType<ArrowShot>()) Destroy(leftover.gameObject);
            Destroy(gameObject);
            new GameObject("StartScreen").AddComponent<StartScreenController>();
        }

        void OnApplicationQuit() => SaveProgress();
    }
}
