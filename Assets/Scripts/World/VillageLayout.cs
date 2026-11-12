using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLG
{
    /// <summary>One placed picture from Maps/village_objects.json (all numbers are in pixels).</summary>
    [Serializable]
    public class LayoutObject
    {
        public string sprite;
        public string[] frames;   // set for animated objects
        public float fps;
        public int x, y;          // where the object's base sits in the world
        public int ax, ay;        // which pixel of the picture is its base
        public int w, h;          // picture size
        public string letter;     // the map letter of the thing (for reading and chopping)
    }

    /// <summary>A part of the world shown on its own: the village, or the inside of a building (in tiles).</summary>
    [Serializable]
    public class Area
    {
        public string key, es, en, shop, keeper;
        public int x, y, w, h;
        public bool interior;

        public bool Contains(Vector2Int tile) => tile.x >= x && tile.x < x + w && tile.y >= y && tile.y < y + h;
    }

    /// <summary>Using the door at (x, y) takes you to tile (tx, ty), facing "face".</summary>
    [Serializable]
    public class Portal
    {
        public int x, y, tx, ty, face;
    }

    /// <summary>Where an animal or monster starts (in tiles).</summary>
    [Serializable]
    public class SpawnPoint
    {
        public string kind;
        public int x, y;
        public bool room;
    }

    [Serializable]
    public class WordEntry
    {
        public string c, es, en;
    }

    /// <summary>A placed object you can click on to read, or hit (trees).</summary>
    public class ClickableObject
    {
        public SpriteRenderer renderer;
        public char letter;
        public Vector2 basePos;
        public Vector2Int Tile => VillageMap.TileAt(basePos + new Vector2(0, 0.1f));
    }

    [Serializable]
    public class VillageLayout
    {
        public LayoutObject[] objects;
        public LayoutObject[] overlays;
        public LayoutObject[] grounds;
        public Area[] areas;
        public Portal[] portals;
        public SpawnPoint[] spawns;
        public WordEntry[] words;
        public string walkable;

        public static VillageLayout Load()
        {
            var asset = Resources.Load<TextAsset>("Maps/village_objects");
            return asset == null ? new VillageLayout() : JsonUtility.FromJson<VillageLayout>(asset.text);
        }

        public Dictionary<char, string[]> WordTable()
        {
            var table = new Dictionary<char, string[]>();
            if (words != null)
                foreach (var w in words)
                    if (!string.IsNullOrEmpty(w.c)) table[w.c[0]] = new[] { w.es, w.en };
            return table;
        }

        /// <summary>Creates the ground pieces, every building/tree/prop (sorted by its base) and the water overlays.</summary>
        public List<ClickableObject> Spawn(Transform parent)
        {
            if (grounds != null)
            {
                foreach (var g in grounds)
                {
                    var r = new GameObject("Ground " + g.sprite).AddComponent<SpriteRenderer>();
                    r.transform.SetParent(parent, false);
                    r.sprite = UIFactory.LoadSprite("Art/World/" + g.sprite);
                    r.transform.position = new Vector2((g.x + g.w / 2f) / 16f, -(g.y + g.h / 2f) / 16f);
                    r.sortingOrder = -30000;
                }
            }
            var clickable = new List<ClickableObject>();
            if (objects != null)
            {
                foreach (var o in objects)
                {
                    var r = Make(parent, o, "Art/World/Objects/");
                    var baseWorld = new Vector2(o.x / 16f, -o.y / 16f);
                    r.transform.position = baseWorld + new Vector2((o.w / 2f - o.ax) / 16f, (o.ay - o.h / 2f) / 16f);
                    r.sortingOrder = Mathf.RoundToInt(-baseWorld.y * 100);
                    if (!string.IsNullOrEmpty(o.letter))
                        clickable.Add(new ClickableObject { renderer = r, letter = o.letter[0], basePos = baseWorld });
                }
            }
            if (overlays != null)
            {
                foreach (var o in overlays)
                {
                    var r = Make(parent, o, "Art/World/");
                    r.transform.position = new Vector2((o.x + o.w / 2f) / 16f, -(o.y + o.h / 2f) / 16f);
                    r.sortingOrder = -29000;
                }
            }
            return clickable;
        }

        static SpriteRenderer Make(Transform parent, LayoutObject o, string folder)
        {
            var go = new GameObject(o.sprite ?? (o.frames != null && o.frames.Length > 0 ? o.frames[0] : "Object"));
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<SpriteRenderer>();
            if (o.frames != null && o.frames.Length > 0)
            {
                var anim = go.AddComponent<SpriteAnimator>();
                anim.frames = new Sprite[o.frames.Length];
                for (int i = 0; i < o.frames.Length; i++) anim.frames[i] = UIFactory.LoadSprite(folder + o.frames[i]);
                anim.fps = o.fps > 0 ? o.fps : 3;
                r.sprite = anim.frames[0];
            }
            else
            {
                r.sprite = UIFactory.LoadSprite(folder + o.sprite);
            }
            return r;
        }
    }

    /// <summary>Flips through a few pictures to animate something (water sparkles, the fountain, windmill sails).</summary>
    public class SpriteAnimator : MonoBehaviour
    {
        public Sprite[] frames;
        public float fps = 3;
        SpriteRenderer r;
        float t;

        void Awake() => r = GetComponent<SpriteRenderer>();

        void Update()
        {
            if (frames == null || frames.Length == 0) return;
            t += Time.deltaTime * fps;
            r.sprite = frames[(int)t % frames.Length];
        }
    }

    /// <summary>A pet that trots after the player, hopping while it moves.</summary>
    public class PetFollower : MonoBehaviour
    {
        public static PetFollower Current;
        public string petId;
        Transform target;
        SpriteRenderer r;
        Sprite idle, hop;
        float hopTimer;

        public static PetFollower Spawn(Transform parent, Transform player, string petId)
        {
            var go = new GameObject("Pet");
            go.transform.SetParent(parent, false);
            go.transform.position = player.position + new Vector3(-0.9f, 0.45f, 0);
            var pet = go.AddComponent<PetFollower>();
            pet.target = player;
            pet.petId = petId;
            Current = pet;
            pet.r = go.AddComponent<SpriteRenderer>();
            pet.idle = UIFactory.LoadSprite("Art/Icons/" + petId);
            pet.hop = UIFactory.LoadSprite("Art/Icons/" + petId + "_hop");
            pet.r.sprite = pet.idle;
            return pet;
        }

        void Update()
        {
            var goal = (Vector2)target.position + new Vector2(-0.8f, 0.45f);
            var pos = (Vector2)transform.position;
            float dist = Vector2.Distance(pos, goal);
            bool moving = dist > 0.25f;
            if (dist > 6) pos = goal;                                    // teleport if left far behind (or through a door)
            else if (moving) pos = Vector2.MoveTowards(pos, goal, Mathf.Max(3f, dist * 3f) * Time.deltaTime);
            if (moving && Mathf.Abs(goal.x - pos.x) > 0.05f) r.flipX = goal.x < pos.x;   // pictures face right
            transform.position = pos;
            hopTimer = moving ? hopTimer + Time.deltaTime : 0;
            r.sprite = moving && (int)(hopTimer * 8) % 2 == 1 ? hop : idle;
            r.sortingOrder = Mathf.RoundToInt(-(pos.y - 0.5f) * 100);
        }
    }
}
