using UnityEngine;

namespace LLG
{
    /// <summary>The short swing (weapon) or jab (fist) shown when the player right-clicks.</summary>
    public class SwingEffect : MonoBehaviour
    {
        const float Duration = 0.2f;
        static readonly float[] FacingAngles = { 180f, -90f, 0f, 90f };   // down, right, up, left

        Transform blade;
        float baseAngle, time;
        bool jab;

        public static void Spawn(Vector3 hand, int facing, Sprite sprite, Color tint, bool jab, int sortingOrder)
        {
            var go = new GameObject("Swing");
            go.transform.position = hand;
            var fx = go.AddComponent<SwingEffect>();
            var child = new GameObject("Blade");
            child.transform.SetParent(go.transform, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = tint;
            renderer.sortingOrder = sortingOrder;
            fx.blade = child.transform;
            fx.jab = jab;
            fx.baseAngle = FacingAngles[facing];
            fx.Apply(0);
        }

        void Apply(float u)
        {
            if (jab)
            {
                transform.rotation = Quaternion.Euler(0, 0, baseAngle);
                blade.localPosition = new Vector3(0, 0.3f + 0.4f * Mathf.Sin(u * Mathf.PI), 0);
            }
            else
            {
                // weapon pictures point to the top-right, so turn them 45 degrees to point outwards
                transform.rotation = Quaternion.Euler(0, 0, baseAngle + Mathf.Lerp(70, -70, u));
                blade.localPosition = new Vector3(0, 0.55f, 0);
                blade.localRotation = Quaternion.Euler(0, 0, -45);
            }
        }

        void Update()
        {
            time += Time.deltaTime;
            Apply(Mathf.Clamp01(time / Duration));
            if (time >= Duration) Destroy(gameObject);
        }
    }

    /// <summary>An arrow fired from the bow. It flies until it hits something solid.</summary>
    public class ArrowShot : MonoBehaviour
    {
        const float Speed = 12f, Range = 7f, Height = 0.6f;
        static readonly float[] FacingAngles = { -90f, 0f, 90f, 180f };   // the arrow picture points right

        VillageController village;
        Vector2 direction;
        float travelled;

        public static void Spawn(VillageController owner, Vector2 feet, int facing, Sprite sprite, int sortingOrder)
        {
            var go = new GameObject("Arrow");
            var dir = PlayerController.FacingVectors[facing];
            go.transform.position = feet + new Vector2(0, Height) + dir * 0.4f;
            go.transform.rotation = Quaternion.Euler(0, 0, FacingAngles[facing]);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            var arrow = go.AddComponent<ArrowShot>();
            arrow.village = owner;
            arrow.direction = dir;
        }

        void Update()
        {
            float step = Speed * Time.deltaTime;
            transform.position += (Vector3)(direction * step);
            travelled += step;

            var tile = VillageMap.TileAt((Vector2)transform.position - new Vector2(0, Height));
            if (!village.Map.Walkable(tile))
            {
                village.Hit(tile);
                Destroy(gameObject);
            }
            else if (travelled > Range)
            {
                Destroy(gameObject);
            }
        }
    }
}
