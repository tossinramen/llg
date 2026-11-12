using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// What lives in the water, and what your gear does. Rarer fish need a better rod; bait and the
    /// fishing hat make rare fish more likely; the net sometimes catches a second fish.
    /// </summary>
    public static class Fishery
    {
        public enum Rarity { Junk, Common, Uncommon, Rare, Epic, Legendary }

        public class Fish
        {
            public string id, water;     // water: "pond", "river" or "any"
            public Rarity rarity;
            public ItemDef Item => ItemDatabase.Get(id);
        }

        public static readonly Fish[] All =
        {
            F("fish_boot", Rarity.Junk, "any"),
            F("fish_sardine", Rarity.Common, "any"), F("fish_anchovy", Rarity.Common, "pond"), F("fish_carp", Rarity.Common, "pond"),
            F("fish_trout", Rarity.Common, "river"),
            F("fish_perch", Rarity.Uncommon, "pond"), F("fish_seabass", Rarity.Uncommon, "any"), F("fish_salmon", Rarity.Uncommon, "river"),
            F("fish_bream", Rarity.Uncommon, "pond"),
            F("fish_crab", Rarity.Rare, "river"), F("fish_cod", Rarity.Rare, "any"), F("fish_tuna", Rarity.Rare, "pond"),
            F("fish_eel", Rarity.Epic, "river"), F("fish_octopus", Rarity.Epic, "pond"), F("fish_swordfish", Rarity.Epic, "any"),
            F("fish_koi", Rarity.Legendary, "pond"), F("fish_rune", Rarity.Legendary, "river"),
        };

        static Fish F(string id, Rarity r, string water) => new Fish { id = id, rarity = r, water = water };

        public static Fish Get(string id) => System.Array.Find(All, f => f.id == id);

        public static readonly string[] RarityEs = { "basura", "común", "poco común", "raro", "épico", "legendario" };
        public static readonly string[] RarityEn = { "junk", "common", "uncommon", "rare", "epic", "legendary" };

        public static Color RarityColor(Rarity r) => r switch
        {
            Rarity.Junk => new Color32(150, 140, 130, 255), Rarity.Common => new Color32(90, 80, 70, 255),
            Rarity.Uncommon => new Color32(60, 160, 70, 255), Rarity.Rare => new Color32(60, 120, 220, 255),
            Rarity.Epic => new Color32(160, 80, 220, 255), _ => new Color32(220, 150, 30, 255),
        };

        /// <summary>Rods from worst to best. A rod's tier is the rarest kind of fish it can land.</summary>
        public static readonly string[] Rods = { "rod", "rod_bamboo", "rod_carbon", "rod_fine", "rod_gold" };
        public static readonly string[] Baits = { "bait_bread", "bait_worm", "bait_shrimp", "bait_squid" };

        public static ItemDef BestRod()
        {
            for (int i = Rods.Length - 1; i >= 0; i--)
                if (GameState.Has(Rods[i])) return ItemDatabase.Get(Rods[i]);
            return null;
        }

        public static bool HasRod() => BestRod() != null;
        public static bool HatOn => GameState.Equipped(EquipSlot.Hat)?.id == "hat_fishing";
        public static bool HasNet => GameState.Has("net");

        /// <summary>Picks what bites, from the fish your rod can land in this water.</summary>
        public static Fish Roll(bool river, string bait)
        {
            int tier = BestRod()?.tier ?? 1;
            float skill = 1 + 0.02f * GameState.Data.skills.Get(SkillType.Fishing).level;
            var pool = new List<(Fish, float)>();
            float total = 0;
            foreach (var f in All)
            {
                if (f.water != "any" && f.water != (river ? "river" : "pond")) continue;
                if ((int)f.rarity > tier) continue;
                float w = f.rarity switch
                {
                    Rarity.Junk => 6, Rarity.Common => 55, Rarity.Uncommon => 25, Rarity.Rare => 10, Rarity.Epic => 3.5f, _ => 0.8f,
                };
                if (f.rarity >= Rarity.Rare) w *= skill * (HatOn ? 1.3f : 1f);
                switch (bait)
                {
                    case "bait_bread": if (f.rarity == Rarity.Common) w *= 1.5f; break;
                    case "bait_worm": if (f.rarity == Rarity.Uncommon) w *= 1.6f; if (f.rarity == Rarity.Junk) w *= 0.5f; break;
                    case "bait_shrimp": if (f.rarity == Rarity.Rare) w *= 2f; if (f.rarity == Rarity.Uncommon) w *= 1.3f; if (f.rarity == Rarity.Junk) w *= 0.3f; break;
                    case "bait_squid":
                        if (f.rarity == Rarity.Epic) w *= 2.5f; if (f.rarity == Rarity.Legendary) w *= 3f;
                        if (f.rarity == Rarity.Rare) w *= 1.5f; if (f.rarity == Rarity.Junk) w *= 0.2f; break;
                }
                pool.Add((f, w));
                total += w;
            }
            float roll = Random.value * total;
            foreach (var (f, w) in pool)
            {
                roll -= w;
                if (roll <= 0) return f;
            }
            return pool[pool.Count - 1].Item1;
        }
    }

    /// <summary>
    /// Fishing, as a little game: cast and wait, strike when the bobber dips (click "¡Tira!" or press
    /// Space), then reel the fish in by building the Spanish sentence word by word. Each right word
    /// pulls the fish closer; each wrong word strains the line (3 and it snaps). Rarer fish bring
    /// longer sentences. Trains Fishing + Grammar.
    /// </summary>
    public class FishingGame : MonoBehaviour
    {
        const int HintCost = 2, LineStrength = 3;
        const float BiteWindow = 1.4f;
        enum Phase { Waiting, Biting, Reeling, Result }

        GameObject window;
        Transform poolRow, answerRow;
        Text english, result, gearLine, lineText;
        LocalizedText heading;
        Image timerBar, bobber, shadow;
        Button hintButton, againButton, hookButton, baitButton;
        SentenceEntry sentence;
        string[] tokens;
        readonly List<int> answer = new List<int>();
        readonly List<int> shuffled = new List<int>();
        Phase phase;
        float timer, totalTime, flash;
        int line, baitIndex = -1;
        bool river;
        Fishery.Fish fish;
        VillageController village;

        public bool Open => window != null && window.activeSelf;

        public void Build(Transform canvas, VillageController owner)
        {
            village = owner;
            window = UIFactory.Panel("Fishing", canvas, new Vector2(0, -8), new Vector2(600, 320)).gameObject;
            var w = window.transform;
            UIFactory.TranslateButton(w, new Vector2(-270, 136));
            UIFactory.LLabel("Title", w, new Vector2(0, 136), new Vector2(460, 28), "La pesca", "Fishing", UIFactory.Purple, TextAnchor.MiddleCenter, 24, true);
            UIFactory.Btn("Close", w, new Vector2(266, 136), new Vector2(24, 22), "x", UIFactory.Red, Close);
            gearLine = UIFactory.Label("Gear", w, new Vector2(0, 112), new Vector2(560, 16), "", UIFactory.Faded, TextAnchor.MiddleCenter, 12);

            // the water: a bobber while you wait, then the fish's shadow as you reel it in
            UIFactory.Box("Water", w, new Vector2(0, 70), new Vector2(560, 60), new Color32(60, 122, 170, 255));
            UIFactory.Img("Shine", w, new Vector2(0, 92), new Vector2(552, 3), new Color32(120, 180, 220, 255));
            UIFactory.Img("Dock", w, new Vector2(-262, 70), new Vector2(28, 60), new Color32(150, 108, 70, 255));
            shadow = UIFactory.Img("Shadow", w, new Vector2(200, 64), new Vector2(32, 32), new Color(0, 0, 0, 0.5f));
            bobber = UIFactory.Img("Bobber", w, new Vector2(-120, 74), new Vector2(12, 18), Color.white, UIFactory.LoadSprite("Art/UI/bobber"));
            hookButton = UIFactory.LBtn("Hook", w, new Vector2(60, 70), new Vector2(170, 30), "¡TIRA!  (Espacio)", "STRIKE!  (Space)", UIFactory.Gold, Hook);
            lineText = UIFactory.Label("Line", w, new Vector2(236, 30), new Vector2(110, 16), "", UIFactory.Ink, TextAnchor.MiddleRight, 12);

            var head = UIFactory.LLabel("Heading", w, new Vector2(-40, 30), new Vector2(440, 20), "", "", UIFactory.Ink);
            heading = head.GetComponent<LocalizedText>();
            english = UIFactory.Label("English", w, new Vector2(0, 10), new Vector2(560, 20), "", UIFactory.Teal);
            UIFactory.Box("TimerBack", w, new Vector2(0, -8), new Vector2(404, 10), UIFactory.Faded);
            timerBar = UIFactory.Img("Timer", w, new Vector2(0, -8), new Vector2(400, 6), UIFactory.Green);
            UIFactory.Box("AnswerLine", w, new Vector2(0, -34), new Vector2(560, 34), new Color32(236, 222, 184, 255));
            answerRow = UIFactory.Rect("Answer", w, new Vector2(0, -34), Vector2.zero);
            poolRow = UIFactory.Rect("Pool", w, new Vector2(0, -72), Vector2.zero);
            result = UIFactory.Label("Result", w, new Vector2(0, -88), new Vector2(560, 36), "", UIFactory.Ink);

            hintButton = UIFactory.LBtn("Hint", w, new Vector2(-206, -134), new Vector2(150, 24), "Pista (2 maná)", "Hint (2 mana)", UIFactory.Blue, Hint);
            baitButton = UIFactory.LBtn("Bait", w, new Vector2(-46, -134), new Vector2(150, 24), "Cebo", "Bait", UIFactory.Teal, NextBait);
            againButton = UIFactory.LBtn("Again", w, new Vector2(106, -134), new Vector2(130, 24), "Lanzar otra vez", "Cast again", UIFactory.Green, Cast);
            UIFactory.LBtn("Leave", w, new Vector2(234, -134), new Vector2(100, 24), "Salir", "Leave", UIFactory.Red, Close);
            window.SetActive(false);
        }

        /// <summary>Starts fishing at the pond, or at the river (different fish live there).</summary>
        public void Begin(bool inRiver = false)
        {
            river = inRiver;
            window.SetActive(true);
            window.transform.SetAsLastSibling();
            window.GetComponent<TranslateToggle>()?.Set(false);
            if (baitIndex < 0 || !HasBait(baitIndex)) baitIndex = BestBait();
            Cast();
        }

        // ------------------------------------------------------------ gear and bait

        bool HasBait(int i) => i >= 0 && i < Fishery.Baits.Length && GameState.Count(Fishery.Baits[i]) > 0;

        int BestBait()
        {
            for (int i = Fishery.Baits.Length - 1; i >= 0; i--)
                if (HasBait(i)) return i;
            return -1;
        }

        string Bait => baitIndex >= 0 ? Fishery.Baits[baitIndex] : null;

        /// <summary>Cycles through the baits you have, then "no bait".</summary>
        void NextBait()
        {
            for (int k = 0; k < Fishery.Baits.Length + 1; k++)
            {
                baitIndex++;
                if (baitIndex >= Fishery.Baits.Length) { baitIndex = -1; break; }
                if (HasBait(baitIndex)) break;
            }
            ShowGear();
        }

        void ShowGear()
        {
            var rod = Fishery.BestRod();
            string bait = Bait == null ? "sin cebo" : ItemDatabase.Get(Bait).spanish + " (x" + GameState.Count(Bait) + ")";
            int seen = GameState.Data.fishSeen.Count;
            gearLine.text = (rod != null ? GameState.Capital(rod.spanish) : "—") + "   ·   Cebo: " + bait
                + (Fishery.HasNet ? "   ·   Red" : "") + (Fishery.HatOn ? "   ·   Sombrero" : "")
                + "   ·   Peces: " + seen + "/" + Fishery.All.Length + "   ·   " + (river ? "El río" : "El estanque");
            baitButton.GetComponentInChildren<LocalizedText>()?.Set(
                Bait == null ? "Cebo: ninguno" : "Cebo: " + ItemDatabase.Get(Bait).spanish.Replace("las ", "").Replace("la ", "").Replace("el ", ""),
                Bait == null ? "Bait: none" : "Bait: " + ItemDatabase.Get(Bait).english);
        }

        // ------------------------------------------------------------ phases

        void Cast()
        {
            if (Lessons.Sentences().Count == 0)
            {
                GameState.Say("No sentences in the language pack yet.");
                Close();
                return;
            }
            if (baitIndex >= 0 && !HasBait(baitIndex)) baitIndex = BestBait();     // ran out: use the next best
            phase = Phase.Waiting;
            float wait = Random.Range(2f, 5f);
            if (Bait == "bait_bread") wait *= 0.7f;
            if (Fishery.HatOn) wait *= 0.6f;      // the hat lures fish in
            timer = wait;
            heading.Set("Esperando a que pique...", "Waiting for a bite...");
            english.text = "";
            result.text = "";
            lineText.text = "";
            answer.Clear();
            Clear(poolRow);
            Clear(answerRow);
            shadow.enabled = false;
            bobber.enabled = true;
            hookButton.gameObject.SetActive(false);
            hintButton.gameObject.SetActive(false);
            againButton.gameObject.SetActive(false);
            baitButton.gameObject.SetActive(true);
            timerBar.rectTransform.sizeDelta = new Vector2(400, 6);
            ShowGear();
        }

        void StartBite()
        {
            phase = Phase.Biting;
            timer = BiteWindow + (Fishery.HatOn ? 0.3f : 0);
            heading.Set("¡Pica! ¡Tira ya!", "A bite! Strike now!");
            hookButton.gameObject.SetActive(true);
            baitButton.gameObject.SetActive(false);
        }

        void Hook()
        {
            if (phase != Phase.Biting) return;
            fish = Fishery.Roll(river, Bait);
            if (Bait != null)
            {
                int slot = GameState.Data.bag.FindIndex(b => b.id == Bait);
                if (slot >= 0) GameState.RemoveAt(slot);
            }
            // rarer fish fight with longer sentences
            var options = Lessons.Sentences();
            options.Sort((a, b) => a.Tokens.Length.CompareTo(b.Tokens.Length));
            int band = Mathf.Clamp((int)fish.rarity - 1, 0, 3);
            int from = options.Count * band / 5, to = Mathf.Max(from + 1, options.Count * (band + 2) / 5);
            sentence = options[Random.Range(from, Mathf.Min(to, options.Count))];
            tokens = sentence.Tokens;

            phase = Phase.Reeling;
            line = LineStrength;
            totalTime = Mathf.Max(8, 8 + tokens.Length * 2.5f + (Fishery.BestRod()?.power ?? 0) - (int)fish.rarity);
            timer = totalTime;
            hookButton.gameObject.SetActive(false);
            bobber.enabled = false;
            shadow.enabled = true;
            shadow.sprite = UIFactory.LoadSprite("Art/Icons/" + fish.id);
            shadow.color = new Color(0.05f, 0.1f, 0.15f, 0.65f);
            float size = 24 + 4 * (int)fish.rarity;
            shadow.rectTransform.sizeDelta = new Vector2(size, size);
            heading.Set("¡Enganchado! Forma la frase para recoger el sedal:", "Hooked! Build the sentence to reel it in:");
            english.text = "\"" + sentence.english + "\"";
            hintButton.gameObject.SetActive(true);
            ShowLine();

            shuffled.Clear();
            for (int i = 0; i < tokens.Length; i++) shuffled.Add(i);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            Redraw();
        }

        void ShowLine() => lineText.text = "Sedal " + new string('●', line) + new string('○', LineStrength - line);

        // ------------------------------------------------------------ tiles

        static void Clear(Transform row)
        {
            foreach (Transform child in row) Destroy(child.gameObject);
        }

        void Redraw()
        {
            Clear(poolRow);
            Clear(answerRow);
            Layout(answerRow, answer, UIFactory.Gold, null, null);
            Layout(poolRow, shuffled, UIFactory.Teal, Pick, answer);
        }

        void Layout(Transform row, List<int> order, Color color, System.Action<int> onClick, List<int> hidden)
        {
            const float gap = 6;
            float total = -gap;
            foreach (int i in order) total += TileWidth(i) + gap;
            float x = -total / 2;
            foreach (int i in order)
            {
                float width = TileWidth(i);
                if (hidden == null || !hidden.Contains(i))
                {
                    int captured = i;
                    UIFactory.Btn("Tile", row, new Vector2(x + width / 2, 0), new Vector2(width, 26), tokens[i], color,
                        () => onClick?.Invoke(captured));
                }
                x += width + gap;
            }
        }

        float TileWidth(int i) => tokens[i].Length * 8 + 18;

        /// <summary>Clicking a word: the right next word reels the fish in, a wrong one strains the line.</summary>
        void Pick(int i)
        {
            if (phase != Phase.Reeling) return;
            if (tokens[i] == tokens[answer.Count])
            {
                answer.Add(i);
                Redraw();
                if (answer.Count >= tokens.Length) Catch();
                return;
            }
            line--;
            flash = 0.3f;
            ShowLine();
            if (line <= 0) Escape("¡Se rompió el sedal! El pez se escapó.");
            else result.text = "¡El pez tira! Esa no es la palabra.";
        }

        void Hint()
        {
            if (phase != Phase.Reeling || answer.Count >= tokens.Length) return;
            if (!GameState.Data.stats.TrySpendMana(HintCost))
            {
                GameState.Say("No tienes maná. (Not enough mana.)");
                return;
            }
            for (int i = 0; i < tokens.Length; i++)
                if (!answer.Contains(i) && tokens[i] == tokens[answer.Count]) { Pick(i); return; }
        }

        void Catch()
        {
            var item = fish.Item;
            bool added = GameState.AddItem(fish.id);
            bool extra = added && Fishery.HasNet && fish.rarity != Fishery.Rarity.Junk && Random.value < 0.25f && GameState.AddItem(fish.id);
            bool isNew = !GameState.Data.fishSeen.Contains(fish.id);
            if (isNew) GameState.Data.fishSeen.Add(fish.id);
            GameState.Data.fishCaught++;
            int r = (int)fish.rarity;
            GameState.Reward(2 + tokens.Length, 1 + r * 2 + (fish.id == "fish_rune" ? 25 : 0));
            GameState.AddXp(SkillType.Fishing, Lessons.Xp(6 + 6 * r));
            GameState.AddXp(SkillType.Grammar, Lessons.Xp(tokens.Length * 2));

            shadow.color = Color.white;
            float size = 40 + 4 * r;
            shadow.rectTransform.sizeDelta = new Vector2(size, size);
            shadow.rectTransform.anchoredPosition = new Vector2(0, 70);
            phase = Phase.Result;
            Clear(poolRow);
            hintButton.gameObject.SetActive(false);
            againButton.gameObject.SetActive(true);
            baitButton.gameObject.SetActive(true);
            heading.Set((isNew ? "¡Nuevo! " : "") + "¡" + GameState.Capital(item.spanish) + "!  (" + Fishery.RarityEs[r] + ")",
                (isNew ? "New! " : "") + GameState.Capital(item.english) + "!  (" + Fishery.RarityEn[r] + ")");
            english.text = "\"" + sentence.text + "\"";
            result.color = Fishery.RarityColor(fish.rarity);
            result.text = (added ? "Va a tu mochila. " : "¡Tu mochila está llena! ") + "Vale " + item.sellPrice + " " + GameConfig.GoldName
                + (extra ? "\n¡La red atrapa otro!" : "");
            ShowGear();
            GameState.Save();
        }

        void Escape(string message)
        {
            bool reeling = phase == Phase.Reeling;
            phase = Phase.Result;
            Clear(poolRow);
            shadow.enabled = false;
            hookButton.gameObject.SetActive(false);
            hintButton.gameObject.SetActive(false);
            againButton.gameObject.SetActive(true);
            baitButton.gameObject.SetActive(true);
            heading.Set("", "");
            result.color = UIFactory.Ink;
            result.text = message + (reeling ? "\n" + sentence.text : "");     // show the right sentence to learn from
            ShowGear();
        }

        public void Close()
        {
            if (!Open) return;
            window.SetActive(false);
            village.NoteWindowClosed();
        }

        void Update()
        {
            if (!Open) return;
            if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }
            if (phase != Phase.Result) result.color = UIFactory.Ink;

            switch (phase)
            {
                case Phase.Waiting:
                    bobber.rectTransform.anchoredPosition = new Vector2(-120, 74 + Mathf.Sin(Time.time * 3) * 2);
                    timer -= Time.deltaTime;
                    if (timer <= 0) StartBite();
                    break;
                case Phase.Biting:
                    bobber.rectTransform.anchoredPosition = new Vector2(-120 + Random.Range(-1f, 1f), 66 + Mathf.Sin(Time.time * 30) * 3);
                    if (Input.GetKeyDown(KeyCode.Space)) { Hook(); break; }
                    timer -= Time.deltaTime;
                    if (timer <= 0) Escape("Llegaste tarde... el pez se comió el cebo y se fue. (Too slow!)");
                    break;
                case Phase.Reeling:
                    timer -= Time.deltaTime;
                    timerBar.rectTransform.sizeDelta = new Vector2(400 * Mathf.Clamp01(timer / totalTime), 6);
                    float progress = answer.Count / (float)tokens.Length;
                    flash -= Time.deltaTime;
                    float pull = flash > 0 ? Random.Range(-3f, 3f) + 14 : 0;
                    shadow.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(210, -110, progress) + pull + Mathf.Sin(Time.time * 4) * 4,
                        62 + Mathf.Sin(Time.time * 2.3f) * 4);
                    if (timer <= 0) Escape("¡Se escapó! Se acabó el tiempo.");
                    break;
            }
        }
    }
}
