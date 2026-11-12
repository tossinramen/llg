using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>One plot in your vegetable patch (saved).</summary>
    [Serializable]
    public class PlotState
    {
        public int x, y;
        public string crop = "";     // "" = empty
        public float growth;         // 0 .. 1 (1 = ripe)
        public float boost;          // seconds of faster growth left (from watering)
    }

    /// <summary>
    /// The vegetable patch ("el huerto") next to your house. Buy seeds at the Semillería, then face a
    /// plot and press E: plant, water (answer a quick Spanish question to make it grow twice as fast
    /// for a while), and harvest when ripe. Different crops take longer but sell for more.
    /// </summary>
    public class Farm : MonoBehaviour
    {
        readonly Dictionary<Vector2Int, (PlotState state, SpriteRenderer r)> plots = new Dictionary<Vector2Int, (PlotState, SpriteRenderer)>();
        WateringWindow watering;

        public void Build(VillageMap map, Transform parent, WateringWindow wateringWindow)
        {
            watering = wateringWindow;
            var saved = GameState.Data.plots;
            foreach (var tile in map.Find('+'))
            {
                var state = saved.Find(p => p.x == tile.x && p.y == tile.y);
                if (state == null)
                {
                    state = new PlotState { x = tile.x, y = tile.y };
                    saved.Add(state);
                }
                var r = new GameObject("Plot").AddComponent<SpriteRenderer>();
                r.transform.SetParent(parent, false);
                r.transform.position = VillageMap.Center(tile) + new Vector2(0, 0.1f);
                r.sortingOrder = Mathf.RoundToInt(-(r.transform.position.y - 0.5f) * 100);
                plots[tile] = (state, r);
                Refresh(state, r);
            }
        }

        static (string id, string es, string en, int seed, int sell, float grow) Crop(string id) =>
            Array.Find(ItemDatabase.Crops, c => c.id == id);

        void Refresh(PlotState s, SpriteRenderer r)
        {
            if (string.IsNullOrEmpty(s.crop)) { r.sprite = null; return; }
            int stage = s.growth >= 1 ? 2 : s.growth >= 0.35f ? 1 : 0;
            r.sprite = UIFactory.LoadSprite("Art/World/Crops/" + s.crop + "_" + stage);
        }

        /// <summary>What pressing E on this plot would do (shown in the prompt).</summary>
        public string Describe(Vector2Int tile)
        {
            if (!plots.TryGetValue(tile, out var p)) return null;
            if (string.IsNullOrEmpty(p.state.crop)) return "Plantar";
            if (p.state.growth >= 1) return "Cosechar " + Crop(p.state.crop).es;
            return "Regar " + Crop(p.state.crop).es + " (" + Mathf.RoundToInt(p.state.growth * 100) + "%)";
        }

        public void Use(Vector2Int tile)
        {
            if (!plots.TryGetValue(tile, out var p)) return;
            var s = p.state;
            if (string.IsNullOrEmpty(s.crop))
            {
                // plant the first seeds in the bag
                var bag = GameState.Data.bag;
                for (int i = 0; i < bag.Count; i++)
                {
                    var item = ItemDatabase.Get(bag[i].id);
                    if (item == null || item.kind != ItemKind.Seed) continue;
                    s.crop = item.id.Substring(5);
                    s.growth = 0;
                    GameState.RemoveAt(i);
                    GameState.Say("Plantas " + Crop(s.crop).es + ".");
                    GameState.AddXp(SkillType.Farming, 2);
                    Refresh(s, p.r);
                    return;
                }
                GameState.Say("No tienes semillas. Cómpralas en la semillería. (No seeds - buy some at the Seed Shop.)");
                return;
            }
            if (s.growth >= 1)
            {
                var crop = Crop(s.crop);
                if (!GameState.AddItem("crop_" + crop.id, 2)) return;
                GameState.Say("¡Cosechas " + crop.es + "! (x2)");
                GameState.AddXp(SkillType.Farming, 4 + crop.seed / 3);
                s.crop = "";
                s.growth = 0;
                Refresh(s, p.r);
                return;
            }
            watering.Begin(() =>
            {
                s.boost = 45f;
                GameState.Say("¡Riegas la planta! Crece más rápido.");
                GameState.AddXp(SkillType.Farming, Lessons.Xp(3));
            });
        }

        void Update()
        {
            foreach (var (s, r) in plots.Values)
            {
                if (string.IsNullOrEmpty(s.crop) || s.growth >= 1) continue;
                float speed = s.boost > 0 ? 2f : 1f;
                s.boost = Mathf.Max(0, s.boost - Time.deltaTime);
                int before = s.growth >= 0.35f ? 1 : 0;
                s.growth = Mathf.Min(1, s.growth + Time.deltaTime * speed / Crop(s.crop).grow);
                int after = s.growth >= 1 ? 2 : s.growth >= 0.35f ? 1 : 0;
                if (after != before) Refresh(s, r);
            }
        }
    }

    /// <summary>The watering exercise: pick the Spanish word for an English one. Right = the plant grows faster.</summary>
    public class WateringWindow : GameWindow
    {
        readonly Button[] options = new Button[3];
        readonly LocalizedText[] optionText = new LocalizedText[3];
        LocalizedText question;
        VocabEntry answer;
        readonly VocabEntry[] shown = new VocabEntry[3];
        Action onRight;
        float closeTimer = -1;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, 0), new Vector2(420, 200), "Regar", "Watering");
            question = UIFactory.LLabel("Question", W, new Vector2(0, 44), new Vector2(360, 24), "", "", UIFactory.Ink).GetComponent<LocalizedText>();
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                options[i] = UIFactory.Btn("Option", W, new Vector2(-128 + i * 128, -10), new Vector2(118, 40), "", UIFactory.Teal, () => Choose(index));
                var label = UIFactory.LLabel("Word", options[i].transform, new Vector2(0, 1), new Vector2(106, 32), "", "", Color.white);
                UIFactory.Outline(label);
                optionText[i] = label.GetComponent<LocalizedText>();
            }
            UIFactory.LLabel("Tip", W, new Vector2(0, -60), new Vector2(360, 20), "Acierta para que crezca más rápido.", "Get it right to make it grow faster.", UIFactory.Faded);
        }

        public void Begin(Action whenRight)
        {
            onRight = whenRight;
            closeTimer = -1;
            var words = Lessons.Words();
            var pool = new List<VocabEntry>(words);
            for (int i = 0; i < 3; i++)
            {
                int pick = UnityEngine.Random.Range(0, pool.Count);
                shown[i] = pool[pick];
                pool.RemoveAt(pick);
                options[i].interactable = true;
                options[i].GetComponent<Image>().color = UIFactory.Teal;
                optionText[i].Set(shown[i].word, shown[i].english);
            }
            answer = shown[UnityEngine.Random.Range(0, 3)];
            question.Set("¿Cómo se dice \"" + answer.english + "\"?", "How do you say \"" + answer.english + "\" in Spanish?");
            OpenWindow();
        }

        void Choose(int i)
        {
            if (closeTimer >= 0) return;
            bool right = shown[i] == answer;
            for (int k = 0; k < 3; k++)
            {
                options[k].interactable = false;
                if (shown[k] == answer) options[k].GetComponent<Image>().color = UIFactory.Green;
                else if (k == i) options[k].GetComponent<Image>().color = UIFactory.Red;
            }
            if (right) onRight?.Invoke();
            else GameState.Say("Casi... " + answer.english + " = " + answer.word);
            closeTimer = 1.0f;
        }

        protected override void Update()
        {
            base.Update();
            if (!Open || closeTimer < 0) return;
            closeTimer -= Time.deltaTime;
            if (closeTimer < 0) Close();
        }
    }
}
