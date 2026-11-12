using System;
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
        public int x, y;          // where the object's base sits in the village
        public int ax, ay;        // which pixel of the picture is its base
        public int w, h;          // picture size
        public string letter;     // the map letter of the thing (for left-click reading)
    }

    /// <summary>A placed object you can click on to read its name.</summary>
    public struct ClickableObject
    {
        public SpriteRenderer renderer;
        public char letter;
    }

    [Serializable]
    public class VillageLayout
    {
        public LayoutObject[] objects;
        public LayoutObject[] overlays;

        public static VillageLayout Load()
        {
            var asset = Resources.Load<TextAsset>("Maps/village_objects");
            return asset == null ? new VillageLayout() : JsonUtility.FromJson<VillageLayout>(asset.text);
        }

        /// <summary>Creates every building, tree and prop as a sprite sorted by its base, plus the water overlays.</summary>
        public System.Collections.Generic.List<ClickableObject> Spawn(Transform parent)
        {
            var clickable = new System.Collections.Generic.List<ClickableObject>();
            if (objects != null)
            {
                foreach (var o in objects)
                {
                    var r = Make(parent, o, "Art/World/Objects/");
                    var baseWorld = new Vector2(o.x / 16f, -o.y / 16f);
                    r.transform.position = baseWorld + new Vector2((o.w / 2f - o.ax) / 16f, (o.ay - o.h / 2f) / 16f);
                    r.sortingOrder = Mathf.RoundToInt(-baseWorld.y * 100);
                    if (!string.IsNullOrEmpty(o.letter)) clickable.Add(new ClickableObject { renderer = r, letter = o.letter[0] });
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

    /// <summary>Flips through a few pictures to animate something (water sparkles, the fountain).</summary>
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
            if (dist > 8) pos = goal;                                    // teleport if left far behind
            else if (moving) pos = Vector2.MoveTowards(pos, goal, Mathf.Max(3f, dist * 3f) * Time.deltaTime);
            if (moving && Mathf.Abs(goal.x - pos.x) > 0.05f) r.flipX = goal.x < pos.x;   // pictures face right
            transform.position = pos;
            hopTimer = moving ? hopTimer + Time.deltaTime : 0;
            r.sprite = moving && (int)(hopTimer * 8) % 2 == 1 ? hop : idle;
            // the sprite's centre is half a tile above its feet
            r.sortingOrder = Mathf.RoundToInt(-(pos.y - 0.5f) * 100);
        }
    }
}
