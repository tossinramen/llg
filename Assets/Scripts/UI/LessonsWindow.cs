using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// The lesson board (L, or the board in the plaza): choose a topic and a level.
    /// Archery, fishing and "El intruso" then use those words; harder levels give more XP.
    /// </summary>
    public class LessonsWindow : GameWindow
    {
        readonly List<(Image img, string id)> topicButtons = new List<(Image, string)>();
        readonly List<Image> levelButtons = new List<Image>();
        LocalizedText summary, sample;

        protected override bool ToggleKeyPressed => Keybinds.Down(GameAction.Lessons);

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -4), new Vector2(560, 330), "Lecciones", "Lessons");
            UIFactory.LLabel("TopicTitle", W, new Vector2(0, 112), new Vector2(400, 20), "Elige un tema", "Choose a topic", UIFactory.Ink);

            var topics = new List<CategoryEntry> { new CategoryEntry { id = "", name = "Todos", english = "Everything" } };
            topics.AddRange(GameState.Pack.categories);
            for (int i = 0; i < topics.Count && i < 16; i++)
            {
                var c = topics[i];
                var pos = new Vector2(-198 + (i % 4) * 132, 86 - (i / 4) * 28);
                var b = UIFactory.LBtn("Topic", W, pos, new Vector2(126, 24), c.name, c.english, UIFactory.Teal, () => Pick(c.id, Lessons.Level));
                topicButtons.Add((b.GetComponent<Image>(), c.id));
            }

            UIFactory.LLabel("LevelTitle", W, new Vector2(0, -6), new Vector2(400, 20), "Elige un nivel", "Choose a level", UIFactory.Ink);
            for (int i = 0; i < 5; i++)
            {
                int level = i + 1;
                var b = UIFactory.LBtn("Level", W, new Vector2(-212 + i * 106, -32), new Vector2(100, 26),
                    Lessons.LevelNames[i], Lessons.LevelEnglish[i], UIFactory.Blue, () => Pick(Lessons.Category, level));
                levelButtons.Add(b.GetComponent<Image>());
            }
            summary = UIFactory.LLabel("Summary", W, new Vector2(0, -62), new Vector2(520, 20), "", "", UIFactory.Purple).GetComponent<LocalizedText>();
            sample = UIFactory.LLabel("Sample", W, new Vector2(0, -88), new Vector2(520, 36), "", "", UIFactory.Faded).GetComponent<LocalizedText>();
            sample.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;
            UIFactory.LBtn("Practice", W, new Vector2(-110, -132), new Vector2(200, 26), "Jugar a El intruso", "Play The Odd One Out", UIFactory.Green, () =>
            {
                Close();
                village.StartIntruso();
            });
            UIFactory.LLabel("Where", W, new Vector2(120, -132), new Vector2(260, 20), "También: tiro con arco y pesca", "Also: archery and fishing", UIFactory.Faded);
        }

        public void Toggle()
        {
            if (Open) { Close(); return; }
            OpenWindow();
            Refresh();
        }

        void Pick(string category, int level)
        {
            GameState.Data.lessonCategory = category;
            GameState.Data.lessonLevel = level;
            GameState.Save();
            Refresh();
        }

        void Refresh()
        {
            foreach (var (img, id) in topicButtons) img.color = id == Lessons.Category ? UIFactory.Gold : UIFactory.Teal;
            for (int i = 0; i < levelButtons.Count; i++) levelButtons[i].color = i + 1 == Lessons.Level ? UIFactory.Gold : UIFactory.Blue;
            string mult = "x" + Lessons.Multiplier.ToString("0.#");
            summary.Set(Lessons.CategoryName() + " - " + Lessons.LevelNames[Lessons.Level - 1] + "   (XP " + mult + ")",
                Lessons.CategoryName(true) + " - " + Lessons.LevelEnglish[Lessons.Level - 1] + "   (XP " + mult + ")");
            var words = Lessons.Words();
            var es = new List<string>();
            var en = new List<string>();
            for (int i = 0; i < words.Count && i < 8; i++) { es.Add(words[i].word); en.Add(words[i].english); }
            sample.Set(string.Join(",  ", es), string.Join(",  ", en));
            translate.Set(translate.English);
        }
    }
}
