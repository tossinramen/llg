using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLG
{
    /// <summary>Something worn on top of the body: a hat, cloak, apron, beard, skirt or shawl.</summary>
    [Serializable]
    public class Accessory
    {
        public string kind;
        public Color32 color;

        public Accessory(string kind, Color32 color)
        {
            this.kind = kind;
            this.color = color;
        }
    }

    /// <summary>
    /// Decides which sprite layers make up a character, in which colour and in which order.
    /// Used by both the world sprite (CharacterSprite) and UI previews (CharacterPreview) so they match.
    /// </summary>
    public static class CharacterLayers
    {
        public struct Layer
        {
            public string sprite;
            public Color color;
            public int order;
        }

        static readonly string[] Directions = { "down", "right", "up", "right" };   // 3 = left (mirrored)

        public static string Direction(int facing) => Directions[facing];

        public static List<Layer> Compose(CharacterAppearance look, List<Accessory> accessories, int facing, string frame)
        {
            string d = Directions[facing];
            var list = new List<Layer>
            {
                L("outline_" + d + frame, Color.white, 0),
                L("pants_" + d + frame, AppearancePalette.Pants[look.pants], 1),
                L("shirt_" + d + frame, AppearancePalette.Shirt[look.shirt], 3),
                L("skin_" + d + frame, AppearancePalette.Skin[look.skin], 6),
                L("detail_" + d + frame, Color.white, 7),
                L("hair" + look.hairStyle + "_" + d, AppearancePalette.Hair[look.hairColor], 10),
            };
            if (accessories != null)
            {
                foreach (var a in accessories)
                {
                    int order = a.kind switch
                    {
                        "skirt" => 2,
                        "apron" => 4,
                        "shawl" => 5,
                        "beard" => 8,
                        "cloak" => facing == 2 ? 9 : -2,     // behind you, unless you face away
                        "hat" => 11,
                        _ => 5,
                    };
                    list.Add(L("acc_" + a.kind + "_" + d, a.color, order));
                }
            }
            list.Sort((a, b) => a.order.CompareTo(b.order));
            return list;
        }

        static Layer L(string sprite, Color color, int order) => new Layer { sprite = sprite, color = color, order = order };

        /// <summary>Hat and cloak you are wearing show on your character.</summary>
        public static List<Accessory> FromEquipment()
        {
            var list = new List<Accessory>();
            if (GameState.Data == null) return list;
            if (GameState.Equipped(EquipSlot.Hat) != null) list.Add(new Accessory("hat", new Color32(56, 50, 58, 255)));
            if (GameState.Equipped(EquipSlot.Cloak) != null) list.Add(new Accessory("cloak", new Color32(170, 44, 54, 255)));
            return list;
        }
    }
}
