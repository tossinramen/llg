using UnityEngine;
using UnityEngine.Rendering;

namespace LLG
{
    /// <summary>
    /// A character in the world, drawn from the same tinted layers as the character creator.
    /// Used for the player and for villagers. The object's position is where the feet are.
    /// </summary>
    public class CharacterSprite : MonoBehaviour
    {
        static readonly string[] Directions = { "down", "right", "up", "right" };   // 3 = left (mirrored)
        static readonly string[] LayerNames = { "outline", "pants", "shirt", "skin", "detail", "hair" };
        static readonly string[] WalkCycle = { "_w1", "", "_w2", "" };

        readonly SpriteRenderer[] layers = new SpriteRenderer[6];
        Transform body;
        SortingGroup group;
        CharacterAppearance look;
        float walkTime;
        string shown;

        public int Facing { get; private set; }     // 0 down, 1 right, 2 up, 3 left
        public bool Walking { get; set; }

        public void Build(CharacterAppearance appearance)
        {
            look = appearance;
            group = gameObject.AddComponent<SortingGroup>();
            body = new GameObject("Body").transform;
            body.SetParent(transform, false);
            body.localPosition = new Vector3(0, 0.5f, 0);
            for (int i = 0; i < layers.Length; i++)
            {
                var go = new GameObject(LayerNames[i]);
                go.transform.SetParent(body, false);
                layers[i] = go.AddComponent<SpriteRenderer>();
                layers[i].sortingOrder = i;
            }
            layers[1].color = AppearancePalette.Pants[look.pants];
            layers[2].color = AppearancePalette.Shirt[look.shirt];
            layers[3].color = AppearancePalette.Skin[look.skin];
            layers[5].color = AppearancePalette.Hair[look.hairColor];
            Refresh();
        }

        public void Face(int direction)
        {
            Facing = direction;
        }

        void LateUpdate()
        {
            walkTime = Walking ? walkTime + Time.deltaTime : 0;
            group.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);   // lower on screen = drawn in front
            Refresh();
        }

        void Refresh()
        {
            string dir = Directions[Facing];
            string frame = Walking ? WalkCycle[(int)(walkTime * 8) % WalkCycle.Length] : "";
            string key = dir + frame + Facing;
            if (key == shown) return;
            shown = key;

            body.localScale = new Vector3(Facing == 3 ? -1 : 1, 1, 1);
            Set(0, "outline_" + dir + frame);
            Set(1, "pants_" + dir + frame);
            Set(2, "shirt_" + dir);
            Set(3, "skin_" + dir + frame);
            Set(4, "detail_" + dir + frame);
            Set(5, "hair" + look.hairStyle + "_" + dir);
        }

        void Set(int layer, string spriteName)
        {
            layers[layer].sprite = UIFactory.LoadSprite("Art/Character/" + spriteName);
        }
    }
}
