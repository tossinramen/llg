using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Draws the player character from stacked, tinted sprite layers and lets it turn to face
    /// four directions. Left is the right-facing art mirrored.
    /// </summary>
    public class CharacterPreview : MonoBehaviour
    {
        static readonly string[] Directions = { "down", "right", "up", "right" };   // index 3 = left (mirrored)

        Image outline, pants, shirt, skin, detail, hair;
        CharacterAppearance appearance;
        int facing;

        public void Build(Vector2 size)
        {
            outline = Layer("Outline", size);
            pants = Layer("Pants", size);
            shirt = Layer("Shirt", size);
            skin = Layer("Skin", size);
            detail = Layer("Detail", size);
            hair = Layer("Hair", size);
        }

        Image Layer(string layerName, Vector2 size)
        {
            return UIFactory.Img(layerName, transform, Vector2.zero, size, Color.white);
        }

        /// <summary>Turn the character. +1 = clockwise, -1 = counter-clockwise.</summary>
        public void Turn(int step)
        {
            facing = ((facing + step) % 4 + 4) % 4;
            Refresh();
        }

        public void Show(CharacterAppearance look)
        {
            appearance = look;
            Refresh();
        }

        void Refresh()
        {
            if (appearance == null) return;
            string dir = Directions[facing];
            transform.localScale = new Vector3(facing == 3 ? -1 : 1, 1, 1);

            Set(outline, "outline_" + dir, Color.white);
            Set(pants, "pants_" + dir, AppearancePalette.Pants[appearance.pants]);
            Set(shirt, "shirt_" + dir, AppearancePalette.Shirt[appearance.shirt]);
            Set(skin, "skin_" + dir, AppearancePalette.Skin[appearance.skin]);
            Set(detail, "detail_" + dir, Color.white);
            Set(hair, "hair" + appearance.hairStyle + "_" + dir, AppearancePalette.Hair[appearance.hairColor]);
        }

        static void Set(Image image, string spriteName, Color color)
        {
            image.sprite = UIFactory.LoadSprite("Art/Character/" + spriteName);
            image.color = color;
        }
    }
}
