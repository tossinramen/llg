using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Draws a character in a UI window (start screen, profile) from the same layers as the world
    /// sprite, and lets it turn to face four directions. Left is the right-facing art mirrored.
    /// </summary>
    public class CharacterPreview : MonoBehaviour
    {
        readonly List<Image> layers = new List<Image>();
        CharacterAppearance appearance;
        List<Accessory> accessories;
        Vector2 size;
        int facing;

        /// <summary>Character art is 24x32 pixels, so use a 3:4 size such as 108x144.</summary>
        public void Build(Vector2 layerSize)
        {
            size = layerSize;
        }

        /// <summary>Turn the character. +1 = clockwise, -1 = counter-clockwise.</summary>
        public void Turn(int step)
        {
            facing = ((facing + step) % 4 + 4) % 4;
            Refresh();
        }

        public void Show(CharacterAppearance look, List<Accessory> worn = null)
        {
            appearance = look;
            accessories = worn;
            Refresh();
        }

        void Refresh()
        {
            if (appearance == null) return;
            transform.localScale = new Vector3(facing == 3 ? -1 : 1, 1, 1);
            var wanted = CharacterLayers.Compose(appearance, accessories, facing, "");
            while (layers.Count < wanted.Count)
                layers.Add(UIFactory.Img("Layer", transform, Vector2.zero, size, Color.white));
            for (int i = 0; i < layers.Count; i++)
            {
                bool used = i < wanted.Count;
                layers[i].enabled = used;
                if (!used) continue;
                layers[i].sprite = UIFactory.LoadSprite("Art/Character/" + wanted[i].sprite);
                layers[i].color = wanted[i].color;
                layers[i].transform.SetSiblingIndex(i);
            }
        }
    }
}
