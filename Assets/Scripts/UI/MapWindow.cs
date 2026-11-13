using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// The big map (M, or click the minimap): the whole village with every building and area labelled
    /// in Spanish (the globe shows English), the shop icons, wolf territory, and an arrow for you.
    /// </summary>
    public class MapWindow : GameWindow
    {
        float Scale = 3.7f;            // canvas units per tile (worked out so the whole world fits the window)
        static readonly (string es, string en, int x, int y)[] Regions =
        {
            ("La plaza", "Town Square", 46, 31), ("La granja", "Farm", 18, 33), ("El bosque", "Forest", 97, 44),
            ("El estanque", "Pond", 58, 57), ("El río", "River", 80, 62), ("El campo de tiro", "Archery Range", 66, 3),
            ("El molino", "Windmill", 8, 5), ("La viña", "Vineyard", 71, 58), ("El olivar", "Olive Grove", 18, 60),
            ("El puente", "Bridge", 81, 26), ("La Ciudadela del Sol", "Citadel of the Sun", 100, 10),
            ("El lago", "Lake", 150, 33), ("Los campos", "Wheat Fields", 46, 83), ("Las colinas", "Hills", 151, 80),
            ("El puente del sur", "South Bridge", 81, 95),
        };
        // label offset from the door, in tiles: neighbours alternate above / below so names don't overlap
        static readonly (char door, string es, string en, int dy)[] Buildings =
        {
            ('B', "Panadería", "Bakery", -3), ('C', "Dulcería", "Candy Shop", 2), ('S', "Tienda", "General Store", -3),
            ('W', "Armería", "Weapon Shop", -3), ('^', "Herrería", "Blacksmith", 2), ('M', "Mascotas", "Pet Shop", -3),
            ('H', "Tu casa", "Your House", -3), ('I', "Iglesia", "Church", -6), ('<', "Ayuntamiento", "Town Hall", -4), ('>', "Taberna", "Tavern", 2),
            (']', "Semillas", "Seed Shop", 2), (')', "Música", "Music Shop", -3), ('\'', "Escuela de música", "Music School", -3), ('Þ', "Torre rúnica", "Rune Tower", -5), ('Œ', "Pesca", "Fishing Shop", 2), ('Ŭ', "La mina", "Mine", -6), ('Λ', "Academia", "Academy", 2),
        };

        RectTransform mapRect, arrow;
        readonly List<Rect> used = new List<Rect>();
        LocalizedText tooltip;
        Transform player;
        CharacterSprite playerSprite;
        Vector2 size;

        protected override bool ToggleKeyPressed => Keybinds.Down(GameAction.Map);

        public void Build(Transform canvas, VillageController owner, VillageMap map, int width, int height,
            Transform playerTransform, CharacterSprite sprite, List<(Vector2Int tile, string icon)> places)
        {
            player = playerTransform;
            playerSprite = sprite;
            Scale = Mathf.Min(600f / width, 266f / height);
            size = new Vector2(width, height) * Scale;
            CreateWindow(canvas, owner, new Vector2(0, -2), new Vector2(size.x + 40, size.y + 92), "Mapa de Valdeluz", "Map of Valdeluz");
            tooltip = UIFactory.LLabel("Tooltip", W, new Vector2(40, -size.y / 2 - 22), new Vector2(size.x - 120, 16),
                "Pasa el ratón por un nombre para traducirlo.", "Point at a name to translate it.", UIFactory.Ink).GetComponent<LocalizedText>();
            var frame = UIFactory.Img("MapFrame", W, new Vector2(0, 2), size + new Vector2(6, 6), new Color32(96, 66, 44, 255));
            mapRect = UIFactory.Img("Map", frame.transform, Vector2.zero, size, Color.white, UIFactory.LoadSprite("Art/UI/minimap")).rectTransform;

            foreach (var (tile, icon) in places)
            {
                bool dot = icon.EndsWith("map_dot");
                var img = UIFactory.Img("Place", mapRect, TileToMap(tile), dot ? new Vector2(5, 5) : new Vector2(12, 12),
                    dot ? (Color)new Color32(255, 220, 90, 255) : Color.white, UIFactory.LoadSprite(icon));
            }
            foreach (var (door, es, en, dy) in Buildings)
            {
                var found = map.Find(door);
                if (found.Count == 0 || found[0].x >= width) continue;
                Place(found[0] + new Vector2Int(0, dy), es, en, Color.white);
            }
            foreach (var (es, en, x, y) in Regions)
                Place(new Vector2Int(x, y), es, en, new Color32(255, 236, 150, 255));
            arrow = UIFactory.Img("You", mapRect, Vector2.zero, new Vector2(12, 12), Color.white, UIFactory.LoadSprite("Art/UI/map_arrow")).rectTransform;
            UIFactory.LLabel("You", W, new Vector2(-size.x / 2 + 50, -size.y / 2 - 22), new Vector2(90, 16), "▲ = tú", "▲ = you", UIFactory.Faded);
        }

        Vector2 TileToMap(Vector2Int tile) => new Vector2((tile.x + 0.5f) * Scale - size.x / 2, size.y / 2 - (tile.y + 0.5f) * Scale);

        /// <summary>A name on the map, on a small plate. If it would overlap another name it moves up or
        /// down until it doesn't. Hovering it shows the translation at the bottom of the window.</summary>
        void Place(Vector2Int tile, string es, string en, Color color)
        {
            var plateSize = new Vector2(Mathf.Max(es.Length, en.Length) * 6 + 12, 16);     // bold 12 px text on a dark plate
            // keep every name inside the map
            var at = TileToMap(tile);
            at.x = Mathf.Clamp(at.x, -size.x / 2 + plateSize.x / 2 + 2, size.x / 2 - plateSize.x / 2 - 2);
            at.y = Mathf.Clamp(at.y, -size.y / 2 + 10, size.y / 2 - 10);
            // try the spot itself, then nearby spots above/below and to the sides, until nothing overlaps
            var pos = at;
            bool placed = false;
            foreach (float dx in new float[] { 0, -50, 50, -100, 100 })
            {
                foreach (float dy in new float[] { 0, -16, 16, -32, 32, -48, 48 })
                {
                    var c = at + new Vector2(dx, dy);
                    var r = new Rect(c - plateSize / 2, plateSize);
                    bool inside = Mathf.Abs(c.y) < size.y / 2 - 8 && Mathf.Abs(c.x) < size.x / 2 - plateSize.x / 2 - 1;
                    if (inside && used.TrueForAll(u => !u.Overlaps(r))) { pos = c; used.Add(r); placed = true; break; }
                }
                if (placed) break;
            }
            var plate = UIFactory.Box("Plate", mapRect, pos, plateSize, new Color(0.1f, 0.08f, 0.1f, 0.82f));
            plate.raycastTarget = true;
            var hover = plate.gameObject.AddComponent<HoverHandle>();
            hover.Entered += () => tooltip.Set(es + "  =  " + en, en + "  =  " + es);
            hover.Exited += () => tooltip.Set("Pasa el ratón por un nombre para traducirlo.", "Point at a name to translate it.");
            var label = UIFactory.LLabel("Label", plate.transform, new Vector2(0, 1), plateSize - new Vector2(4, 0), es, en, color);
            label.fontSize = 12;
            label.resizeTextMaxSize = 12;
            UIFactory.Outline(label);
        }

        public void Toggle()
        {
            if (Open) { Close(); return; }
            OpenWindow();
        }

        protected override void Update()
        {
            base.Update();
            if (!Open) return;
            var p = player.position;
            arrow.anchoredPosition = new Vector2(p.x * Scale - size.x / 2, size.y / 2 + p.y * Scale);
            float[] angles = { 180, -90, 0, 90 };
            arrow.localRotation = Quaternion.Euler(0, 0, angles[playerSprite.Facing]);
            arrow.gameObject.SetActive(VillageController.Instance == null || VillageController.Instance.PlayerInVillage);
        }
    }
}
