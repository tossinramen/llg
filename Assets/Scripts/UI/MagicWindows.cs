using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// The spellbook (K): every spell with its level, rank and mana cost. Learn and level up spells
    /// with Lexa, choose which one you use in battle ("Preparar"), or cast one now ("Lanzar").
    /// </summary>
    public class SpellbookWindow : GameWindow
    {
        Transform rows;
        Text lexa;
        CastWindow cast;

        protected override bool ToggleKeyPressed => Keybinds.Down(GameAction.Spells);

        public void Build(Transform canvas, VillageController owner, CastWindow castWindow)
        {
            cast = castWindow;
            CreateWindow(canvas, owner, new Vector2(0, -2), new Vector2(580, 344), "Libro de hechizos", "Spellbook");
            UIFactory.LLabel("Intro", W, new Vector2(-40, 124), new Vector2(400, 16),
                "Para lanzar un hechizo, escribe su palabra letra a letra.", "To cast a spell, spell its word letter by letter.", UIFactory.Faded);
            UIFactory.Img("LexaIcon", W, new Vector2(214, 124), new Vector2(12, 12), Color.white, UIFactory.LoadSprite("Art/UI/lexa"));
            lexa = UIFactory.Label("Lexa", W, new Vector2(248, 124), new Vector2(50, 14), "", UIFactory.Ink, TextAnchor.MiddleLeft, 12);
            rows = UIFactory.Rect("Rows", W, Vector2.zero, Vector2.zero);
            GameState.Data.stats.Changed += Rebuild;
        }

        void OnDestroy() { if (GameState.Data != null) GameState.Data.stats.Changed -= Rebuild; }

        public void Toggle()
        {
            if (Open) { Close(); return; }
            OpenWindow();
            Rebuild();
        }

        void Rebuild()
        {
            if (!Open) return;
            foreach (Transform c in rows) Destroy(c.gameObject);
            lexa.text = GameState.Short(GameState.Data.stats.coins);
            int level = GameState.Data.level;
            for (int i = 0; i < Magic.Spells.Length; i++)
            {
                var s = Magic.Spells[i];
                int rank = Magic.Rank(s.id);
                bool canLearn = level >= s.level;
                float y = 96 - i * 37;
                var back = UIFactory.Box("Row", rows, new Vector2(0, y), new Vector2(540, 34),
                    GameState.Data.preparedSpell == s.id && rank > 0 ? new Color32(230, 214, 250, 255) : new Color32(246, 236, 214, 255));
                UIFactory.Img("Icon", back.transform, new Vector2(-250, 0), new Vector2(20, 20), canLearn ? Color.white : new Color(0.4f, 0.4f, 0.4f),
                    UIFactory.LoadSprite("Art/Icons/" + s.icon));
                UIFactory.LLabel("Name", back.transform, new Vector2(-180, 6), new Vector2(110, 14), s.es, s.en, UIFactory.Ink, TextAnchor.MiddleLeft, 16);
                UIFactory.Label("Info", back.transform, new Vector2(-180, -8), new Vector2(110, 12),
                    "Nv " + s.level + "  ·  " + s.mana + " maná", canLearn ? UIFactory.Faded : UIFactory.Red, TextAnchor.MiddleLeft, 12);
                UIFactory.LLabel("Effect", back.transform, new Vector2(-40, 0), new Vector2(160, 28), s.effectEs, s.effectEn, UIFactory.Faded, TextAnchor.MiddleLeft, 12);
                UIFactory.Label("Rank", back.transform, new Vector2(60, 0), new Vector2(40, 16),
                    rank == 0 ? "—" : "Rango " + Magic.Roman(rank), UIFactory.Purple, TextAnchor.MiddleCenter, 12);
                var spell = s;
                int cost = rank == 0 ? s.LearnCost : s.UpgradeCost(rank);
                string up = rank >= Magic.MaxSpellRank ? "Máximo" : (rank == 0 ? "Aprender " : "Mejorar ") + GameState.Short(cost);
                var upBtn = UIFactory.LBtn("Upgrade", back.transform, new Vector2(140, 0), new Vector2(104, 24), up,
                    rank >= Magic.MaxSpellRank ? "Max" : (rank == 0 ? "Learn " : "Upgrade ") + GameState.Short(cost),
                    canLearn && rank < Magic.MaxSpellRank ? UIFactory.Gold : UIFactory.Faded, () => { if (Magic.Upgrade(spell)) Rebuild(); });
                UIFactory.LBtn("Prepare", back.transform, new Vector2(212, 0), new Vector2(34, 24), "★", "★",
                    rank > 0 ? UIFactory.Purple : UIFactory.Faded, () =>
                    {
                        if (Magic.Rank(spell.id) == 0) return;
                        GameState.Data.preparedSpell = spell.id;
                        GameState.Say(spell.es + " está listo para la batalla.");
                        Rebuild();
                    });
                UIFactory.LBtn("Cast", back.transform, new Vector2(250, 0), new Vector2(30, 24), "▶", "▶",
                    rank > 0 ? UIFactory.Green : UIFactory.Faded, () =>
                    {
                        if (Magic.Rank(spell.id) == 0) return;
                        Close();
                        village.CastInWorld(spell);
                    });
            }
            UIFactory.LLabel("Legend", rows, new Vector2(0, -150), new Vector2(520, 14),
                "★ = preparar para la batalla     ▶ = lanzar ahora", "★ = ready it for battle     ▶ = cast it now", UIFactory.Faded, TextAnchor.MiddleCenter, 12);
            translate.Set(translate.English);
        }
    }

    /// <summary>Casting a spell outside battle: click the letters of the spell's word in order.</summary>
    public class CastWindow : GameWindow
    {
        Transform letters;
        Text built, info;
        Magic.Spell spell;
        Action onSuccess;
        string word;
        int next;
        float closeTimer = -1;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, 20), new Vector2(460, 200), "Lanzar hechizo", "Cast a spell");
            built = UIFactory.Label("Built", W, new Vector2(0, 36), new Vector2(400, 28), "", UIFactory.Purple, TextAnchor.MiddleCenter, 24, true);
            letters = UIFactory.Rect("Letters", W, new Vector2(0, -12), Vector2.zero);
            info = UIFactory.Label("Info", W, new Vector2(0, -60), new Vector2(400, 18), "", UIFactory.Faded);
        }

        public void Begin(Magic.Spell s, Action whenCast)
        {
            spell = s;
            onSuccess = whenCast;
            word = s.word;
            next = 0;
            closeTimer = -1;
            OpenWindow();
            info.text = s.es + "  (" + s.mana + " maná)";
            Redraw();
        }

        void Redraw()
        {
            foreach (Transform c in letters) Destroy(c.gameObject);
            built.text = word.Substring(0, next).ToUpper() + new string('_', word.Length - next);
            var order = new List<int>();
            for (int i = 0; i < word.Length; i++) order.Add(i);
            var rng = new System.Random(word.GetHashCode());
            for (int i = order.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); (order[i], order[j]) = (order[j], order[i]); }
            float x = -(order.Count - 1) * 18f;
            foreach (int i in order)
            {
                if (i < next) { x += 36; continue; }
                int index = i;
                UIFactory.Btn("Letter", letters, new Vector2(x, 0), new Vector2(32, 32), word[i].ToString().ToUpper(), UIFactory.Teal, () => Click(index));
                x += 36;
            }
        }

        void Click(int index)
        {
            if (closeTimer >= 0) return;
            if (word[index] != word[next])
            {
                built.text = "¡Fsss!";
                info.text = "El hechizo se apaga. (The spell fizzles.)";
                closeTimer = 1.0f;
                return;
            }
            next++;
            Redraw();
            if (next >= word.Length)
            {
                info.text = "¡" + spell.es + "!";
                closeTimer = 0.4f;
                var done = onSuccess;
                onSuccess = null;
                done?.Invoke();
            }
        }

        protected override void Update()
        {
            base.Update();
            if (!Open || closeTimer < 0) return;
            closeTimer -= Time.deltaTime;
            if (closeTimer < 0) Close();
        }
    }

    /// <summary>
    /// El altar rúnico in La Torre Rúnica: choose something you are wearing, then an enchantment.
    /// Each enchantment has 3 ranks and costs Lexa. One enchantment per item (a new one replaces it).
    /// </summary>
    public class EnchantWindow : GameWindow
    {
        Transform items, enchants;
        LocalizedText info;
        Text lexa;
        ItemDef selected;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -2), new Vector2(560, 334), "El altar rúnico", "The Rune Altar");
            UIFactory.LLabel("Pick", W, new Vector2(-150, 118), new Vector2(220, 16), "1. Elige algo que llevas puesto", "1. Choose something you're wearing", UIFactory.Ink, TextAnchor.MiddleCenter, 12);
            UIFactory.LLabel("Pick2", W, new Vector2(130, 118), new Vector2(260, 16), "2. Elige un encantamiento", "2. Choose an enchantment", UIFactory.Ink, TextAnchor.MiddleCenter, 12);
            items = UIFactory.Rect("Items", W, Vector2.zero, Vector2.zero);
            enchants = UIFactory.Rect("Enchants", W, Vector2.zero, Vector2.zero);
            info = UIFactory.LLabel("Info", W, new Vector2(-30, -140), new Vector2(420, 18), "", "", UIFactory.Ink).GetComponent<LocalizedText>();
            UIFactory.Img("LexaIcon", W, new Vector2(200, -140), new Vector2(12, 12), Color.white, UIFactory.LoadSprite("Art/UI/lexa"));
            lexa = UIFactory.Label("Lexa", W, new Vector2(234, -140), new Vector2(50, 14), "", UIFactory.Ink, TextAnchor.MiddleLeft, 12);
        }

        public void Begin()
        {
            selected = GameState.EquippedWeapon();
            OpenWindow();
            info.Set("Las runas dan poder a tus armas y armaduras.", "Runes give power to your weapons and armour.");
            Rebuild();
        }

        void Rebuild()
        {
            foreach (Transform c in items) Destroy(c.gameObject);
            foreach (Transform c in enchants) Destroy(c.gameObject);
            lexa.text = GameState.Short(GameState.Data.stats.coins);
            int row = 0;
            foreach (EquipSlot slot in Enum.GetValues(typeof(EquipSlot)))
            {
                if (slot == EquipSlot.None) continue;
                var item = GameState.Equipped(slot);
                if (item == null || item.kind == ItemKind.Instrument) continue;
                var it = item;
                var e = Magic.On(item.id);
                var card = UIFactory.Btn("Item", items, new Vector2(-150, 92 - row * 30), new Vector2(230, 26), "",
                    selected == item ? UIFactory.Gold : new Color32(246, 236, 214, 255), () => { selected = it; Rebuild(); });
                UIFactory.Img("Icon", card.transform, new Vector2(-100, 0), new Vector2(16, 16), Color.white, UIFactory.LoadSprite("Art/Icons/" + item.icon));
                string tag = e == null ? "" : "  · " + Magic.GetEnchant(e.ench).es + " " + Magic.Roman(e.rank);
                UIFactory.Label("Name", card.transform, new Vector2(10, 1), new Vector2(190, 16), item.spanish + tag, UIFactory.Ink, TextAnchor.MiddleLeft, 12);
                row++;
            }
            if (row == 0)
                UIFactory.LLabel("None", items, new Vector2(-150, 60), new Vector2(230, 40), "No llevas nada. Equípate un arma o armadura.",
                    "You aren't wearing anything. Equip a weapon or armour.", UIFactory.Faded);

            for (int i = 0; i < Magic.Enchants.Length; i++)
            {
                var en = Magic.Enchants[i];
                bool fits = Magic.Fits(en, selected);
                var cur = selected == null ? null : Magic.On(selected.id);
                int rank = cur != null && cur.ench == en.id ? cur.rank : 0;
                string cost = rank >= Magic.MaxEnchantRank ? "Máx." : GameState.Short(Magic.EnchantCost[rank]);
                var card = UIFactory.Btn("Enchant", enchants, new Vector2(130, 92 - i * 38), new Vector2(270, 34), "",
                    fits ? new Color32(236, 230, 250, 255) : new Color32(200, 194, 186, 255), () =>
                    {
                        if (selected == null) { info.Set("Primero elige algo.", "Choose something first."); return; }
                        if (Magic.Apply(en, selected)) Rebuild();
                    });
                UIFactory.Img("Icon", card.transform, new Vector2(-116, 0), new Vector2(20, 20), fits ? Color.white : new Color(0.45f, 0.45f, 0.45f),
                    UIFactory.LoadSprite("Art/Icons/" + en.icon));
                UIFactory.LLabel("Name", card.transform, new Vector2(-40, 6), new Vector2(130, 14), en.es + (rank > 0 ? " " + Magic.Roman(rank) : ""), en.en, UIFactory.Ink, TextAnchor.MiddleLeft, 12);
                UIFactory.LLabel("Effect", card.transform, new Vector2(-40, -8), new Vector2(130, 12), en.effectEs, en.effectEn, UIFactory.Faded, TextAnchor.MiddleLeft, 12);
                UIFactory.Img("Lexa", card.transform, new Vector2(70, 0), new Vector2(12, 12), Color.white, UIFactory.LoadSprite("Art/UI/lexa"));
                UIFactory.Label("Cost", card.transform, new Vector2(104, 0), new Vector2(50, 14), cost, UIFactory.Purple, TextAnchor.MiddleLeft, 12);
            }
            translate.Set(translate.English);
        }
    }
}
