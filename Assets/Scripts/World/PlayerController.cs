using UnityEngine;

namespace LLG
{
    /// <summary>
    /// Moves the player with the movement keys (WASD by default, arrow keys always work too),
    /// and handles jump / crouch / run. Solid tiles block movement.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public static readonly Vector2[] FacingVectors = { Vector2.down, Vector2.right, Vector2.up, Vector2.left };
        static readonly Vector2 HalfSize = new Vector2(0.3f, 0.2f);   // the player's footprint
        const float WalkSpeed = 4.5f, RunSpeed = 7.5f, CrouchSpeed = 2f;

        VillageController village;
        CharacterSprite sprite;

        /// <summary>True while moving with the run key held (running makes you hungry faster).</summary>
        public bool Running => sprite.Running && sprite.Walking;

        public void Init(VillageController owner, CharacterSprite characterSprite)
        {
            village = owner;
            sprite = characterSprite;
        }

        /// <summary>The tile directly in front of the player.</summary>
        public Vector2Int FacingTile => VillageMap.TileAt((Vector2)transform.position + FacingVectors[sprite.Facing] * 0.75f);

        static float Axis(GameAction negative, KeyCode negativeArrow, GameAction positive, KeyCode positiveArrow)
        {
            float value = 0;
            if (Keybinds.Held(positive) || Input.GetKey(positiveArrow)) value += 1;
            if (Keybinds.Held(negative) || Input.GetKey(negativeArrow)) value -= 1;
            return value;
        }

        void Update()
        {
            var input = Vector2.zero;
            bool free = !village.Busy;
            if (free)
            {
                input = new Vector2(Axis(GameAction.Left, KeyCode.LeftArrow, GameAction.Right, KeyCode.RightArrow),
                                    Axis(GameAction.Down, KeyCode.DownArrow, GameAction.Up, KeyCode.UpArrow));
                if (Keybinds.Down(GameAction.Jump)) sprite.Jump();
            }
            if (village.Sitting)
            {
                sprite.Walking = false;
                if (input != Vector2.zero || (free && Keybinds.Down(GameAction.Jump))) village.StandUp();     // any move = get up
                return;
            }
            sprite.Crouching = free && Keybinds.Held(GameAction.Crouch) && !sprite.Jumping;
            sprite.Running = free && Keybinds.Held(GameAction.Run) && !sprite.Crouching;

            sprite.Walking = input != Vector2.zero;
            if (!sprite.Walking) return;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y)) sprite.Face(input.x > 0 ? 1 : 3);
            else sprite.Face(input.y > 0 ? 2 : 0);

            float speed = sprite.Crouching ? CrouchSpeed : sprite.Running ? RunSpeed : WalkSpeed;
            if (GameState.Data.stats.hunger <= 0) speed *= 0.55f;          // starving = slow
            var step = input.normalized * speed * Time.deltaTime;
            var pos = (Vector2)transform.position;
            if (Free(pos + new Vector2(step.x, 0))) pos.x += step.x;          // slide along walls
            if (Free(pos + new Vector2(0, step.y))) pos.y += step.y;
            transform.position = pos;
        }

        public bool Free(Vector2 feet)
        {
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                    if (!village.Map.Walkable(VillageMap.TileAt(feet + new Vector2(HalfSize.x * sx, HalfSize.y * sy))))
                        return false;
            return true;
        }
    }
}
