using UnityEngine;

namespace LLG
{
    /// <summary>How a melee attack moves: a fist jab, a blade stab, or an axe chop.</summary>
    public enum SwingStyle { Jab, Stab, Chop }

    /// <summary>
    /// The attack shown when the player right-clicks. Fists jab and swords stab straight out from the
    /// side you face; axes chop in a short arc in front of you (mirrored for left and right).
    /// </summary>
    public class SwingEffect : MonoBehaviour
    {
        static readonly float[] FacingAngles = { 180f, -90f, 0f, 90f };   // down, right, up, left

        Transform blade;
        SpriteRenderer bladeRenderer;
        float baseAngle, time, duration;
        int facing;
        SwingStyle style;
        bool upright;      // icons drawn standing up (the double axe) instead of pointing top-right

        public static void Spawn(Vector3 hand, int facing, Sprite sprite, Color tint, SwingStyle style, int sortingOrder, Transform follow = null)
        {
            var go = new GameObject("Swing");
            go.transform.position = hand;
            if (follow != null) go.transform.SetParent(follow, true);     // moves with you while you walk
            var fx = go.AddComponent<SwingEffect>();
            var child = new GameObject("Blade");
            child.transform.SetParent(go.transform, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = tint;
            renderer.sortingOrder = sortingOrder;
            fx.blade = child.transform;
            fx.bladeRenderer = renderer;
            fx.style = style;
            fx.upright = sprite != null && sprite.name == "item_doubleaxe";
            fx.facing = facing;
            fx.baseAngle = FacingAngles[facing];
            fx.duration = style == SwingStyle.Chop ? 0.26f : style == SwingStyle.Stab ? 0.22f : 0.2f;
            fx.Apply(0);
        }

        void Apply(float u)
        {
            transform.rotation = Quaternion.Euler(0, 0, baseAngle);
            switch (style)
            {
                case SwingStyle.Jab:
                {
                    // the fist starts at the edge of the body on the side you face and only moves outwards
                    float s = Mathf.Sin(u * Mathf.PI);
                    blade.localPosition = new Vector3(0, 0.3f * s, 0);
                    blade.localRotation = Quaternion.Euler(0, 0, -baseAngle);      // keep the fist upright
                    blade.localScale = Vector3.one * (0.35f + 0.25f * s);
                    break;
                }
                case SwingStyle.Stab:
                {
                    // a quick thrust straight out and back: no swinging at all
                    float thrust = u < 0.35f ? 1 - Mathf.Pow(1 - u / 0.35f, 3) : 1 - (u - 0.35f) / 0.65f * 0.7f;
                    blade.localPosition = new Vector3(0, 0.1f + 0.55f * thrust, 0);
                    blade.localRotation = Quaternion.Euler(0, 0, -45);              // pictures point top-right: turn them to point out
                    blade.localScale = Vector3.one;
                    break;
                }
                default:
                {
                    // an axe chop: a short, fast arc in front of you, from above down, mirrored when facing left
                    float side = facing == 3 ? -1 : 1;
                    float e = u < 0.55f ? 1 - Mathf.Pow(1 - u / 0.55f, 2) : 1;
                    transform.rotation = Quaternion.Euler(0, 0, baseAngle + side * Mathf.Lerp(55, -45, e));
                    blade.localPosition = new Vector3(0, 0.5f, 0);
                    blade.localRotation = Quaternion.Euler(0, 0, upright ? 0 : -45);
                    blade.localScale = new Vector3(side, 1, 1) * 1.1f;
                    break;
                }
            }
            var c = bladeRenderer.color;
            c.a = u > 0.8f ? 1 - (u - 0.8f) / 0.2f : 1;
            bladeRenderer.color = c;
        }

        void Update()
        {
            time += Time.deltaTime;
            Apply(Mathf.Clamp01(time / duration));
            if (time >= duration) Destroy(gameObject);
        }
    }

    /// <summary>
    /// Bows and guns: the weapon is held out in front of you, pointing where you shoot, bigger than the
    /// arms so it is easy to see. Guns kick back with a muzzle flash; bows pull back and release.
    /// </summary>
    public class AimEffect : MonoBehaviour
    {
        const float Duration = 0.32f;
        Transform weapon, flash;
        SpriteRenderer weaponRenderer, flashRenderer;
        Vector2 dir;
        bool gun;
        float time;
        static Sprite flashSprite;

        public static void Spawn(Vector2 hand, Vector2 direction, Sprite sprite, bool isGun, int sortingOrder, Transform follow)
        {
            var go = new GameObject("Aim");
            go.transform.position = hand;
            if (follow != null) go.transform.SetParent(follow, true);
            var fx = go.AddComponent<AimEffect>();
            fx.dir = direction.normalized;
            fx.gun = isGun;
            var w = new GameObject("Weapon");
            w.transform.SetParent(go.transform, false);
            fx.weaponRenderer = w.AddComponent<SpriteRenderer>();
            fx.weaponRenderer.sprite = sprite;
            fx.weaponRenderer.sortingOrder = sortingOrder;
            fx.weapon = w.transform;
            float angle = Mathf.Atan2(fx.dir.y, fx.dir.x) * Mathf.Rad2Deg;
            bool flip = fx.dir.x < -0.01f;                                  // pointing left: mirror so it is not upside down
            w.transform.localRotation = Quaternion.Euler(0, 0, flip ? angle - 135 : angle - 45);
            w.transform.localScale = new Vector3(flip ? -1.35f : 1.35f, 1.35f, 1);
            if (isGun)
            {
                var f = new GameObject("Flash");
                f.transform.SetParent(go.transform, false);
                fx.flashRenderer = f.AddComponent<SpriteRenderer>();
                fx.flashRenderer.sprite = FlashSprite();
                fx.flashRenderer.sortingOrder = sortingOrder + 1;
                fx.flash = f.transform;
                f.transform.localPosition = fx.dir * 1.15f;
            }
            fx.Apply(0);
        }

        /// <summary>A small yellow-white star for the muzzle flash (made once, in code).</summary>
        static Sprite FlashSprite()
        {
            if (flashSprite != null) return flashSprite;
            const int n = 12;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = x - 5.5f, dy = y - 5.5f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    bool ray = Mathf.Abs(dx) < 1 || Mathf.Abs(dy) < 1 || Mathf.Abs(Mathf.Abs(dx) - Mathf.Abs(dy)) < 0.8f;
                    var c = r < 2.2f ? new Color(1, 1, 0.85f, 1) : (ray && r < 5.8f) ? new Color(1, 0.8f, 0.2f, 1 - r / 6f) : Color.clear;
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            flashSprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 16);
            return flashSprite;
        }

        void Apply(float u)
        {
            float kick;
            if (gun) kick = u < 0.12f ? -0.18f * (u / 0.12f) : -0.18f * (1 - (u - 0.12f) / 0.88f);      // recoil
            else kick = u < 0.3f ? -0.12f * (u / 0.3f) : u < 0.4f ? Mathf.Lerp(-0.12f, 0.06f, (u - 0.3f) / 0.1f) : 0.06f;   // draw, release
            weapon.localPosition = dir * (0.45f + kick);
            var c = weaponRenderer.color;
            c.a = u > 0.75f ? 1 - (u - 0.75f) / 0.25f : 1;
            weaponRenderer.color = c;
            if (flash != null)
            {
                float f = u < 0.25f ? 1 - u / 0.25f : 0;
                flash.localScale = Vector3.one * (0.6f + 0.8f * f);
                flashRenderer.color = new Color(1, 1, 1, f);
            }
        }

        void Update()
        {
            time += Time.deltaTime;
            Apply(Mathf.Clamp01(time / Duration));
            if (time >= Duration) Destroy(gameObject);
        }
    }

    /// <summary>An arrow fired from a bow. It flies until it hits an animal or something solid.</summary>
    public class ArrowShot : MonoBehaviour
    {
        const float Speed = 13f, Range = 8f, Height = 0.85f;

        VillageController village;
        Vector2 direction;
        float travelled;
        int damage;

        public static void Spawn(VillageController owner, Vector2 feet, Vector2 dir, int damage, Sprite sprite, int sortingOrder)
        {
            var go = new GameObject("Arrow");
            dir = dir.normalized;
            go.transform.position = feet + new Vector2(0, Height) + dir * 0.4f;
            go.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);   // the picture points right
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder + 6;
            bool bullet = sprite != null && sprite.name.Contains("bullet");
            go.transform.localScale = Vector3.one * (bullet ? 1.6f : 1.25f);
            var trail = go.AddComponent<TrailRenderer>();                  // a streak behind it so you can follow the shot
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.time = bullet ? 0.09f : 0.12f;
            trail.startWidth = bullet ? 0.12f : 0.08f;
            trail.endWidth = 0;
            trail.startColor = bullet ? new Color(1f, 0.9f, 0.4f, 0.9f) : new Color(1f, 1f, 1f, 0.6f);
            trail.endColor = new Color(1f, 1f, 1f, 0);
            trail.sortingOrder = sortingOrder + 5;
            var arrow = go.AddComponent<ArrowShot>();
            arrow.village = owner;
            arrow.direction = dir;
            arrow.damage = damage;
        }

        void Update()
        {
            float step = Speed * Time.deltaTime;
            transform.position += (Vector3)(direction * step);
            travelled += step;

            var ground = (Vector2)transform.position - new Vector2(0, Height);
            foreach (var mob in village.Mobs)
            {
                if (!mob.Dead && Vector2.Distance(mob.Position, ground + new Vector2(0, 0.4f)) < 0.7f)
                {
                    village.HitMob(mob, damage, ground - direction);
                    Destroy(gameObject);
                    return;
                }
            }
            var tile = VillageMap.TileAt(ground);
            if (!village.Map.Walkable(tile))
            {
                village.Hit(tile, 1);
                Destroy(gameObject);
            }
            else if (travelled > Range)
            {
                Destroy(gameObject);
            }
        }
    }
}
