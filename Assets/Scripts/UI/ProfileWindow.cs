using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// The character sheet (U), laid out like an MMO: your character in the middle with equipment
    /// slots on both sides (head, neck, back / chest, legs, wrists) and both hands underneath,
    /// your stats below, and all your skills with their levels on the right.
    /// Click a worn item to take it off; wear things by clicking them in the bag.
    /// </summary>
    public class ProfileWindow : GameWindow
    {
        static readonly (EquipSlot slot, Vector2 pos)[] SlotLayout =
        {
            (EquipSlot.Hat, new Vector2(-196, 72)), (EquipSlot.Necklace, new Vector2(-196, 32)), (EquipSlot.Cloak, new Vector2(-196, -8)),
            (EquipSlot.Chest, new Vector2(-36, 72)), (EquipSlot.Legs, new Vector2(-36, 32)), (EquipSlot.Wrists, new Vector2(-36, -8)),
            (EquipSlot.Primary, new Vector2(-138, -64)), (EquipSlot.Secondary, new Vector2(-94, -64)),
        };

        static readonly string[] SkillSpanish =
        {
            "Vocabulario", "Gramática", "Lectura", "Comprensión", "Ortografía", "Conversación",
            "Cocina", "Pesca", "Agricultura", "Comercio", "Artesanía", "Exploración", "Magia", "Tiro con arco", "Combate", "Leñador", "Música", "Minería",
        };
        static readonly string[] SkillEnglish =
        {
            "Vocabulary", "Grammar", "Reading", "Listening", "Spelling", "Conversation",
            "Cooking", "Fishing", "Farming", "Trading", "Crafting", "Exploration", "Magic", "Archery", "Combat", "Woodcutting", "Music", "Mining",
        };

        /// <summary>The three kinds of skill, each on its own tab.</summary>
        static readonly (string es, string en, string blurbEs, string blurbEn, SkillType[] skills)[] Tabs =
        {
            ("Idioma", "Language", "Lo que sabes de español", "How much Spanish you know",
                new[] { SkillType.Vocabulary, SkillType.Grammar, SkillType.Reading, SkillType.Listening, SkillType.Spelling, SkillType.Conversation }),
            ("Oficios", "Trades", "Trabajos y pasatiempos", "Jobs and pastimes",
                new[] { SkillType.Fishing, SkillType.Farming, SkillType.Mining, SkillType.Woodcutting, SkillType.Cooking, SkillType.Crafting,
                        SkillType.Trading, SkillType.Music, SkillType.Exploration }),
            ("Combate", "Combat", "Luchar con armas y magia", "Fighting with weapons and magic",
                new[] { SkillType.Combat, SkillType.Archery, SkillType.Magic }),
        };
        readonly List<Image> tabButtons = new List<Image>();
        Transform tabRoot;
        int tab;

        readonly Dictionary<EquipSlot, ItemSlotView> slotViews = new Dictionary<EquipSlot, ItemSlotView>();
        readonly List<Text> levels = new List<Text>();
        readonly List<RectTransform> bars = new List<RectTransform>();
        CharacterPreview preview;
        Text playerName, stats;
        LocalizedText info, infoEnglish;

        protected override bool ToggleKeyPressed => Keybinds.Down(GameAction.Profile);

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(-88, -6), new Vector2(456, 330), "Perfil", "Profile");

            // ---- character sheet (left half)
            playerName = UIFactory.Label("Name", W, new Vector2(-116, 106), new Vector2(200, 20), "", UIFactory.Ink);
            UIFactory.Box("Stage", W, new Vector2(-116, 32), new Vector2(104, 120), new Color32(214, 200, 168, 255));
            preview = UIFactory.Rect("Preview", W, new Vector2(-116, 30), new Vector2(81, 108)).gameObject.AddComponent<CharacterPreview>();
            preview.Build(new Vector2(81, 108));
            foreach (var (slot, pos) in SlotLayout)
            {
                var s = slot;
                var view = ItemSlotView.Create(W, pos, 34, () => GameState.Unequip(s));
                view.hover.Entered += () => Describe(s);
                view.hover.Exited += () => Describe(EquipSlot.None);
                slotViews[slot] = view;
            }
            stats = UIFactory.Label("Stats", W, new Vector2(-116, -102), new Vector2(210, 36), "", UIFactory.Ink);
            info = UIFactory.LLabel("Info", W, new Vector2(-116, -134), new Vector2(210, 20), "", "", UIFactory.Teal).GetComponent<LocalizedText>();
            infoEnglish = info;

            // ---- right half: language skills, trades and combat are kept apart, one tab each
            UIFactory.Img("Divider", W, new Vector2(8, -8), new Vector2(2, 250), new Color32(200, 170, 120, 255));
            for (int i = 0; i < Tabs.Length; i++)
            {
                int index = i;
                tabButtons.Add(UIFactory.LBtn("Tab", W, new Vector2(52 + i * 62, 106), new Vector2(58, 20), Tabs[i].es, Tabs[i].en, UIFactory.Teal,
                    () => { tab = index; Refresh(); }).GetComponent<Image>());
            }
            tabRoot = UIFactory.Rect("TabRows", W, Vector2.zero, Vector2.zero);
            GameState.InventoryChanged += Refresh;
        }

        void OnDestroy() => GameState.InventoryChanged -= Refresh;

        public void Toggle()
        {
            if (Open) { Close(); return; }
            OpenWindow();
            Describe(EquipSlot.None);
            Refresh();
        }

        void Refresh()
        {
            if (!Open) return;
            var data = GameState.Data;
            var profile = PlayerProfile.Current;
            playerName.text = profile.playerName;
            preview.Show(profile.appearance, CharacterLayers.FromEquipment());

            foreach (var (slot, _) in SlotLayout)
            {
                var item = GameState.Equipped(slot);
                var sprite = item != null ? UIFactory.LoadSprite("Art/Icons/" + item.icon) : UIFactory.LoadSprite("Art/UI/slot_" + SlotKey(slot));
                slotViews[slot].Show(sprite, 1, Color.white);
            }

            var s = data.stats;
            stats.text = "ATQ " + Combat.AttackPower().ToString("0.#") + "   DEF " + GameState.Defense() + "   INT " + s.intelligence
                + "\nVida " + s.hp + "/" + s.maxHp + "   Maná " + s.mana + "/" + s.maxMana;

            ShowTab();
        }

        /// <summary>The rows of the tab you picked: each skill with its level and progress; the combat tab also lists your fighting numbers.</summary>
        void ShowTab()
        {
            foreach (Transform c in tabRoot) Destroy(c.gameObject);
            for (int i = 0; i < tabButtons.Count; i++) tabButtons[i].color = i == tab ? UIFactory.Gold : UIFactory.Teal;
            var data = GameState.Data;
            float y = 82;
            UIFactory.LLabel("Blurb", tabRoot, new Vector2(112, y), new Vector2(196, 14), Tabs[tab].blurbEs, Tabs[tab].blurbEn, UIFactory.Faded, TextAnchor.MiddleCenter, 12);
            y -= 20;
            foreach (var skill in Tabs[tab].skills)
            {
                var p = data.skills.Get(skill);
                UIFactory.LLabel("Skill", tabRoot, new Vector2(72, y), new Vector2(110, 16), SkillSpanish[(int)skill], SkillEnglish[(int)skill], UIFactory.Ink, TextAnchor.MiddleLeft);
                UIFactory.Label("Level", tabRoot, new Vector2(148, y), new Vector2(40, 16), "Nv " + p.level, UIFactory.Purple, TextAnchor.MiddleRight);
                UIFactory.Img("BarBack", tabRoot, new Vector2(194, y), new Vector2(44, 7), new Color32(120, 100, 84, 255));
                var bar = UIFactory.Img("Bar", tabRoot, new Vector2(194, y), new Vector2(42, 5), UIFactory.Gold).rectTransform;
                bar.pivot = new Vector2(0, 0.5f);
                bar.anchoredPosition = new Vector2(173, y);
                bar.sizeDelta = new Vector2(42 * Mathf.Clamp01(p.xp / (float)SkillSet.XpForNextLevel(p.level)), 5);
                y -= 18;
            }
            if (Tabs[tab].es == "Combate")
            {
                var s = data.stats;
                y -= 6;
                var lines = new (string es, string en)[]
                {
                    ("Nivel " + data.level, "Level " + data.level),
                    ("Ataque " + Combat.AttackPower().ToString("0.#") + " por golpe", "Attack " + Combat.AttackPower().ToString("0.#") + " per hit"),
                    ("Crítico " + Mathf.RoundToInt(Combat.CritChance() * 100) + "% (daño doble)", "Critical " + Mathf.RoundToInt(Combat.CritChance() * 100) + "% (double damage)"),
                    ("Defensa " + GameState.Defense() + ": bloquea " + Mathf.RoundToInt(Combat.Mitigation(data.level) * 100) + "%", "Defence " + GameState.Defense() + ": blocks " + Mathf.RoundToInt(Combat.Mitigation(data.level) * 100) + "%"),
                    ("Vida " + s.hp + "/" + s.maxHp + "   Maná " + s.mana + "/" + s.maxMana, "HP " + s.hp + "/" + s.maxHp + "   Mana " + s.mana + "/" + s.maxMana),
                };
                foreach (var (es, en) in lines)
                {
                    UIFactory.LLabel("Stat", tabRoot, new Vector2(112, y), new Vector2(196, 16), es, en, UIFactory.Ink, TextAnchor.MiddleLeft);
                    y -= 17;
                }
            }
            translate.Set(translate.English);
        }

        static string SlotKey(EquipSlot slot) => slot switch
        {
            EquipSlot.Hat => "hat", EquipSlot.Necklace => "necklace", EquipSlot.Cloak => "cloak", EquipSlot.Chest => "chest",
            EquipSlot.Legs => "legs", EquipSlot.Wrists => "wrists", EquipSlot.Primary => "primary", _ => "secondary",
        };

        void Describe(EquipSlot slot)
        {
            if (slot == EquipSlot.None)
            {
                info.Set("Clic en un objeto: quitar", "Click a worn item to take it off");
                return;
            }
            int si = (int)slot;
            string es = ItemDatabase.SlotNames[si, 0], en = ItemDatabase.SlotNames[si, 1];
            var item = GameState.Equipped(slot);
            if (item != null)
            {
                string bonus = item.power > 0 ? "  ATQ +" + item.power : item.defense > 0 ? "  DEF +" + item.defense : item.mana > 0 ? "  Maná +" + item.mana : "";
                info.Set(es + ": " + item.spanish + bonus, en + ": " + item.english + bonus);
            }
            else
            {
                info.Set(es + ": vacío", en + ": empty");
            }
        }

        // keep the preview turning slowly, like a character sheet in an MMO
        float turnTimer;

        protected override void Update()
        {
            base.Update();
            if (!Open) return;
            turnTimer += Time.unscaledDeltaTime;
            if (turnTimer > 2.5f) { turnTimer = 0; preview.Turn(1); }
        }
    }
}
