using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LLG
{
    /// <summary>
    /// A character in the world, built from tinted sprite layers (see CharacterLayers).
    /// Used for the player and for villagers. The object's position is where the feet are.
    /// Handles the walk cycle, jumping (a hop with a shrinking shadow) and crouching (squashed down).
    /// </summary>
    public class CharacterSprite : MonoBehaviour
    {
        static readonly string[] WalkCycle = { "_w1", "", "_w2", "" };
        const float JumpTime = 0.45f, JumpHeight = 0.55f, FeetOffset = 0.94f;

        readonly List<SpriteRenderer> layers = new List<SpriteRenderer>();
        Transform body, shadow;
        SortingGroup group;
        CharacterAppearance look;
        List<Accessory> accessories;
        float walkTime, jumpTimer = -1, flashTimer;
        string shown;

        public int Facing { get; private set; }     // 0 down, 1 right, 2 up, 3 left
        public bool Walking { get; set; }
        public bool Running { get; set; }
        public bool Crouching { get; set; }
        public bool Sitting { get; set; }        // on a bench: lifted onto the seat, facing you
        public bool Jumping => jumpTimer >= 0;

        public void Build(CharacterAppearance appearance, List<Accessory> worn = null)
        {
            look = appearance;
            accessories = worn;
            group = gameObject.AddComponent<SortingGroup>();

            var shadowRenderer = new GameObject("Shadow").AddComponent<SpriteRenderer>();
            shadow = shadowRenderer.transform;
            shadow.SetParent(transform, false);
            shadow.localPosition = new Vector3(0, -0.05f, 0);
            shadow.localScale = new Vector3(1.3f, 1.1f, 1);
            shadowRenderer.sprite = UIFactory.LoadSprite("Art/UI/shadow");
            shadowRenderer.sortingOrder = -10;

            body = new GameObject("Body").transform;
            body.SetParent(transform, false);
            body.localPosition = new Vector3(0, FeetOffset, 0);
            Refresh();
        }

        /// <summary>Changes what the character is wearing (e.g. after equipping a hat).</summary>
        public void SetAccessories(List<Accessory> worn)
        {
            accessories = worn;
            shown = null;
            Refresh();
        }

        float faceLockUntil = -1;

        /// <summary>Turns to a direction - unless an attack is holding the facing for a moment.</summary>
        public void Face(int direction)
        {
            if (Time.time < faceLockUntil) return;
            Facing = direction;
        }

        /// <summary>Keeps facing this way for a moment (while a punch or swing plays), even while walking.</summary>
        public void LockFacing(float seconds) => faceLockUntil = Time.time + seconds;

        /// <summary>Turns to look along a direction (used when attacking something with the mouse).</summary>
        public void FaceTowards(Vector2 dir)
        {
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) Facing = dir.x > 0 ? 1 : 3;
            else Facing = dir.y > 0 ? 2 : 0;
        }

        /// <summary>Flashes red for a moment (when hurt).</summary>
        public void Flash()
        {
            flashTimer = 0.2f;
            shown = null;
        }

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
            float squash = Crouching ? 0.8f : Sitting ? 0.84f : 1f;
            float widen = Crouching ? 1.08f : Sitting ? 1.04f : 1f;
            float seat = Sitting ? 0.34f : 0f;
            body.localScale = new Vector3((Facing == 3 ? -1 : 1) * widen, squash, 1);
            body.localPosition = new Vector3(0, FeetOffset - (1 - squash) * FeetOffset + height + seat, 0);
            shadow.localScale = Sitting ? Vector3.zero : new Vector3(1.3f, 1.1f, 1) * (1 - height * 0.7f);

            // lower on screen = drawn in front; sitting, you are drawn over the bench you sit on
            group.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100) + (Sitting ? 80 : 0);
            if (flashTimer > 0)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0) shown = null;
            }
            Refresh();
        }

        void Refresh()
        {
            string frame = Walking && !Jumping ? WalkCycle[(int)(walkTime * 8) % WalkCycle.Length] : "";
            string key = frame + Facing + (flashTimer > 0 ? "!" : "");
            if (key == shown) return;
            shown = key;

            var wanted = CharacterLayers.Compose(look, accessories, Facing, frame);
            while (layers.Count < wanted.Count)
            {
                var go = new GameObject("Layer");
                go.transform.SetParent(body, false);
                layers.Add(go.AddComponent<SpriteRenderer>());
            }
            for (int i = 0; i < layers.Count; i++)
            {
                bool used = i < wanted.Count;
                layers[i].enabled = used;
                if (!used) continue;
                layers[i].sprite = UIFactory.LoadSprite("Art/Character/" + wanted[i].sprite);
                layers[i].color = flashTimer > 0 ? wanted[i].color * new Color(1f, 0.45f, 0.45f) : wanted[i].color;
                layers[i].sortingOrder = wanted[i].order;
            }
        }
    }
}
