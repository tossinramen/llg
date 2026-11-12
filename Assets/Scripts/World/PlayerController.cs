using UnityEngine;

namespace LLG
{
    /// <summary>Moves the player with WASD / arrow keys and stops them walking through solid tiles.</summary>
    public class PlayerController : MonoBehaviour
    {
        static readonly Vector2[] FacingVectors = { Vector2.down, Vector2.right, Vector2.up, Vector2.left };
        static readonly Vector2 HalfSize = new Vector2(0.3f, 0.2f);   // the player's footprint

        VillageController village;
        CharacterSprite sprite;

        public void Init(VillageController owner, CharacterSprite characterSprite)
        {
            village = owner;
            sprite = characterSprite;
        }

        /// <summary>The tile directly in front of the player.</summary>
        public Vector2Int FacingTile => VillageMap.TileAt((Vector2)transform.position + FacingVectors[sprite.Facing] * 0.75f);

        void Update()
        {
            var input = Vector2.zero;
            if (!village.Busy)
                input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            sprite.Walking = input != Vector2.zero;
            if (!sprite.Walking) return;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y)) sprite.Face(input.x > 0 ? 1 : 3);
            else sprite.Face(input.y > 0 ? 2 : 0);

            float speed = GameState.Data.stats.hunger > 0 ? 4.5f : 2.5f;     // starving = slow
            var step = input.normalized * speed * Time.deltaTime;
            var pos = (Vector2)transform.position;
            if (Free(pos + new Vector2(step.x, 0))) pos.x += step.x;          // slide along walls
            if (Free(pos + new Vector2(0, step.y))) pos.y += step.y;
            transform.position = pos;
        }

        bool Free(Vector2 feet)
        {
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                    if (!village.Map.Walkable(VillageMap.TileAt(feet + new Vector2(HalfSize.x * sx, HalfSize.y * sy))))
                        return false;
            return true;
        }
    }
}
