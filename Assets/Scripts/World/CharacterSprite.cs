using UnityEngine;
using UnityEngine.Rendering;

namespace LLG
{
    /// <summary>
    /// A character in the world, drawn from the same tinted layers as the character creator.
    /// Used for the player and for villagers. The object's position is where the feet are.
    /// Handles the walk cycle, jumping (a hop with a shrinking shadow) and crouching (squashed down).
    /// </summary>
    public class CharacterSprite : MonoBehaviour
    {
        static readonly string[] Directions = { "down", "right", "up", "right" };   // 3 = left (mirrored)
        static readonly string[] LayerNames = { "outline", "pants", "shirt", "skin", "detail", "hair" };
        static readonly string[] WalkCycle = { "_w1", "", "_w2", "" };
        const float JumpTime = 0.45f, JumpHeight = 0.55f, BodyHalfHeight = 0.75f;

        readonly SpriteRenderer[] layers = new SpriteRenderer[6];
        Transform body, shadow;
        SortingGroup group;
        CharacterAppearance look;
        float walkTime, jumpTimer = -1;
        string shown;

        public int Facing { get; private set; }     // 0 down, 1 right, 2 up, 3 left
        public bool Walking { get; set; }
        public bool Running { get; set; }
        public bool Crouching { get; set; }
        public bool Jumping => jumpTimer >= 0;

        public void Build(CharacterAppearance appearance)
        {
            look = appearance;
            group = gameObject.AddComponent<SortingGroup>();

            var shadowRenderer = new GameObject("Shadow").AddComponent<SpriteRenderer>();
            shadow = shadowRenderer.transform;
            shadow.SetParent(transform, false);
            shadow.localPosition = new Vector3(0, -0.2f, 0);
            shadowRenderer.sprite = UIFactory.LoadSprite("Art/UI/shadow");
            shadowRenderer.sortingOrder = -1;

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

        public void Face(int direction) => Facing = direction;

        /// <summary>Starts a hop, unless already in the air or crouching.</summary>
        public void Jump()
        {
            if (!Jumping && !Crouching) jumpTimer = 0;
        }

        void LateUpdate()
        {
            float animSpeed = Running ? 1.7f : Crouching ? 0.6f : 1f;
            walkTime = Walking ? walkTime + Time.deltaTime * animSpeed : 0;

            float height = 0;
            if (Jumping)
            {
                jumpTimer += Time.deltaTime;
                if (jumpTimer >= JumpTime) jumpTimer = -1;
                else height = Mathf.Sin(Mathf.PI * jumpTimer / JumpTime) * JumpHeight;
            }

            // crouching squashes the body down while keeping the feet on the ground
            float squash = Crouching ? 0.8f : 1f;
            float widen = Crouching ? 1.08f : 1f;
            body.localScale = new Vector3((Facing == 3 ? -1 : 1) * widen, squash, 1);
            body.localPosition = new Vector3(0, 0.5f - (1 - squash) * BodyHalfHeight + height, 0);
            shadow.localScale = Vector3.one * (1 - height * 0.7f);

            group.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);   // lower on screen = drawn in front
            Refresh();
        }

        void Refresh()
        {
            string dir = Directions[Facing];
            string frame = Walking && !Jumping ? WalkCycle[(int)(walkTime * 8) % WalkCycle.Length] : "";
            string key = dir + frame + Facing;
            if (key == shown) return;
            shown = key;

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
