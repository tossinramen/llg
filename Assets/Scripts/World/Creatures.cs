using System.Collections.Generic;
using UnityEngine;

namespace LLG
{
    /// <summary>Shared little helpers for world effects.</summary>
    public static class WorldFx
    {
        static Sprite white, pixel, square;

        /// <summary>A 1x1 unit white square centred on its position.</summary>
        public static Sprite Square => square ??= Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);

        /// <summary>A 1-unit wide white bar whose left edge is its position (scale X to set its width).</summary>
        public static Sprite Bar => white ??= Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0, 0.5f), 4);

        /// <summary>One art pixel (1/16 of a tile), for particles.</summary>
        public static Sprite Pixel => pixel ??= Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 64);

        /// <summary>A puff of little squares (leaves from a tree, sparks from a hit).</summary>
        public static void Burst(Vector2 at, Color color, int count, int order)
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Particle");
                go.transform.position = at;
                var r = go.AddComponent<SpriteRenderer>();
                r.sprite = Pixel;
                r.color = color;
                r.sortingOrder = order;
                go.transform.localScale = Vector3.one * Random.Range(1f, 2.2f);
                go.AddComponent<Particle>().velocity = new Vector2(Random.Range(-2f, 2f), Random.Range(0.5f, 3f));
            }
        }
    }

    public class Particle : MonoBehaviour
    {
        public Vector2 velocity;
        float life = 0.6f;
        SpriteRenderer r;

        void Awake() => r = GetComponent<SpriteRenderer>();

        void Update()
        {
            life -= Time.deltaTime;
            velocity.y -= 7 * Time.deltaTime;
            transform.position += (Vector3)(velocity * Time.deltaTime);
            var c = r.color;
            c.a = Mathf.Clamp01(life / 0.6f);
            r.color = c;
            if (life <= 0) Destroy(gameObject);
        }
    }

    /// <summary>A small health bar that floats above a mob.</summary>
    public class HealthBar
    {
        readonly Transform root;
        readonly SpriteRenderer back, fill;
        const float Width = 1f;

        public HealthBar(Transform parent, float height)
        {
            root = new GameObject("HealthBar").transform;
            root.SetParent(parent, false);
            root.localPosition = new Vector3(-Width / 2, height, 0);
            back = Make("Back", new Color(0.15f, 0.08f, 0.08f), 0);
            back.transform.localScale = new Vector3(Width, 0.14f, 1);
            fill = Make("Fill", new Color(0.86f, 0.22f, 0.2f), 1);
            fill.transform.localPosition = new Vector3(0.03f, 0, 0);
        }

        SpriteRenderer Make(string name, Color c, int order)
        {
            var r = new GameObject(name).AddComponent<SpriteRenderer>();
            r.transform.SetParent(root, false);
            r.sprite = WorldFx.Bar;
            r.color = c;
            r.sortingOrder = 30000 + order;
            return r;
        }

        public void Set(float fraction, bool visible)
        {
            root.gameObject.SetActive(visible);
            fill.transform.localScale = new Vector3((Width - 0.06f) * Mathf.Clamp01(fraction), 0.08f, 1);
            fill.color = fraction > 0.5f ? new Color(0.3f, 0.8f, 0.3f) : fraction > 0.25f ? new Color(0.95f, 0.75f, 0.2f) : new Color(0.86f, 0.22f, 0.2f);
        }
    }

    /// <summary>
    /// A wild animal: wolves in the forest, wild dogs in the olive grove. They leave you alone until
    /// you hit one; then it (and its pack) chases and bites you. Each has an HP bar. When killed it
    /// drops Lexa (and sometimes a pelt) and comes back after a while.
    /// </summary>
    public class Mob : MonoBehaviour
    {
        public string kind, spanish, english;
        public int maxHp, hp, damage, xp, level;
        public bool Dead => hp <= 0;
        public bool Hostile { get; private set; }

        VillageController village;
        SpriteRenderer r;
        Sprite[] frames;
        HealthBar bar;
        Vector2 home, wanderGoal;
        float speed, thinkTimer, biteTimer, flashTimer, respawnTimer, animTimer;
        Vector2 knock;
        float frozen, slowed;

        /// <summary>Hielo (the spell): the animal can't move or bite for a while.</summary>
        public void Freeze(float seconds) { if (!Dead) frozen = Mathf.Max(frozen, seconds); }

        /// <summary>The Hielo rune on your weapon: the animal moves at half speed for a while.</summary>
        public void Slow(float seconds) { if (!Dead) slowed = Mathf.Max(slowed, seconds); }

        public static Mob Spawn(VillageController owner, Transform parent, string kind, Vector2 at, int level = 0)
        {
            var go = new GameObject("Mob " + kind);
            go.transform.SetParent(parent, false);
            go.transform.position = at;
            var m = go.AddComponent<Mob>();
            m.village = owner;
            m.kind = kind;
            m.home = at;
            m.wanderGoal = at;
            m.r = go.AddComponent<SpriteRenderer>();
            m.frames = new[] { UIFactory.LoadSprite("Art/Mobs/" + kind + "_0"), UIFactory.LoadSprite("Art/Mobs/" + kind + "_1") };
            m.r.sprite = m.frames[0];
            if (kind == "wolf") { m.spanish = "El lobo"; m.english = "Wolf"; m.maxHp = 14; m.damage = 3; m.speed = 3.3f; m.xp = 30; }
            else { m.spanish = "El perro salvaje"; m.english = "Wild Dog"; m.maxHp = 9; m.damage = 2; m.speed = 3.5f; m.xp = 20; }
            m.level = level > 0 ? level : kind == "wolf" ? 6 : 2;
            m.maxHp = Combat.MobHp(m.maxHp, m.level);            // stronger with every level (see Combat)
            m.hp = m.maxHp;
            m.bar = new HealthBar(go.transform, 0.75f);
            m.bar.Set(1, false);
            return m;
        }

        public Vector2 Position => transform.position;

        /// <summary>True if this point (e.g. the mouse) is over the animal.</summary>
        public bool Covers(Vector2 world) => !Dead && Mathf.Abs(world.x - Position.x) < 0.9f && world.y > Position.y - 0.7f && world.y < Position.y + 0.9f;

        public void TakeHit(int amount, Vector2 from)
        {
            if (Dead) return;
            hp -= amount;
            flashTimer = 0.15f;
            knock = ((Vector2)transform.position - from).normalized * 4f;
            WorldFx.Burst(Position, new Color(0.9f, 0.2f, 0.2f), 5, r.sortingOrder + 1);
            Provoke();
            foreach (var other in village.Mobs)       // the rest of the pack joins in
                if (other != this && other.kind == kind && Vector2.Distance(other.Position, Position) < 6) other.Provoke();
            if (hp <= 0) Die();
            bar.Set(hp / (float)maxHp, !Dead);
        }

        public void Provoke() => Hostile = true;

        public void Calm() => Hostile = false;

        void Die()
        {
            r.enabled = false;
            bar.Set(0, false);
            respawnTimer = 30f;
            Hostile = false;
            int coins = kind == "wolf" ? Combat.GoldReward(6, 12, level) : Combat.GoldReward(3, 8, level);
            GameState.AddGold(coins);
            GameState.AddXp(SkillType.Combat, Combat.XpReward(xp, level));
            GameState.Say("¡Derrotaste a " + spanish.ToLower() + "!");
            if (kind == "wolf" && Random.value < 0.6f) village.DropItem("pelt", Position);
        }

        void Update()
        {
            if (Dead)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0)
                {
                    hp = maxHp;
                    transform.position = home;
                    r.enabled = true;
                }
                return;
            }

            if (frozen > 0)
            {
                frozen -= Time.deltaTime;
                r.color = new Color(0.6f, 0.85f, 1f);
                return;
            }
            slowed -= Time.deltaTime;

            var player = village.PlayerPosition;
            Vector2 pos = transform.position;
            Vector2 move = Vector2.zero;
            float dist = Vector2.Distance(pos, player);

            if (Hostile && (dist > 14 || Vector2.Distance(pos, home) > 18 || !village.PlayerInVillage))
                Hostile = false;     // gave up: wander back home

            if (Hostile)
            {
                if (dist > 0.9f) move = (player - pos).normalized * speed;
                biteTimer -= Time.deltaTime;
                if (dist < 1.1f && biteTimer <= 0)
                {
                    biteTimer = 1.2f;
                    village.DamagePlayer(Combat.MobHit(damage, level), pos);
                }
            }
            else
            {
                thinkTimer -= Time.deltaTime;
                if (thinkTimer <= 0)
                {
                    thinkTimer = Random.Range(1.5f, 4f);
                    wanderGoal = home + Random.insideUnitCircle * 3f;
                }
                if (Vector2.Distance(pos, wanderGoal) > 0.2f) move = (wanderGoal - pos).normalized * 1.2f;
            }

            if (slowed > 0) move *= 0.5f;
            move += knock;
            knock = Vector2.MoveTowards(knock, Vector2.zero, 20 * Time.deltaTime);
            var step = move * Time.deltaTime;
            if (village.Map.Walkable(VillageMap.TileAt(pos + new Vector2(step.x, -0.4f)))) pos.x += step.x;
            if (village.Map.Walkable(VillageMap.TileAt(pos + new Vector2(0, step.y - 0.4f)))) pos.y += step.y;
            transform.position = pos;

            if (Mathf.Abs(move.x) > 0.1f) r.flipX = move.x < 0;      // pictures face right
            animTimer = move.sqrMagnitude > 0.05f ? animTimer + Time.deltaTime : 0;
            r.sprite = frames[(int)(animTimer * 8) % 2];
            flashTimer -= Time.deltaTime;
            r.color = flashTimer > 0 ? new Color(1, 0.4f, 0.4f) : slowed > 0 ? new Color(0.75f, 0.9f, 1f) : Color.white;
            r.sortingOrder = Mathf.RoundToInt(-(pos.y - 0.45f) * 100);
            bar.Set(hp / (float)maxHp, Hostile || hp < maxHp);
        }
    }

    /// <summary>A harmless animal that wanders around: village cats and dogs, chickens, pets in the shop.</summary>
    public class Wanderer : MonoBehaviour
    {
        public static readonly List<Wanderer> All = new List<Wanderer>();
        public string kind;
        VillageController village;
        SpriteRenderer r;
        Sprite a, b;
        Vector2 home, goal;
        float radius, timer, anim;
        bool peck;

        public static void Spawn(VillageController owner, Transform parent, string kind, Vector2 at, float radius)
        {
            var go = new GameObject("Animal " + kind);
            go.transform.SetParent(parent, false);
            go.transform.position = at;
            var w = go.AddComponent<Wanderer>();
            w.village = owner;
            w.kind = kind;
            w.home = w.goal = at;
            w.radius = radius;
            w.r = go.AddComponent<SpriteRenderer>();
            if (kind == "chicken")
            {
                w.a = UIFactory.LoadSprite("Art/Mobs/chicken_0");
                w.b = UIFactory.LoadSprite("Art/Mobs/chicken_1");
                w.peck = true;
            }
            else
            {
                w.a = UIFactory.LoadSprite("Art/Icons/" + kind);
                w.b = UIFactory.LoadSprite("Art/Icons/" + kind + "_hop");
            }
            w.r.sprite = w.a;
        }

        void OnEnable() => All.Add(this);
        void OnDisable() => All.Remove(this);

        /// <summary>True if this point (e.g. the mouse) is over the animal and it is visible.</summary>
        public bool Covers(Vector2 world) => r.enabled && gameObject.activeInHierarchy
            && Mathf.Abs(world.x - transform.position.x) < 0.6f && Mathf.Abs(world.y - transform.position.y) < 0.6f;

        void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = Random.Range(1.5f, 5f);
                goal = Random.value < 0.4f ? (Vector2)transform.position : home + Random.insideUnitCircle * radius;
            }
            Vector2 pos = transform.position;
            var move = Vector2.MoveTowards(pos, goal, 1.1f * Time.deltaTime) - pos;
            bool moving = move.sqrMagnitude > 0.00001f;
            if (moving && village.Map.Walkable(VillageMap.TileAt(pos + move + new Vector2(0, -0.4f)))) pos += move;
            else goal = pos;
            if (Mathf.Abs(move.x) > 0.0001f) r.flipX = move.x < 0;
            transform.position = pos;
            anim += Time.deltaTime;
            r.sprite = (moving || peck) && (int)(anim * (peck && !moving ? 2 : 8)) % 2 == 1 ? b : a;
            r.sortingOrder = Mathf.RoundToInt(-(pos.y - 0.45f) * 100);
        }
    }

    /// <summary>Something lying on the ground (wood from a tree, a wolf pelt). Walk over it to pick it up.</summary>
    public class Pickup : MonoBehaviour
    {
        public string itemId;
        VillageController village;
        float t, life = 90f;
        Vector2 start;

        public static void Spawn(VillageController owner, string id, Vector2 at)
        {
            var go = new GameObject("Pickup " + id);
            go.transform.position = at;
            var p = go.AddComponent<Pickup>();
            p.itemId = id;
            p.village = owner;
            p.start = at;
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = UIFactory.LoadSprite("Art/Icons/" + ItemDatabase.Get(id).icon);
            r.sortingOrder = Mathf.RoundToInt(-(at.y - 0.3f) * 100);
            go.transform.localScale = Vector3.one * 0.8f;
        }

        void Update()
        {
            t += Time.deltaTime;
            life -= Time.deltaTime;
            transform.position = start + new Vector2(0, 0.12f + Mathf.Abs(Mathf.Sin(t * 3)) * 0.12f);
            if (life <= 0 || village == null) { Destroy(gameObject); return; }
            if (Vector2.Distance(village.PlayerPosition, start) < 0.9f && GameState.AddItem(itemId))
            {
                var item = ItemDatabase.Get(itemId);
                GameState.Say("+1 " + GameState.Capital(item.spanish));
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Chopping trees: every hit adds cracks (like breaking a block in Minecraft) and shakes the tree.
    /// When it breaks it leaves a stump and drops wood, then grows back a few seconds later.
    /// Axes chop much faster than fists or swords.
    /// </summary>
    public class Trees : MonoBehaviour
    {
        const int TreeHealth = 6;
        const float RegrowTime = 3f;

        class TreeState
        {
            public ClickableObject obj;
            public Sprite sprite;
            public Vector3 restPos;
            public int damage;
            public float shake, regrow;
            public SpriteRenderer cracks;
        }

        readonly Dictionary<Vector2Int, TreeState> trees = new Dictionary<Vector2Int, TreeState>();
        Sprite[] crackSprites;
        Sprite stump;
        VillageController village;

        public void Build(VillageController owner, List<ClickableObject> objects)
        {
            village = owner;
            crackSprites = new Sprite[4];
            for (int i = 0; i < 4; i++) crackSprites[i] = UIFactory.LoadSprite("Art/World/crack_" + i);
            stump = UIFactory.LoadSprite("Art/World/stump");
            foreach (var o in objects)
                if ("ZOY}".IndexOf(o.letter) >= 0)
                    trees[o.Tile] = new TreeState { obj = o, sprite = o.renderer.sprite, restPos = o.renderer.transform.position };
        }

        public bool IsTree(Vector2Int tile) => trees.ContainsKey(tile);

        public ClickableObject At(Vector2Int tile) => trees.TryGetValue(tile, out var t) ? t.obj : null;

        /// <summary>Hits the tree on this tile. Returns true if it was a tree.</summary>
        public bool Chop(Vector2Int tile, int strength)
        {
            if (!trees.TryGetValue(tile, out var t) || t.regrow > 0) return false;
            t.damage += Mathf.Max(1, strength);
            t.shake = 0.18f;
            var r = t.obj.renderer;
            var b = r.bounds;
            WorldFx.Burst(new Vector2(b.center.x, b.center.y + b.extents.y * 0.3f), LeafColour(t.obj.letter), 6, r.sortingOrder + 1);
            if (t.cracks == null)
            {
                t.cracks = new GameObject("Cracks").AddComponent<SpriteRenderer>();
                t.cracks.transform.SetParent(r.transform, false);
            }
            t.cracks.enabled = true;
            t.cracks.sortingOrder = r.sortingOrder + 1;
            t.cracks.sprite = crackSprites[Mathf.Clamp(t.damage * 4 / TreeHealth, 0, 3)];
            t.cracks.transform.localScale = new Vector3(b.size.x / 2f * 0.8f, b.size.y / 2f * 0.8f, 1);

            if (t.damage >= TreeHealth)
            {
                r.sprite = stump;
                r.transform.position = new Vector3(t.obj.basePos.x, t.obj.basePos.y + 0.4f, 0);
                t.cracks.enabled = false;
                t.damage = 0;
                t.regrow = RegrowTime;
                int pieces = Random.Range(1, 3);
                for (int i = 0; i < pieces; i++)
                    village.DropItem("wood", t.obj.basePos + new Vector2(Random.Range(-0.6f, 0.6f), -0.6f));
                GameState.AddXp(SkillType.Woodcutting, 6);
            }
            return true;
        }

        static Color LeafColour(char letter) => letter switch
        {
            'O' => new Color(0.6f, 0.66f, 0.45f),
            'Z' => new Color(0.35f, 0.6f, 0.3f),
            _ => new Color(0.2f, 0.4f, 0.25f),
        };

        void Update()
        {
            foreach (var t in trees.Values)
            {
                var tr = t.obj.renderer.transform;
                if (t.regrow > 0)
                {
                    t.regrow -= Time.deltaTime;
                    if (t.regrow <= 0)
                    {
                        t.obj.renderer.sprite = t.sprite;
                        tr.position = t.restPos;
                    }
                    continue;
                }
                if (t.shake > 0)
                {
                    t.shake -= Time.deltaTime;
                    tr.position = t.restPos + new Vector3(Mathf.Sin(t.shake * 90) * 0.06f, 0, 0);
                    if (t.shake <= 0) tr.position = t.restPos;
                }
            }
        }
    }
}
