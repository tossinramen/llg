using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// La mina: what each rock can give, and what your pickaxe can break. Relics (fossils, Roman coins,
    /// an amphora, flint arrowheads) are the archaeology side of mining: rare finds in any rock.
    /// </summary>
    public static class Mine
    {
        public enum Rock { Stone, Rich, Crystal }

        public static readonly string[] Picks = { "pickaxe", "pickaxe_iron", "pickaxe_steel", "pickaxe_diamond" };
        public static readonly string[] Relics = { "relic_arrowhead", "relic_fossil", "relic_coin", "relic_vase" };

        static readonly (string id, int weight, int tier)[] StoneLoot = { ("ore_stone", 40, 1), ("ore_coal", 30, 1), ("ore_copper", 20, 1), ("ore_iron", 10, 2) };
        static readonly (string id, int weight, int tier)[] RichLoot = { ("ore_copper", 20, 1), ("ore_iron", 30, 2), ("ore_silver", 25, 2), ("ore_gold", 15, 3) };
        static readonly (string id, int weight, int tier)[] CrystalLoot = { ("ore_silver", 20, 2), ("gem_emerald", 35, 3), ("gem_ruby", 30, 3), ("gem_sapphire", 22, 4), ("gem_diamond", 8, 4) };

        public static Rock KindOf(char letter) => letter == 'Ř' ? Rock.Rich : letter == 'ř' ? Rock.Crystal : Rock.Stone;
        public static int TierNeeded(Rock r) => (int)r + 1;
        public static readonly string[] RockEs = { "la roca", "la veta de mineral", "la roca de cristal" };

        public static ItemDef BestPick()
        {
            for (int i = Picks.Length - 1; i >= 0; i--)
                if (GameState.Has(Picks[i])) return ItemDatabase.Get(Picks[i]);
            return null;
        }

        public static bool HelmetOn => GameState.Equipped(EquipSlot.Hat)?.id == "hat_miner";

        /// <summary>What comes out of a rock you broke: an ore or gem your pickaxe allows, or now and then a relic.</summary>
        public static string Loot(Rock rock)
        {
            int tier = BestPick()?.tier ?? 1;
            float relic = 0.06f * (HelmetOn ? 2 : 1) + 0.002f * GameState.Data.skills.Get(SkillType.Mining).level;
            if (Random.value < relic)
            {
                float r = Random.value;
                return r < 0.30f ? Relics[0] : r < 0.65f ? Relics[1] : r < 0.90f ? Relics[2] : Relics[3];
            }
            var table = rock == Rock.Stone ? StoneLoot : rock == Rock.Rich ? RichLoot : CrystalLoot;
            int total = 0;
            foreach (var e in table) if (e.tier <= tier) total += e.weight;
            int roll = Random.Range(0, total);
            foreach (var e in table)
            {
                if (e.tier > tier) continue;
                roll -= e.weight;
                if (roll < 0) return e.id;
            }
            return "ore_stone";
        }
    }

    /// <summary>
    /// Mining a rock: spell Spanish words letter by letter to swing the pickaxe. Every right letter is
    /// a hit (cracks appear); spell all the rock's words to break it. A wrong letter blunts the
    /// pickaxe (3 and the rock holds). Stone needs 1 word, ore veins 2, crystal rocks 3.
    /// Trains Mining (Minería) and Spelling (Ortografía).
    /// </summary>
    public class MiningGame : GameWindow
    {
        const int Sharpness = 3, HintCost = 2;
        Image rockImage, crack, loot;
        Text wordText, englishText, edgeText, gearText, resultText;
        Image timerBar;
        Transform letters;
        Button hintButton;
        Mine.Rock rock;
        Vector2Int rockTile;
        System.Action<Vector2Int> onBroken;
        string word, article;
        int next, wordsLeft, wordsTotal, edge, hits, freeHints;
        float timer, totalTime;
        bool playing;
        readonly HashSet<string> used = new HashSet<string>();

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -6), new Vector2(560, 312), "La mina", "The Mine");
            gearText = UIFactory.Label("Gear", W, new Vector2(0, 108), new Vector2(520, 16), "", UIFactory.Faded, TextAnchor.MiddleCenter, 12);
            UIFactory.Box("RockBack", W, new Vector2(-180, 26), new Vector2(150, 130), new Color32(60, 52, 46, 255));
            rockImage = UIFactory.Img("Rock", W, new Vector2(-180, 26), new Vector2(112, 96), Color.white);
            crack = UIFactory.Img("Crack", W, new Vector2(-180, 26), new Vector2(64, 64), Color.white);
            loot = UIFactory.Img("Loot", W, new Vector2(-180, 26), new Vector2(64, 64), Color.white);
            englishText = UIFactory.Label("English", W, new Vector2(80, 70), new Vector2(330, 20), "", UIFactory.Teal);
            wordText = UIFactory.Label("Word", W, new Vector2(80, 38), new Vector2(330, 26), "", UIFactory.Purple, TextAnchor.MiddleCenter, 16);
            edgeText = UIFactory.Label("Edge", W, new Vector2(80, 12), new Vector2(330, 16), "", UIFactory.Ink, TextAnchor.MiddleCenter, 12);
            UIFactory.Box("TimerBack", W, new Vector2(80, -6), new Vector2(304, 10), UIFactory.Faded);
            timerBar = UIFactory.Img("Timer", W, new Vector2(80, -6), new Vector2(300, 6), UIFactory.Green);
            letters = UIFactory.Rect("Letters", W, new Vector2(0, -58), Vector2.zero);
            resultText = UIFactory.Label("Result", W, new Vector2(0, -98), new Vector2(520, 36), "", UIFactory.Ink);
            hintButton = UIFactory.LBtn("Hint", W, new Vector2(-150, -132), new Vector2(170, 24), "Pista (2 maná)", "Hint (2 mana)", UIFactory.Blue, Hint);
            UIFactory.LBtn("Leave", W, new Vector2(150, -132), new Vector2(150, 24), "Salir", "Leave", UIFactory.Red, Close);
        }

        /// <summary>Starts mining the rock on this tile. "broken" is called when it breaks.</summary>
        public void Begin(Mine.Rock kind, Vector2Int tile, System.Action<Vector2Int> broken)
        {
            rock = kind;
            rockTile = tile;
            onBroken = broken;
            wordsTotal = wordsLeft = (int)kind + 1;
            edge = Sharpness;
            hits = 0;
            freeHints = Mine.HelmetOn ? 1 : 0;
            used.Clear();
            OpenWindow();
            rockImage.sprite = UIFactory.LoadSprite("Art/World/Objects/rock_" + kind.ToString().ToLower());
            rockImage.enabled = true;
            loot.enabled = false;
            resultText.text = "";
            var pick = Mine.BestPick();
            totalTime = 14 + (pick?.power ?? 0);
            int seen = 0;
            foreach (var r in Mine.Relics) if (GameState.Data.relicsFound.Contains(r)) seen++;
            gearText.text = GameState.Capital(pick.spanish) + (Mine.HelmetOn ? "   ·   Casco de minero" : "") + "   ·   Minería nv "
                + GameState.Data.skills.Get(SkillType.Mining).level + "   ·   Reliquias " + seen + "/" + Mine.Relics.Length;
            hintButton.gameObject.SetActive(true);
            NextWord();
        }

        void NextWord()
        {
            var pool = Lessons.Words(8);
            var candidates = new List<(string article, string word, string english)>();
            foreach (var v in pool)
            {
                var (a, w) = Split(v.word);
                if (w.Length < 3 || w.Length > 10 || w.Contains(" ") || used.Contains(w)) continue;
                candidates.Add((a, w, v.english));
            }
            if (candidates.Count == 0) { used.Clear(); NextWord(); return; }
            // tougher rocks ask for longer words
            candidates.Sort((x, y) => x.word.Length.CompareTo(y.word.Length));
            int from = rock == Mine.Rock.Stone ? 0 : rock == Mine.Rock.Rich ? candidates.Count / 3 : candidates.Count / 2;
            var pickWord = candidates[Random.Range(from, candidates.Count)];
            word = pickWord.word;
            article = pickWord.article;
            used.Add(word);
            englishText.text = "\"" + pickWord.english + "\"   (" + (wordsTotal - wordsLeft + 1) + "/" + wordsTotal + ")";
            next = 0;
            timer = totalTime;
            playing = true;
            if (freeHints > 0) { freeHints--; next = 1; hits++; }      // the helmet lamp shows the first letter
            Redraw();
        }

        /// <summary>"GAT__" -> "G A T _ _", so each letter is easy to see.</summary>
        static string Spaced(string s) => string.Join(" ", s.ToCharArray());

        static (string, string) Split(string spanish)
        {
            string s = spanish.Trim().ToLower();
            foreach (var a in new[] { "el ", "la ", "los ", "las ", "un ", "una " })
                if (s.StartsWith(a)) return (a.Trim(), s.Substring(a.Length));
            return ("", s);
        }

        void Redraw()
        {
            foreach (Transform c in letters) Destroy(c.gameObject);
            wordText.text = (article.Length > 0 ? article + "  " : "") + Spaced(word.Substring(0, next).ToUpper() + new string('_', word.Length - next));
            edgeText.text = "Pico " + new string('●', edge) + new string('○', Sharpness - edge);
            crack.enabled = hits > 0;
            int stage = Mathf.Clamp(hits / Mathf.Max(1, wordsTotal * 2), 0, 3);      // cracks grow as you hit
            if (crack.enabled) crack.sprite = UIFactory.LoadSprite("Art/World/crack_" + stage);

            // the word's letters still to place, plus a few decoys, shuffled
            var tiles = new List<char>();
            for (int i = next; i < word.Length; i++) tiles.Add(word[i]);
            const string abc = "abcdefghijlmnoprstuvz";
            var rng = new System.Random(word.GetHashCode() + next);
            for (int k = 0; k < 3; k++) tiles.Add(abc[rng.Next(abc.Length)]);
            for (int i = tiles.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); (tiles[i], tiles[j]) = (tiles[j], tiles[i]); }
            float x = -(tiles.Count - 1) * 19f;
            foreach (char ch in tiles)
            {
                char c = ch;
                UIFactory.Btn("Letter", letters, new Vector2(x, 0), new Vector2(34, 32), c.ToString().ToUpper(), new Color32(150, 120, 90, 255), () => Swing(c));
                x += 38;
            }
        }

        void Swing(char c)
        {
            if (!playing) return;
            if (c == word[next])
            {
                next++;
                hits++;
                shake = 0.15f;
                if (next >= word.Length)
                {
                    wordsLeft--;
                    if (wordsLeft <= 0) { Break(); return; }
                    NextWord();
                    return;
                }
                Redraw();
                return;
            }
            edge--;
            resultText.text = "¡El pico rebota! Esa letra no va ahí.";
            if (edge <= 0) { Fail("Tu pico se ha desafilado. La roca aguanta... (The rock holds.)"); return; }
            Redraw();
        }

        void Hint()
        {
            if (!playing) return;
            if (!GameState.Data.stats.TrySpendMana(HintCost)) { GameState.Say("No tienes maná. (Not enough mana.)"); return; }
            Swing(word[next]);
        }

        void Break()
        {
            playing = false;
            foreach (Transform t in letters) Destroy(t.gameObject);
            string id = Mine.Loot(rock);
            var item = ItemDatabase.Get(id);
            bool relic = id.StartsWith("relic_");
            bool extra = !relic && Random.value < GameState.Data.skills.Get(SkillType.Mining).level * 0.01f;
            bool added = GameState.AddItem(id, extra ? 2 : 1);
            if (relic && !GameState.Data.relicsFound.Contains(id)) GameState.Data.relicsFound.Add(id);
            int level = (int)rock;
            GameState.Reward(2 + wordsTotal * 2, 1 + level);
            GameState.AddXp(SkillType.Mining, Lessons.Xp(5 + 6 * level + (relic ? 15 : 0)));
            GameState.AddXp(SkillType.Spelling, Lessons.Xp(hits));
            rockImage.enabled = false;
            crack.enabled = false;
            loot.enabled = true;
            loot.sprite = UIFactory.LoadSprite("Art/Icons/" + item.icon);
            wordText.text = "¡" + GameState.Capital(item.spanish) + (extra ? " ×2" : "") + "!";
            englishText.text = item.english + (relic ? "  —  ¡Un hallazgo arqueológico! (An archaeological find!)" : "");
            edgeText.text = "";
            resultText.text = (added ? "Va a tu mochila. " : "¡Tu mochila está llena! ") + "Vale " + item.sellPrice + " " + GameConfig.GoldName + " cada uno.";
            hintButton.gameObject.SetActive(false);
            onBroken?.Invoke(rockTile);
            GameState.Save();
        }

        void Fail(string message)
        {
            playing = false;
            foreach (Transform t in letters) Destroy(t.gameObject);
            wordText.text = (article.Length > 0 ? article + "  " : "") + Spaced(word.ToUpper());
            resultText.text = message;
            hintButton.gameObject.SetActive(false);
        }

        float shake;

        protected override void Update()
        {
            base.Update();
            if (!Open) return;
            shake -= Time.deltaTime;
            rockImage.rectTransform.anchoredPosition = new Vector2(-180, 26) + (shake > 0 ? Random.insideUnitCircle * 3 : Vector2.zero);
            if (!playing) return;
            timer -= Time.deltaTime;
            timerBar.rectTransform.sizeDelta = new Vector2(300 * Mathf.Clamp01(timer / totalTime), 6);
            if (timer <= 0) Fail("¡Se acabó el tiempo! (Out of time.)");
        }
    }

    /// <summary>Rocks in the mine turn to rubble when broken and come back after a while.</summary>
    public class MineRocks : MonoBehaviour
    {
        const float Respawn = 45f;
        readonly Dictionary<Vector2Int, (ClickableObject obj, Sprite sprite, float timer)> broken = new Dictionary<Vector2Int, (ClickableObject, Sprite, float)>();
        List<ClickableObject> objects;
        Sprite rubble;

        public void Build(List<ClickableObject> all)
        {
            objects = all;
            rubble = UIFactory.LoadSprite("Art/World/Objects/rock_rubble");
        }

        ClickableObject Find(Vector2Int tile, char letter)
        {
            ClickableObject best = null;
            float bestDist = 1.6f;
            var center = VillageMap.Center(tile);
            foreach (var o in objects)
            {
                if (o.letter != letter) continue;
                float d = Vector2.Distance(o.basePos, center);
                if (d < bestDist) { best = o; bestDist = d; }
            }
            return best;
        }

        public bool IsBroken(Vector2Int tile, char letter)
        {
            var o = Find(tile, letter);
            return o != null && broken.ContainsKey(o.Tile);
        }

        public void Break(Vector2Int tile, char letter)
        {
            var o = Find(tile, letter);
            if (o == null || broken.ContainsKey(o.Tile)) return;
            broken[o.Tile] = (o, o.renderer.sprite, Respawn);
            o.renderer.sprite = rubble;
        }

        void Update()
        {
            if (broken.Count == 0) return;
            var keys = new List<Vector2Int>(broken.Keys);
            foreach (var k in keys)
            {
                var b = broken[k];
                b.timer -= Time.deltaTime;
                if (b.timer <= 0)
                {
                    b.obj.renderer.sprite = b.sprite;
                    broken.Remove(k);
                }
                else broken[k] = b;
            }
        }
    }
}
