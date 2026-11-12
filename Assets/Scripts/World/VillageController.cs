using System.Collections.Generic;
using UnityEngine;

namespace LLG
{
    /// <summary>
    /// Runs the village: draws the map, creates the player and villagers, and decides what the
    /// E key does depending on what the player is facing (shop door, pond, villager...).
    /// Like the start screen, everything is created from code when the game runs.
    /// </summary>
    public class VillageController : MonoBehaviour
    {
        class Villager
        {
            public string Name;
            public CharacterAppearance Look;
            public List<Line> Lines;
            public CharacterSprite Sprite;
        }

        public static VillageController Instance { get; private set; }
        public VillageMap Map { get; private set; }

        PlayerController player;
        CharacterSprite playerSprite;
        Camera cam;
        Hud hud;
        DialogueBox dialogue;
        ShopWindow shop;
        ArcheryGame archery;
        FishingGame fishing;
        readonly Dictionary<char, Villager> villagers = new Dictionary<char, Villager>();
        Vector2 spawnPoint;
        int closedFrame = -1;
        float hungerTimer, manaTimer;

        /// <summary>True while a window is open, so the player cannot walk around behind it.</summary>
        public bool Busy => dialogue.Open || shop.Open || archery.Open || fishing.Open || closedFrame == Time.frameCount;

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

            var ground = new GameObject("VillageGround").AddComponent<SpriteRenderer>();
            ground.transform.SetParent(transform, false);
            ground.sprite = UIFactory.LoadSprite("Art/World/village");
            ground.sortingOrder = -30000;
            ground.transform.position = new Vector3(Map.Width / 2f, -Map.Height / 2f, 0);

            SpawnPlayer();
            SpawnVillagers();

            var canvas = UIFactory.Canvas("VillageCanvas", 0);
            canvas.SetParent(transform, false);
            hud = gameObject.AddComponent<Hud>();
            hud.Build(canvas);
            dialogue = gameObject.AddComponent<DialogueBox>();
            dialogue.Build(canvas, this);
            shop = gameObject.AddComponent<ShopWindow>();
            shop.Build(canvas, this);
            archery = gameObject.AddComponent<ArcheryGame>();
            archery.Build(canvas, this);
            fishing = gameObject.AddComponent<FishingGame>();
            fishing.Build(canvas, this);

            FollowPlayer();
            string playerName = PlayerProfile.Current.playerName;
            dialogue.Show("", "¡Hola, " + playerName + "! Este es tu nuevo pueblo.", "Hello, " + playerName + "! This is your new village.");
        }

        // ------------------------------------------------------------ setup

        void SpawnPlayer()
        {
            var start = Map.Find('P');
            var tile = start.Count > 0 ? start[0] : new Vector2Int(Map.Width / 2, Map.Height / 2);
            Map.Set(tile, '.');
            spawnPoint = VillageMap.Center(tile);

            var go = new GameObject("Player");
            go.transform.SetParent(transform, false);
            go.transform.position = spawnPoint;
            playerSprite = go.AddComponent<CharacterSprite>();
            playerSprite.Build(PlayerProfile.Current.appearance);
            player = go.AddComponent<PlayerController>();
            player.Init(this, playerSprite);
        }

        static CharacterAppearance Look(int skin, int hairStyle, int hairColor, int shirt, int pants)
        {
            return new CharacterAppearance { skin = skin, hairStyle = hairStyle, hairColor = hairColor, shirt = shirt, pants = pants };
        }

        void SpawnVillagers()
        {
            string money = GameConfig.CurrencyName;
            AddVillager('1', "Abuela Rosa", Look(1, 1, 5, 5, 3),
                new Line("Abuela Rosa", "¡Hola! Te doy la bienvenida al pueblo.", "Hello! I welcome you to the village."),
                new Line("Abuela Rosa", "El pan de la panadería es delicioso.", "The bread from the bakery is delicious."),
                new Line("Abuela Rosa", "Come algo si tienes hambre.", "Eat something if you are hungry."));
            AddVillager('2', "Mateo", Look(3, 0, 0, 1, 1),
                new Line("Mateo", "Buenos días. Soy Mateo, el pescador.", "Good morning. I am Mateo, the fisherman."),
                new Line("Mateo", "Necesitas una caña de pescar para pescar.", "You need a fishing rod to fish."),
                new Line("Mateo", "Compra una en la tienda.", "Buy one at the store."));
            AddVillager('3', "Luna", Look(0, 3, 3, 3, 0),
                new Line("Luna", "¡Me encantan los caramelos!", "I love candy!"),
                new Line("Luna", "La dulcería es rosa. ¡Es mi color favorito!", "The candy shop is pink. It is my favourite colour!"));
            AddVillager('4', "Don Felipe", Look(4, 4, 4, 2, 2),
                new Line("Don Felipe", "Soy Don Felipe, el maestro de tiro con arco.", "I am Don Felipe, the archery master."),
                new Line("Don Felipe", "Dispara a la palabra correcta.", "Shoot the correct word."));
            AddVillager('5', "Sabia", Look(2, 1, 6, 5, 5),
                new Line("Sabia", "Las runas guardan el poder de las palabras.", "The runes hold the power of words."),
                new Line("Sabia", "Aprende palabras y ganarás " + money + ".", "Learn words and you will earn " + money + "."),
                new Line("Sabia", "Con " + money + " puedes comprar casi todo.", "With " + money + " you can buy almost anything."));
        }

        void AddVillager(char marker, string villagerName, CharacterAppearance look, params Line[] lines)
        {
            var tiles = Map.Find(marker);
            if (tiles.Count == 0) return;
            var go = new GameObject(villagerName);
            go.transform.SetParent(transform, false);
            go.transform.position = VillageMap.Center(tiles[0]) + new Vector2(0, -0.2f);
            var sprite = go.AddComponent<CharacterSprite>();
            sprite.Build(look);
            villagers[marker] = new Villager { Name = villagerName, Look = look, Lines = new List<Line>(lines), Sprite = sprite };
        }

        // ------------------------------------------------------------ every frame

        void Update()
        {
            var stats = GameState.Data.stats;

            hungerTimer += Time.deltaTime;
            if (hungerTimer >= 8f) { hungerTimer = 0; stats.GetHungrier(1); }
            manaTimer += Time.deltaTime;
            if (manaTimer >= 20f) { manaTimer = 0; stats.RestoreMana(1); }

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

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                Interact(facing);
            else if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
                shop.ShowBackpack();
        }

        void LateUpdate() => FollowPlayer();

        void FollowPlayer()
        {
            float halfH = cam.orthographicSize, halfW = halfH * cam.aspect;
            var p = player.transform.position;
            float x = Map.Width <= halfW * 2 ? Map.Width / 2f : Mathf.Clamp(p.x, halfW, Map.Width - halfW);
            float y = Map.Height <= halfH * 2 ? -Map.Height / 2f : Mathf.Clamp(p.y + 0.5f, -Map.Height + halfH, -halfH);
            cam.transform.position = new Vector3(x, y, -10);
        }

        void Faint()
        {
            var stats = GameState.Data.stats;
            int lost = stats.coins / 10;
            stats.AddCoins(-lost);
            stats.Heal(stats.maxHp / 2);
            player.transform.position = spawnPoint;
            dialogue.Show("", "Te desmayaste...", "You fainted and woke up in the plaza. (-" + lost + " " + GameConfig.CurrencyName + ")");
            GameState.Save();
        }

        // ------------------------------------------------------------ interaction

        string Describe(char what)
        {
            switch (what)
            {
                case 'B': return "Panadería (Bakery)";
                case 'C': return "Dulcería (Candy shop)";
                case 'S': return "Tienda (Store)";
                case 'H': return "Tu casa (Your house)";
                case 'D': return "Llamar (Knock)";
                case 'A': return "Tiro con arco (Archery)";
                case 'F': return "Pescar (Fish)";
                case 'R': return "Piedra rúnica (Rune stone)";
            }
            return villagers.TryGetValue(what, out var villager) ? "Hablar con " + villager.Name + " (Talk)" : null;
        }

        /// <summary>Does whatever the given map letter stands for. Public so tools/tests can trigger it.</summary>
        public void Interact(char what)
        {
            switch (what)
            {
                case 'B':
                    shop.Show("PANADERÍA", "Bakery - ¿Qué quieres? (What would you like?)", ItemDatabase.Bakery);
                    break;
                case 'C':
                    shop.Show("DULCERÍA", "Candy shop - ¿Qué quieres? (What would you like?)", ItemDatabase.CandyShop);
                    break;
                case 'S':
                    shop.Show("TIENDA", "Store - cañas, armas y libros (rods, weapons and books)", ItemDatabase.Store);
                    break;
                case 'H':
                    dialogue.Show("", "Esta es tu casa.", "This is your house. (Furniture and upgrades are coming later.)");
                    break;
                case 'D':
                    dialogue.Show("", "No hay nadie en casa.", "Nobody is home.");
                    break;
                case 'A':
                    archery.Begin();
                    break;
                case 'F':
                    if (GameState.Has("rod") || GameState.Has("rod_fine")) fishing.Begin();
                    else dialogue.Show("", "Necesitas una caña de pescar.", "You need a fishing rod. Buy one at the store (Tienda).");
                    break;
                case 'R':
                    var stats = GameState.Data.stats;
                    stats.RestoreMana(stats.maxMana);
                    dialogue.Show("", "La piedra brilla. Tu maná vuelve.", "The stone glows. Your mana returns.");
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

        void OnApplicationQuit() => GameState.Save();
    }
}
