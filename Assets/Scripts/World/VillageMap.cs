using System.Collections.Generic;
using UnityEngine;

namespace LLG
{
    /// <summary>
    /// The village as a grid of letters, loaded from Resources/Maps/village.txt
    /// (that file and the village picture are both made by Tools/art/generate_world.py).
    /// See the top of generate_world.py for what every letter means.
    /// Tile (0,0) is the top-left corner; one tile is one world unit.
    /// </summary>
    public class VillageMap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        char[,] cells;

        public static VillageMap Load()
        {
            var map = new VillageMap();
            var asset = Resources.Load<TextAsset>("Maps/village");
            var lines = asset.text.Replace("\r", "").Split('\n');
            map.Height = lines.Length;
            map.Width = lines[0].Length;
            map.cells = new char[map.Width, map.Height];
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map.cells[x, y] = x < lines[y].Length ? lines[y][x] : '#';
            return map;
        }

        public char At(Vector2Int tile)
        {
            if (tile.x < 0 || tile.y < 0 || tile.x >= Width || tile.y >= Height) return '#';
            return cells[tile.x, tile.y];
        }

        public void Set(Vector2Int tile, char value) => cells[tile.x, tile.y] = value;

        /// <summary>Grass, path, dock and the archery range floor can be walked on.</summary>
        public bool Walkable(Vector2Int tile)
        {
            char c = At(tile);
            return c == '.' || c == ',' || c == '=' || c == ';';
        }

        public List<Vector2Int> Find(char value)
        {
            var found = new List<Vector2Int>();
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (cells[x, y] == value) found.Add(new Vector2Int(x, y));
            return found;
        }

        public static Vector2 Center(Vector2Int tile) => new Vector2(tile.x + 0.5f, -(tile.y + 0.5f));

        public static Vector2Int TileAt(Vector2 world) => new Vector2Int(Mathf.FloorToInt(world.x), Mathf.FloorToInt(-world.y));
    }
}
